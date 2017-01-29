﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataRecorder : MonoBehaviour {
    public string playerProfileName;
    public bool enablePlaystyleLabeling;

    public Player recordedPlayer;
    public Player opponentPlayer;

    bool returnedToNeutral;
    bool matchOver;

    float previousTime = GameManager.currentFrame;
    string previousState = "";

    List<Session> sessions;
    Session currentSession;

    void Start()
    {
        returnedToNeutral = true;

        sessions = new List<Session>(); 
        currentSession = new Session(playerProfileName);
    }

	// Update is called once per frame
	void Update () 
    {
        /*For each button press, make sure that the game has a snap shot of it*/
        if (!GameManager.instance.roundOver)
        {
            //opponentPlayer.sprite.color = 0.5f * Color.red + 0.5f * Color.black;
            RecordAction();
        }
        else if(currentSession.snapshots.Count > 0)
        {
            sessions.Add(currentSession);
            currentSession = new Session(playerProfileName);
        }
    }

    public void writeToLog(int p1Class, int p2Class)
    {
        int pClass = (GameManager.instance.p1 == this.recordedPlayer)? p1Class : p2Class;

        string strClass = pClass == 1 ? "offense" : "defense";
        foreach (Session session in sessions)
        {
            foreach (GameSnapshot snapshot in session.snapshots)
                snapshot.labels.Add(strClass);
            Debug.Log(session.snapshots.Count);
            session.writeToLog();
        }
    }

    //TODO: Seperate generating a snapshot from storing it in the session. This helps with code reuse (like witht he Model comparer)
    private void RecordAction()
    {
        string currentState = recordedPlayer.ActionFsm.CurrentState.ToString();

        if (currentState == previousState)
        {
            return;
        }
        previousState = currentState;

        Action actionTaken = Action.Stand;
        float delay = previousTime - GameManager.currentFrame;
        if (currentState == "AttackState")
        {
            actionTaken = Action.Attack;
            GameSnapshot snapshot = new GameSnapshot(opponentPlayer, recordedPlayer, delay,
                                                    GameManager.currentFrame, actionTaken);
            currentSession.addSnapshot(snapshot);
            previousTime = GameManager.currentFrame;
        }
        else if (currentState == "BlockState")
        {
            actionTaken = Action.Block;
            GameSnapshot snapshot = new GameSnapshot(opponentPlayer, recordedPlayer, delay,
                                                    GameManager.currentFrame, actionTaken);
            currentSession.addSnapshot(snapshot);
            previousTime = GameManager.currentFrame;
        }
        else if (currentState == "JumpState")
        {
            if (((JumpState)recordedPlayer.ActionFsm.CurrentState).jumpDir == 0)
                actionTaken = Action.JumpNeutral;
            if (((JumpState)recordedPlayer.ActionFsm.CurrentState).jumpDir < 0)
                actionTaken = Action.JumpLeft;
            if (((JumpState)recordedPlayer.ActionFsm.CurrentState).jumpDir > 0)
                actionTaken = Action.JumpRight;

            GameSnapshot snapshot = new GameSnapshot(opponentPlayer, recordedPlayer, delay,
                                                    GameManager.currentFrame, actionTaken);
            currentSession.addSnapshot(snapshot);
            previousTime = GameManager.currentFrame;
        }
        else if (currentState == "MovementState")
        {
            if (returnedToNeutral)
            {
                if (((MovementState)recordedPlayer.ActionFsm.CurrentState).moveDir < 0)
                    actionTaken = Action.WalkLeft;
                if (((MovementState)recordedPlayer.ActionFsm.CurrentState).moveDir > 0)
                    actionTaken = Action.WalkRight;
                GameSnapshot snapshot = new GameSnapshot(opponentPlayer, recordedPlayer, delay,
                                                        GameManager.currentFrame, actionTaken);
                currentSession.addSnapshot(snapshot);
                previousTime = GameManager.currentFrame;
            }

            returnedToNeutral = false;
            return;
        }

        if (!returnedToNeutral)
        {
            actionTaken = Action.Stand;
            GameSnapshot snapshot = new GameSnapshot(opponentPlayer, recordedPlayer, delay,
                                            GameManager.currentFrame, actionTaken);
            currentSession.addSnapshot(snapshot);
            previousTime = GameManager.currentFrame;
            returnedToNeutral = true;
        }
    }
}
