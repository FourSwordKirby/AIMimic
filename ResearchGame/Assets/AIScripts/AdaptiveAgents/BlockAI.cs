using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// An RL AI which is focused on learning how to properly block quickly.
/// It has most functions that aren't related to blocking stripped away from it
/// </summary>
public class BlockAI : AIAgent {
    int backpropDepth = 1;

    AdaptiveActionSelector actionSelector;
    List<Action> ConstrainActions(AISituation situation) { return new List<Action>() { Action.StandBlock, Action.CrouchBlock }; }

    Snapshot currentState = null;

    AISituation currentSituation = null;
    List<AISituation> pastSituations = new List<AISituation>();
    List<Action> pastActions = new List<Action>();

    public Text DebugText;

    private void Awake()
    {
        actionSelector = new AdaptiveActionSelector();
        actionSelector.ConstrainActions = ConstrainActions;
    }

    void LateUpdate()
    {
        if (!AIPlayer.enabled || GameManager.instance.roundOver || AIPlayer.stunned)
            return;

        GameRecorder.instance.CaptureFrame();
        if (GameManager.instance.currentFrame % 2 == 1)
        {
            ObserveState();
            Action action = GetAction();
            PerformAction(action);
        }
    }

    public override void ObserveState()
    {
        currentState = GetGameState();
        currentSituation = new AISituation(currentState, AIPlayer.isPlayer1);
    }

    public override Action GetAction()
    {
        Action action = actionSelector.GetBestAction(currentSituation);
        return action;
    }

    public override void PerformAction(Action action)
    {
        //Edge case which is not covered by the base system due to how we're tracking player actions
        //Prevents the AI from standing or crouching once commiting itself to an attack
        if (AIPlayer.ActionFsm.CurrentState is AttackState)
        {
            if (action == Action.Stand || action == Action.Crouch)
                return;
        }

        bool actionSucceeded = AIPlayer.PerformAction(action);

        //If we successfully did the action, update the past action and past situation
        if (actionSucceeded)
        {
            //Debug to show how good the last performed action was
            DebugText.text = "Last action: " + action + "\n" + "Current Weight: " + actionSelector.GetWeight(currentSituation, action);

            pastActions.Add(action);
            pastSituations.Add(currentSituation);

            if (pastActions.Count > backpropDepth && pastSituations.Count > backpropDepth)
            {
                pastActions.RemoveAt(0);
                pastSituations.RemoveAt(0);
            }
        }
    }

    public void Hit(Hitbox hitbox)
    {
        //Take a penalty if we were hit
        if(hitbox.owner != AIPlayer)
        {
            //apply rewards and what not if applicable
            if (pastSituations.Count > 0)
            {
                float reward = -0.5f;
                float gamma = 0.9f;
                for (int i = 0; i < pastSituations.Count; i++)
                {
                    AISituation pastSituation = pastSituations[pastSituations.Count - 1 - i];
                    Action pastAction = pastActions[pastSituations.Count - 1 - i];

                    actionSelector.IncreaseWeight(pastSituation, pastAction, Mathf.Pow(gamma, i) * reward);
                }
                print("Penalty");
            }
        }
    }

    public void Block(Hitbox hitbox)
    {
        //Become successful if we blocked correctly
        if (hitbox.owner != AIPlayer)
        {
            //apply rewards and what not if applicable
            if (pastSituations.Count > 0)
            {
                float reward = 1;
                float gamma = 0.9f;
                for (int i = 0; i < pastSituations.Count; i++)
                {
                    AISituation pastSituation = pastSituations[pastSituations.Count - 1 - i];
                    Action pastAction = pastActions[pastSituations.Count - 1 - i];

                    actionSelector.IncreaseWeight(pastSituation, pastAction, Mathf.Pow(gamma, i) * reward);
                }
                print("Reward");
            }
        }
    }
}
