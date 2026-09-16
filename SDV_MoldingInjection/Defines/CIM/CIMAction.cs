using EQX.Core.Common;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.Core.Recipe;
using EQX.Core.Sequence;
using EQX.UI.Controls;
using EQX.UI.MVVM;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json.Linq;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.Recipe;
using System.Reflection;
using TOPENG_Device;
using static EQX.Core.Communication.CIM.Custom.WordArea.PPIDListArea;

namespace SDV_MoldingInjection.Defines.CIM
{
    public class CIMAction
    {
        #region Events
        public event Action<CIMCommandArgs> FromCIMCommandAction;
        #endregion

        public CIMAction(MachineStatus machineStatus,
                         ICIMMapHelper mapHelper,
                         RecipeSelector recipeSelector,
                         CIMFunctionViewModel cimFunctionVM,
                         RecipeCatalog recipeCatalog,
                         CIMCollection cIMCollection,
                         IViewModelFactory viewModelFactory)
        {
            _mapHelper = mapHelper;
            _recipeSelector = recipeSelector;
            _cimFunctionVM = cimFunctionVM;
            _recipeCatalog = recipeCatalog;
            _cIMCollection = cIMCollection;
            _viewModelFactory = viewModelFactory;
            _machineStatus = machineStatus;
            cimCommandDetail = new CIMCommandDetail(_mapHelper);

            FromCIMCommandAction += CIMAction_FromCIMCommandAction;
        }

