using Microsoft.AspNetCore.Mvc;
using CyberEnvironment.Security;

namespace SecurityDemoApi.Controllers;

[ApiController]
[Route("[controller]")]
public class SecurityDemoController : SecureController<SecurityDemoDbContext, User>
{
    private readonly ILogger<SecurityDemoController> _logger;

    public SecurityDemoController(ILogger<SecurityDemoController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        if (SecurityDbContext is null)
        {
            return StatusCode(500);
        }

        _logger.LogInformation("INFORMATION GET REQUEST...");

        _logger.LogInformation("User is authenticated: {1}", IsAuthencticated);

        _logger.LogInformation("User id: {1}", UserId);

        _logger.LogInformation("Local user id: {1}", LocalUser?.Id);

        _logger.LogInformation("User is admin: {1}", IsUserInRole("Admin"));

        _logger.LogInformation("User is moderator: {1}", IsUserInRole("Moderator"));

        var familiarDocument = SecurityDbContext
            .Importants
            .First(important => important.Content == "Familiar information");

        var unknownDocument = SecurityDbContext
            .Importants
            .First(important => important.Content == "Unknown information");

        var foreignMystery = SecurityDbContext
            .Mysteries
            .First();

        if (LocalUser is not null)
        {
            _logger.LogInformation("User is author of familiar document: {1}",
                HasUserRelation("author", LocalUser, familiarDocument));

            _logger.LogInformation("User is author of unknown document: {1}",
                HasUserRelation("author", LocalUser, unknownDocument));

            _logger.LogInformation("User is author of foreign mystery: {1}",
                HasUserRelation("author", LocalUser, foreignMystery));
        }

        return Ok();
    }

    [HttpPost]
    public IActionResult Post()
    {
        _logger.LogInformation("INITIAL POST REQUEST...");

        if (!IsAuthencticated || UserId is null)
        {
            return Forbid();
        }

        if (SecurityDbContext is null)
        {
            return StatusCode(500);
        }

        var user = new User
        {
            ExternalUserId = UserId,
        };

        if (SecurityDbContext.Role.Count() == 0)
        {
            SecurityDbContext.Role.Add(new SecureRole<User> { Role = "Admin" });
            var moderatorRole = new SecureRole<User> { Role = "Moderator" };
            SecurityDbContext.Role.Add(moderatorRole);
            SecurityDbContext.Role.Add(new SecureRole<User> { Role = "CommonMan" });

            AddRoleToUser(moderatorRole, user);

            var userDocument = new ImportantDocument { Content = "Familiar information" };
            SecurityDbContext.Importants.Add(userDocument);
            SecurityDbContext.Importants.Add(new ImportantDocument { Content = "Unknown information" });

            SecurityDbContext.Mysteries.Add(new AwfulMystery { StrangeAmount = 54f });

            AddUserRelation("author", user, userDocument);

            SecurityDbContext.SaveChanges();
        }

        return CreatedAtAction(null, null);
    }
}
