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

    private bool frameCaptured;
    private int lastCapturedFrame = -1;

    private void Start()
    {
        if (instance != this)
            instance = this;
    }

    private void Update()
    {
        //This ensures that the frame is captured at the beginning of the frame. We will never capture frames mid interaction.
        CaptureFrame();
    }

    /// <summary>
    /// Call this to get a capture of what is currently on the screen
    /// Changing this needs to be manually called by the AI because it's hard to maintain an explicit order of operations
    /// </summary>
    ///<returns>Returns the latest frame that was captured</returns>
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
        }

        //A check to make sure we only capture one snapshot per frame
        //if (lastCapturedFrame == GameManager.instance.currentFrame)
        //    return snapshots[snapshots.Count-1];
        lastCapturedFrame = GameManager.instance.currentFrame;

        Snapshot snapshot = new Snapshot(GameManager.instance.currentFrame, player1, player2);
        snapshots.Add(snapshot);

        //Pruning very old states, used for saving memory
        if (snapshots.Count > 100)
            snapshots.RemoveRange(0, 50);

    }

    public Snapshot LatestFrame(int offset = 0)
    {
        int targetIdx = Mathf.Max(0, snapshots.Count - offset - 1);
        if (snapshots.Count > 0)
            return snapshots[targetIdx];
        else
            return null;
    }

    public Snapshot PreviousFrame()
    {
        if(snapshots.Count > 1)
            return snapshots[snapshots.Count - 2];
        else
            return null;
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