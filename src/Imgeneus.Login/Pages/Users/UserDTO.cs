using Imgeneus.Authentication.Entities;
using System;
using System.Collections.Generic;

namespace Imgeneus.Login.Pages.Users
{
    public class UserDTO
    {
        public int Id { get; }

        public string UserName { get; }

        public uint Points { get; }

        public IList<string> Roles { get; }

        public DateTime LastConnectionTime { get; }

        public bool IsDeleted { get; }

        public UserDTO(DbUser user, IList<string> roles)
        {
            Id = user.Id;
            UserName = user.UserName;
            Points = user.Points;
            LastConnectionTime = user.LastConnectionTime;
            IsDeleted = user.IsDeleted;
            Roles = roles;
        }
    }
}
