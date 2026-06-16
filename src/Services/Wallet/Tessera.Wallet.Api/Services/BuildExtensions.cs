using MassTransit;
using Tessera.Wallet.Api.Consumers;
using Tessera.Wallet.Api.Db;

namespace Tessera.Wallet.Api.Services;

public static class BuildExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMassTransitConfiguration(string? connectionRabbitMq)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<ReserveFundsConsumer>();
                x.AddConsumer<SettleBetConsumer>();
                x.AddConsumer<RefundBetConsumer>();

                x.AddEntityFrameworkOutbox<WalletDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(connectionRabbitMq);
                    cfg.ConfigureEndpoints(ctx);
                });
            });

            return services;
        }
    }
}