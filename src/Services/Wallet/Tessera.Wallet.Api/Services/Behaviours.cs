using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Serilog;
using Tessera.Wallet.Api.Db;
using Tessera.Wallet.Api.Handlers.Models;

namespace Tessera.Wallet.Api.Services;

public class LoggingBehavior<TReq, TRes> : IPipelineBehavior<TReq, TRes> where TReq : class
{
    public async Task<TRes> Handle(TReq req, RequestHandlerDelegate<TRes> next, CancellationToken ct)
    {
        Log.Information("Handling {Request}", typeof(TReq).Name);
        var result = await next();
        Log.Information("Handled {Request}", typeof(TReq).Name);
        return result;
    }
}

public class TransactionBehavior<TReq, TRes> : IPipelineBehavior<TReq, TRes> where TReq : ICommand<TRes>
{
    private readonly WalletDbContext _db;

    private static readonly ResiliencePipeline Retry = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder()
                .Handle<DbUpdateConcurrencyException>()
                .Handle<DbUpdateException>(),
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = TimeSpan.FromMilliseconds(20),
            OnRetry = args =>
            {
                Log.Warning("Concurrency conflict on {Request}, retry {Attempt}",
                    typeof(TReq).Name, args.AttemptNumber + 1);
                return default;
            }
        })
        .Build();

    public TransactionBehavior(WalletDbContext db) => _db = db;

    public async Task<TRes> Handle(TReq req, RequestHandlerDelegate<TRes> next, CancellationToken ct)
    {
        if (_db.Database.CurrentTransaction is not null)
            return await next();

        return await Retry.ExecuteAsync(async token =>
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                _db.ChangeTracker.Clear();

                await using var tx =
                    await _db.Database.BeginTransactionAsync(token);

                var result = await next();

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(token);

                return result;
            });
        }, ct);
    }
}