using EQX.Core.Common;

namespace SDV_MoldingInjection.Services.Security
{
    public enum EPasswordMode
    {
        RMS_OPERATOR,
        EQP
    }

    public class SecurityControlSettings
    {
        public EPasswordMode PasswordMode { get; set; } = EPasswordMode.RMS_OPERATOR;

        public int ScreenSaverSec { get; set; } = 1000;

        public Dictionary<EPermission, string> Passwords { get; set; } = new()
        {
            { EPermission.Operator, string.Empty },
            { EPermission.Admin, "1234" },
            { EPermission.SuperUser, "3141" }
        };
    }
}
