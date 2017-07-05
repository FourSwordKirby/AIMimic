﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

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

        actionTable[situation][(int)action].weight = actionTable[situation][(int)action].weight + reward;
        RebalanceWeights(actionTable[situation]);
    }

    //Displays the weight in readable format
    public float GetWeight(AISituation situation, Action action)
    {
        List<AIAction> actions = actionTable[situation];

        float totalWeight = actions.Sum(x => x.weight);
        return actions[(int)action].weight / totalWeight;
    }

    public Action GetAction(AISituation situation)
    {
        if (!actionTable.ContainsKey(situation))
        {
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
            {
                return a.action;
            }
        }
        return actionTable[situation][0].action;
    }

    public void StoreTable(string filePath)
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

    public void LoadTable(string filePath)
    {
        actionTable = new Dictionary<AISituation, List<AIAction>>();

        string contents = File.ReadAllText(filePath + ".txt");
        string[] situationStrings = contents.Split(new string[] { "~~~\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < situationStrings.Length; i++)
        {
            string[] dataStrings =  situationStrings[i].Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            AISituation situation = JsonUtility.FromJson<AISituation>(dataStrings[0]);
            actionTable.Add(situation, new List<AIAction>());

            for (int j = 1; j < dataStrings.Length; j++)
            {
                AIAction action = JsonUtility.FromJson<AIAction>(dataStrings[j]);
                actionTable[situation].Add(action);
            }
        }
    }

    private void RebalanceWeights(List<AIAction> actions)
    {
        float minWeight = actions.Min(x => x.weight);

        if(minWeight < 0)
        {
            //Nomalizes all of the action weights so that the min action has at least value 0
            foreach (AIAction action in actions)
                action.weight += (-minWeight);
        }

        //One problem with rebalancing is that truly negative states are not represented as such
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

/// <summary>
/// A simplified version of the game state used by the AI. Simplifies things by removing the continuous elements
/// </summary>
public class AISituation : System.IEquatable<AISituation>
{
    public Side side;
    public xDistance deltaX;
    public yDistance deltaY;

    public Health health;
    public Health opponentHealth;

    public Cornered cornered;
    public Cornered opponentCornered;
    public PlayerStatus status;
    public PlayerStatus opponentStatus;

    public AISituation(Snapshot snapshot, bool isPlayer1)
    {
        float xDist;
        float yDist;

        if (isPlayer1)
        {
            xDist = snapshot.xDistance;
            yDist = snapshot.yDistance;
        }
        else
        {
            xDist = -snapshot.xDistance;
            yDist = -snapshot.yDistance;
        }

        //Side
        if (xDist < 0)
            side = Side.Left;
        else
            side = Side.Right;

        //xDistance
        if (Mathf.Abs(xDist) < 1)
            deltaX = xDistance.Adjacent;
        else if (Mathf.Abs(xDist) < 3)
            deltaX = xDistance.Near;
        else
            deltaX = xDistance.Far;

        //yDistance
        if (yDist <= -1)
            deltaY = yDistance.FarBelow;
        else if (-1 < yDist && yDist <= -0.2)
            deltaY = yDistance.NearBelow;
        else if (-0.2 < yDist && yDist <= 0.2)
            deltaY = yDistance.Level;
        else if (0.2 < yDist && yDist <= 1)
            deltaY = yDistance.NearAbove;
        else
            deltaY = yDistance.FarAbove;

        //Health
        if (0 < snapshot.p1Health && snapshot.p1Health <= 30)
            health = Health.Low;
        else if (30 < snapshot.p1Health && snapshot.p1Health <= 70)
            health = Health.Med;
        else if (70 < snapshot.p1Health && snapshot.p1Health <= 100)
            health = Health.High;

        if (0 < snapshot.p2Health && snapshot.p2Health <= 30)
            opponentHealth = Health.Low;
        else if (30 < snapshot.p2Health && snapshot.p2Health <= 70)
            opponentHealth = Health.Med;
        else if (70 < snapshot.p2Health && snapshot.p2Health <= 100)
            opponentHealth = Health.High;

        //Cornered
        if (isPlayer1)
        {
            cornered =  snapshot.p1CornerDistance < 1 ? Cornered.yes : Cornered.no;
            opponentCornered = snapshot.p2CornerDistance < 1 ? Cornered.yes : Cornered.no;
        }
        else
        {
            cornered = snapshot.p1CornerDistance < 1 ? Cornered.yes : Cornered.no;
            opponentCornered = snapshot.p2CornerDistance < 1 ? Cornered.yes : Cornered.no;
        }

        //Status
        if (isPlayer1)
        {
            status = snapshot.p1Status;
            opponentStatus = snapshot.p2Status;
        }
        else
        {
            status = snapshot.p2Status;
            opponentStatus = snapshot.p1Status;
        }
    }

    public AISituation(GameEvent gameEvent)
    {
        //xDistance
        if (gameEvent.xDistance < 1)
            deltaX = xDistance.Adjacent;
        else if (gameEvent.xDistance < 3)
            deltaX = xDistance.Near;
        else
            deltaX = xDistance.Far;

        //yDistance
        if (gameEvent.yDistance < 0.2f)
            deltaY = yDistance.Level;
        else if (gameEvent.yDistance < 1)
            deltaY = yDistance.NearAbove;
        else
            deltaY = yDistance.FarAbove;

        //Status
        opponentStatus = gameEvent.p1Status;
    }

    //Filler to stop an angry compiler
    public AISituation(Snapshot currentState)
    {
    }

    public bool Equals(AISituation situation)
    {
        return side == situation.side &&
                deltaX == situation.deltaX &&
                deltaY == situation.deltaY &&
                //Don't account for health for now bc it isn't pertient towards making the AI navigate neutral effectively
                //health == situation.health &&
                //opponentHealth == situation.opponentHealth &&
                cornered == situation.cornered &&
                opponentCornered == situation.opponentCornered &&
                status == situation.status &&
                opponentStatus == situation.opponentStatus;
    }


    public override int GetHashCode()
    {
        return ((int)deltaX + (int)deltaY + (int)opponentStatus);
    }

    public override string ToString()
    {
        return side + " " + deltaX + " " + deltaY + " " + opponentStatus;
    }
}