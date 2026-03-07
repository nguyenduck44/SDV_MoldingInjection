using EQX.Core.Common;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.UI.MVVM;
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

        public MaterialPortsViewModel(CIMCollection cimCollection, CIMAction cimAction, ICIMMapHelper mapHelper)
        {
            _cimCollection = cimCollection;
            _cimAction = cimAction;
            _mapHelper = mapHelper;
            _cimAction.FromCIMCommandAction += _cimAction_FromCIMCommandAction;
        }

        private void _cimAction_FromCIMCommandAction(EQX.Core.Communication.CIM.CIMCommandArgs obj)
        {
            if (obj.CIMCommand != CIMCommand.MaterialInfoSend1) return;

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

            if (mp.LastKittingCEID == EMaterialKittingCEID.KITTING)
            {
                mp.MaterialState = cimArea.MaterialST;
                mp.PortState = cimArea.MaterialState;
                mp.TotalQty = cimArea.MaterialTotalQTY;
                mp.RemainQty = cimArea.MaterialTotalQTY;
                mp.UseQty = cimArea.MaterialUseQTY;
            }
            if (mp.LastKittingCEID == EMaterialKittingCEID.KITTING_CANCEL)
            {
                mp.MaterialState = "";
                mp.PortState = "";
                mp.TotalQty = 0;
                mp.RemainQty = 0;
                mp.UseQty = 0;
            }

            EquipEventDetail equipEvent = new EquipEventDetail(_mapHelper)
            {
                Event = EquipEvent.MaterialLocationUpdate1
            };
            equipEvent.Write(mp.ToCIMData());

            Debug.WriteLine($"{cimArea.MaterialReplyText} {cimArea.MaterialTotalQTY} {cimArea.MaterialProtID}");
        }

        protected override void Dispose(bool disposing)
        {
            _cimAction.FromCIMCommandAction -= _cimAction_FromCIMCommandAction;
            base.Dispose(disposing);
        }
    }
}
