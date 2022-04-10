using CyberEnvironment.Security;

public class AwfulMystery : IProtectedObject<User>
{
    public Guid Id { get; set; }
    public SecureObject<User> SecureObject { get; set; } = null!;
    public float StrangeAmount { get; set; }
}