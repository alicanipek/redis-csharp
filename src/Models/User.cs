using System.Security.Cryptography;
using System.Text;
using codecrafters_redis.src.Infrastructure;

namespace codecrafters_redis.src.Models;

public class User(string userName)
{
    public string UserName { get; private set; } = userName;
    private string[] Passwords { get; set; } = Array.Empty<string>();
    private string[] Flags { get; set; } = ["nopass"];

    public void SetPassword(string password)
    {
        var sha256Hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLower();
        Passwords = [sha256Hash];
        Flags = [.. Flags.Where(f => f != "nopass")];
    }

    public byte[] ToRespArrayBytes()
    {
        return RespParser.EncodeRespArrayBytes(["flags", Flags, "passwords", Passwords]);
    }

    public bool ValidatePassword(string password)
    {
        var sha256Hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).ToLower();
        return Passwords.Contains(sha256Hash);
    }

}
