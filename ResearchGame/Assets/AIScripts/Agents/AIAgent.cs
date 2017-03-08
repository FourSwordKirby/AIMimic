using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AIAgent : MonoBehaviour{
    public string playerProfileName;
    public Color AIColor;
    public DataRecorder dataRecorder;

    public Player Opponent;
    public Player AIPlayer;
    public Text DebugText;

    void Start()
    {
        Opponent = GameManager.instance.p1;

        AIPlayer = GameManager.instance.p2;
        AIPlayer.AIControlled = true;

        AIPlayer.sprite.color = AIColor;
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
