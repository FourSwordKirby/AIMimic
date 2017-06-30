using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EventRecorder : MonoBehaviour {
    public string recordingName;
    public bool enablePlaystyleLabeling;
    public bool currentlyLogging;

    public Player player1;
    public Player player2;

    public Session currentSession;
    List<Session> sessions;

    void Start()
    {
        sessions = new List<Session>(); 
        currentSession = new Session(recordingName);

        player1.SetRecorder(this);
        player2.SetRecorder(this);

        if (currentlyLogging)
        {
            player2.sprite.color = Color.yellow;
        }
    }


    bool roundInProgress;
    private void Update()
    {
        if(roundInProgress && GameManager.instance.roundOver)
        {
            roundInProgress = false;
            //At the end of a round, we log that round in the data recorder
            if (currentlyLogging)
                LogSession();
        }
        if (!GameManager.instance.roundOver)
            roundInProgress = true;
    }

    public void LogSession()
    {
        if (currentSession.snapshots.Count > 0)
            sessions.Add(currentSession);
        ClearSession();
    }

    public void ClearSession()
    {
        currentSession = new Session(recordingName);
        player1Action = Action.Stand;
        player2Action = Action.Stand;
        player1StartFrame = 0;
        player2StartFrame = 0;
    }

    void OnDestroy()
    {
        WriteToLog(GameManager.instance.rematchUI.p1Class, GameManager.instance.rematchUI.p2Class);
    }

    public void WriteToLog(int p1Class, int p2Class)
    {
        //Catchall for any incomplete session that we haven't logged
        LogSession();

        int pClass = (GameManager.instance.p1 == this.player2)? p1Class : p2Class;

        string strClass = pClass == 1 ? "offense" : "defense";
        foreach (Session session in sessions)
        {
            foreach (GameEvent snapshot in session.snapshots)
                snapshot.labels.Add(strClass);
            Debug.Log(session.snapshots.Count);
            session.WriteToLog();
        }
    }


    public Action player1Action = Action.Stand;
    public Action player2Action = Action.Stand;
    public float player1StartFrame = 0;
    public float player2StartFrame = 0;

    public void InterruptAction(bool isPlayer1)
    {
        int initIndex = isPlayer1 ? 0 : 1;

        float p1Duration = GameManager.instance.currentFrame - player1StartFrame;
        float p2Duration = GameManager.instance.currentFrame - player2StartFrame;

        bool p1Interrupt = isPlayer1;
        bool p2Interrupt = !isPlayer1;

        GameEvent snapshot = new GameEvent(initIndex, GameManager.instance.currentFrame,
                                                player1, player2,
                                                p1Duration, p2Duration,
                                                player1Action, player2Action,
                                                p1Interrupt, p2Interrupt);
        currentSession.AddSnapshot(snapshot);

        if (!isPlayer1)
            Debug.Log("Action Recorded:" + snapshot.p2Action + " " + snapshot.p2Duration);
    }

    public void ResumeRecording(bool isPlayer1, bool isCrouching, bool isBlocking)
    {
        if (isPlayer1)
        {
            player1StartFrame = GameManager.instance.currentFrame;
            if (isBlocking)
            {
                if (!isCrouching)
                    player1Action = Action.StandBlock;
                else
                    player1Action = Action.CrouchBlock;
            }
            else
            {
                if (!isCrouching)
                    player1Action = Action.Stand;
                else
                    player1Action = Action.Crouch;
            }
        }
        else
        {
            player2StartFrame = GameManager.instance.currentFrame;
            if (isBlocking)
            {
                if (!isCrouching)
                    player2Action = Action.StandBlock;
                else
                    player2Action = Action.CrouchBlock;
            }
            else
            {
                if (!isCrouching)
                    player2Action = Action.Stand;
                else
                    player2Action = Action.Crouch;
            }
        }
    }

    public void RecordAction(Action actionTaken, bool isPlayer1)
    {
        int initIndex = isPlayer1 ? 0 : 1;

        float p1Duration = GameManager.instance.currentFrame - player1StartFrame;
        float p2Duration = GameManager.instance.currentFrame - player2StartFrame;

        GameEvent snapshot = new GameEvent(initIndex, GameManager.instance.currentFrame,
                                                player1, player2, 
                                                p1Duration, p2Duration,
                                                player1Action, player2Action);
        currentSession.AddSnapshot(snapshot);

        if (isPlayer1)
        {
            player1Action = actionTaken;
            player1StartFrame = GameManager.instance.currentFrame;
        }
        else
        {
            player2Action = actionTaken;
            player2StartFrame = GameManager.instance.currentFrame;
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