        private void CIMAction_FromCIMCommandAction(CIMCommandArgs obj)
        {
            if (CIMConstants.FromCIMCommands.Contains(obj.CIMCommand) == false) return;
            if (obj.CIMCommand == CIMCommand.AliveBit) return;

            switch (obj.CIMCommand)
            {
                case CIMCommand.TerminalDisplay:
                    {
                        TerminalDisplayCimToPlcArea cimArea = new TerminalDisplayCimToPlcArea();
                        cimArea.FromCIMData(obj.Buffer);

                        CIMCommandDetail.Create(obj.CIMCommand).WaitForCIMBitOff();
                        CIMCommandDetail.Create(obj.CIMCommand).SetPLCBitOff();

                        MessageBoxEx.Show($"[{cimArea.TerminalNumber}] {cimArea.TerminalDisplayText}", false, "TerminalDisplay", closeItSelf: true);
                        LogManager.GetLogger("Terminal").Info($"{cimArea.TerminalNumber} , {cimArea.TerminalDisplayText}");
                    }
                    return;
                case CIMCommand.DatetimeSet:
                    {
                        DatetimeSetArea cimArea = new DatetimeSetArea();
                        cimArea.FromCIMData(obj.Buffer);

                        SystemTimeSetter.SetTimeFromString(cimArea.Datetime);

                        CIMCommandDetail.Create(obj.CIMCommand).WaitForCIMBitOff();
                        CIMCommandDetail.Create(obj.CIMCommand).SetPLCBitOff();
                    }
                    break;
                case CIMCommand.OperatorCall:
                    {
                        OperatorCallCimToPlcArea cimArea = new OperatorCallCimToPlcArea();
                        cimArea.FromCIMData(obj.Buffer);

                        OperatorCallPlcToCimArea plcArea = new OperatorCallPlcToCimArea()
                        {
                            OPCallIDComfirm = cimArea.OperatorCallID,
                            OPCallMessageConfirm = cimArea.OperatorCallText
                        };

                        ExecuteScenario(CIMScenario.OPCallOccurAndRelease,
                            new CIMScenarioContext
                            {
                                OPCallOccurOnly = true,
                                OPCallID = plcArea.OPCallIDComfirm,
                                OPCallMsg = plcArea.OPCallMessageConfirm,
                            });

                        bool? result = MessageBoxEx.ShowDialog($"[{cimArea.OperatorCallID} {cimArea.OperatorCallText}]", false, "OPCall");

                        if (result == null || result == false) break;

                        ExecuteScenario(CIMScenario.OPCallOccurAndRelease,
                            new CIMScenarioContext
                            {
                                OPCallOccurOnly = false,
                                OPCallID = plcArea.OPCallIDComfirm,
                                OPCallMsg = plcArea.OPCallMessageConfirm,
                            });

                        LogManager.GetLogger("OPCall").Info($"{plcArea.OPCallIDComfirm} , {plcArea.OPCallMessageConfirm}");
                    }
                    break;
                case CIMCommand.Interlock:
                    {
                        InterlockCimToPlcArea cimArea = new InterlockCimToPlcArea();
                        cimArea.FromCIMData(obj.Buffer);

                        _machineStatus.EquipState.IsInterlock = true;
                        _machineStatus.OPCommand = EOperationCommand.Stop;

                        // WRITE INTERLOCK MESSAGE
                        InterlockPlcToCimArea plcArea = new InterlockPlcToCimArea()
                        {
                            InterlockIDComfirm = cimArea.InterlockID,
                            InterlockMessageConfirm = cimArea.InterlockMessage
                        };
                        _mapHelper.GetWordAddress(obj.CIMCommand, out _, out string plcAddress, out _, out int plcLength);
                        CIMAddressMap.WriteWords(CIMAddressMap.GetWriteWordIndexFromDAddress(plcAddress), plcLength, plcArea.ToCIMData());

                        lock (_locker)
                        {
                            if (_commandHandling != null && _commandHandling.ContainsKey(CIMCommand.Interlock))
                                _commandHandling.Remove(CIMCommand.Interlock);
                        }

                        MessageBoxEx.ShowDialog($"[{cimArea.InterlockID} {cimArea.InterlockMessage}]", true, "Interlock");

                        LogManager.GetLogger("InterlockMsg").Info($"{cimArea.InterlockID} , {cimArea.InterlockMessage}");
                        // CONFIRM INTERLOCK RELEASE
                        ExecuteScenario(CIMScenario.InterlockRelease,
                            new CIMScenarioContext
                            {
                                OPCallOccurOnly = false,
                                InterlockID = plcArea.InterlockIDComfirm,
                                InterlockMsg = plcArea.InterlockMessageConfirm,
                            });
                    }
                    break;
                case CIMCommand.FormattedProcessProgramSend2:
                    {
                        var fppsArea = CIMCommandHelpers.PPIDDownload();

                        ParameterWordArea parameterWordArea = new ParameterWordArea();
                        parameterWordArea.FromCIMData(fppsArea.RmsParameterList);

                        bool isValid = true;
                        if (int.TryParse(fppsArea.RecipeNumber.Substring(0, 2), out int recipeNumber) == false)
                        {
                            isValid = false;
                        }
                        else
                        {
                            if (recipeNumber <= 90 && parameterWordArea.PPIDName.StartsWith("TT")) isValid = false;
                            if (recipeNumber > 90 && parameterWordArea.PPIDName.StartsWith("TT") == false) isValid = false;
                            // Create PPID that existed
                            if (_recipeCatalog.ContainsId(recipeNumber) && fppsArea.CCode == "1") isValid = false;
                            // Delete PPID that not exist
                            if (_recipeCatalog.ContainsId(recipeNumber) == false && fppsArea.CCode == "2") isValid = false;
                        }

                        short[] buf = new short[] { isValid ? (short)'0' : (short)'7' };
                        if (_recipeCatalog.ContainsId(recipeNumber) && fppsArea.CCode == "1")
                        {
                            buf = new short[] { (short)'8' };
                        }

                        EquipEventDetail.Create(EquipEvent.FormattedProcessProgramSend).Write(buf);
                        CIMCommandDetail.Create(CIMCommand.FormattedProcessProgramSend2).SetPLCBitOn();
                        CIMCommandDetail.Create(CIMCommand.FormattedProcessProgramSend2).WaitForCIMBitOff();
                        CIMCommandDetail.Create(CIMCommand.FormattedProcessProgramSend2).SetPLCBitOff();

                        if (isValid == false)
                        {
                            return;
                        }

                        if (fppsArea.CCode == "1") // CREATE NEW
                        {
                            _recipeSelector.Create(parameterWordArea.PPIDName, recipeNumber, fppsArea.RecipeNumber);
                        }
                        else if (fppsArea.CCode == "2") // Delete PPID
                        {
                            _recipeSelector.Delete(parameterWordArea.PPIDName, recipeNumber, fppsArea.RecipeNumber);
                        }
                        else if (fppsArea.CCode == "3") // Parameter Update
                        {
                            _recipeSelector.UpdateRecipeFromCIM(parameterWordArea.Parameters);
                        }
                    }
                    break;
                case CIMCommand.FormattedProcessProgramRequest:
                    {
                        FormattedProcessProgramRequestArea fpprArea = CIMCommandHelpers.GetParameterRequestInfor();
                        EquipEventHelpers.ParameterRequestResponse(fpprArea, "0");
                        //var fpprArea = CIMCommandHelpers.GetParameterRequestInfor();

                        //string recipeName = fpprArea.ReqPPID;
                        //ParameterWordArea parameterWordArea = new ParameterWordArea();
                        //parameterWordArea.Parameters = _recipeSelector.GetParameters(fpprArea.ReqPPID);
                        //parameterWordArea.PPIDName = recipeName;
                        //parameterWordArea.PPIDMode = 0;

                        //CIMCommandDetail.Create(CIMCommand.FormattedProcessProgramRequest).PLCWrite(parameterWordArea.ToCIMData());
                        //CIMCommandDetail.Create(CIMCommand.FormattedProcessProgramRequest).SetPLCBitOn();
                        //CIMCommandDetail.Create(CIMCommand.FormattedProcessProgramRequest).WaitForCIMBitOff();
                        //CIMCommandDetail.Create(CIMCommand.FormattedProcessProgramRequest).SetPLCBitOff();

                        //EquipEventHelpers.ParameterRequestResponse(fpprArea, "1D34");
                    }
                    break;
                case CIMCommand.CurrentEquipPPIDListRequest:
                    {
                        EquipEventHelpers.PPIDListInquiryResponse(_recipeSelector.ValidRecipes.ToArray());
                    }
                    break;
                case CIMCommand.EquipConstantNameList:
                    {

                    }
                    break;
                case CIMCommand.EquipFunctionChangeCommand:
                    {
                        var functionChangeCommand = CIMCommandDetail.Create(CIMCommand.EquipFunctionChangeCommand);
                        functionChangeCommand.ReadCIMWords();

                        EquipFunctionChangeCommandReceiveArea receiveArea = new EquipFunctionChangeCommandReceiveArea();
                        receiveArea.FromCIMData(functionChangeCommand.FromCIMDataBuffer);

                        if (int.TryParse(receiveArea.EquipCmdEFID, out int efid) == false) return;

                        if (efid >= 6
                            || _cimFunctionVM.IsUseAPC == false && efid == 14
                            || efid == 3 && receiveArea.EquipCmdEFST.ToUpper() == "NOTHING"
                            || efid == 4 && receiveArea.EquipCmdEFST.ToUpper() == "NOTHING"
                            || efid == 5 && receiveArea.EquipCmdEFST.ToUpper() == "NOTHING")
                        {
                            EquipEventDetail.Create(EquipEvent.EquipFunctionChangeCMDHcack).Write(new short[] { (short)'5' });

                            functionChangeCommand.SetPLCBitOn();
                            functionChangeCommand.WaitForCIMBitOff();
                            functionChangeCommand.SetPLCBitOff();

                            return;
                        }

                        bool res = _cimFunctionVM.UpdateSingle(efid, receiveArea.EquipCmdEFST);
                        EquipEventDetail.Create(EquipEvent.EquipFunctionChangeCMDHcack).Write(new short[] { res ? (short)'0' : (short)'5' });

                        functionChangeCommand.SetPLCBitOn();
                        functionChangeCommand.WaitForCIMBitOff();
                        functionChangeCommand.SetPLCBitOff();

                        while (_machineStatus.EquipState.IsCellInEquip == true)
                        {
                            Thread.Sleep(100);
                        }

                        // Make sure Equip State (Run State) is now IDLE before calling cim Funciton Update
                        Thread.Sleep(100);
                        _cimFunctionVM.SaveCommand?.Execute("HOST");
                    }
                    break;
            }
        }

