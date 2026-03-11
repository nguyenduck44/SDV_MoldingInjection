using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Communication;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.UI.Controls;
using EQX.UI.MVVM;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SDV_MoldingInjection.Defines.CIM;
using SDV_MoldingInjection.MVVM.Views;
using SDV_MoldingInjection.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TOPENG_Device;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class CIMTestViewModel : ViewModelBase
    {
        private readonly ICIMMapHelper _mapHelper;
        private readonly RecipeSelector _recipeSelector;
        private readonly IConfiguration _configuration;
        private string _message;

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        #region Commands
        public ICommand SpecificValidationRequest1
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    CellTracking(1);
                });
            }
        }

        public ICommand SpecificValidationRequest2
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    CellTracking(2);
                });
            }
        }

        public ICommand TPMLostTest
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    EquipEventDetail.Create(EquipEvent.TPMLossReady).SetPLCBitOn();

                    TPMLossWindow tpmLossWindow = new TPMLossWindow();
                    tpmLossWindow.ShowDialog();

                    if (tpmLossWindow.SelectedTPMMode == null) return;

                    EquipEventHelpers.TPMLostReport((ETPMLossDesciption)tpmLossWindow.SelectedTPMMode);
                });
            }
        }
        public ICommand ECMChangeTest
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CIMAction.ECMValues[0] = random.Next(10000);
                    EquipEventHelpers.EquipmentConstantParameterChanged(1, CIMAction.ECMValues[0]);
                });
            }
        }

        Random random = new Random();

        public ICommand OpenCIMFunctionTestCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var filePath = _configuration["Files:CIMFunctionFile"];
                    var window = new Window
                    {
                        Title = "EQP Function Change",
                        Content = new CIMFunctionView(),
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    window.DataContext = new CIMFunctionViewModel(filePath);
                    window.Show();
                });
            }
        }
        #endregion

        public CIMTestViewModel(ICIMMapHelper mapHelper, RecipeSelector recipeSelector, IConfiguration configuration)
        {
            _mapHelper = mapHelper;
            _recipeSelector = recipeSelector;
            _configuration = configuration;

            Log = LogManager.GetLogger("CIMTest");
        }

        #region Privates
        private async Task CellTracking(int jigIndex)
        {
            string cellID = $"ABCDE12345_{jigIndex}";
            if (EquipEventHelpers.SpecificValidation(jigIndex, cellID) == false)
            {
                // TODO: ALARM RAISING (TIMEOUT? FAIL?)
                MessageBoxEx.Show("CELL LOT INFOR FAIL");
                return;
            }

            if (EquipEventHelpers.CellTrackIn(jigIndex, cellID) == false)
            {
                // TODO: ALARM RAISING (TIMEOUT? FAIL?)
                MessageBoxEx.Show("CELL TRACK IN FAIL");
                return;
            }

            CellJobProcessCimToPlcArea cellJobProcess = EquipEventHelpers.CellJobProcessConfirm(jigIndex, cellID);

            bool isJobProcessFail = false;
            if (cellJobProcess.CellJobProcessRCMD != $"{(int)ECellJobProcessRCMD.CellJobProcessStart}")
            {
                // TODO: ALARM RAISING (TIMEOUT? FAIL?)
                isJobProcessFail = true;
                MessageBoxEx.Show("CellJobProcessRCMD FAIL");
            }
            else
            {
                Message = "INJECTING...";
                await Task.Delay(3000);
            }

            EquipEventHelpers.CellTrackOut(jigIndex, cellJobProcess, isJobProcessFail);
            Message = "TRACKOUT DONE";
        }
        #endregion
    }
}
