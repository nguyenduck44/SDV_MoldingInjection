using CommunityToolkit.Mvvm.ComponentModel;
using EQX.Core.Common;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.UI.Controls;
using System.Windows;
using TOPENG_Device;

namespace SDV_MoldingInjection.Defines
{
    public class TactTimeList : ObservableObject
    {
        #region Privates 
        private bool IsTactStarted { get; set; } = false;

        private int TactCounter { get; set; }
        private NonOverlappingTimer Timer;
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

            Timer = new NonOverlappingTimer(200);
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            WriteCurrentTactTimeToCIM();
        }

        public TactTime Chamber { get; }
        public TactTime DryPump { get; }
        public TactTime SPDHead1 { get; }
        public TactTime SPDHead2 { get; }
        public TactTime SPDHead3 { get; }
        public TactTime SPDHead4 { get; }
        public void TactCounterStart()
        {
            //if (IsTactStarted) return;

            TactCounter = Environment.TickCount;
            Timer.Start();

            IsTactStarted = true;
        }

        public void TactCounterEnd(int jigOutputCount)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                double currentTact;
                currentTact = (Environment.TickCount - TactCounter) / (1000.0 * jigOutputCount);

                for (int i = 0; i < jigOutputCount; i++)
                {
                    TactTimeListViewModel.TactTimes.Insert(0, currentTact);

                    if (TactTimeListViewModel.TactTimes.Count > 30)
                    {
                        TactTimeListViewModel.TactTimes.RemoveAt(TactTimeListViewModel.TactTimes.Count - 1);
                    }
                }
            });

            TactCounter = Environment.TickCount;

            WriteTactTimeToCIM();
        }

        public TactTimeListViewModel TactTimeListViewModel { get; }

        private void WriteTactTimeToCIM()
        {
            int wordLength = TactTimeListViewModel.TactTimes.Count * 2;
            short[] data = new short[wordLength];
            int offset = 0;

            for (int i = 0; i < TactTimeListViewModel.TactTimes.Count; i++)
            {
                PlcDataConverter.SetInt32(data, offset, (int)(TactTimeListViewModel.TactTimes[i] * 1000));
                offset += 2;
            }

            CIMAddressMap.WriteWords(0xB500, wordLength, data);
        }

        private void WriteCurrentTactTimeToCIM()
        {
            short[] data = new short[2];
            double currentTact = (Environment.TickCount - TactCounter) / 1000.0;

            PlcDataConverter.SetInt32(data, 0, (int)(currentTact * 1000));

            CIMAddressMap.WriteWords(0xB53C, 2, data , false);
        }
    }
}
