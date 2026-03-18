using EQX.Core.Common;
using SDV_MoldingInjection.Services.Security;

namespace SDV_MoldingInjection.Services
{
    public class SDVAuthenticationService(
        IUserStore userStore,
        ISecurityControlStore securityControlStore)
        : AuthenticationService(userStore)
    {
        protected override bool ValidatePassword(EPermission permission, string password)
        {
            return securityControlStore.ValidatePassword(permission, password);
        }
    }
}
