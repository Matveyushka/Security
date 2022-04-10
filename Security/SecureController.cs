using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CyberEnvironment.Security;

public class SecureController<SDBContext, SUser> : ControllerBase
    where SDBContext : DbContext, ISecureDbContext<SUser>
    where SUser : class, ISecureUser<SUser>
{
    private SDBContext? securityDbContext;

    protected SDBContext? SecurityDbContext => this.securityDbContext ??
        (this.securityDbContext = HttpContext.RequestServices.GetService<SDBContext>());

    protected bool IsAuthencticated => this.User.Identity?.IsAuthenticated ?? false;

    protected string? UserId => this.User.Identity?.Name ?? null;

    protected SUser? LocalUser => UserId is not null ?
        this.SecurityDbContext?
            .User
            .Include(user => user.Roles)
            .Include(user => user.Relations)
            .FirstOrDefault(user => user.ExternalUserId == UserId) :
        null;


    private string GetProtectedObjectIdName(IProtectedObject<SUser> protectedObject) => 
        protectedObject.GetType().ToString() + "Id";

    private SecureObject<SUser>? GetSecureObject(IProtectedObject<SUser> protectedObject)
    {   
        var keyName = GetProtectedObjectIdName(protectedObject);

        var secureObject = SecurityDbContext?
            .SecureObject
            .FirstOrDefault(so => EF.Property<Guid>(so, keyName) == protectedObject.Id);

        if (secureObject is null && SecurityDbContext is not null)
        {
            secureObject = new SecureObject<SUser>();
            protectedObject.SecureObject = secureObject;
        }

        return secureObject;
    }

    #region IsUserInRole

    protected bool IsUserInRole(string roleName) => this.LocalUser?
        .Roles
        .FirstOrDefault(r => r.Role == roleName) is SecureRole<SUser>;

    protected bool IsUserInRole(SecureRole<SUser> role) => this.LocalUser?
        .Roles
        .Contains(role) ?? false;

    #endregion

    #region AddRoleToUser

    protected void AddRoleToUser(SecureRole<SUser> role, SUser user)
    {
        if (role is not null && user is not null)
        {
            if (user.Roles is not null)
            {
                user
                    .Roles
                    .Add(role);
            }
            else
            {
                SecurityDbContext?
                    .User
                    .FirstOrDefault(u => u == user)?
                    .Roles
                    .Add(role);
            }
        }
    }

    protected void AddRoleToUser(string roleName, SUser user)
    {
        var role = SecurityDbContext?.Role.FirstOrDefault(r => r.Role == roleName);
        if (role is not default(SecureRole<SUser>))
        {
            this.AddRoleToUser(role, user);
        }
    }

    protected void AddRoleToUser(SecureRole<SUser> role)
    {
        if (role is not null && LocalUser is SUser user)
        {
            this.AddRoleToUser(role, user);
        }
    }

    protected void AddRoleToUser(string roleName)
    {
        if (LocalUser is SUser user)
        {
            this.AddRoleToUser(roleName, user);
        }
    }

    #endregion

    #region RemoveRoleFromUser

    protected void RemoveRoleFromUser(SecureRole<SUser> role, SUser user)
    {
        if (role is not null && user is not null)
        {
            if (user.Roles is not null)
            {
                user
                    .Roles
                    .Remove(role);
            }
            else
            {
                SecurityDbContext?
                    .User
                    .FirstOrDefault(u => u == user)?
                    .Roles
                    .Remove(role);
            }
        }
    }

    protected void RemoveRoleFromUser(string roleName, SUser user)
    {
        var role = SecurityDbContext?.Role.FirstOrDefault(r => r.Role == roleName);
        if (role is not default(SecureRole<SUser>))
        {
            this.RemoveRoleFromUser(role, user);
        }
    }

    protected void RemoveRoleFromUser(SecureRole<SUser> role)
    {
        if (role is not null && LocalUser is SUser user)
        {
            this.RemoveRoleFromUser(role, user);
        }
    }

    protected void RemoveRoleFromUser(string roleName)
    {
        if (LocalUser is SUser user)
        {
            this.RemoveRoleFromUser(roleName, user);
        }
    }

    #endregion

    #region HasUserRelation

    protected bool HasUserRelation(string relationName, SUser user, IProtectedObject<SUser> protectedObject)
    {
        if (protectedObject?.SecureObject is null && protectedObject is not null)
        {
            SecurityDbContext?
                .Entry(protectedObject)
                .Reference<SecureObject<SUser>>("SecureObject")
                .Load();
        }
        
        return protectedObject?
            .SecureObject?
            .Relations
            .Any(r => r.User == user && r.Relation == relationName) ?? false;
    }

    protected bool HasUserRelation(string relationName, IProtectedObject<SUser> protectedObject) =>
        LocalUser is not null ?
        HasUserRelation(relationName, LocalUser, protectedObject) :
        false;

    #endregion

    #region AddUserRelation

    protected void AddUserRelation(string relationName, SUser user, IProtectedObject<SUser> protectedObject)
    {
        var secureObject = GetSecureObject(protectedObject);
        if (user is not null)
        {
            var relation = new SecureRelation<SUser>();
            relation.User = user;
            relation.SecureObject = secureObject!;
            relation.Relation = relationName;
            SecurityDbContext?.Relation.Add(relation);
        }
    }

    protected void AddUserRelation(string relationName, IProtectedObject<SUser> protectedObject)
    {
        if (LocalUser is SUser localUser)
        {
            AddUserRelation(relationName, localUser, protectedObject);
        }
    }

    #endregion

    #region RemoveUserRelation

    protected void RemoveUserRelation(string relationName, SUser user, IProtectedObject<SUser> protectedObject)
    {
        var secureObject = GetSecureObject(protectedObject);
        if (user is not null)
        {
            var toRemove = user
                .Relations?
                .FirstOrDefault(r => r.Relation == relationName && r.SecureObject == secureObject) ??
                SecurityDbContext?
                .Relation
                .FirstOrDefault(r => r.Relation == relationName && r.SecureObject == secureObject);
            if (toRemove is not null)
            {
                SecurityDbContext?
                    .Relation
                    .Remove(toRemove);
            }
        }
    }

    protected void RemoveUserRelation(string relationName, IProtectedObject<SUser> protectedObject)
    {
        if (LocalUser is SUser localUser)
        {
            RemoveUserRelation(relationName, localUser, protectedObject);
        }
    }

    #endregion
}