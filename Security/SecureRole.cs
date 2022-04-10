namespace CyberEnvironment.Security;

public class SecureRole<SUser>
    where SUser : class, ISecureUser<SUser>
{
    public Guid Id { get; set; }
    public string Role { get; set; } = null!;

    public ICollection<SUser> Users { get; set; } = new List<SUser>();
}