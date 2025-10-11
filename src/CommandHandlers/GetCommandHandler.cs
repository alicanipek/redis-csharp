using System.Text;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class GetCommandHandler : ICommandHandler
{
    private readonly StorageService _storageService;

    public string CommandName => "GET";
    public bool IsWriteCommand => false; 

    public GetCommandHandler(StorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<byte[]> HandleAsync(List<object> arguments)
    {
        if (arguments.Count < 2)
        {
            return RespParser.EncodeErrorString("wrong number of arguments");
        }

        var key = arguments[1].ToString()!;
        var value = await _storageService.GetAsync(key);

        return RespParser.EncodeBulkStringBytes(value);
    }
}