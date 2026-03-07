using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom.WordArea;
using log4net;
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
                return new RelayCommand(() =>
                {
                    // 1. Request SpecificValidation
                    EquipEventDetail equipEvent = new EquipEventDetail(_mapHelper)
                    {
                        Event = EquipEvent.SpecificValidationRequest1
                    };
                    SpecificValidationRequestArea validationRequestArea = new()
                    {
                        OptionCode = "CELL",
                        CellID = "ABCDE12345",
                    };
                    equipEvent.Write(validationRequestArea.ToCIMData());
                    
                    // 2. CIM Data
                    CIMCommandDetail cimCommad = new CIMCommandDetail(_mapHelper)
                    {
                        Command = CIMCommand.SpecificValidationDataSend1
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
                });
            }
        }

        public ICommand SpecificValidationRequest2
        {
            get
            {
                return new RelayCommand(() =>
                {
                    // 1. Request SpecificValidation
                    EquipEventDetail eventDetail = new EquipEventDetail(_mapHelper)
                    {
                        Event = EquipEvent.SpecificValidationRequest2
                    };
                    SpecificValidationRequestArea validationRequestArea = new()
                    {
                        OptionCode = "CELL",
                        CellID = "12345ABCDE",
                    };
                    eventDetail.Write(validationRequestArea.ToCIMData());

                    // 2. CIM Data
                    CIMCommandDetail cimCommad = new CIMCommandDetail(_mapHelper)
                    {
                        Command = CIMCommand.SpecificValidationDataSend2
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
                });
            }
        }

        public CIMTestViewModel(ICIMMapHelper mapHelper)
        {
            _mapHelper = mapHelper;
            Log = LogManager.GetLogger("CIMTest");
        }
    }
}
