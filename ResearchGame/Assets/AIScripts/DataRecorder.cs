using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataRecorder : MonoBehaviour {
    public string playerProfileName;
    public bool enablePlaystyleLabeling;


    Player controlledPlayer;
    Player opponentPlayer;

    bool returnedToNeutral;
    bool matchOver;

    float previousTime = GameManager.timeRemaining;
    string previousState = "";

    Session session;

    void Start()
    {
        controlledPlayer = GameManager.Players[0];
        opponentPlayer = GameManager.Players[1];
        opponentPlayer.sprite.color = Color.blue;

        returnedToNeutral = true;

        session = new Session(playerProfileName);
    }

	// Update is called once per frame
	void Update () 
    {
        if (GameManager.timeRemaining < 0)
        {
            opponentPlayer.sprite.color = Color.red;
            if(enablePlaystyleLabeling)
            {
                GameManager.instance.RoundText.text = "press o for offensive";
                if (Input.GetKeyDown("o"))
                {
                    matchOver = true;
                }
                if (Input.GetKeyDown("p"))
                {
                    matchOver = true;
                }
            }
            else
            {
                matchOver = true;
            }
        }

        if(matchOver)
        {
            session.writeToLog();
        }

        /*For each button press, make sure that the game has a snap shot of it*/
        RecordAction();
    }

    //TODO: Seperate generating a snapshot from storing it in the session. This helps with code reuse (like witht he Model comparer)
    private void RecordAction()
    {
        string currentState = opponentPlayer.ActionFsm.CurrentState.ToString();

        if (currentState == previousState)
        {
            return;
        }
        previousState = currentState;

        Action actionTaken = Action.Stand;
        float delay = previousTime - GameManager.timeRemaining;
        if (currentState == "AttackState")
        {
            actionTaken = Action.Attack;
            GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer, delay,
                                                    GameManager.timeRemaining, actionTaken);
            session.addSnapshot(snapshot);
            previousTime = GameManager.timeRemaining;
        }
        else if (currentState == "BlockState")
        {
            actionTaken = Action.Block;
            GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer, delay,
                                                    GameManager.timeRemaining, actionTaken);
            session.addSnapshot(snapshot);
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
            session.addSnapshot(snapshot);
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
                session.addSnapshot(snapshot);
                previousTime = GameManager.timeRemaining;
            }

            returnedToNeutral = false;
            return;
        }

        if (!returnedToNeutral)
        {
            actionTaken = Action.Stand;
            GameSnapshot snapshot = new GameSnapshot(controlledPlayer, opponentPlayer, delay,
                                            GameManager.timeRemaining, actionTaken);
            session.addSnapshot(snapshot);
            previousTime = GameManager.timeRemaining;
            returnedToNeutral = true;
        }
    }
}
