namespace Meepo.Core.StateMachine
{
    internal struct StateTransition
    {
        public State State { get; }

        public Command Command { get; }

        public StateTransition(State state, Command command)
        {
            State = state;
            Command = command;
        }
    }
}
