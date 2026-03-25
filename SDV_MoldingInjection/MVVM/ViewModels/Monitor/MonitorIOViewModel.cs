using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.InOut;
using SDV_MoldingInjection.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class MonitorIOViewModel : ViewModelBase
    {
        public MonitorIOViewModel(Inputs inputList, Outputs outputList,
            MachineStatus machineStatus,
            NavigationStore navigationStore)
        {
            _inputList = inputList;
            _outputList = outputList;
            MachineStatus = machineStatus;
            _navigationStore = navigationStore;

            NonOverlappingTimer timer = new NonOverlappingTimer(100);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            CurrentInputList = _inputList.Head1;
            CurrentOutputList = _outputList.Head1;
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (_navigationStore.CurrentViewModel != this) return;

            foreach (IDInput input in CurrentInputList)
            {
                input.RaiseValueUpdated();
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

        public List<IDInput> CurrentInputList { get; set; }
        public List<IDOutput> CurrentOutputList { get; set; }

        private readonly Inputs _inputList;
        private readonly Outputs _outputList;

        public MachineStatus MachineStatus { get; }

        private int MaxInputDeviceIndex => CurrentInputList
            .GroupBy(i => i.Id / 16)
            .Count() - 1;

        private int MaxOutputDeviceIndex => CurrentOutputList
            .GroupBy(o => o.Id / 16)
            .Count() - 1;

        private int _selectedOutputDeviceIndex;
        private readonly NavigationStore _navigationStore;
    }
}
