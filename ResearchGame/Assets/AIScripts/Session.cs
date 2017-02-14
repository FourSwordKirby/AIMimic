﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class Session {
    public RoundMetadata roundMetaData;
    public List<GameSnapshot> snapshots;

    private string playerProfileName;
    private string directoryPath;
    private DirectoryInfo playerDir;
    private string filePath;

    public Session(string playerProfileName, int p1Wins, int p2Wins)
    {
        this.playerProfileName = playerProfileName;
        roundMetaData = new RoundMetadata(p1Wins, p2Wins);
        snapshots = new List<GameSnapshot>();

        directoryPath = Application.streamingAssetsPath + "/PlayerLogs/" + this.playerProfileName + "/";
        Directory.CreateDirectory(directoryPath);

        playerDir = new DirectoryInfo(directoryPath);
    }

    public void addSnapshot(GameSnapshot snapshot)
    {
        snapshots.Add(snapshot);
    }

    public void writeToLog()
    {
        filePath = directoryPath + "Log_" + playerDir.GetFiles().Where(x => !x.Name.EndsWith(".meta")).Count() + ".txt";

        string datalog = "Metadata";

        for (int i = 0; i < snapshots.Count; i++)
        {
            datalog += JsonUtility.ToJson(snapshots[i], true);
            if(i != snapshots.Count-1)
                datalog += "\n~~~~\n";
        }
        File.WriteAllText(filePath, datalog);
        Debug.Log("wrote to log");
    }

    public static List<List<GameSnapshot>> RetrievePlayerHistory(string playerProfileName)
    {
        string directoryPath = Application.streamingAssetsPath + "/PlayerLogs/" + playerProfileName + "/";
        Directory.CreateDirectory(directoryPath);

        DirectoryInfo playerDir = new DirectoryInfo(directoryPath);

        List<List<GameSnapshot>> playerHistory = new List<List<GameSnapshot>>();
        int logCount = playerDir.GetFiles().Where(x => !x.Name.EndsWith(".meta")).Count();
        for (int i = 0; i < logCount; i++)
        {
            playerHistory.Add(RetrievePlayerSession(playerProfileName, i));
        }
        return playerHistory;
    }

    //If sessionNumber is -1, we retrieve the latest recorded session
    public static List<GameSnapshot> RetrievePlayerSession(string playerProfileName, int sessionNumber = -1)
    {
        string directoryPath = Application.streamingAssetsPath + "/PlayerLogs/" + playerProfileName + "/";
        DirectoryInfo playerDir = new DirectoryInfo(directoryPath);
        if(sessionNumber == -1)
            sessionNumber  = playerDir.GetFiles().Where(x => !x.Name.EndsWith(".meta")).Count()-1;

        string filePath = Application.streamingAssetsPath + "/PlayerLogs/" + playerProfileName + "/" + "Log_" + sessionNumber + ".txt";

        //deserialize
        string contents = File.ReadAllText(filePath);
        string[] serializeObjects = contents.Split(new string[]{"~~~~"} , StringSplitOptions.RemoveEmptyEntries);
        List<GameSnapshot> mySnapshots = new List<GameSnapshot>();
        for (int i = 0; i < serializeObjects.Length; i++)
        {
            mySnapshots.Add(JsonUtility.FromJson<GameSnapshot>(serializeObjects[i]));
        }
        return mySnapshots;
    }
}
