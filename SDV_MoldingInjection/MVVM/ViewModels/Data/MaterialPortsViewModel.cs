using EQX.Core.Common;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom;
using EQX.Core.Communication.CIM.Custom.WordArea;
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

        private readonly CIMCollection _cimCollection;
        private readonly CIMAction _cimAction;
        private readonly ICIMMapHelper _mapHelper;
        private readonly SyringAmountStatusList _syringAmountStatusList;

        public MaterialPortsViewModel(CIMCollection cimCollection, CIMAction cimAction, ICIMMapHelper mapHelper, SyringAmountStatusList syringAmountStatusList)
        {
            _cimCollection = cimCollection;
            _cimAction = cimAction;
            _mapHelper = mapHelper;
            _syringAmountStatusList = syringAmountStatusList;
            _cimAction.FromCIMCommandAction += _cimAction_FromCIMCommandAction;

            Log = LogManager.GetLogger("CIM");
        }

        private void _cimAction_FromCIMCommandAction(EQX.Core.Communication.CIM.CIMCommandArgs obj)
        {
            if (obj == null) return;
            if (obj.CIMCommand != CIMCommand.MaterialInfoSend1) return;

            Log.Info($"[CIM] {obj.CIMCommand} From CIMCOMMAND");

            MaterialInfoSendCimtoPLCArea cimArea = new MaterialInfoSendCimtoPLCArea();
            cimArea.FromCIMData(obj.Buffer);

            if (MaterialPorts.Any(mp => mp.Id.ToString() == cimArea.MaterialProtID) == false)
            {
                Log.Error($"[CIM] {obj.CIMCommand} send {cimArea.MaterialProtID} missmatch");
                return;
            }

            var mp = MaterialPorts.First(mp => mp.Id.ToString() == cimArea.MaterialProtID);

            mp.Result = cimArea.MaterialReplyStatus;

            if (mp.Result.ToUpper() != "PASS")
            {
                mp.UIUpdate();
                return;
            }

            mp.KittingFeedback(cimArea);

            _syringAmountStatusList.SyringeAmounts[int.Parse(cimArea.MaterialProtID) - 1].IsAttached = mp.LastKittingCEID == EMaterialKittingCEID.KITTING;
            _syringAmountStatusList.SyringeAmounts[int.Parse(cimArea.MaterialProtID) - 1].MaxVolume = mp.TotalQty;
            _syringAmountStatusList.SyringeAmounts[int.Parse(cimArea.MaterialProtID) - 1].RemainVolume = mp.RemainQty;

            Log.Info($"{cimArea.MaterialReplyText} {cimArea.MaterialTotalQTY} {cimArea.MaterialProtID}");
        }

        protected override void Dispose(bool disposing)
        {
            //_cimAction.FromCIMCommandAction -= _cimAction_FromCIMCommandAction;
            base.Dispose(disposing);
        }
    }
}
