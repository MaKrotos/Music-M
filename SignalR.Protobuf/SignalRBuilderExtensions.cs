using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace SignalR.Protobuf;

public static class SignalRBuilderExtensions
{
    public static TBuilder AddProtobufProtocol<TBuilder>(
        this TBuilder builder,
        IReadOnlyDictionary<int, Type> protobufTypes
    ) where TBuilder : ISignalRBuilder
    {
        builder.Services.RemoveAll<IHubProtocol>();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHubProtocol>(
                new ProtobufProtocol(protobufTypes)
            )
        );
        return builder;
    }
}