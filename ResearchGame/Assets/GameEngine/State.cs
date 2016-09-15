using UnityEngine;
using System.Collections;

public abstract class State<CoreType> where CoreType : MonoBehaviour {

    protected CoreType Owner;
    protected StateMachine<CoreType> Fsm;

    public State(CoreType owner, StateMachine<CoreType> fsm)
    {
        this.Owner = owner;
        this.Fsm = fsm;
    }

    /// <summary>
    /// Enter() is called whenever we switch to this State.
    /// Any variables in State should be initialized here.
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// Actions to be run in the Update() method of a MonoBehavior.
    /// General state logic and behavior should go here.
    /// </summary>
    public abstract void Execute();

    /// <summary>
    /// Action to be run in the FixedUpdate() method of a MonoBehavior.
    /// Everything that directly affects the Physics Engine related should go here.
    /// </summary>
    public abstract void FixedExecute();
    
    /// <summary>
    /// Exit is called whenever we leave this State.
    /// Any cleanup or last minute calls (to the GameManager?) should go here.
    /// </summary>
    public abstract void Exit();
}