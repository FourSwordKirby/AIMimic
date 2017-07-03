using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameRecorder : MonoBehaviour
{
    public string recordingName;
    public bool currentlyLogging;

    public Player player1;
    public Player player2;

    public List<Snapshot> snapshots = new List<Snapshot>();

    bool roundInProgress;
    private void Update()
    {
        if (!roundInProgress && !GameManager.instance.roundOver)
        {
            roundInProgress = true;
            snapshots = new List<Snapshot>();
        }
        if (GameManager.instance.roundOver)
        {
            roundInProgress = false;
            return;
        }

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

    }
}