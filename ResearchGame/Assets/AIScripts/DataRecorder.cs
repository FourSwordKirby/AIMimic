using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataRecorder : MonoBehaviour {
    public string playerProfileName;
    public bool enablePlaystyleLabeling;
    public bool currentlyLogging;

    public Player player1;
    public Player player2;

    public Session currentSession;
    List<Session> sessions;

    void Start()
    {
        sessions = new List<Session>(); 
        currentSession = new Session(playerProfileName);

        player1.SetRecorder(this);
        player2.SetRecorder(this);

        if (currentlyLogging)
            player2.sprite.color = Color.yellow;
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

        int pClass = (GameManager.instance.p1 == this.player2)? p1Class : p2Class;

        string strClass = pClass == 1 ? "offense" : "defense";
        foreach (Session session in sessions)
        {
            foreach (GameSnapshot snapshot in session.snapshots)
                snapshot.labels.Add(strClass);
            Debug.Log(session.snapshots.Count);
            session.writeToLog();
        }
    }


    public Action player1Action = Action.Stand;
    public Action player2Action = Action.Stand;
    public float player1StartFrame = GameManager.currentFrame;
    public float player2StartFrame = GameManager.currentFrame;

    public void InterruptAction(bool isPlayer1)
    {
        if (isPlayer1)
        {
            player1Action = Action.Stand;
            player1StartFrame = GameManager.currentFrame;
        }
        else
        {
            player2Action = Action.Stand;
            player2StartFrame = GameManager.currentFrame;
        }
    }

    public void RecordAction(Action actionTaken, bool isPlayer1)
    {
        int initIndex = isPlayer1 ? 0 : 1;

        float p1Duration = GameManager.currentFrame - player1StartFrame;
        float p2Duration = GameManager.currentFrame - player2StartFrame;

        GameSnapshot snapshot = new GameSnapshot(initIndex, GameManager.currentFrame,
                                                player1, player2, 
                                                p1Duration, p2Duration,
                                                player1Action, player2Action);
        currentSession.addSnapshot(snapshot);

        if (!isPlayer1)
            Debug.Log("Action Recorded:" + snapshot.p2Action + " " + snapshot.p2Duration);

        if (isPlayer1)
        {
            player1Action = actionTaken;
            player1StartFrame = GameManager.currentFrame;
        }
        else
        {
            player2Action = actionTaken;
            player2StartFrame = GameManager.currentFrame;
        }
    }

    //Basically used to determine if a state transition occured because of the player or from natural means
    private bool isPlayerTransition(Action startingAction, Action newAction)
    {
        if(startingAction == Action.JumpLeft || startingAction == Action.JumpNeutral || startingAction == Action.JumpRight)
        {

        }
        return true;
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
