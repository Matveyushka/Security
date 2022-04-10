using Microsoft.EntityFrameworkCore;
using CyberEnvironment.Security;

public class SecurityDemoDbContext : DbContext, ISecureDbContext<User>
{
    public DbSet<User> User => Set<User>();
    public DbSet<SecureObject<User>> SecureObject => Set<SecureObject<User>>();
    public DbSet<SecureRole<User>> Role => Set<SecureRole<User>>();
    public DbSet<SecureRelation<User>> Relation => Set<SecureRelation<User>>();

    public DbSet<AwfulMystery> Mysteries => Set<AwfulMystery>();
    public DbSet<ImportantDocument> Importants => Set<ImportantDocument>();
    public DbSet<PrivateDoc> Privates => Set<PrivateDoc>();

    public SecurityDemoDbContext(DbContextOptions<SecurityDemoDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ((ISecureDbContext<User>)this).SetDbRelations(modelBuilder);
    }
}