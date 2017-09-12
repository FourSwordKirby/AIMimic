using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// This will be used to denote a string of actions that occurs commonly.For example executing a combo etc.
/// We hope to be able to derive action sequences from the data we've collected
/// </summary>
public class ActionSequence
{
    //A tuple of the action and the frame that action was executed.
    private List<GameEvent> actionSequence;
    private int actionIndex;

    public ActionSequence()
    {
        actionSequence = new List<GameEvent>();//{ new KeyValuePair<Action, int>(Action.WalkLeft, 10),
                                                              //    new KeyValuePair<Action, int>(Action.JumpNeutral, 30),};
    }

    public void AddAction(Action a, int frame)
    {
        Debug.Log(a + frame);
        GameEvent gameEvent = new GameEvent(0, frame,
                                        GameManager.instance.p1, GameManager.instance.p1,
                                        0, 0,
                                        a, a);

        actionSequence.Add(gameEvent);
        Debug.Assert(actionSequence.Count == 1 || frame > actionSequence[actionSequence.Count - 2].frameTaken);
    }

    public Action GetAction(int currentFrame)
    {
        if(actionIndex+1 >= actionSequence.Count)
        {
            return Action.Stand;
        }

        if (currentFrame < actionSequence[actionIndex].frameTaken)
            return Action.Stand;

        if(currentFrame >= actionSequence[actionIndex+1].frameTaken)
            actionIndex++;
        return actionSequence[actionIndex].p2Action;
    }

    public void RestartSequence()
    {
        actionIndex = 0;
    }

    public void SaveSequence(string sequenceName)
    {
        string directoryPath = Application.streamingAssetsPath + "/Sequences/";
        string filePath = directoryPath + sequenceName + ".txt";

        string datalog = "";//"Metadata";
        for (int i = 0; i < actionSequence.Count; i++)
        {
            datalog += JsonUtility.ToJson(actionSequence[i], true);
            if (i != actionSequence.Count - 1)
                datalog += "\n~~~~\n";
        }
        File.WriteAllText(filePath, datalog);
        Debug.Log("wrote to log");
    }

    public void LoadSequence(string sequenceName)
    {
        string directoryPath = Application.streamingAssetsPath + "/Sequences/";
        string filePath = directoryPath + sequenceName + ".txt";

        //deserialize
        string contents = File.ReadAllText(filePath);
        string[] serializeObjects = contents.Split(new string[] { "~~~~" }, StringSplitOptions.RemoveEmptyEntries);
        actionSequence = new List<GameEvent>();
        for (int i = 0; i < serializeObjects.Length; i++)
        {
            actionSequence.Add(JsonUtility.FromJson<GameEvent> (serializeObjects[i]));
        }
    }

    override public string ToString()
    {
        string finString = "";
        foreach(GameEvent pair in actionSequence)
        {
            finString += pair.p2Action + " " + pair.frameTaken;
        }
        return finString;
    }
}
