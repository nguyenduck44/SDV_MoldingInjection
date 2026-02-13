using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using SDV_DemoEQ.Defines;
using SDV_DemoEQ.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace SDV_DemoEQ.MVVM.ViewModels
{
    public class IOMonitoringViewModel : ViewModelBase
    {
        public IOMonitoringViewModel(Inputs inputList, Outputs outputList,
            MachineStatus machineStatus,
            NavigationStore navigationStore)
        {
            InputList = inputList;
            OutputList = outputList;
            MachineStatus = machineStatus;
            _navigationStore = navigationStore;

            System.Timers.Timer timer = new System.Timers.Timer(100);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (_navigationStore.CurrentViewModel != this) return;

            for (int i = SelectedInputDeviceIndex * 16; i < SelectedInputDeviceIndex * 16 + 16; i++)
            {
                InputList.All[i].RaiseValueUpdated();
            }
        }

        private int _selectedInputDeviceIndex;
        public int SelectedInputDeviceIndex
        {
            get => _selectedInputDeviceIndex;
            set
            {
                if (_selectedInputDeviceIndex != value)
                {
                    _selectedInputDeviceIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedInputBoardNumber));
                }
            }
        }

        public int SelectedInputBoardNumber => SelectedInputDeviceIndex + 1;

        private int _selectedOutputDeviceIndex;
        private readonly NavigationStore _navigationStore;

        public int SelectedOutputDeviceIndex
        {
            get => _selectedOutputDeviceIndex;
            set
            {
                if (_selectedOutputDeviceIndex != value)
                {
                    _selectedOutputDeviceIndex = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedOutputBoardNumber));
                }
            }
        }

        public int SelectedOutputBoardNumber => SelectedOutputDeviceIndex + 1;

        public ICommand InputDeviceIndexDecrease
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedInputDeviceIndex > 0)
                    {
                        SelectedInputDeviceIndex--;
                    }
                });
            }
        }
        public ICommand InputDeviceIndexIncrease
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedInputDeviceIndex < MaxInputDeviceIndex)
                    {
                        SelectedInputDeviceIndex++;
                    }
                });
            }
        }

        public ICommand OutputDeviceIndexDecrease
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedOutputDeviceIndex > 0)
                    {
                        SelectedOutputDeviceIndex--;
                    }
                });
            }
        }
        public ICommand OutputDeviceIndexIncrease
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedOutputDeviceIndex < MaxOutputDeviceIndex)
                    {
                        SelectedOutputDeviceIndex++;
                    }
                });
            }
        }

        public Inputs InputList { get; }
        public Outputs OutputList { get; }
        public MachineStatus MachineStatus { get; }

        private int MaxInputDeviceIndex => InputList.All
            .GroupBy(i => i.Id / 16)
            .Count() - 1;

        private int MaxOutputDeviceIndex => OutputList.All
            .GroupBy(o => o.Id / 16)
            .Count() - 1;
    }
}
