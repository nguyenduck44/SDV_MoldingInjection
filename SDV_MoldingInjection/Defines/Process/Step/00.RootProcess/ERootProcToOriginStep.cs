namespace SDV_MoldingInjection.Defines
{
    public enum ERootProcToOriginStep
    {
        Start,
        AutoModeSwitchCheck,
        DoorClose,
        DoorSensorCheck,
        DoorLockCheck,
        InOutMachine_DoorClose_Check,
        Motion_AlarmReset,
        Motion_AlarmReset_Wait,
        ChildsToOriginDone_Wait,
        End
    }
}
