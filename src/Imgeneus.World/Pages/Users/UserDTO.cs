using Imgeneus.Authentication.Entities;
using Imgeneus.Database.Entities;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Pages.Users
{
    public class UserDTO
    {
        public int Id { get; }

        public string UserName { get; }

        public uint Points { get; }

        public string Faction { get; }

        public IList<string> Roles { get; }

        public DateTime LastConnectionTime { get; }

        public bool IsDeleted { get; }

        public UserDTO(DbUser user, IList<string> roles)
        {
            Id = user.Id;
            UserName = user.UserName;
            Points = user.Points;
            Faction = user.Faction.ToString();
            LastConnectionTime = user.LastConnectionTime;
            IsDeleted = user.IsDeleted;
            Roles = roles;
        }
    }
}
