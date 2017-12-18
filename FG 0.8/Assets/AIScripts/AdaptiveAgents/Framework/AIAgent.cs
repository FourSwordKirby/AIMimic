using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for the AI agent
/// </summary>
public abstract class AIAgent : MonoBehaviour{
    public string playerProfileName;
    public Color AIColor;

    public Player AIPlayer;
    public Player Opponent;

    public GameRecorder gameRecorder;

    public int frameDelay;

    void Start()
    {
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
        return gameRecorder.LatestFrame(frameDelay);
    }

    /// <summary>
    /// Used to reset an AI to it's starting parameters. This may differ depending on the AI. For many simple AI's it does nothing
    /// </summary>
    virtual public void Reset()
    {
        ;
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
