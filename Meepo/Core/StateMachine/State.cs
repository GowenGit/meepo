namespace Meepo.Core.StateMachine
{
    public enum State
    {
        Invalid,
        Stopped,
        Running
    }

    public enum Command
    {
        SendToClients,
        SendToClient,
        GetClientIds,
        Start,
        Stop
    }
}
