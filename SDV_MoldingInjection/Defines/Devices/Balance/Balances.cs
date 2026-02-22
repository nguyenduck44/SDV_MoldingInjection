using EQX.Device.Balance;
using Microsoft.Extensions.DependencyInjection;

namespace SDV_MoldingInjection.Defines.Devices.Balance
{
    public class Balances
    {
        public MettlerToledoWKC204C BalanceLeft { get; }
        public MettlerToledoWKC204C BalanceRight { get; }

        public Balances([FromKeyedServices("BalanceLeft")] MettlerToledoWKC204C balanceLeft, [FromKeyedServices("BalanceRight")] MettlerToledoWKC204C balanceRight)
        {
            BalanceLeft = balanceLeft;
            BalanceRight = balanceRight;
        }
    }
}
