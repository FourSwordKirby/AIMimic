using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This agent will play the game optimally, that is, it will perform the actions which are most likely to let it
//win the game
//This AI is basically a cleaned up version of the backprop AI
public class OptimalAI : AIAgent {

    public int backpropDepth = 3;

    private AdaptiveActionSelector actionSelector = new AdaptiveActionSelector();

    Snapshot currentState = null;
    Snapshot previousState = null;

    AISituation currentSituation = null;
    List<AISituation> pastSituations = new List<AISituation>();
    List<Action> pastActions = new List<Action>();

    void Start()
    {
        //Reset();
    }

    void Update()
    {
        if (GameManager.instance.roundOver)
        {
            //Reset();
            //return;
        }

        if (Input.GetKeyDown(KeyCode.N))
            actionSelector.StoreTable(Application.streamingAssetsPath + "/ActionTables/OptimalTable");

        if (GameManager.instance.currentFrame % 3 == 0)
        {
            ObserveState();
            Action action = GetAction();
            PerformAction(action);
        }
    }

    public override Action GetAction()
    {
        Action action = actionSelector.GetAction(currentSituation);
        print(action);
        return action;
    }

    public override void ObserveState()
    {
        //Get the current game snapshot
        currentState = GetGameState();
        currentSituation = new AISituation(currentState);
        if (currentState == null)
            return;
    }

    public override void PerformAction(Action action)
    {
        AIPlayer.PerformAction(action);

        //If we successfully did the action, update the past action and past situation
        //if (actionSucceeded)
        //{
        //    pastActions.Add(action);
        //    pastSituations.Add(currentSituation);

        //    if (pastActions.Count > backpropDepth && pastSituations.Count > backpropDepth)
        //    {
        //        pastActions.RemoveAt(0);
        //        pastSituations.RemoveAt(0);
        //    }
        //}
        //pastState = currentState;
    }

    private void Reset()
    {
        actionSelector.StoreTable(Application.streamingAssetsPath + "/ActionTables/OptimalTable");
        actionSelector.LoadTable(Application.streamingAssetsPath + "/ActionTables/OptimalTable");
    }
}
