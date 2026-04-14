using System;
using System.Text;
using codecrafters_redis.src.CommandHandlers;
using codecrafters_redis.src.Infrastructure;
using codecrafters_redis.src.Models;
using codecrafters_redis.src.Replica;
using codecrafters_redis.src.Services;

namespace codecrafters_redis.src.CommandHandlers;

public class PsyncCommandHandler(Config config, ReplicaManager replicaManager) : ICommandHandler
{
    public string CommandName => "PSYNC";
    public bool IsWriteCommand => false;

    public Task<byte[]> HandleAsync(List<object> arguments, Dictionary<int, Dictionary<string, bool>> _watchedKeys, ClientSession? clientSession = null)
    {
        string rdbfile = "UkVESVMwMDEx+glyZWRpcy12ZXIFNy4yLjD6CnJlZGlzLWJpdHPAQPoFY3RpbWXCbQi8ZfoIdXNlZC1tZW3CsMQQAPoIYW9mLWJhc2XAAP/wbjv+wP9aog==";
        byte[] encodedFile = Convert.FromBase64String(rdbfile);
        var length = Encoding.ASCII.GetBytes($"${encodedFile.Length}\r\n");
        var emptyRdbFileBytes = new byte[length.Length + encodedFile.Length];
        Array.Copy(length, 0, emptyRdbFileBytes, 0, length.Length);
        Array.Copy(encodedFile, 0, emptyRdbFileBytes, length.Length, encodedFile.Length);

        var fullres = RespParser.EncodeSimpleString($"FULLRESYNC {config.ReplicaInfo?.Id ?? "8371b4fb1155b71f4a04d3e1bc3e18c4a990aeeb"} {config.ReplicaInfo?.Offset ?? 0}");
        byte[] response = new byte[fullres.Length + emptyRdbFileBytes.Length];
        Array.Copy(fullres, 0, response, 0, fullres.Length);
        Array.Copy(emptyRdbFileBytes, 0, response, fullres.Length, emptyRdbFileBytes.Length);

        if (clientSession != null && clientSession.IsReplica && clientSession.ClientStream != null)
        {
            replicaManager.AddReplica(clientSession.ClientStream);
        }
        return Task.FromResult(response);
    }
}
