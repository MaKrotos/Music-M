namespace SignalR.Protobuf;

internal enum HubMessageType
{
    CancelInvocation = 0,
    Close = 1,
    Completion = 2,
    HandshakeRequest = 3,
    HandshakeResponse = 4,
    // InvocationBindingFailure not used, because it is not sent over the wire
    Invocation = 5,
    Ping = 6,
    // StreamBindingFailure not used, because it is not sent over the wire
    StreamInvocation = 7,
    StreamItem = 8
}