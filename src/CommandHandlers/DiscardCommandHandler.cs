using System;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.CommandHandlers;

public class DiscardCommandHandler : ICommandHandler
{
    public string CommandName => "DISCARD";

    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, ClientSession? clientSession = null)
    {
            if (clientSession != null && clientSession.IsMultiActive)
            {
                clientSession.ToggleMultiActiveState(false);
                clientSession.CommandQueue.Clear();
                return Task.FromResult(RespParser.OkBytes);
            }
            return Task.FromResult(RespParser.EncodeErrorString("DISCARD without MULTI"));
    }

}
