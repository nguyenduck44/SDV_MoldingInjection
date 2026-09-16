using EQX.Core.Common;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.UI.Controls;
using EQX.UI.MVVM;
using log4net;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.CIM;
using System.Diagnostics;
using TOPENG_Device;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class MaterialPortsViewModel : ViewModelBase
    {
        public List<MaterialPort> MaterialPorts => _cimCollection.MaterialPorts;

        public MachineStatus MachineStatus { get; }

        private readonly CIMCollection _cimCollection;
        private readonly CIMAction _cimAction;
        private readonly ICIMMapHelper _mapHelper;
        private readonly SyringAmountStatusList _syringAmountStatusList;

        public MaterialPortsViewModel(CIMCollection cimCollection, 
            CIMAction cimAction, 
            ICIMMapHelper mapHelper, 
            SyringAmountStatusList syringAmountStatusList,
            MachineStatus machineStatus)
        {
            _cimCollection = cimCollection;
            _cimAction = cimAction;
            _mapHelper = mapHelper;
            _syringAmountStatusList = syringAmountStatusList;
            MachineStatus = machineStatus;
            for (int i = 0; i < MaterialPorts.Count; i++)
            {
                MaterialPorts[i].TotalQty = (int)(_syringAmountStatusList.SyringeAmounts[i].MaxVolume);
                MaterialPorts[i].UseQty = (int)(_syringAmountStatusList.SyringeAmounts[i].UsedVolume);
                MaterialPorts[i].OnKittingSent += MaterialPortsViewModel_OnKittingSent;
                MaterialPorts[i].OnCancelSent += MaterialPortsViewModel_OnCancelSent;
            }

            _cimAction.FromCIMCommandAction += _cimAction_FromCIMCommandAction;

            Log = LogManager.GetLogger("CIM");
        }

        private void MaterialPortsViewModel_OnCancelSent(object? sender, EventArgs e)
        {
            if (sender is MaterialPort mp == false) return;

            if (_cimCollection.CimFunction.MaterialTrackingState != OnOffNothing.ON)
            {
                MaterialInfoSend1Handle(null, true, mp);
            }
        }

        private void MaterialPortsViewModel_OnKittingSent(object? sender, EventArgs e)
        {
            if (sender is MaterialPort mp == false) return;

            if (_cimCollection.CimFunction.MaterialTrackingState != OnOffNothing.ON)
            {
                MaterialInfoSend1Handle(null, true, mp);
            }
        }

        private bool handlingAction = false;

        private void _cimAction_FromCIMCommandAction(CIMCommandArgs obj)
        {
            if (obj == null) return;
            if (obj.CIMCommand != CIMCommand.MaterialInfoSend1) return;

            MaterialInfoSend1Handle(obj);
        }

        private void MaterialInfoSend1Handle(CIMCommandArgs obj, bool isAutoPass = false, MaterialPort? autoPassMP = null)
        {
            if (handlingAction) return;
            handlingAction = true;

            if (isAutoPass & autoPassMP == null)
            {
                throw new ArgumentException("autoPassMP must not be null, while isAutoPass");
            }

            MaterialInfoSendCimtoPLCArea cimArea = new MaterialInfoSendCimtoPLCArea();
            if (isAutoPass == false)
            {
                Log.Info($"[CIM] {obj.CIMCommand} From CIMCOMMAND");

                cimArea.FromCIMData(obj.Buffer);
            }
            else
            {
                cimArea = new MaterialInfoSendCimtoPLCArea()
                {
                    MaterialProtID = autoPassMP!.Id.ToString(),
                    MaterialReplyStatus = "AUTO_PASS",
                    MaterialReplyText = "AUTO_PASS_REPLY",
                    MaterialTotalQTY = autoPassMP.LastKittingCEID == EMaterialKittingCEID.KITTING ? 1031 : 0,
                    MaterialUseQTY = 0,
                };
            }

            if (MaterialPorts.Any(mp => mp.Id.ToString() == cimArea.MaterialProtID) == false)
            {
                Log.Error($"[CIM] {obj.CIMCommand} send {cimArea.MaterialProtID} missmatch");
                handlingAction = false;
                return;
            }

            var mp = MaterialPorts.First(mp => mp.Id.ToString() == cimArea.MaterialProtID);

            mp.Result = cimArea.MaterialReplyStatus;

            if (mp.Result.Contains("PASS") == false)
            {
                if (mp.LastKittingCEID == EMaterialKittingCEID.KITTING)
                {
                    MessageBoxEx.Show($"Kitting '{mp.BatchID}' Fail");
                }
                else
                {
                    MessageBoxEx.Show($"Cancel '{mp.BatchID}' Fail");
                }
                mp.UIUpdate();
                handlingAction = false;

                CIMCommandDetail.Create(CIMCommand.MaterialInfoSend1).SetPLCBitOn();
                CIMCommandDetail.Create(CIMCommand.MaterialInfoSend1).WaitForCIMBitOff();
                CIMCommandDetail.Create(CIMCommand.MaterialInfoSend1).SetPLCBitOff();

                return;
            }

            mp.KittingFeedback(cimArea);

            _syringAmountStatusList.SyringeAmounts[int.Parse(cimArea.MaterialProtID) - 1].IsAttached = mp.LastKittingCEID == EMaterialKittingCEID.KITTING;
            _syringAmountStatusList.SyringeAmounts[int.Parse(cimArea.MaterialProtID) - 1].MaxVolume = mp.TotalQty;
            _syringAmountStatusList.SyringeAmounts[int.Parse(cimArea.MaterialProtID) - 1].RemainVolume = mp.RemainQty;

            Log.Info($"{cimArea.MaterialReplyText} {cimArea.MaterialTotalQTY} {cimArea.MaterialProtID}");
            handlingAction = false;
        }

        protected override void Dispose(bool disposing)
        {
            //_cimAction.FromCIMCommandAction -= _cimAction_FromCIMCommandAction;
            base.Dispose(disposing);
        }
    }
}
