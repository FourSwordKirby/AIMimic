using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// An AI which is preprogrammed to do a certain series of moves in a certain order. 
/// This will be used to generate training instances
/// </summary>
public class SimplePressureAI : AIAgent {
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
        Action action;
        if (Mathf.Abs(currentState.xDistance) > 2.0f)
        {
            if (currentState.xDistance < 0)
                action = Action.WalkLeft;
            else
                action = Action.WalkRight;
        }
        else
        {
            float random = Random.Range(0.0f, 1.0f);
            if (random < 0.1f)
                action = Action.Attack;
            else if (random < 0.7f)
                action = Action.LowAttack;
            else
            {
                if (currentState.xDistance < 0)
                    action = Action.WalkLeft;
                else
                    action = Action.WalkRight;
            }
        }

        if ((currentState.p1Status == PlayerStatus.StandAttack || currentState.p1Status == PlayerStatus.Stand) && action == Action.LowAttack)
            action = Action.Crouch;
        if (currentState.p1Status == PlayerStatus.Crouch && (action == Action.Attack ||
                                                            action == Action.WalkLeft ||
                                                            action == Action.WalkRight))
            action = Action.Stand;
        return action;
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
