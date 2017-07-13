using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This agent will play the game optimally, that is, it will perform the actions which are most likely to let it win the game
//This AI is basically a cleaned up version of the ReinforceAI
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
        currentSituation = new AISituation(currentState, AIPlayer.isPlayer1);

        //apply rewards and what not if applicable
        if (previousState != null && pastSituations.Count > 0)
        {
            float reward = GetReward(previousState, currentState);
            float gamma = 0.9f;
            for (int i = 0; i < pastSituations.Count; i++)
            {
                AISituation pastSituation = pastSituations[pastSituations.Count - 1 - i];
                Action pastAction = pastActions[pastSituations.Count - 1 - i];
                
                actionSelector.IncreaseWeight(pastSituation, pastAction, Mathf.Pow(gamma, i) * reward);
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
        //Prevents the AI from standing or crouching once commiting itself to an attack
        if(AIPlayer.ActionFsm.CurrentState is AttackState)
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
        //If we didn't successfully complete the action, reduce its weight since the AI just failing to do anythin
        //This doesn't actually work because we don't store our state in the scenario lol
        //This also gets messed up bc the player doesn't know what actions it can do during hitstun
        //else
        //{
        //    print("Last action: " + action + "\n" + "Current Weight: " + actionSelector.GetWeight(currentSituation, action));
        //    actionSelector.IncreaseWeight(currentSituation, action, -1);
        //    print("New Last action: " + action + "\n" + "Current Weight: " + actionSelector.GetWeight(currentSituation, action));
        //}
    }

    bool freshReset = false;
    private void Reset()
    {
        print("Storing/Loading action tables");
        if(actionSelector != null)
            actionSelector.StoreTable(Application.streamingAssetsPath + "/ActionTables/" + playerProfileName);
        else
            actionSelector = new AdaptiveActionSelector();

        actionSelector.LoadTable(Application.streamingAssetsPath + "/ActionTables/" + playerProfileName);
        print("Finished Loading action tables");
    }

    private float GetReward(Snapshot previousState, Snapshot currentState)
    {
        if (AIPlayer.isPlayer1)
            return (previousState.p2Health - currentState.p2Health);// + (currentState.p1Health - previousState.p1Health);
        else
            return (previousState.p1Health - currentState.p1Health);// + (currentState.p2Health - previousState.p2Health);
    }
}
