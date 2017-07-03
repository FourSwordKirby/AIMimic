using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This agent will play the game optimally, that is, it will perform the actions which are most likely to let it win the game
//This AI is basically a cleaned up version of the backprop AI
public class OptimalAI : AIAgent {

    public int backpropDepth = 3;

    private AdaptiveActionSelector actionSelector = null;
    Snapshot currentState = null;
    Snapshot previousState = null;

    AISituation currentSituation = null;
    List<AISituation> pastSituations = new List<AISituation>();
    List<Action> pastActions = new List<Action>();

    public Text DebugText;

    void LateUpdate()
    {
        if (!freshReset && GameManager.instance.roundOver)
        {
            Reset();
            freshReset = true;
            return;
        }
        else
        {
            freshReset = GameManager.instance.roundOver;
        }

        if (!AIPlayer.enabled || GameManager.instance.roundOver)
            return;

        if (GameManager.instance.currentFrame % 3 == 1)
        {
            ObserveState();
            Action action = GetAction();
            PerformAction(action);
        }
    }

    public override void ObserveState()
    {
        previousState = currentState;
        currentState = GetGameState();
        currentSituation = new AISituation(currentState);

        //apply rewards and what not if applicable
        if (previousState != null && pastSituations.Count > 0)
        {
            float reward = GetReward(previousState, currentState);
            float gamma = 0.9f;
            for (int i = 0; i < pastSituations.Count; i++)
            {
                AISituation pastSituation = pastSituations[pastSituations.Count - 1 - i];
                Action pastAction = pastActions[pastSituations.Count - 1 - i];

                //Hacky fix to prevent the agent from crashing if it's in an unfamiliar situation
                //Should really make the AI have a handle on some kind of strategy for all situations
                actionSelector.IncreaseWeight(pastSituation, pastAction, Mathf.Pow(gamma, i) * reward);
                DebugText.text = "Last action: " + pastAction + "\n" + "Current Weight: " + actionSelector.GetWeight(pastSituation, pastAction);
            }
        }
    }

    public override Action GetAction()
    {
        Action action = actionSelector.GetAction(currentSituation);
        return action;
    }

    public override void PerformAction(Action action)
    {
        //Edge case which is not covered by the base system due to how we're tracking player actions
        if(AIPlayer.ActionFsm.CurrentState is AttackState)
        {
            if (action == Action.Stand || action == Action.Crouch)
                return;
        }

        bool actionSucceeded = AIPlayer.PerformAction(action);

        //If we successfully did the action, update the past action and past situation
        if (actionSucceeded)
        {
            print(action);

            pastActions.Add(action);
            pastSituations.Add(currentSituation);

            if (pastActions.Count > backpropDepth && pastSituations.Count > backpropDepth)
            {
                pastActions.RemoveAt(0);
                pastSituations.RemoveAt(0);
            }
        }
    }

    bool freshReset = false;
    private void Reset()
    {
        if(actionSelector != null)
            actionSelector.StoreTable(Application.streamingAssetsPath + "/ActionTables/" + playerProfileName);
        else
            actionSelector = new AdaptiveActionSelector();

        actionSelector.LoadTable(Application.streamingAssetsPath + "/ActionTables/" + playerProfileName);
    }

    private float GetReward(Snapshot previousState, Snapshot currentState)
    {
        return (previousState.p1Health - currentState.p1Health) + (currentState.p2Health - previousState.p2Health);
    }
}
