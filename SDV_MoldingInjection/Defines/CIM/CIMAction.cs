using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.Core.Sequence;
using EQX.UI.Controls;
using log4net;
using log4net.Repository.Hierarchy;
using SDV_MoldingInjection.Recipe;
using System.Diagnostics;
using TOPENG_Device;

namespace SDV_MoldingInjection.Defines.CIM
{
    public class CIMAction
    {
        #region Events
        public event Action<CIMCommandArgs> FromCIMCommandAction;
        #endregion

        public CIMAction(MachineStatus machineStatus, ICIMMapHelper mapHelper, RecipeSelector recipeSelector)
        {
            _mapHelper = mapHelper;
            _recipeSelector = recipeSelector;
            _machineStatus = machineStatus;
            cimCommandDetail = new CIMCommandDetail(_mapHelper);

            FromCIMCommandAction += CIMAction_FromCIMCommandAction;
        }

        private void CIMAction_FromCIMCommandAction(CIMCommandArgs obj)
        {
            if (CIMConstants.FromCIMCommands.Contains(obj.CIMCommand) == false) return;

            switch (obj.CIMCommand)
            {
                case CIMCommand.TerminalDisplay:
                    {
                        TerminalDisplayCimToPlcArea cimArea = new TerminalDisplayCimToPlcArea();
                        cimArea.FromCIMData(obj.Buffer);

                        CIMCommandDetail.Create(obj.CIMCommand).WaitForCIMBitOff();
                        CIMCommandDetail.Create(obj.CIMCommand).SetPLCBitOff();

                        MessageBoxEx.Show($"[{cimArea.TerminalNumber}] {cimArea.TerminalDisplayText}", false, "TerminalDisplay", closeItSelf : true);
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

                        CIMCommandDetail.Create(obj.CIMCommand).WaitForCIMBitOff();
                        CIMCommandDetail.Create(obj.CIMCommand).SetPLCBitOff();

                        MessageBoxEx.ShowDialog($"[{cimArea.OperatorCallID} {cimArea.OperatorCallText}]", false, "OPCall", closeItSelf: true);

                        OperatorCallPlcToCimArea plcArea = new OperatorCallPlcToCimArea()
                        {
                            OPCallIDComfirm = cimArea.OperatorCallID,
                            OPCallMessageConfirm = cimArea.OperatorCallText
                        };

                        ExecuteScenario(CIMScenario.OPCallOccurAndRelease, 
                            new CIMScenarioContext
                            {
                                OPCallOccurOnly = false,
                                OPCallID = plcArea.OPCallIDComfirm,
                                OPCallMsg = plcArea.OPCallMessageConfirm,
                            });
                    }
                    break;
                case CIMCommand.Interlock:
                    {
                        InterlockCimToPlcArea cimArea = new InterlockCimToPlcArea();
                        cimArea.FromCIMData(obj.Buffer);

                        _machineStatus.IsInterlock = true;
                        _machineStatus.EquipReportState = EquipReportStateKind.Interlock;
                        _machineStatus.OPCommand = EOperationCommand.Stop;

                        // WRITE INTERLOCK MESSAGE
                        InterlockPlcToCimArea plcArea = new InterlockPlcToCimArea()
                        {
                            InterlockIDComfirm = cimArea.InterlockID,
                            InterlockMessageConfirm = cimArea.InterlockMessage
                        };
                        _mapHelper.GetWordAddress(obj.CIMCommand, out _, out string plcAddress, out _, out int plcLength);
                        CIMAddressMap.WriteWords(CIMAddressMap.GetWriteWordIndexFromDAddress(plcAddress), plcLength, plcArea.ToCIMData());

                        CIMCommandDetail.Create(obj.CIMCommand).WaitForCIMBitOff();
                        CIMCommandDetail.Create(obj.CIMCommand).SetPLCBitOff();

                        MessageBoxEx.ShowDialog($"[{cimArea.InterlockID} {cimArea.InterlockMessage}]", false, "Interlock", closeItSelf: true);

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

                        if (fppsArea.CCode == "1") // CREATE NEW
                        {
                            _recipeSelector.Create(parameterWordArea.PPIDName, fppsArea.RecipeNumber);
                        }
                        else if (fppsArea.CCode == "2") // Delete PPID
                        {
                            _recipeSelector.Delete(parameterWordArea.PPIDName, fppsArea.RecipeNumber);
                        }
                        else if (fppsArea.CCode == "3") // Parameter Update
                        {
                            _recipeSelector.CurrentRecipe.CommonRecipe.LogSaveDay = parameterWordArea.Parameters[0];
                        }
                    }
                    break;
                case CIMCommand.FormattedProcessProgramRequest:
                    {
                        var fpprArea = CIMCommandHelpers.GetParameterRequestInfor();
                        EquipEventHelpers.ParameterRequestResponse(fpprArea, "1D34");
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
            }
        }

        #region Public Methods
        public void Start()
        {
            if (_cimAliveTask == null)
                _cimAliveTask = new CIMAliveTask();

            _cimAliveTask.Start();

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
                    CIMScenarioDispatcher.ApplyEquipReportState(_machineStatus.EquipReportState);
                    foreach (var recipe in _recipeSelector.AllRecipe)
                    {
                        EquipEventHelpers.PPIDListSinglePPIDWrite(recipe);
                    }

                    for (int i = 0; i < ECMValues.Length; i++)
                    {
                        EquipEventHelpers.WriteEcmSingle(i + 1, ECMValues[i]);
                    }
                    EquipEventHelpers.WriteFdcItem();

                    await Task.Delay(100);
                }
            }, token);
        }

        public static int[] ECMValues = new int[]
        {
            1234,
            4567,
            1357,
            10000
        };

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

        private readonly CIMCommandDetail cimCommandDetail;

        #region Privates Methods
        private async Task CIMCommandHandleAsync()
        {
            foreach (var fromCIMCommand in CIMConstants.FromCIMCommands)
            {
                cimCommandDetail.Command = fromCIMCommand;
                bool bitOn = cimCommandDetail.IsCIMBitOn();
                if (bitOn)
                {
                    lock(_locker)
                    {
                        if (_commandHandling.ContainsKey(CIMCommandDetail.Create(fromCIMCommand).Command)) continue;
                        _commandHandling.Add(CIMCommandDetail.Create(fromCIMCommand).Command, true);
                    }

                    LogManager.GetLogger("CIM").Info($"{CIMCommandDetail.Create(fromCIMCommand).Command} CIM bit {CIMCommandDetail.Create(fromCIMCommand).CIMAddress} ON");

                    _ = Task.Run(() =>
                    {
                        var cimCommand = CIMCommandDetail.Create(fromCIMCommand);
                        if (CIMCommandDetail.Create(fromCIMCommand).CIMWordLength > 0)
                        {
                            cimCommand.ReadCIMWords();
                        }

                        Task.Run(() =>
                        {
                            FromCIMCommandAction?.Invoke(new CIMCommandArgs(cimCommand.Command, cimCommand.FromCIMDataBuffer));

                            lock (_locker)
                            {
                                _commandHandling.Remove(CIMCommandDetail.Create(fromCIMCommand).Command);
                            }
                        });
                    });
                }
            }
        }
        #endregion

        #region Privates
        private static CIMAliveTask _cimAliveTask;
        private static Task _cimTrasmitTask;
        private static Task _cimStatusUpdateTask;
        private static CancellationTokenSource ctsCIMTrasmitTask = new CancellationTokenSource();
        private ICIMMapHelper _mapHelper;
        private readonly RecipeSelector _recipeSelector;
        private Dictionary<CIMCommand, bool> _commandHandling = new Dictionary<CIMCommand, bool>();
        private readonly MachineStatus _machineStatus;
        private object _locker = new object();
        #endregion
    }
}
