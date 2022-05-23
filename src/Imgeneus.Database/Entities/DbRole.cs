using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("Roles")]
    public class DbRole : IdentityRole<int>
    {
        public const string SUPER_ADMIN = "SuperAdmin";

        public const string ADMIN = "Admin";

        public const string USER = "User";
    }
}