        #region Public Methods
        public void Start()
        {
            if (_cimAliveTask == null)
                _cimAliveTask = new CIMAliveTask();

            _cimAliveTask.Start();
            _cIMCollection.Start();

            CancellationToken token = ctsCIMTrasmitTask.Token;

            _cimTrasmitTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await CIMCommandHandleAsync();
                    await Task.Delay(50);
                }
            }, token);

            _cimStatusUpdateTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    EquipEventHelpers.EquipStateReport(_machineStatus.EquipState);

                    CIMAddressMap.WriteWordSingle("D30", (short)'2');
                    CIMAddressMap.WriteWordSingle("D31", (short)'2');
                    CIMAddressMap.WriteWordSingle("D32", (short)'2');

                    foreach (var recipe in _recipeSelector.AllRecipe)
                    {
                        if (_recipeCatalog.TryGetRecipeId(recipe, out int recipeId))
                        {
                            EquipEventHelpers.PPIDListSinglePPIDWrite(recipe, recipeId.ToString());
                        }
                    }

                    //Copy Alarm InOut -> CIM
                    short[] buffer = CCLinkIEHelper.ReadWords(0x17000, 200);
                    CIMAddressMap.WriteWords(0xC617, 200, buffer , false);

                    //Copy RMS -> CIM
                    short[] bufferRMS = CCLinkIEHelper.ReadWords(0x17100, 54);
                    CIMAddressMap.WriteWords(0x94A0, 54, bufferRMS, false);

                    //Copy ECM -> CIM
                    short[] bufferECM = CCLinkIEHelper.ReadWords(0x17200, 54);
                    CIMAddressMap.WriteWords(0xA352, 54, bufferECM, false);

                    await Task.Delay(100);
                }
            }, token);

            _ = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    if (CCLinkIEHelper.ReadCCIEBit(0x12DF))
                    {
                        _viewModelFactory.Create<CurrentAlarmViewModel>().AlarmResetCommand.Execute(null);

                        while (CCLinkIEHelper.ReadCCIEBit(0x12DF))
                        {
                            await Task.Delay(10);
                        }
                    }

                    await Task.Delay(100);
                }
            }, token);

            if (_cIMCollection.WriteMCCFromToMap() == false)
            {
                LogManager.GetLogger("CIM").Error("Load MCC Map Fail");
            }
        }

        bool isFirstTime = true;

        public void Stop()
        {
            _cimAliveTask.Stop();
        }

        public void ExecuteScenario(CIMScenario scenario, CIMScenarioContext context = null)
        {
            CIMScenarioDispatcher.ExecuteScenario(scenario, context);
        }
        #endregion

        #region Privates Methods
        private async Task CIMCommandHandleAsync()
        {
            foreach (var fromCIMCommand in CIMConstants.FromCIMCommands)
            {
                cimCommandDetail.Command = fromCIMCommand;
                bool bitOn = cimCommandDetail.IsCIMBitOn();
                if (bitOn)
                {
                    lock (_locker)
                    {
                        if (_commandHandling.ContainsKey(CIMCommandDetail.Create(fromCIMCommand).Command)) continue;
                        _commandHandling.Add(CIMCommandDetail.Create(fromCIMCommand).Command, true);
                    }

                    LogManager.GetLogger("CIM").Info($"{fromCIMCommand} CIM bit {CIMCommandDetail.Create(fromCIMCommand).CIMAddress} ON");

                    _ = Task.Run(() =>
                    {
                        var cimCommand = CIMCommandDetail.Create(fromCIMCommand);

                        if (cimCommand.Command == CIMCommand.OperatorCall || cimCommand.Command == CIMCommand.Interlock)
                        {
                            cimCommand.SetPLCBitOn();
                            cimCommand.WaitForCIMBitOff();
                            cimCommand.SetPLCBitOff();
                        }

                        if (CIMCommandDetail.Create(fromCIMCommand).CIMWordLength > 0)
                        {
                            cimCommand.ReadCIMWords();
                        }

                        Task.Run(() =>
                        {
                            FromCIMCommandAction?.Invoke(new CIMCommandArgs(cimCommand.Command, cimCommand.FromCIMDataBuffer));

                            lock (_locker)
                            {
                                if (_commandHandling != null && _commandHandling.ContainsKey(CIMCommandDetail.Create(fromCIMCommand).Command))
                                    _commandHandling.Remove(CIMCommandDetail.Create(fromCIMCommand).Command);
                            }
                        });

                        if (cimCommand.Command == CIMCommand.OperatorCall)
                        {
                            lock (_locker)
                            {
                                if (_commandHandling != null && _commandHandling.ContainsKey(CIMCommandDetail.Create(fromCIMCommand).Command))
                                    _commandHandling.Remove(CIMCommandDetail.Create(fromCIMCommand).Command);
                            }
                        }
                    });
                }
            }
        }

        public void PPIDListInquiryResponse(List<RecipeInfo> recipeInfos)
        {
            if (recipeInfos.Count() > 100)
            {
                throw new Exception("ppidList size must have exact 100 member");
            }

            for (int i = 0; i < recipeInfos.Count; i++)
            {
                EquipEventHelpers.PPIDListSinglePPIDWrite(recipeInfos[i].Name, recipeInfos[i].Id.ToString());
            }

            EquipEventDetail.Create(EquipEvent.CurrentEquipPPIDListResponse).SetPLCBitOn();
            EquipEventDetail.Create(EquipEvent.CurrentEquipPPIDListResponse).WaitForCIMBitOff();
            EquipEventDetail.Create(EquipEvent.CurrentEquipPPIDListResponse).SetPLCBitOff();
        }
        #endregion

        #region Privates
        private static CIMAliveTask _cimAliveTask;
        private static Task _cimTrasmitTask;
        private static Task _cimStatusUpdateTask;
        private static CancellationTokenSource ctsCIMTrasmitTask = new CancellationTokenSource();
        private ICIMMapHelper _mapHelper;
        private readonly RecipeSelector _recipeSelector;
        private readonly CIMFunctionViewModel _cimFunctionVM;
        private readonly RecipeCatalog _recipeCatalog;
        private readonly CIMCollection _cIMCollection;
        private readonly IViewModelFactory _viewModelFactory;
        private Dictionary<CIMCommand, bool> _commandHandling = new Dictionary<CIMCommand, bool>();
        private readonly MachineStatus _machineStatus;
        private object _locker = new object();

        private readonly CIMCommandDetail cimCommandDetail;
        #endregion
    }
}
