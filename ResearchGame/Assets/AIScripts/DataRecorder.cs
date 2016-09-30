using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataRecorder : MonoBehaviour {
    Player controlledPlayer;
    Player opponentPlayer;

    bool returnedToNeutral;
    bool matchOver;

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
        Action actionTaken = Action.Idle;
        if (Controls.attackInputDown(opponentPlayer))
        {
            actionTaken = Action.Attack;
            GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer,
                                                    (int)GameManager.timeRemaining, actionTaken);
            KthNearestCollector.addSnapshot(snapshot);
        }
        else if (Controls.shieldInputDown(opponentPlayer))
        {
            actionTaken = Action.Block;
            GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer,
                                                    (int)GameManager.timeRemaining, actionTaken);
            KthNearestCollector.addSnapshot(snapshot);
        }
        else if (Controls.jumpInputDown(opponentPlayer))
        {
            if (Controls.getDirection(opponentPlayer).x == 0)
                actionTaken = Action.JumpNeutral;
            if (Controls.getDirection(opponentPlayer).x < 0)
                actionTaken = Action.JumpLeft;
            if (Controls.getDirection(opponentPlayer).x > 0)
                actionTaken = Action.JumpRight;
            
            GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer,
                                                    (int)GameManager.timeRemaining, actionTaken);
            KthNearestCollector.addSnapshot(snapshot);
        }
        else if (Controls.getDirection(opponentPlayer).magnitude > 0)
        {
            if (returnedToNeutral || opponentPlayer.ActionFsm.CurrentState.GetType() == typeof(IdleState))
            {
                if (Controls.getDirection(opponentPlayer).x < 0)
                    actionTaken = Action.WalkLeft;
                if (Controls.getDirection(opponentPlayer).x > 0)
                    actionTaken = Action.WalkRight;
                Debug.Log("added snapshot");
                Debug.Log(KthNearestCollector.snapshots.Count);
                GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer,
                                                        (int)GameManager.timeRemaining, actionTaken);
                KthNearestCollector.addSnapshot(snapshot);
            }

            returnedToNeutral = false;
            return;
        }

        if(!returnedToNeutral)
        {
            actionTaken = Action.Idle;
            GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer,
                                            (int)GameManager.timeRemaining, actionTaken);
            returnedToNeutral = true;
        }
    }
}
