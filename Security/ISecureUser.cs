namespace CyberEnvironment.Security;

public interface ISecureUser<SUser>
    where SUser : class, ISecureUser<SUser>
{
    Guid Id { get; set; }
    string ExternalUserId { get; set; }

    ICollection<SecureRole<SUser>> Roles { get; set; }
    ICollection<SecureRelation<SUser>> Relations { get; set; }
}