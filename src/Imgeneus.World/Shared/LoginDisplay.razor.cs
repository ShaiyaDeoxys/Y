using System.Linq;

namespace Imgeneus.World.Shared
{
    public partial class LoginDisplay
    {
        public bool ShowRegisterButton { get; private set; }

        public LoginDisplay()
        {
        }

        protected override void OnInitialized()
        {
            ShowRegisterButton = _database.Users.FirstOrDefault() is null;
        }
    }
}
