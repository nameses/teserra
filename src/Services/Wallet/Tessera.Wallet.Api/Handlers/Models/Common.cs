using MediatR;

namespace Tessera.Wallet.Api.Handlers.Models;

public interface ICommand<TResponse> : IRequest<TResponse>;
public interface IQuery<TResponse> : IRequest<TResponse>;