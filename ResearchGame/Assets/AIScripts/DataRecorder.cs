using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataRecorder : MonoBehaviour {
    Player opponentPlayer;
    Player targetPlayer;

    bool returnedToNeutral;
    bool matchOver;

    void Start()
    {
        opponentPlayer = GameManager.Players[0];
        targetPlayer = GameManager.Players[1];

        returnedToNeutral = true;
    }

	// Update is called once per frame
	void Update () {
        if (GameManager.timeRemaining < 0 && !matchOver)
        {
            KthNearestCollector.writeToLog();
            matchOver = true;
        }

        /*For each button press, make sure that the game has a snap shot of it*/
        Action actionTaken = Action.Idle;
        if (Controls.attackInputDown(targetPlayer))
        {
            actionTaken = Action.Attack;
            GameSnapshot snapshot = new GameSnapshot(opponentPlayer, targetPlayer,
                                                    (int)GameManager.timeRemaining, actionTaken);
            KthNearestCollector.addSnapshot(snapshot);
        }
        else if (Controls.shieldInputDown(targetPlayer))
        {
            actionTaken = Action.Block;
            GameSnapshot snapshot = new GameSnapshot(opponentPlayer, targetPlayer,
                                                    (int)GameManager.timeRemaining, actionTaken);
            KthNearestCollector.addSnapshot(snapshot);
        }
        else if (Controls.jumpInputDown(targetPlayer))
        {
            if (Controls.getDirection(targetPlayer).x == 0)
                actionTaken = Action.JumpNeutral;
            if (Controls.getDirection(targetPlayer).x < 0)
                actionTaken = Action.JumpLeft;
            if (Controls.getDirection(targetPlayer).x > 0)
                actionTaken = Action.JumpRight;
            
            GameSnapshot snapshot = new GameSnapshot(opponentPlayer, targetPlayer,
                                                    (int)GameManager.timeRemaining, actionTaken);
            KthNearestCollector.addSnapshot(snapshot);
        }
        else if (Controls.getDirection(targetPlayer).magnitude > 0)
        {
            if (returnedToNeutral)
            {
                if (Controls.getDirection(targetPlayer).x < 0)
                    actionTaken = Action.WalkLeft;
                if (Controls.getDirection(targetPlayer).x > 0)
                    actionTaken = Action.WalkRight;

                GameSnapshot snapshot = new GameSnapshot(opponentPlayer, targetPlayer,
                                                        (int)GameManager.timeRemaining, actionTaken);
                KthNearestCollector.addSnapshot(snapshot);
            }

            returnedToNeutral = false;
            return;
        }

        returnedToNeutral = true;
    }
}
