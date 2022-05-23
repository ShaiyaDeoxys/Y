using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Imgeneus.Database.Entities
{
    [Table("Users")]
    public class DbUser : IdentityUser<int>
    {
        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>
        [Required]
        [MaxLength(16)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the user's current status.
        /// </summary>
        [DefaultValue(0)]
        public byte Status { get; set; }

        /// <summary>
        /// Gets or sets the user's current status.
        /// </summary>
        [DefaultValue(0)]
        public byte Authority { get; set; }

        /// <summary>
        /// Gets or sets the user's current points.
        /// </summary>
        public uint Points { get; set; }

        /// <summary>
        /// Gets or sets the user current faction.
        /// </summary>
        [DefaultValue(Fraction.NotSelected)]
        public Fraction Faction { get; set; }

        /// <summary>
        /// Gets or sets the user current maximum mode allowed.
        /// </summary>
        [DefaultValue(0)]
        public Mode MaxMode { get; set; }

        /// <summary>
        /// Gets the user's creation time.
        /// </summary>
        [Column(TypeName = "DATETIME")]
        public DateTime CreateTime { get; private set; }

        /// <summary>
        /// Gets or sets the last time user login.
        /// </summary>
        [Column(TypeName = "DATETIME")]
        public DateTime LastConnectionTime { get; set; }

        /// <summary>
        /// Gets or sets the user's characters list.
        /// </summary>
        public ICollection<DbCharacter> Characters { get; set; }

        /// <summary>
        /// User bank items
        /// </summary>
        public ICollection<DbBankItem> BankItems { get; set; }

        /// <summary>
        /// User stored items
        /// </summary>
        public ICollection<DbWarehouseItem> WarehouseItems { get; set; }

        /// <summary>
        /// Gets or sets a flag that indicates if the user is deleted.
        /// </summary>
        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Creates a new <see cref="DbUser"/> instance.
        /// </summary>
        public DbUser()
        {
            this.CreateTime = DateTime.UtcNow;
            this.Characters = new HashSet<DbCharacter>();
            Password = "1";
        }

    }

    public enum Fraction : byte
    {
        Light = 0,
        Dark = 1,
        NotSelected = 2
    }
}
