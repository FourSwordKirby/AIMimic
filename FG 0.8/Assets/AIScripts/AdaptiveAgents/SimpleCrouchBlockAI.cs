using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// An AI which is preprogrammed to do a certain series of moves in a certain order. 
/// This will be used to generate training instances
/// </summary>
public class SimpleCrouchBlockAI : AIAgent {
    Snapshot currentState;
    public Text DebugText;

    void Update()
    {
        if (!AIPlayer.enabled || GameManager.instance.roundOver)
            return;

        gameRecorder.LatestFrame();

        ObserveState();
        Action action = GetAction();
        PerformAction(action);
    }

    public override void ObserveState()
    {
        currentState = GetGameState();
    }

    public override Action GetAction()
    {
        return Action.CrouchBlock;
    }

    public override void PerformAction(Action action)
    {
        //Edge case which is not covered by the base system due to how we're tracking player actions
        //Prevents the AI from standing or crouching once commiting itself to an attack
        if(AIPlayer.ActionFsm.CurrentState is AttackState)
        {
            if (action == Action.Stand || action == Action.Crouch)
                return;
        }

        bool actionSucceeded = AIPlayer.PerformAction(action);
    }
}
