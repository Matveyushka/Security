using CyberEnvironment.Security;

public class PrivateDoc : IProtectedObject<User>
{
    public Guid Id { get; set; }
    public SecureObject<User> SecureObject { get; set; } = null!;
    public int SecretNumber { get; set; }
}