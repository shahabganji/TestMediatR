using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace TestMediatr.Commands
{
    public class PingCommand : IRequest<string>
    {
        
        
        // Todo: this registers the decorators in the di for creation of the command handler
        [Database]
        [Audit]
        // [Pong]
        internal sealed class PingCommandHandler : IRequestHandler<PingCommand, string>
        {
            public Task<string> Handle(PingCommand request, CancellationToken cancellationToken)
            {
                return Task.FromResult("pong");
            }
        }
        
        internal sealed class PongDecorator : IRequestHandler<PingCommand,string>
        {
            private readonly IRequestHandler<PingCommand, string> _handler;

            public PongDecorator( IRequestHandler<PingCommand,string> handler)
            {
                _handler = handler;
            }
            
            public async Task<string> Handle(PingCommand request, CancellationToken cancellationToken)
            {
                var result = await _handler.Handle(request, cancellationToken)
                    .ConfigureAwait(false);
                return $"Extended {result}";
            }
        }

        internal sealed class DatabaseDecorator<TCommand, TResult> 
            : IRequestHandler<TCommand, TResult> where TCommand : IRequest<TResult>
        {
            private readonly IRequestHandler<TCommand, TResult> _handler;

            public DatabaseDecorator(IRequestHandler<TCommand, TResult> handler)
            {
                _handler = handler;
            }
            
            public Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
            {
                Console.WriteLine("###### Database decorator ########");
                return _handler.Handle(request, cancellationToken);

            }
        }
        internal sealed class AuditDecorator<TCommand,TResult> : IRequestHandler<TCommand, TResult> where TCommand : IRequest<TResult>
        {
            private readonly  IRequestHandler<TCommand, TResult> _handler;

            public AuditDecorator( IRequestHandler<TCommand, TResult> handler)
            {
                _handler = handler;
            }
            
            public Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
            {
                Console.WriteLine("@@@@@@@ Audit decorator @@@@@@@");
                return _handler.Handle(request, cancellationToken);
            }
        }

    }
 
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    internal sealed class PongAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    internal sealed class AuditAttribute : Attribute
    {
    }
        
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    internal sealed class DatabaseAttribute : Attribute
    {
    }
    
}
