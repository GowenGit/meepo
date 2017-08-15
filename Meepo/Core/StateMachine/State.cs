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
        RemovieClient,
        SendToClients,
        SendToClient,
        GetClientIds,
        Start,
        Stop
    }
}
