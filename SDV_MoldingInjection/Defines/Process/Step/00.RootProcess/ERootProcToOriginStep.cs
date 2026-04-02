namespace SDV_MoldingInjection.Defines
{
    public enum ERootProcToOriginStep
    {
        Start,
        AutoModeSwitchCheck,
        DoorClose,
        DoorSensorCheck,
        DoorLockCheck,
        Motion_AlarmReset,
        Motion_AlarmReset_Wait,
        ChildsToOriginDone_Wait,
        End
    }
}
