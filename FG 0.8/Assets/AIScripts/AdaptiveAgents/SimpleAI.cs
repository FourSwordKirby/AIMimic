using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A simple, baseline AI that the optimal AI will learn how to beat
/// This AI will be very simple, it will always walk towards the opponent. It will always walk towards the opponent. 
/// If it sees the opponent in the air, it will block high for a full second. 
/// If it sees the opponent attack on the ground it will block low for a full second. 
/// Otherwise, it will continously attack mid when the opponent is within 1 unit of it and will attack low if it is within 1/2 a unit of it.
/// </summary>
public class SimpleAI : AIAgent {
    Snapshot currentState;
    public Text DebugText;

    private float delay = 0.0f;

    void Update()
    {
        if (!AIPlayer.enabled || GameManager.instance.roundOver)
            return;

        GameRecorder.instance.LatestFrame();
        if (GameManager.instance.currentFrame % 3 == 1)
        {
            if (delay > 0)
            {
                delay -= Time.deltaTime;
                return;
            }

            ObserveState();
            Action action = GetAction();
            PerformAction(action);
        }
    }

    public override void ObserveState()
    {
        currentState = GetGameState();
    }

    public override Action GetAction()
    {
        Action action = Action.Stand;
        float xAbs = Mathf.Abs(currentState.xDistance);
        float yAbs = Mathf.Abs(currentState.yDistance);

        if (AIPlayer.isPlayer1)
        {
            if (currentState.p2Status == PlayerStatus.Air && xAbs < 3.0f)
            {
                if (currentState.p1Status == PlayerStatus.Lowblock)
                    action = Action.Crouch;
                else if (currentState.p1Status == PlayerStatus.Crouch)
                    action = Action.Stand;
                else
                    action = Action.StandBlock;
            }
            else if (xAbs < 3.0f && yAbs < 1.0f)
            {
                if (currentState.p1Status == PlayerStatus.Stand)
                    action = Action.Crouch;
                else
                    action = Action.CrouchBlock;
            }
            else if (1.5 <= xAbs && xAbs < 3.0)
            {
                if (currentState.p1Status == PlayerStatus.Crouch)
                    action = Action.Stand;
                else
                    action = Action.Attack;

            }
            else if (xAbs < 1.5)
            {
                if (currentState.p1Status == PlayerStatus.Stand)
                    action = Action.Crouch;
                else
                    action = Action.LowAttack;
            }
            else
            {
                if (currentState.p1Status == PlayerStatus.Crouch || currentState.p1Status == PlayerStatus.Highblock)
                    action = Action.Stand;
                else if (currentState.p1Status == PlayerStatus.Lowblock)
                    action = Action.Crouch;
                else
                {
                    if (currentState.xDistance > 0)
                        action = Action.WalkLeft;
                    else
                        action = Action.WalkRight;
                }
            }
        }
        else
        {
            if(currentState.p2Grounded)
            {
                if (xAbs < 3.0f)
                {
                    if (currentState.p1Grounded == false)
                    {
                        if (currentState.p2Status == PlayerStatus.Lowblock)
                            action = Action.Crouch;
                        else if (currentState.p2Status == PlayerStatus.Crouch)
                            action = Action.Stand;
                        else
                            action = Action.StandBlock;
                    }
                    else if (currentState.p1Status == PlayerStatus.Crouch)
                    {
                        if (currentState.xDistance > 0)
                            action = Action.JumpRight;
                        else
                            action = Action.JumpLeft;
                    }
                    else if(!currentState.p2Grounded && currentState.yDistance < 2.0f)
                    {
                        action = Action.AirAttack;
                    }
                    else
                    {
                        float rng = Random.Range(0.0f, 1.0f);
                        if (rng < 0.25f)
                        {
                            if (currentState.p2Status == PlayerStatus.Lowblock)
                                action = Action.Crouch;
                            else if (currentState.p2Status == PlayerStatus.Crouch || currentState.p2Status == PlayerStatus.Highblock)
                                action = Action.Stand;
                            else
                                action = Action.Attack;
                        }
                        else if (rng < 0.5)
                        {
                            if (currentState.p2Status == PlayerStatus.Lowblock)
                                action = Action.Crouch;
                            else if (currentState.p2Status == PlayerStatus.Crouch || currentState.p2Status == PlayerStatus.Highblock)
                                action = Action.Stand;
                            else
                            {
                                action = Action.StandBlock;
                                delay = 0.1f;
                            }
                        }
                        else if (rng < 0.75f)
                        {
                            if (currentState.p2Status == PlayerStatus.Highblock)
                                action = Action.Stand;
                            if (currentState.p2Status == PlayerStatus.Stand || currentState.p2Status == PlayerStatus.Lowblock)
                                action = Action.Crouch;
                            else
                                action = Action.LowAttack;
                        }
                        else
                        {
                            if (currentState.p2Status == PlayerStatus.Highblock)
                                action = Action.Stand;
                            if (currentState.p2Status == PlayerStatus.Stand || currentState.p2Status == PlayerStatus.Lowblock)
                                action = Action.Crouch;
                            else
                            {
                                action = Action.CrouchBlock;
                                delay = 0.1f;
                            }
                        }
                    }
                }
                else
                {

                    if (currentState.p2Status == PlayerStatus.Crouch || currentState.p2Status == PlayerStatus.Highblock)
                        action = Action.Stand;
                    else if (currentState.p2Status == PlayerStatus.Lowblock)
                        action = Action.Crouch;
                    else
                    {
                        if (currentState.xDistance > 0)
                            action = Action.WalkRight;
                        else
                            action = Action.WalkLeft;
                    }
                }
            }
            if (currentState.p2Status == PlayerStatus.KnockdownHit)
            {
                action = Action.TechNeutral;
            }
        }
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
