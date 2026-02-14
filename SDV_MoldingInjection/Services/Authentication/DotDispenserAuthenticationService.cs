using EQX.Core.Common;

namespace SDV_MoldingInjection.Services
{
    public class DotDispenserAuthenticationService(IUserStore userStore)
        : AuthenticationService(userStore)
    {
        protected override bool ValidatePassword(EPermission permission, string password)
        {
            return permission switch
            {
                EPermission.Operator => true,
                EPermission.Admin => true,
                EPermission.SuperUser => (password == "3141") || (password == DateTime.Now.ToString("HHdd")),
                _ => false,
            };
        }
    }
}
