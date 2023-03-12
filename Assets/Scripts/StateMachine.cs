using System;
using System.Collections;
using System.Collections.Generic;

public class StateMachine<T> where T : IEqualityComparer<T>
{
    private class State
    {
        public Action OnStateEnter { get; private set; }
        public Action OnStateUpdate { get; private set; }
        public Action OnStateExit { get; private set; }

        public State(Action onStateEnter, Action onStateUpdate, Action onStateExit)
        {
            OnStateEnter = onStateEnter;
            OnStateUpdate = onStateUpdate;
            OnStateExit = onStateExit;
        }
    }

    private State currentState = default;

    private Dictionary<T, State> stateMap = new Dictionary<T, State>();

    public void AddState(T stateId, Action onStateEnter, Action onStateUpdate, Action onStateExit)
    {
        var state = new State(onStateEnter, onStateUpdate, onStateExit);
        stateMap.Add(stateId, state);
    }

    public bool GoToState(T stateId)
    {
        if (stateMap != null && stateMap.TryGetValue(stateId, out var newState))
        {
            if (currentState != null)
            {
                currentState.OnStateExit();
            }
            newState.OnStateEnter();
            currentState = newState;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Update()
    {
        if (currentState != null)
        {
            currentState.OnStateUpdate();
        }
    }
}
