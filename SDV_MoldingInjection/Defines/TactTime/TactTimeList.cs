using CommunityToolkit.Mvvm.ComponentModel;
using EQX.UI.Controls;
using System.Windows;

namespace SDV_MoldingInjection.Defines
{
    public class TactTimeList : ObservableObject
    {
        #region Privates 
        private int TactCounter { get; set; }
        #endregion
        public TactTimeList(TactTimeListViewModel tactTimeListViewModel)
        {
            Chamber = new TactTime("Chamber");
            DryPump = new TactTime("DryPump");
            SPDHead1 = new TactTime("SPDHead1");
            SPDHead2 = new TactTime("SPDHead2");
            SPDHead3 = new TactTime("SPDHead3");
            SPDHead4 = new TactTime("SPDHead4");
            TactTimeListViewModel = tactTimeListViewModel;
        }

        public TactTime Chamber { get; }
        public TactTime DryPump { get; }
        public TactTime SPDHead1 { get; }
        public TactTime SPDHead2 { get; }
        public TactTime SPDHead3 { get; }
        public TactTime SPDHead4 { get; }
        public void TactCounterStart()
        {
            TactCounter = Environment.TickCount;
        }

        public void TactCounterEnd()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                double currentTact;
                currentTact = (Environment.TickCount - TactCounter) / 1000.0 / 2.0;

                TactTimeListViewModel.TactTimes.Add(currentTact);

                if (TactTimeListViewModel.TactTimes.Count > 30)
                    TactTimeListViewModel.TactTimes.RemoveAt(0);
            });
        }

        public TactTimeListViewModel TactTimeListViewModel { get; }
    }
}
