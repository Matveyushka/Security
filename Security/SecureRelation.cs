namespace CyberEnvironment.Security;

public class SecureRelation<SUser>
    where SUser : class, ISecureUser<SUser>
{
    public Guid Id { get; set; }
    public string Relation { get; set; } = null!;

    public Guid UserId { get; set; }
    public Guid SecureObjectId { get; set; }

    public SUser User { get; set; } = null!;
    public SecureObject<SUser> SecureObject { get; set; } = null!;
}