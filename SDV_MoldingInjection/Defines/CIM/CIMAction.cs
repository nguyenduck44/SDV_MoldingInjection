using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.Core.Sequence;
using EQX.UI.Controls;
using EQX.UI.MVVM;
using log4net;
using log4net.Repository.Hierarchy;
using System.Diagnostics;
using TOPENG_Device;

namespace SDV_MoldingInjection.Defines.CIM
{
    public class CIMCollection
    {
        public List<MaterialPort> MaterialPorts { get; }

        public CIMCollection(IEnumerable<MaterialPort> materialPorts)
        {
            MaterialPorts = materialPorts.ToList();

            for (int i = 0; i < MaterialPorts.Count(); i++)
            {
                MaterialPorts[i].Id = i + 1;
                MaterialPorts[i].Name = $"Material Port Head {i + 1}";
                MaterialPorts[i].Type = "RESIN";
            }
            MaterialPorts.First().IsCurrentActived = true;
        }
    }

    public class CIMAction
    {
        #region Events
        public event Action<CIMCommandArgs> FromCIMCommandAction;
        #endregion

        public CIMAction(MachineStatus machineStatus, ICIMMapHelper mapHelper)
        {
            _mapHelper = mapHelper;
            _machineStatus = machineStatus;

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

                        MessageBoxEx.Show($"[{cimArea.TerminalNumber}] {cimArea.TerminalDisplayText}", false, "TerminalDisplay", closeItSelf : true);
                    }
                    return;
                case CIMCommand.DatetimeSet:
                    {
                        DatetimeSetArea cimArea = new DatetimeSetArea();
                        cimArea.FromCIMData(obj.Buffer);

                        SystemTimeSetter.SetTimeFromString(cimArea.Datetime);
                    }
                    break;
                case CIMCommand.OperatorCall:
                    {
                        OperatorCallCimToPlcArea cimArea = new OperatorCallCimToPlcArea();
                        cimArea.FromCIMData(obj.Buffer);

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
                case CIMCommand.FormattedProcessProgramSend:
                    {
                        
                    }
                    break;
                case CIMCommand.FormattedProcessProgramRequest:
                    {

                    }
                    break;
                case CIMCommand.CurrentEquipPPIDListRequest:
                    {

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

                    await Task.Delay(50);
                }
            }, token);
        }

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
                CIMCommandDetail cimMap = new CIMCommandDetail(_mapHelper)
                {
                    Command = fromCIMCommand
                };

                bool bitOn = cimMap.IsCIMBitOn();
                if (bitOn)
                {
                    lock(_locker)
                    {
                        if (_commandHandling.ContainsKey(cimMap.Command)) continue;
                        _commandHandling.Add(cimMap.Command, true);
                    }

                    LogManager.GetLogger("CIM").Info($"{cimMap.Command} CIM bit {cimMap.CIMAddress} ON");

                    CIMCommandDetail commandDetail = cimMap;
                    _ = Task.Run(() =>
                    {
                        if (commandDetail.CIMWordLength > 0)
                        {
                            commandDetail.ReadCIMWords();
                        }

                        CIMCommandDetail tmpDetail = commandDetail;
                        Task.Run(() =>
                        {
                            FromCIMCommandAction?.Invoke(new CIMCommandArgs(tmpDetail.Command, tmpDetail.FromCIMDataBuffer));
                        });

                        LogManager.GetLogger("CIM").Info($"{commandDetail.Command} LOCAL bit {commandDetail.PLCAddress} ON then OFF");

                        commandDetail.SetLocalPLCBitOn();

                        // TODO : Clear _commandHandling for Display Command

                        commandDetail.WaitForCIMBitOff(10000);

                        lock (_locker)
                        {
                            _commandHandling.Remove(commandDetail.Command);
                        }

                        LogManager.GetLogger("CIM").Info($"{commandDetail.Command} CIM bit {commandDetail.CIMAddress} OFF");
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
        private Dictionary<CIMCommand, bool> _commandHandling = new Dictionary<CIMCommand, bool>();
        private readonly MachineStatus _machineStatus;
        private object _locker = new object();
        #endregion
    }
}
