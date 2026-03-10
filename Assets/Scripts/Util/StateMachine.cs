using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> where T : struct
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

    public Action<T?, T?> OnStateChange;

    public T CurrentState => _currentState?.Id ?? default;

    public bool EnableDebugLogging = true;

    private readonly Dictionary<T, State> _stateMap = new();
    private State _currentState;

    private readonly string _name;

    public StateMachine(string name)
    {
        _name = name;
    }

    public void AddState(T stateId, Action onStateEnter, Action onStateUpdate, Action onStateExit)
    {
        var state = new State(stateId, onStateEnter, onStateUpdate, onStateExit);
        _stateMap.Add(stateId, state);
    }

    public bool GoToState(T stateId)
    {
        if (_stateMap != null && _stateMap.TryGetValue(stateId, out var newState))
        {
            var previousState = _currentState;

            DebugLogStateTransition(previousState, newState);

            previousState?.OnStateExit?.Invoke();

            _currentState = newState;
            newState.OnStateEnter?.Invoke();

            OnStateChange?.Invoke(previousState?.Id, newState?.Id);

            return true;
        }
        else
        {
            return false;
        }
    }

    public void Update()
    {
        if (_currentState != null)
        {
            _currentState.OnStateUpdate?.Invoke();
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

        Debug.Log($"State machine {_name} transition: {oldStateId} -> {newStateId}");
    }
}
