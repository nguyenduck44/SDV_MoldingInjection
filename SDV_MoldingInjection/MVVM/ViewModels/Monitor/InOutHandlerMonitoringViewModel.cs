using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using SDV_MoldingInjection.Defines;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class InOutHandlerMonitoringViewModel : ViewModelBase
    {
        public InOutHandlerMonitoringViewModel(InOutHandler inOutHandler)
        {
            InOutHandler = inOutHandler;
        }

        public InOutHandler InOutHandler { get; }

        public ObservableCollection<string> InOutHandlerSendList { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> InOutHandlerReceivedList { get; } = new ObservableCollection<string>();

        public ICommand SendEndCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    InOutHandler.Handler_Send_End(new EJigStatus[] { EJigStatus.MoldingFinish , EJigStatus.MoldingFinish });
                });
            }
        }
        private void InOutHandlerSendEventHandler(object? sender , EventArgs eventArgs)
        {
            if (sender is byte[] data == false) return;
            
            string str = Encoding.ASCII.GetString(data);

            string hexStr = "hex :";

            foreach(byte b in data)
            {
                hexStr += $" {b:X2}";
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                InOutHandlerSendList.Add(str + hexStr);
            });
        }

        private void InOutHandlerReceivedEventHandler(object? sender, EventArgs eventArgs)
        {
            if (sender is string data == false) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                InOutHandlerReceivedList.Add(data);
            });
        }
    }
}
