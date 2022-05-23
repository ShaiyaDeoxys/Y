using System.Linq;

namespace Imgeneus.World.Shared
{
    public partial class LoginDisplay
    {
        public bool ShowRegisterButton { get; private set; }

        protected override void OnInitialized()
        {
            ShowRegisterButton = _database.Users.FirstOrDefault() is null;
        }
    }
}
