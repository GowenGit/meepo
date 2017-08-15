using System.Collections.Generic;
using Meepo.Core.Logging;

namespace Meepo.Core.StateMachine
{
    internal class MeepoStateMachine
    {
        private readonly ILogger logger;

        public State CurrenState { get; private set; } = State.Stopped;

        private readonly Dictionary<StateTransition, State> transitions = new Dictionary<StateTransition, State>
        {
            { new StateTransition(State.Running, Command.RemovieClient), State.Running},
            { new StateTransition(State.Running, Command.GetClientIds), State.Running},
            { new StateTransition(State.Running, Command.SendToClients), State.Running},
            { new StateTransition(State.Running, Command.SendToClient), State.Running},
            { new StateTransition(State.Running, Command.Stop), State.Stopped},
            { new StateTransition(State.Stopped, Command.Start), State.Running},
        };

        public MeepoStateMachine(ILogger logger)
        {
            this.logger = logger;
        }

        private State GetNext(Command command)
        {
            var transition = new StateTransition(CurrenState, command);

            if (transitions.TryGetValue(transition, out State nextState)) return nextState;

            var message = $"Can't call {command} when server is {CurrenState}";

            logger.Warning(message);

            return State.Invalid;
        }

        public State MoveNext(Command command)
        {
            CurrenState = GetNext(command);

            return CurrenState;
        }
    }
}
