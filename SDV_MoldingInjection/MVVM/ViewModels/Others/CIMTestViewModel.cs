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
            Message = $"TRACKING START {jigIndex}";
            await Task.Delay(1000);

            EquipEvent validRequest = EquipEvent.SpecificValidationRequest1 + jigIndex - 1;
            CIMCommand validData = CIMCommand.SpecificValidationDataSend1 + jigIndex - 1;

            // 1. Request SpecificValidation
            SpecificValidationRequestArea validationRequestArea = new()
            {
                OptionCode = "CELL",
                CellID = $"ABCDE12345_{jigIndex}",
            };
            EquipEventDetail.Create(validRequest).WriteAndBitOnOff(validationRequestArea.ToCIMData());

            // 2. CIM Data
            bool isBitOn = CIMCommandDetail.Create(validData).WaitForCIMBitOn(3000);
            if (isBitOn == false)
            {
                Message = $"Bit {CIMCommandDetail.Create(validData).CIMAddress} ON timeout (over 3000ms)";
                return;
            }
            var validDataCommand = CIMCommandDetail.Create(validData);
            validDataCommand.ReadCIMWords();

            // 3. Data processing
            SpecificValidationDataSendArea dataSendArea = new SpecificValidationDataSendArea();
            dataSendArea.FromCIMData(validDataCommand.FromCIMDataBuffer);

            Message = $"REPLY : {dataSendArea.ReplyText}\r\n" +
                      $"STATUS : {dataSendArea.ReplyStatus}";

            if (dataSendArea.ReplyStatus.ToUpper() == "FAIL")
            {
                MessageBoxEx.Show("CELL LOT INFOR FAIL");
                return;
            }

            // 4. CELL TRACK IN
            EquipEvent cellStartPortEvent = EquipEvent.CellStartPort1 + jigIndex - 1;
            CIMCommand cellJobProc = CIMCommand.CellJobProcess1 + jigIndex - 1;
            CellStartPortArea cellStartPort = new CellStartPortArea()
            {
                TrackInCellID = validationRequestArea.CellID,
                TrackInReaderID = jigIndex.ToString(),
                TrackInRRC = "0",
            };
            
            EquipEventDetail.Create(cellStartPortEvent).WriteAndBitOnOff(cellStartPort.ToCIMData());

            // 5. CELL JOB PROCESS CONFIRM
            EquipEvent cellCompPort = EquipEvent.CellCompPort1 + jigIndex - 1;
            isBitOn = CIMCommandDetail.Create(cellJobProc).WaitForCIMBitOn(3000);
            if (isBitOn == false)
            {
                CellCompPortArea compPortTimeout = new CellCompPortArea()
                {
                    TrackOutCellID = jigIndex.ToString(),
                    TrackOutJudge = "O",
                    TrackOutDescription = "CELL_VALIDATION_TIMEOUT",
                };
                
                EquipEventDetail.Create(cellCompPort).WriteAndBitOnOff(compPortTimeout.ToCIMData());
                Message = $"Bit {CIMCommandDetail.Create(cellJobProc).CIMAddress} ON timeout (over 3000ms)";
                return;
            }
            var cellProcCommand = CIMCommandDetail.Create(cellJobProc);
            cellProcCommand.ReadCIMWords();

            CellJobProcessCimToPlcArea cellJobProcess = new CellJobProcessCimToPlcArea();
            cellJobProcess.FromCIMData(cellProcCommand.FromCIMDataBuffer);

            cellStartPort.TrackInProductID = cellJobProcess.CellJobProcessProductID;
            cellStartPort.TrackInStepID = cellJobProcess.CellJobProcessStepID;

            if (cellJobProcess.CellJobProcessRCMD == $"{(int)ECellJobProcessRCMD.CellJobProcessStart}")
            {
                Message = "INJECTING...";
                await Task.Delay(3000);
            }

            // 6. CELL TRACK OUT
            Message = "TRACK OUT";
            CellCompPortArea compPortArea = new CellCompPortArea()
            {
                TrackOutCellID = jigIndex.ToString(),
                TrackOutProductID = cellJobProcess.CellJobProcessProductID,
                TrackOutStepID = cellJobProcess.CellJobProcessStepID
            };
            if (cellJobProcess.CellJobProcessRCMD == $"{(int)ECellJobProcessRCMD.CellJobProcessCancel}")
            {
                compPortArea.TrackOutJudge = "O";
                compPortArea.TrackOutDescription = "CELL_VALIDATION_FAIL";
            }

            EquipEventDetail.Create(cellCompPort).WriteAndBitOnOff(compPortArea.ToCIMData());

            if (cellJobProcess.CellJobProcessRCMD == $"{(int)ECellJobProcessRCMD.CellJobProcessCancel}")
            {
                Message = "TRACK OUT WITH FAIL";
                MessageBoxEx.Show("CELL JOB PROCESS CANCEL");
                return;
            }
        }
        #endregion
    }
}
