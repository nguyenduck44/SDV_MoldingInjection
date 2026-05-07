namespace SDV_MoldingInjection.Defines
{
    public enum ERootProcToRunStep
    {
        Start,
        AutoModeSwitchCheck,
        DoorClose,
        DoorSensorCheck,
        DoorLock_Check,
        InOutMachine_DoorClose_Check,
        ChildsToRunDone_Wait,
        End
    }
}
