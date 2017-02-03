using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataRecorder : MonoBehaviour {
    public string playerProfileName;
    public bool enablePlaystyleLabeling;
    public bool currentlyLogging;

    public Player opponentPlayer;
    public Player recordedPlayer;

    public Action latestOpponentAction = Action.Idle;
    public Action latestPlayerAction = Action.Idle;

    float previousTime = GameManager.currentFrame;

    public Session currentSession;
    List<Session> sessions;

    void Start()
    {
        sessions = new List<Session>(); 
        currentSession = new Session(playerProfileName);

        opponentPlayer.SetRecorder(this, true);
        recordedPlayer.SetRecorder(this, false);

        if (currentlyLogging)
            recordedPlayer.sprite.color = Color.yellow;
    }

    public void LogSession()
    {
        if (currentSession.snapshots.Count > 0)
            sessions.Add(currentSession);
        ClearSession();
    }

    public void ClearSession()
    {
        currentSession = new Session(playerProfileName);
    }

    public void WriteToLog(int p1Class, int p2Class)
    {
        //Catchall for any incomplete session that we haven't logged
        LogSession();

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

    public void RecordAction(Action actionTaken, bool isOpponent)
    {
        if (isOpponent)
            latestOpponentAction = actionTaken;
        else
            latestPlayerAction = actionTaken;

        int initIndex = isOpponent ? 0 : 1;

        float delay = previousTime - GameManager.currentFrame;
        GameSnapshot snapshot = new GameSnapshot(initIndex, opponentPlayer, recordedPlayer, delay,
                                                GameManager.currentFrame, latestOpponentAction, latestPlayerAction);
        currentSession.addSnapshot(snapshot);
        previousTime = GameManager.currentFrame;
    }



    /*Code that was used when we were trying to record the player's action by just observing it's state. Obsolete now.
    //TODO: Seperate generating a snapshot from storing it in the session. This helps with code reuse (like witht he Model comparer)
    private void RecordAction()
    {
        string currentState = recordedPlayer.ActionFsm.CurrentState.ToString();
        bool isCrouching = recordedPlayer.isCrouching;

        if (currentState == "SuspendState" || currentState == "HitState" || (currentState == previousState && wasCrouching == isCrouching))
        {
            return;
        }

        previousState = currentState;

        Action actionTaken = Action.Stand;
        float delay = previousTime - GameManager.currentFrame;

        if(wasCrouching != isCrouching)
        {
            if (isCrouching)
                actionTaken = Action.Crouch;
            else
                actionTaken = Action.Stand;
            wasCrouching = isCrouching;
        }
        else if (currentState == "AttackState")
        {
            actionTaken = Action.Attack;
        }
        else if (currentState == "AirAttackState")
        {
            actionTaken = Action.AirAttack;
        }
        else if (currentState == "BlockState")
        {
            actionTaken = Action.Block;
        }
        else if (currentState == "JumpState")
        {
            if (((JumpState)recordedPlayer.ActionFsm.CurrentState).jumpDir == 0)
                actionTaken = Action.JumpNeutral;
            if (((JumpState)recordedPlayer.ActionFsm.CurrentState).jumpDir < 0)
                actionTaken = Action.JumpLeft;
            if (((JumpState)recordedPlayer.ActionFsm.CurrentState).jumpDir > 0)
                actionTaken = Action.JumpRight;
        }
        else if (currentState == "MovementState")
        {
            if (returnedToNeutral)
            {
                if (((MovementState)recordedPlayer.ActionFsm.CurrentState).moveDir < 0)
                    actionTaken = Action.WalkLeft;
                if (((MovementState)recordedPlayer.ActionFsm.CurrentState).moveDir > 0)
                    actionTaken = Action.WalkRight;
            }
        }
        else if (currentState == "IdleState" && !returnedToNeutral)
        {
            actionTaken = Action.Stand;
        }

        GameSnapshot snapshot = new GameSnapshot(opponentPlayer, recordedPlayer, delay,
                                GameManager.currentFrame, actionTaken);
        currentSession.addSnapshot(snapshot);
        previousTime = GameManager.currentFrame;
    }
    */
}
