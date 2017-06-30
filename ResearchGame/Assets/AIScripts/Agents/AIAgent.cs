using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AIAgent : MonoBehaviour{
    public string playerProfileName;
    public Color AIColor;

    public Player AIPlayer;
    public Player Opponent;

    public GameRecorder gameRecorder;
    public Text DebugText;

    public int frameDelay;

    void Start()
    {
        Opponent = GameManager.instance.p1;

        AIPlayer = GameManager.instance.p2;
        AIPlayer.AIControlled = true;

        AIPlayer.sprite.color = AIColor;
    }

    public Snapshot GetGameState()
    {
        //float p1Duration = GameManager.currentFrame - dataRecorder.player1StartFrame;
        //float p2Duration = GameManager.currentFrame - dataRecorder.player2StartFrame;


        //GameSnapshot snapshot = new GameSnapshot(0, GameManager.currentFrame,
        //                                        GameManager.instance.player1, player2,
        //                                        p1Duration, p2Duration,
        //                                        player1Action, player2Action,
        //                                        p1Interrupt, p2Interrupt);

        int sessionLength = gameRecorder.snapshots.Count;
        int snapshotIndex = Mathf.Max(0, sessionLength - frameDelay - 1);
        return gameRecorder.snapshots[snapshotIndex];
    }

    /// <summary>
    /// This should be callsed whenever the agent should be updated with whatever is going on in the game's state
    /// </summary>
    public abstract void ObserveState();

    /// <summary>
    /// Call this to get an action to perform
    /// </summary>
    public abstract Action GetAction();

    /// <summary>
    /// Call this do to the action in question. Important as different AI will have different update when performing an action
    /// </summary>
    public abstract void PerformAction(Action action);
}
