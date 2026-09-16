using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using EQX.UI.MVVM;
using Microsoft.Extensions.Configuration;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Windows.Data.Json;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class OptionViewModel : ViewModelBase
    {
        private readonly RecipeSelector _recipeSelector;
        private readonly Devices _devices;

        public RecipeList RecipeList { get; }
        public MachineStatus MachineStatus { get; }

        public OptionViewModel(
            RecipeSelector recipeSelector,
            RecipeList recipeList,
            MachineStatus machineStatus,
            Devices devices)
        {
            _recipeSelector = recipeSelector;
            RecipeList = _recipeSelector.CurrentRecipe;
            MachineStatus = machineStatus;
            _devices = devices;
            NonOverlappingTimer timer = new NonOverlappingTimer(200);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }
        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            OnPropertyChanged(nameof(IsNoError));
            OnPropertyChanged(nameof(IsNoJig));
            OnPropertyChanged(nameof(IsCIMConnect));
            OnPropertyChanged(nameof(IsNoInterlock));
            OnPropertyChanged(nameof(IsEnableCIMModeChange));
        }
        public bool IsNoError => MachineStatus.CurrentAlarms.Count <= 0;
        public bool IsNoJig => ( _devices.Inputs.Jig1Detect.Value ||
                                 _devices.Inputs.Jig2Detect.Value ||
                                 _devices.Inputs.Jig3Detect.Value ||
                                 _devices.Inputs.Jig4Detect.Value)  == false;

        public bool IsCIMConnect => MachineStatus.EquipState.IsMasterAlive;
        public bool IsNoInterlock => MachineStatus.EquipState.IsInterlock == false;

        public bool IsEnableCIMModeChange
        {
            get
            {
                if (RecipeList.OptionRecipe.UseCIM == false)
                {
                    return IsNoError && IsNoJig && IsCIMConnect && IsNoInterlock && MachineStatus.IsStandByProcessMode;
                }

                return IsNoError && IsCIMConnect && IsNoInterlock && MachineStatus.IsStandByProcessMode;
            }
        }
        public ICommand SaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_SaveAllData"]) == true)
                    {
                        _recipeSelector.Save();
                        OnPropertyChanged();
                    }
                });
            }
        }
    }
}
