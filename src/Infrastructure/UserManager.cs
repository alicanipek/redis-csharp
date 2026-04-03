using codecrafters_redis.src.Models;
namespace codecrafters_redis.src.Infrastructure;

public class UserManager
{
    public static readonly List<User> Users =
    [
        new User("default")
    ];

    public User? GetUser(string userName)
    {
        return Users.FirstOrDefault(u => u.UserName == userName);
    }

    public bool ValidatePassword(User user, string password)
    {
        return user.ValidatePassword(password);
    }
}