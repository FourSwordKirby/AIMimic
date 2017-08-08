using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class Session {
    //Future
    public RoundMetadata roundMetaData;
    public List<GameEvent> snapshots;

    private string playerProfileName;
    private string directoryPath;
    private DirectoryInfo playerDir;
    private string filePath;

    public Session(string playerProfileName)
    {
        this.playerProfileName = playerProfileName;
        roundMetaData = new RoundMetadata(0, 0);
        snapshots = new List<GameEvent>();

        directoryPath = Application.streamingAssetsPath + "/PlayerLogs/" + this.playerProfileName + "/";
        Directory.CreateDirectory(directoryPath);

        playerDir = new DirectoryInfo(directoryPath);
    }

    public void AddSnapshot(GameEvent snapshot)
    {
        snapshots.Add(snapshot);
    }

    public void WriteToLog()
    {
        filePath = directoryPath + "Log_" + playerDir.GetFiles().Where(x => !x.Name.EndsWith(".meta")).Count() + ".txt";

        string datalog = "";//"Metadata";

        for (int i = 0; i < snapshots.Count; i++)
        {
            datalog += JsonUtility.ToJson(snapshots[i], true);
            if(i != snapshots.Count-1)
                datalog += "\n~~~~\n";
        }
        File.WriteAllText(filePath, datalog);
    }

    public static List<List<GameEvent>> RetrievePlayerHistory(string playerProfileName)
    {
        string directoryPath = Application.streamingAssetsPath + "/PlayerLogs/" + playerProfileName + "/";
        Directory.CreateDirectory(directoryPath);

        DirectoryInfo playerDir = new DirectoryInfo(directoryPath);

        List<List<GameEvent>> playerHistory = new List<List<GameEvent>>();
        int logCount = playerDir.GetFiles().Where(x => !x.Name.EndsWith(".meta")).Count();
        for (int i = 0; i < logCount; i++)
        {
            playerHistory.Add(RetrievePlayerSession(playerProfileName, i));
        }
        return playerHistory;
    }

    //If sessionNumber is -1, we retrieve the latest recorded session
    public static List<GameEvent> RetrievePlayerSession(string playerProfileName, int sessionNumber = -1)
    {
        string directoryPath = Application.streamingAssetsPath + "/PlayerLogs/" + playerProfileName + "/";
        DirectoryInfo playerDir = new DirectoryInfo(directoryPath);
        if(sessionNumber == -1)
            sessionNumber  = playerDir.GetFiles().Where(x => !x.Name.EndsWith(".meta")).Count()-1;

        string filePath = Application.streamingAssetsPath + "/PlayerLogs/" + playerProfileName + "/" + "Log_" + sessionNumber + ".txt";

        //deserialize
        string contents = File.ReadAllText(filePath);
        string[] serializeObjects = contents.Split(new string[]{"~~~~"} , StringSplitOptions.RemoveEmptyEntries);
        List<GameEvent> mySnapshots = new List<GameEvent>();
        for (int i = 0; i < serializeObjects.Length; i++)
        {
            mySnapshots.Add(JsonUtility.FromJson<GameEvent>(serializeObjects[i]));
        }
        return mySnapshots;
    }
}
