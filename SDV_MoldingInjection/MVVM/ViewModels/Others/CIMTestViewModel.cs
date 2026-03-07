using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.UI.Controls;
using log4net;
using SDV_MoldingInjection.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TOPENG_Device;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class CIMTestViewModel : ViewModelBase
    {
        private readonly ICIMMapHelper _mapHelper;
        private readonly RecipeSelector _recipeSelector;
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

        public CIMTestViewModel(ICIMMapHelper mapHelper, RecipeSelector recipeSelector)
        {
            _mapHelper = mapHelper;
            _recipeSelector = recipeSelector;

            Log = LogManager.GetLogger("CIMTest");
        }

        #region Privates
        private async Task CellTracking(int jigIndex)
        {
            Message = $"TRACKING START {jigIndex}";
            await Task.Delay(1000);

            // 1. Request SpecificValidation
            EquipEventDetail equipEvent = new EquipEventDetail(_mapHelper)
            {
                Event = EquipEvent.SpecificValidationRequest1 + jigIndex - 1
            };
            SpecificValidationRequestArea validationRequestArea = new()
            {
                OptionCode = "CELL",
                CellID = $"ABCDE12345_{jigIndex}",
            };
            equipEvent.Write(validationRequestArea.ToCIMData());

            // 2. CIM Data
            CIMCommandDetail cimCommad = new CIMCommandDetail(_mapHelper)
            {
                Command = CIMCommand.SpecificValidationDataSend1 + jigIndex - 1
            };
            bool isBitOn = cimCommad.WaitForCIMBitOn(3000);
            if (isBitOn == false)
            {
                Message = $"Bit {cimCommad.CIMAddress} ON timeout (over 3000ms)";
                return;
            }
            cimCommad.ReadCIMWords();

            // 3. Data processing
            SpecificValidationDataSendArea dataSendArea = new SpecificValidationDataSendArea();
            dataSendArea.FromCIMData(cimCommad.FromCIMDataBuffer);

            Message = $"REPLY : {dataSendArea.ReplyText}\r\n" +
                      $"STATUS : {dataSendArea.ReplyStatus}";

            if (dataSendArea.ReplyStatus.ToUpper() == "FAIL")
            {
                MessageBoxEx.Show("CELL LOT INFOR FAIL");
                return;
            }

            // 4. CELL TRACK IN
            CellStartPortArea cellStartPort = new CellStartPortArea()
            {
                TrackInCellID = validationRequestArea.CellID,
                TrackInReaderID = jigIndex.ToString(),
                TrackInRRC = "0",
            };
            EquipEventDetail equipEvent1 = new EquipEventDetail(_mapHelper)
            {
                Event = EquipEvent.CellStartPort1 + jigIndex - 1
            };
            equipEvent1.Write(cellStartPort.ToCIMData());

            // 5. CELL JOB PROCESS CONFIRM
            CIMCommandDetail cimCommand1 = new CIMCommandDetail(_mapHelper)
            {
                Command = CIMCommand.CellJobProcess1 + jigIndex - 1
            };
            isBitOn = cimCommand1.WaitForCIMBitOn(3000);
            if (isBitOn == false)
            {
                CellCompPortArea compPortTimeout = new CellCompPortArea()
                {
                    TrackOutCellID = jigIndex.ToString(),
                    TrackOutJudge = "O",
                    TrackOutDescription = "CELL_VALIDATION_TIMEOUT",
                };
                EquipEventDetail equipEventTimeOut = new EquipEventDetail(_mapHelper)
                {
                    Event = EquipEvent.CellCompPort1 + jigIndex - 1,
                };
                equipEventTimeOut.Write(compPortTimeout.ToCIMData());
                Message = $"Bit {cimCommand1.CIMAddress} ON timeout (over 3000ms)";
                return;
            }
            cimCommand1.ReadCIMWords();

            CellJobProcessCimToPlcArea cellJobProcess = new CellJobProcessCimToPlcArea();
            cellJobProcess.FromCIMData(cimCommand1.FromCIMDataBuffer);

            cellStartPort.TrackInProductID = cellJobProcess.CellJobProcessProductID;
            cellStartPort.TrackInStepID = cellJobProcess.CellJobProcessStepID;

            if (cellJobProcess.CellJobProcessRCMD == $"{(int)ECellJobProcessRCMD.CellJobProcessStart}")
            {
                Message = "INJECTING...";
                await Task.Delay(3000);
            }

            // TRACK OUT
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
            EquipEventDetail equipEvent2 = new EquipEventDetail(_mapHelper)
            {
                Event = EquipEvent.CellCompPort1 + jigIndex - 1
            };
            equipEvent2.Write(compPortArea.ToCIMData());

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
