using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Imgeneus.Database.Entities
{
    public class DbUserModeAndCountry
    {
        [Key]
        public int UserId { get; set; }

        public Mode MaxMode { get; set; }

        [DefaultValue(Fraction.NotSelected)]
        public Fraction Country { get; set; }
    }
}
