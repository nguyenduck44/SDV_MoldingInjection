using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using SDV_MoldingInjection.Defines;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class InterfaceInOutViewModel : ViewModelBase
    {
        public InterfaceInOutViewModel(InOutHandler inOutHandler)
        {
            _inOutHandler = inOutHandler;
            _inOutHandler.MessageReceived += OnMessageReceived;
            _inOutHandler.MessageTransmitted += OnMessageTransmitted;
        }

        public ObservableCollection<string> SendMessages { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ReceiveMessages { get; } = new ObservableCollection<string>();

        public bool UseHead12 { get; set; } = true;
        public bool UseHead34 { get; set; } = true;
        public string ErrorCodeText { get; set; } = "1001";

        public ICommand SendTransmitCommand
        {
            get
            {
                return new RelayCommand<string>((command) =>
                {
                    switch (command)
                    {
                        case "CTRD":
                            _inOutHandler.SendTransmitCTRD();
                            break;
                        case "CTRN":
                            _inOutHandler.SendTransmitCTRN();
                            break;
                        case "CTCH":
                            _inOutHandler.SendTransmitCTCH();
                            break;
                        case "CTPT":
                            _inOutHandler.SendTransmitCTPT();
                            break;
                        case "CTST":
                            _inOutHandler.SendTransmitCTST();
                            break;
                        case "CTSP":
                            _inOutHandler.SendTransmitCTSP();
                            break;
                        case "CTSI":
                            _inOutHandler.SendTransmitCTSI();
                            break;
                        case "CTCI":
                            _inOutHandler.SendTransmitCTCI();
                            break;
                    }
                });
            }
        }

        public ICommand SendHeadUseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _inOutHandler.SendTransmitCTUSAndCTUE(1, UseHead12);
                    _inOutHandler.SendTransmitCTUSAndCTUE(2, UseHead34);
                });
            }
        }

        public ICommand SendEndCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _inOutHandler.SendTransmitCTEN();
                });
            }
        }

        public ICommand SendErrorCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!int.TryParse(ErrorCodeText, out int errorCode))
                    {
                        AppendSendMessage("APP: Invalid error code. Please input 0-9999.");
                        return;
                    }

                    if (errorCode < 0 || errorCode > 9999)
                    {
                        AppendSendMessage("APP: Error code must be in range 0-9999.");
                        return;
                    }

                    _inOutHandler.SendTransmitCTER(errorCode);
                });
            }
        }

        public ICommand ClearLogCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SendMessages.Clear();
                    ReceiveMessages.Clear();
                });
            }
        }

        private void OnMessageReceived(string message)
        {
            AppendReceiveMessage(message);
        }

        private void OnMessageTransmitted(string message)
        {
            AppendSendMessage(message);
        }

        private void AppendReceiveMessage(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            if (Application.Current?.Dispatcher is null || Application.Current.Dispatcher.CheckAccess())
            {
                AddMessage(ReceiveMessages, line);
                return;
            }

            Application.Current.Dispatcher.Invoke(() => AddMessage(ReceiveMessages, line));
        }

        private void AppendSendMessage(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            if (Application.Current?.Dispatcher is null || Application.Current.Dispatcher.CheckAccess())
            {
                AddMessage(SendMessages, line);
                return;
            }

            Application.Current.Dispatcher.Invoke(() => AddMessage(SendMessages, line));
        }

        private static void AddMessage(ObservableCollection<string> target, string message)
        {
            target.Insert(0, message);
            if (target.Count > 500)
            {
                target.RemoveAt(target.Count - 1);
            }
        }

        private readonly InOutHandler _inOutHandler;

    }
}
