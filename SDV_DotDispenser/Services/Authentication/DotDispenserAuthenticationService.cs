using EQX.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DotDispenser.Services
{
    public class DotDispenserAuthenticationService : AuthenticationService
    {
        public DotDispenserAuthenticationService(UserStore userStore) : base(userStore)
        {
        }

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
