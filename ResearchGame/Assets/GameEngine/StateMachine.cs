using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class StateMachine<CoreType> where CoreType : MonoBehaviour {

    public CoreType owner { get; private set; }
    public Stack<State<CoreType>> StateStack { get; private set; }
    public State<CoreType> CurrentState { get; private set; }

    public StateMachine(CoreType owner)
    {
        this.owner = owner;
        StateStack = new Stack<State<CoreType>>();
        CurrentState = null;
    } 

    /// <summary>
    /// InitializeState is used to set the starting state for the state machine
    /// </summary>
    public void InitialState(State<CoreType> init)
    {
        CurrentState = init;
        CurrentState.Enter();
    }

    /// <summary>
    /// Actions to be run in the Update() method of a MonoBehavior.
    /// General state logic and behavior should go here.
    /// </summary>
    public void Execute()
    {
        CurrentState.Execute();
    }

    /// <summary>
    /// Action to be run in the FixedUpdate() method of a MonoBehavior.
    /// Everything that directly affects the Physics Engine related should go here.
    /// </summary>
    public void FixedExecute()
    {
        CurrentState.FixedExecute();
    }

    /// <summary>
    /// ChangeState is used for normal state machine transitions
    /// </summary>
    public void ChangeState(State<CoreType> newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    /// <summary>
    /// Suspend state is used for transitions where it may be important to keep the state we just transitioned from
    /// Examples of this include things like enemy patrol patterns and other sorts of advancedish AI
    /// </summary>
    public void SuspendState(State<CoreType> newState)
    {
        StateStack.Push(CurrentState);
        CurrentState = newState;
        CurrentState.Enter();
    }

    /// <summary>
    /// Resume state is used for when we want to go back to a state we previously suspended.
    /// </summary>
    public void ResumeState()
    {
        if (StateStack.Count != 0)
        {
            CurrentState.Exit();
            CurrentState = StateStack.Pop();
        }
    }

    public void ClearStates()
    {
        while (StateStack.Count != 0)
        {
            CurrentState.Exit();
            CurrentState = StateStack.Pop();
        }
        CurrentState.Exit();
    }
}
