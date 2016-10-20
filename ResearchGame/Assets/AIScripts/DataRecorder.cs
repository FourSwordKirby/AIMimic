using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataRecorder : MonoBehaviour {
    Player controlledPlayer;
    Player opponentPlayer;

    bool returnedToNeutral;
    bool matchOver;

    float previousTime = GameManager.timeRemaining;
    string previousState = "";

    void Start()
    {
        controlledPlayer = GameManager.Players[0];
        opponentPlayer = GameManager.Players[1];
        opponentPlayer.sprite.color = Color.blue;

        returnedToNeutral = true;
    }

	// Update is called once per frame
    //TODO: make this actually data record the results of the input
	void Update () {
        if (GameManager.timeRemaining < 0 && !matchOver)
        {
            opponentPlayer.sprite.color = Color.red;
            KthNearestCollector.writeToLog();
            matchOver = true;
        }

        /*For each button press, make sure that the game has a snap shot of it*/
        RecordAction();
    }

    private void RecordAction()
    {
        string currentState = opponentPlayer.ActionFsm.CurrentState.ToString();

        if (currentState == previousState)
        {
            return;
        }
        previousState = currentState;

        Action actionTaken = Action.Idle;
        float delay = previousTime - GameManager.timeRemaining;
        if (currentState == "AttackState")
        {
            actionTaken = Action.Attack;
            GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer, delay,
                                                    GameManager.timeRemaining, actionTaken);
            KthNearestCollector.addSnapshot(snapshot);
            previousTime = GameManager.timeRemaining;
        }
        else if (currentState == "BlockState")
        {
            actionTaken = Action.Block;
            GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer, delay,
                                                    GameManager.timeRemaining, actionTaken);
            KthNearestCollector.addSnapshot(snapshot);
            previousTime = GameManager.timeRemaining;
        }
        else if (currentState == "JumpState")
        {
            if (((JumpState)opponentPlayer.ActionFsm.CurrentState).jumpDir == 0)
                actionTaken = Action.JumpNeutral;
            if (((JumpState)opponentPlayer.ActionFsm.CurrentState).jumpDir < 0)
                actionTaken = Action.JumpLeft;
            if (((JumpState)opponentPlayer.ActionFsm.CurrentState).jumpDir > 0)
                actionTaken = Action.JumpRight;

            GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer, delay,
                                                    GameManager.timeRemaining, actionTaken);
            KthNearestCollector.addSnapshot(snapshot);
            previousTime = GameManager.timeRemaining;
        }
        else if (currentState == "MovementState")
        {
            if (returnedToNeutral)
            {
                if (((MovementState)opponentPlayer.ActionFsm.CurrentState).moveDir < 0)
                    actionTaken = Action.WalkLeft;
                if (((MovementState)opponentPlayer.ActionFsm.CurrentState).moveDir > 0)
                    actionTaken = Action.WalkRight;
                GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer, delay,
                                                        GameManager.timeRemaining, actionTaken);
                KthNearestCollector.addSnapshot(snapshot);
                previousTime = GameManager.timeRemaining;
            }

            returnedToNeutral = false;
            return;
        }

        if (!returnedToNeutral)
        {
            actionTaken = Action.Idle;
            GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer, delay,
                                            GameManager.timeRemaining, actionTaken);
            KthNearestCollector.addSnapshot(snapshot);
            previousTime = GameManager.timeRemaining;
            returnedToNeutral = true;
        }
    }
}
