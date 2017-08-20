namespace Meepo.Core.StateMachine
{
    internal struct StateTransition
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public State State { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Command Command { get; }

        public StateTransition(State state, Command command)
        {
            State = state;
            Command = command;
        }
    }
}
