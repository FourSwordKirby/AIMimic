using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdviceTrainer:MonoBehaviour {
    public Player AIPlayer;
    public Player Opponent;

    public GameRecorder gameRecorder;
    public int frameDelay;

    Snapshot currentState = null;
    //Dictionary<Advice, float> adviceWeights = new Dictionary<Advice, float>() { { new Advice(Action.StandBlock, Result.Block), 0},
    //                                                                            { new Advice(Action.JumpLeft, Result.Dodged), 0} };
    AdviceLookupTable adviceTable = new AdviceLookupTable();

    bool actionTaken;

    public Snapshot GetGameState()
    {
        int sessionLength = gameRecorder.snapshots.Count;
        int snapshotIndex = Mathf.Max(0, sessionLength - frameDelay - 1);
        return gameRecorder.snapshots[snapshotIndex];
    }

    public void Retest()
    {
        actionTaken = false;
    }

    public Advice RetrieveAdvice()
    {
        return adviceTable.PickAdvice(new AISituation(currentState));
    }

    public void TestAdvice(Advice advice)
    {
        GameRecorder.instance.CaptureFrame();
        currentState = GetGameState();

        if(!actionTaken)
        {
            Action action = advice.recommendedAction;

            //Performing the action
            if (AIPlayer.ActionFsm.CurrentState is AttackState)
            {
                if (action == Action.Stand || action == Action.Crouch)
                    return;
            }

            bool actionSucceeded = AIPlayer.PerformAction(action);
            if (actionSucceeded)
                actionTaken = true;
        }
    }

    public void EvaluateResults(Advice advice, List<Result> results)
    {
        bool successful = results.Contains(advice.purportedResult);
        adviceTable.UpdateAdvice(new AISituation(currentState), advice, successful);
        print(adviceTable.GetWeight(new AISituation(currentState), advice));
    }
}
