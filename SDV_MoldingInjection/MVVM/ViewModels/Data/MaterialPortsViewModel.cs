using EQX.Core.Common;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.UI.MVVM;
using log4net;
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

            Log.Info($"{cimArea.MaterialReplyText} {cimArea.MaterialTotalQTY} {cimArea.MaterialProtID}");
            
            if (mp.LastKittingCEID == EMaterialKittingCEID.KITTING)
            {
                mp.MaterialState = cimArea.MaterialState;
                mp.State = cimArea.MaterialST;
                mp.TotalQty = cimArea.MaterialTotalQTY;
                mp.RemainQty = cimArea.MaterialTotalQTY;
                mp.UseQty = cimArea.MaterialUseQTY;
                mp.UIUpdate();
                EquipEventDetail equipEvent = new EquipEventDetail(_mapHelper)
                {
                    Event = EquipEvent.MaterialLocationUpdate1
                };
                equipEvent.WriteAndBitOnOff(mp.ToCIMData());

                EquipEventDetail materialPortState = new EquipEventDetail(_mapHelper)
                {
                    Event = EquipEvent.MaterialPortState1 + mp.Id - 1
                };
                MaterialPortStateItemArea area = new MaterialPortStateItemArea()
                {
                    Type = mp.Type,
                    LST = "1",  // MOUNT
                    ID = mp.BatchID,
                    LoaderNo = (short)mp.Id,
                    Usage = (short)mp.RemainQty,
                };
                materialPortState.WriteAndBitOnOff(area.ToCIMData());
            }
            if (mp.LastKittingCEID == EMaterialKittingCEID.KITTING_CANCEL)
            {
                mp.MaterialState = cimArea.MaterialState;
                mp.State = cimArea.MaterialST;
                mp.TotalQty = 0;
                mp.RemainQty = 0;
                mp.UseQty = 0;
                mp.UIUpdate();
                EquipEventDetail equipEvent = new EquipEventDetail(_mapHelper)
                {
                    Event = EquipEvent.MaterialShortage1
                };
                equipEvent.WriteAndBitOnOff(mp.ToCIMData());

                EquipEventDetail materialPortState = new EquipEventDetail(_mapHelper)
                {
                    Event = EquipEvent.MaterialPortState1 + mp.Id - 1
                };
                MaterialPortStateItemArea area = new MaterialPortStateItemArea()
                {
                    Type = mp.Type,
                    LST = "3",  // UNMOUNT
                    ID = "",
                    LoaderNo = (short)mp.Id,
                    Usage = 0,
                };
                materialPortState.WriteAndBitOnOff(area.ToCIMData());
            }
        }

        protected override void Dispose(bool disposing)
        {
            //_cimAction.FromCIMCommandAction -= _cimAction_FromCIMCommandAction;
            base.Dispose(disposing);
        }
    }
}
