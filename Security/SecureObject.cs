namespace CyberEnvironment.Security;

public class SecureObject<SUser>
    where SUser : class, ISecureUser<SUser>
{
    public Guid Id { get; set; }
    public ICollection<SecureRelation<SUser>> Relations { get; set; } = new List<SecureRelation<SUser>>();
}