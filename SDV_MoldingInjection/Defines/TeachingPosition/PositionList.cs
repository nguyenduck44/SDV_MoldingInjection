namespace SDV_MoldingInjection.Defines.TeachingPosition
{
    public class PositionList
    {
        public PositionList(DryPumpMaintenanceTeachingPosition dryPumpMaintenanceTeachingPosition,
            InjectMaintenanceTeachingPosition injectMaintenanceTeachingPosition,
            IEnumerable<SPDHeadMaintenanceTeachingPosition> sPDHeadMaintenanceTeachingPositions)
        {
            DryPumpMaintenanceTeachingPosition = dryPumpMaintenanceTeachingPosition;
            InjectMaintenanceTeachingPosition = injectMaintenanceTeachingPosition;
            SPDHead1MaintenanceTeachingPosition = sPDHeadMaintenanceTeachingPositions.First(p => p.Head == ESPDHead.SPDHead1);
            SPDHead2MaintenanceTeachingPosition = sPDHeadMaintenanceTeachingPositions.First(p => p.Head == ESPDHead.SPDHead2);
            SPDHead3MaintenanceTeachingPosition = sPDHeadMaintenanceTeachingPositions.First(p => p.Head == ESPDHead.SPDHead3);
            SPDHead4MaintenanceTeachingPosition = sPDHeadMaintenanceTeachingPositions.First(p => p.Head == ESPDHead.SPDHead4);
        }

        public DryPumpMaintenanceTeachingPosition DryPumpMaintenanceTeachingPosition { get; }
        public InjectMaintenanceTeachingPosition InjectMaintenanceTeachingPosition { get; }
        public SPDHeadMaintenanceTeachingPosition SPDHead1MaintenanceTeachingPosition { get; }
        public SPDHeadMaintenanceTeachingPosition SPDHead2MaintenanceTeachingPosition { get; }
        public SPDHeadMaintenanceTeachingPosition SPDHead3MaintenanceTeachingPosition { get; }
        public SPDHeadMaintenanceTeachingPosition SPDHead4MaintenanceTeachingPosition { get; }
    }
}
