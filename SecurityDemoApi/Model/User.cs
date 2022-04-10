using CyberEnvironment.Security;

public class User : ISecureUser<User>
{
    public Guid Id { get; set; }
    public string ExternalUserId { get; set; } = null!;
    public ICollection<SecureRole<User>> Roles { get; set; } = new List<SecureRole<User>>();
    public ICollection<SecureRelation<User>> Relations { get; set; } = new List<SecureRelation<User>>();
}