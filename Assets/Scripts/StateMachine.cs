using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> where T : IConvertible
{
    private class State
    {
        public T Id { get; private set; }
        public Action OnStateEnter { get; private set; }
        public Action OnStateUpdate { get; private set; }
        public Action OnStateExit { get; private set; }

        public State(T id, Action onStateEnter, Action onStateUpdate, Action onStateExit)
        {
            Id = id;
            OnStateEnter = onStateEnter;
            OnStateUpdate = onStateUpdate;
            OnStateExit = onStateExit;
        }
    }

    public bool EnableDebugLogging = true;

    private Dictionary<T, State> stateMap = new Dictionary<T, State>();
    private State currentState = null;

    private string name;

    public StateMachine(string name)
    {
        this.name = name;
    }

    public void AddState(T stateId, Action onStateEnter, Action onStateUpdate, Action onStateExit)
    {
        var state = new State(stateId, onStateEnter, onStateUpdate, onStateExit);
        stateMap.Add(stateId, state);
    }

    public bool GoToState(T stateId)
    {
        if (stateMap != null && stateMap.TryGetValue(stateId, out var newState))
        {
            DebugLogStateTransition(currentState, newState);

            currentState?.OnStateExit?.Invoke();
            newState.OnStateEnter?.Invoke();

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
            currentState.OnStateUpdate?.Invoke();
        }
    }

    private void DebugLogStateTransition(State oldState, State newState)
    {
        if (!EnableDebugLogging)
        {
            return;
        }

        var oldStateId = oldState != null ? oldState.Id.ToString() : "none";
        var newStateId = newState != null ? newState.Id.ToString() : "none";

        Debug.Log($"State machine {name} transition: {oldStateId} -> {newStateId}");
    }
}
