using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class Datastructures
{
    //Lol this is empty
}

public class ActionLookupTable
{
    List<AIAction> actionTable;

    public ActionLookupTable()
    {
        actionTable = new List<AIAction>();
        foreach (Action action in System.Enum.GetValues(typeof(Action)))
        {
            actionTable.Add(new AIAction(action));
        }
    }

    public void IncreaseFrequency(Action action)
    {
        if (actionTable[(int)action] == null)
            actionTable[(int)action] = new AIAction(action);
        actionTable[(int)action].freq++;
    }

    public void IncreaseWeight(Action action, float reward)
    {
        if (actionTable[(int)action] == null)
            actionTable[(int)action] = new AIAction(action);
        actionTable[(int)action].weight = Mathf.Max(0, actionTable[(int)action].weight + reward);
    }

    public float GetValue(Action action)
    {
        return actionTable[(int)action].freq * actionTable[(int)action].weight;
    }

    public Action GetRandomAction()
    {
        float totalweight = actionTable.Sum(x => x.freq * x.weight);
        float weightThreshold = Random.Range(0, totalweight);

        float runningSum = 0.0f;
        foreach (AIAction a in actionTable)
        {
            runningSum += a.freq * a.weight;
            if (runningSum > weightThreshold)
                return a.action;
        }
        return actionTable[0].action;
    }
}

public class AdaptiveActionSelector
{
    Dictionary<AISituation, List<AIAction>> actionTable;

    public AdaptiveActionSelector()
    {
        actionTable = new Dictionary<AISituation, List<AIAction>>();
    }

    public void IncreaseWeight(AISituation situation, Action action, float reward)
    {
        if (!actionTable.ContainsKey(situation))
        {
            actionTable.Add(situation, new List<AIAction>());
            foreach (Action a in System.Enum.GetValues(typeof(Action)))
            {
                actionTable[situation].Add(new AIAction(a));
            }
        }

        actionTable[situation][(int)action].weight = Mathf.Max(0, actionTable[situation][(int)action].weight + reward);
    }

    public float GetWeight(AISituation situation, Action action)
    {
        return actionTable[situation][(int)action].weight;
    }

    public Action GetAction(AISituation situation)
    {
        if (!actionTable.ContainsKey(situation))
        {
            Debug.Log("Novel situation");
            actionTable.Add(situation, new List<AIAction>());
            foreach (Action a in System.Enum.GetValues(typeof(Action)))
            {
                actionTable[situation].Add(new AIAction(a));
            }
        }

        float totalweight = actionTable[situation].Sum(x => x.weight);
        float weightThreshold = Random.Range(0, totalweight);

        float runningSum = 0.0f;
        foreach (AIAction a in actionTable[situation])
        {
            runningSum += a.weight;
            if (runningSum > weightThreshold)
                return a.action;
        }
        return actionTable[situation][0].action;
    }

    public void storeTable(string filePath)
    {
        string datalog = "";//"Metadata";

        foreach(AISituation situation in actionTable.Keys)
        {
            datalog += JsonUtility.ToJson(situation) + "\n";
            foreach (AIAction action in actionTable[situation])
            {
                datalog += JsonUtility.ToJson(action) + "\n";
            }
            datalog += "~~~\n";
        }
        File.WriteAllText(filePath + ".txt", datalog);

        //A prettier version of the file aswell
        datalog = "";//"Metadata";

        foreach (AISituation situation in actionTable.Keys)
        {
            datalog += situation + ": \n";
            foreach (AIAction action in actionTable[situation])
            {
                datalog += action + "\n";
            }
            datalog += "~~~\n";
        }
        File.WriteAllText(filePath + "_readable.txt", datalog);
    }

    public void loadTable(string filePath)
    {
        actionTable = new Dictionary<AISituation, List<AIAction>>();

        string contents = File.ReadAllText(filePath);
        string[] situationStrings = contents.Split(new string[] { "~~~~" }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < situationStrings.Length; i++)
        {
            string[] dataStrings =  situationStrings[i].Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            AISituation situation = JsonUtility.FromJson<AISituation>(dataStrings[0]);
            actionTable.Add(situation, new List<AIAction>());

            for (int j = 1; i < dataStrings.Length; i++)
            {
                AIAction action = JsonUtility.FromJson<AIAction>(dataStrings[j]);
                actionTable[situation].Add(action);
            }
        }
    }
}

internal class AIAction
{
    public Action action;
    public int freq;
    public float weight;

    public AIAction(Action a)
    {
        action = a;
        freq = 0;
        weight = 1.0f;
    }

    public override string ToString()
    {
        return action + " frequency: " + freq + " weight: " + weight;
    }
}

public class AISituation : System.IEquatable<AISituation>
{
    public xDistance deltaX;
    public yDistance deltaY;

    public PlayerStatus opponentStatus;

    public AISituation(GameSnapshot snapshot)
    {
        //xDistance
        if (snapshot.xDistance < 1)
            deltaX = xDistance.Adjacent;
        else if (snapshot.xDistance < 3)
            deltaX = xDistance.Near;
        else
            deltaX = xDistance.Far;

        //yDistance
        if (snapshot.yDistance < 0.5f)
            deltaY = yDistance.Level;
        else if (snapshot.yDistance < 1)
            deltaY = yDistance.Near;
        else
            deltaY = yDistance.Far;

        //Status
        opponentStatus = snapshot.p1Status;
    }

    public bool Equals(AISituation situation)
    {
        return deltaX == situation.deltaX &&
                deltaY == situation.deltaY &&
                opponentStatus == situation.opponentStatus;
    }


    public override int GetHashCode()
    {
        return ((int)deltaX + (int)deltaY + (int)opponentStatus);
    }

    public override string ToString()
    {
        return deltaX + " " + deltaY + " " + opponentStatus;
    }
}