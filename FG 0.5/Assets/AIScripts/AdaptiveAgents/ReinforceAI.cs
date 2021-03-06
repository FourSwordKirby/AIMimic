﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An AI agent which chooses moves based on reinforcement learning
/// </summary>
public class ReinforceAI : AIAgent{
    public int backpropDepth = 3;
    private List<Snapshot> priorSnapshots;

    //Implementation of a generic RL AI
    private AdaptiveActionSelector actionSelector = new AdaptiveActionSelector();

    int frameInterval = 5;

    Snapshot currentState = null;
    AISituation currentSituation = null;
    Snapshot pastState = null;
    List<AISituation> pastSituations = new List<AISituation>();
    List<Action> pastActions = new List<Action>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            //actionSelector.loadTable(Application.streamingAssetsPath + "/ActionTables/TestTable");
            actionSelector.StoreTable(Application.streamingAssetsPath + "/ActionTables/TestTable");

        if (!AIPlayer.enabled)
            return;

        GameRecorder.instance.LatestFrame();
        if (GameManager.instance.currentFrame % frameInterval == 0)
        {
            ObserveState();
            Action action = GetAction();
            PerformAction(action);
        }
    }

    public override void ObserveState()
    {
        //Get the current game snapshot
        currentState = GetGameState();
        currentSituation = new AISituation(currentState);
        if (currentState == null)
            return;

        //Get the reward if applicable
        if (pastState != null && pastSituations.Count() > 0)
        {
            float reward = GetReward(pastState, currentState);
            float gamma = 0.9f;
            for (int i = 0; i < pastSituations.Count(); i++)
            {
                AISituation pastSituation = pastSituations[pastSituations.Count() - 1 - i];
                Action pastAction = pastActions[pastSituations.Count() - 1 - i];

                //Hacky fix to prevent the agent from crashing if it's in an unfamiliar situation
                //Should really make the AI have a handle on some kind of strategy for all situations
                actionSelector.IncreaseWeight(pastSituation, pastAction, Mathf.Pow(gamma, i) * reward);
                //DebugText.text = "Last action: " + pastAction + "\n" + "Current Weight: " + actionSelector.GetWeight(pastSituation, pastAction);
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
        bool actionSucceeded = AIPlayer.PerformAction(action);

        //If we successfully did the action, update the past action and past situation
        if (actionSucceeded)
        {
            pastActions.Add(action);
            pastSituations.Add(currentSituation);

            if (pastActions.Count > backpropDepth && pastSituations.Count > backpropDepth)
            {
                pastActions.RemoveAt(0);
                pastSituations.RemoveAt(0);
            }
        }
        pastState = currentState;
    }


    private float GetReward(Snapshot pastState, Snapshot currentState)
    {
        return (pastState.p1Health - currentState.p1Health) + (currentState.p2Health - pastState.p2Health);
    }
}
