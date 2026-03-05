using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.UI.Controls;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Windows;
using TOPENG_Device;
using Windows.Storage.Streams;

namespace SDV_MoldingInjection.Defines.CIM
{
    public class CIMAction
    {
        #region Events
        public event Action<CIMCommandArgs> FromCIMCommandAction;
        #endregion

        public CIMAction()
        {
            _mapHelper = new SDVCIMMapHeler();

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

                        MessageBoxEx.Show($"[{cimArea.TerminalNumber}] {cimArea.TerminalDisplayText}");
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

                        MessageBoxEx.ShowDialog($"[{cimArea.OperatorCallID} {cimArea.OperatorCallText}]", false);

                        OperatorCallPlcToCimArea plcArea = new OperatorCallPlcToCimArea()
                        {
                            OPCallIDComfirm = cimArea.OperatorCallID,
                            OPCallMessageConfirm = cimArea.OperatorCallText
                        };

                    }
                    break;
                case CIMCommand.Interlock:
                    {

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
                    if (_commandHandling.ContainsKey(cimMap.Command)) return;
                    _commandHandling.Add(cimMap.Command, true);

                    CIMCommandDetail commandDetail = cimMap;
                    _ = Task.Run(() =>
                    {
                        var buf = new short[commandDetail.CIMWordLength];

                        if (commandDetail.CIMWordLength > 0)
                        {
                            CIMAddressMap.ReadWords(CIMAddressMap.GetReadWordIndexFromDAddress(commandDetail.CIMWordAddress), commandDetail.CIMWordLength, buf);
                        }

                        FromCIMCommandAction?.Invoke(new CIMCommandArgs(commandDetail.Command, buf));

                        commandDetail.SetLocalPLCBitOn();

                        commandDetail.WaitForCIMBitOff();
                        _commandHandling.Remove(commandDetail.Command);
                    });
                }
            }
        }
        #endregion

        #region Privates
        private static CIMAliveTask _cimAliveTask;
        private static Task _cimTrasmitTask;
        private static CancellationTokenSource ctsCIMTrasmitTask = new CancellationTokenSource();
        private ICIMMapHelper _mapHelper;
        private Dictionary<CIMCommand, bool> _commandHandling = new Dictionary<CIMCommand, bool>();
        #endregion
    }
}
