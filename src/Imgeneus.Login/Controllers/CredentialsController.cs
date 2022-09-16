using Imgeneus.Authentication.Context;
using Imgeneus.Authentication.Entities;
using Imgeneus.Login.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Imgeneus.Login.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CredentialsController : ControllerBase
    {
        private readonly IUsersDatabase _database;
        private readonly IPasswordHasher<DbUser> _passwordHasher;

        public CredentialsController(IUsersDatabase database, IPasswordHasher<DbUser> passwordHasher)
        {
            _database = database;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        public IActionResult Post([FromBody] CredentialDTO credential)
        {
            var dbUser = _database.Users.FirstOrDefault(x => x.UserName == credential.UserName);
            if (dbUser is null)
                return new UnauthorizedResult();

            var result = _passwordHasher.VerifyHashedPassword(dbUser, dbUser.PasswordHash, credential.Password);
            if (result == PasswordVerificationResult.Failed)
                return new UnauthorizedResult();

            return new OkResult();
        }
    }
}
