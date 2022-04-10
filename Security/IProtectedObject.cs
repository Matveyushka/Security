namespace CyberEnvironment.Security;

public interface IProtectedObject<SUser>
    where SUser : class, ISecureUser<SUser>
{
    Guid Id { get; set; }
    SecureObject<SUser> SecureObject { get; set; }
}
