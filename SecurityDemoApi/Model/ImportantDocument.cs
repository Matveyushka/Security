using CyberEnvironment.Security;

public class ImportantDocument : IProtectedObject<User>
{
    public Guid Id { get; set; }
    public SecureObject<User> SecureObject { get; set; } = null!;
    public string Content { get; set; } = null!;
}