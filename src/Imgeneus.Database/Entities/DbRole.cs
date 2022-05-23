using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("Roles")]
    public class DbRole : IdentityRole<int>
    {
    }
}
