using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameRecorder : MonoBehaviour
{
    public static GameRecorder instance;

    public string recordingName;
    public bool currentlyLogging;

    public Player player1;
    public Player player2;

    public List<Snapshot> snapshots = new List<Snapshot>();
    bool roundInProgress = false;

    private int lastCapturedFrame = -1;

    private void Start()
    {
        if (instance != this)
            instance = this;
    }

    /// <summary>
    /// Call this to get a capture of what is currently on the screen
    /// Changing this needs to be manually called by the AI because it's hard to maintain an explicit order of operations
    /// </summary>
    public void CaptureFrame()
    {
        if (!roundInProgress && !GameManager.instance.roundOver)
        {
            roundInProgress = true;
            ClearSession();
        }
        if (GameManager.instance.roundOver)
        {
            roundInProgress = false;
            return;
        }

        //A check to make sure we only capture one snapshot per frame
        if (lastCapturedFrame == GameManager.instance.currentFrame)
            return;
        lastCapturedFrame = GameManager.instance.currentFrame;

        Snapshot snapshot = new Snapshot(GameManager.instance.currentFrame, player1, player2);
        snapshots.Add(snapshot);
    }

    void OnDestroy()
    {
        //Maybe save the replay somewhere?
        //WriteToLog(GameManager.instance.rematchUI.p1Class, GameManager.instance.rematchUI.p2Class);
    }

    public void ClearSession()
    {
        snapshots = new List<Snapshot>();
    }
}