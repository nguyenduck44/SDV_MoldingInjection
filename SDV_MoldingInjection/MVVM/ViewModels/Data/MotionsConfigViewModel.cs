using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Motion;
using EQX.Motion;
using EQX.Motion.ByVendor.Inovance;
using EQX.UI.Controls;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class MotionsConfigViewModel : ViewModelBase
    {
        public MotionsConfigViewModel(Motions motions,
            MachineStatus machineStatus, 
            IConfiguration configuration,
            RecipeSelector recipeSelector)
        {
            Motions = motions;
            MachineStatus = machineStatus;
            _configuration = configuration;
            _recipeSelector = recipeSelector;
        }
        public MotionSpeedRecipe MotionSpeedRecipe => _recipeSelector.CurrentRecipe.MotionSpeedRecipe;
        public Motions Motions { get; }
        public MachineStatus MachineStatus { get; }

        public ObservableCollection<IMotion> MotionList => new ObservableCollection<IMotion>(Motions.All);
        public ICommand SaveCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_SaveAllData"]) == false) return;
                    _recipeSelector.Save();

                });
            }
        }
        public ICommand SaveMotionConfigCommand => new RelayCommand(() =>
        {
            try
            {
                var result = MessageBoxEx.ShowDialog("Do you want to save the motion configurations?", true, "Confirm Save");
                if (result == true)
                {
                    SaveMotionConfigurations();
                }
            }
            catch (Exception ex)
            {
                MessageBoxEx.ShowDialog($"Error saving motion configurations: {ex.Message}");
            }
        });

        private void SaveMotionConfigurations()
        {
            //SaveAjinMotionConfigurations();
            SaveInovanceMotionConfigurations();
            SaveFastechMotionConfigurations();
        }

        //private void SaveAjinMotionConfigurations()
        //{
        //    var ajinConfigPath = _configuration["Files:MotionAjinParaConfigFile"];
        //    if (string.IsNullOrWhiteSpace(ajinConfigPath) || !File.Exists(ajinConfigPath))
        //    {
        //        return;
        //    }

        //    var existingAjinParams = JsonConvert.DeserializeObject<List<MotionAjinParameter>>(File.ReadAllText(ajinConfigPath))
        //        ?? new List<MotionAjinParameter>();

        //    for (int i = 0; i < Motions.AjinMotions.Count && i < existingAjinParams.Count; i++)
        //    {
        //        existingAjinParams[i].Velocity = Motions.AjinMotions[i].Parameter.Velocity;
        //        existingAjinParams[i].Acceleration = Motions.AjinMotions[i].Parameter.Acceleration;
        //        existingAjinParams[i].Deceleration = Motions.AjinMotions[i].Parameter.Deceleration;
        //    }

        //    File.WriteAllText(ajinConfigPath, JsonConvert.SerializeObject(existingAjinParams, Formatting.Indented));
        //}
        private void SaveInovanceMotionConfigurations()
        {
            var inovanceConfigPath = _configuration["Files:MotionInovanceParaConfigFile"];
            if (string.IsNullOrWhiteSpace(inovanceConfigPath) || !File.Exists(inovanceConfigPath))
            {
                return;
            }

            var existingInovanceParams = JsonConvert.DeserializeObject<List<MotionInovanceParameter>>(File.ReadAllText(inovanceConfigPath))
                ?? new List<MotionInovanceParameter>();

            for (int i = 0; i < Motions.InovanceMotions.Count && i < existingInovanceParams.Count; i++)
            {
                existingInovanceParams[i].Velocity = Motions.InovanceMotions[i].Parameter.Velocity;
                existingInovanceParams[i].Acceleration = Motions.InovanceMotions[i].Parameter.Acceleration;
                existingInovanceParams[i].Deceleration = Motions.InovanceMotions[i].Parameter.Deceleration;
            }

            File.WriteAllText(inovanceConfigPath, JsonConvert.SerializeObject(existingInovanceParams, Formatting.Indented));
        }

        private void SaveFastechMotionConfigurations()
        {
            var fastechConfigPath = _configuration["Files:MotionFastechParaConfigFile"];
            if (string.IsNullOrWhiteSpace(fastechConfigPath) || !File.Exists(fastechConfigPath))
            {
                return;
            }

            var existingFastechParams = JsonConvert.DeserializeObject<List<MotionParameter>>(File.ReadAllText(fastechConfigPath))
                ?? new List<MotionParameter>();

            for (int i = 0; i < Motions.FastechMotions.Count && i < existingFastechParams.Count; i++)
            {
                existingFastechParams[i].Velocity = Motions.FastechMotions[i].Parameter.Velocity;
                existingFastechParams[i].Acceleration = Motions.FastechMotions[i].Parameter.Acceleration;
                existingFastechParams[i].Deceleration = Motions.FastechMotions[i].Parameter.Deceleration;
            }

            File.WriteAllText(fastechConfigPath, JsonConvert.SerializeObject(existingFastechParams, Formatting.Indented));
        }

        private readonly IConfiguration _configuration;
        private readonly RecipeSelector _recipeSelector;
    }
}
