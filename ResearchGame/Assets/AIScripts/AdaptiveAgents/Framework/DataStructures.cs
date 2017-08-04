using System.Collections;
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
    Dictionary<AISituation, Dictionary<Action, AIAction>> actionTable;

    public System.Func<AISituation, List<Action>> ConstrainActions;
    public List<Action> DefaultConstrainActions(AISituation sit) { return System.Enum.GetValues(typeof(Action)).Cast<Action>().ToList(); }

    public AdaptiveActionSelector()
    {
        actionTable = new Dictionary<AISituation, Dictionary<Action, AIAction>>();
        ConstrainActions = DefaultConstrainActions;
    }

    public void IncreaseWeight(AISituation situation, Action action, float reward)
    {
        if (!actionTable.ContainsKey(situation))
        {
            actionTable.Add(situation, new Dictionary<Action, AIAction>());
            List<Action> availableActions = ConstrainActions(situation);

            foreach (Action a in availableActions)
            {
                actionTable[situation].Add(a, new AIAction(a));
            }
        }

        actionTable[situation][action].weight = actionTable[situation][action].weight + reward;
        RebalanceWeights(actionTable[situation].Values.ToList());
    }

    //Displays the weight in readable format
    public float GetWeight(AISituation situation, Action action)
    {
        Dictionary<Action, AIAction> actions = actionTable[situation];

        float totalWeight = actions.Sum(x => x.Value.weight);
        return actions[action].weight / totalWeight;
    }

    public Action GetAction(AISituation situation)
    {
        //If we haven't seen this situation before, select a random action within our constraints
        if (!actionTable.ContainsKey(situation))
        {
            actionTable.Add(situation, new Dictionary<Action, AIAction>());
            List<Action> availableActions = ConstrainActions(situation);

            foreach (Action a in availableActions)
            {
                actionTable[situation].Add(a, new AIAction(a));
            }
        }

        float totalweight = actionTable[situation].Sum(x => x.Value.weight);
        float weightThreshold = Random.Range(0, totalweight);

        float runningSum = 0.0f;
        foreach (AIAction a in actionTable[situation].Values)
        {
            runningSum += a.weight;
            if (runningSum > weightThreshold)
            {
                return a.action;
            }
        }
        return actionTable[situation][0].action;
    }

    public Action GetBestAction(AISituation situation)
    {
        //If we haven't seen this situation before, select a random action within our constraints
        if (!actionTable.ContainsKey(situation))
        {
            actionTable.Add(situation, new Dictionary<Action, AIAction>());
            List<Action> availableActions = ConstrainActions(situation);

            foreach (Action a in availableActions)
            {
                actionTable[situation].Add(a, new AIAction(a));
            }
        }

        float highestWeight = 0.0f;
        Action chosenAction = Action.Stand;
        foreach (Action a in actionTable[situation].Keys)
        {
            if(actionTable[situation][a].weight > highestWeight)
            {
                highestWeight = actionTable[situation][a].weight;
                chosenAction = a;
            }
        }
        return chosenAction;
    }

    public void StoreTable(string filePath)
    {
        string datalog = "";//"Metadata";

        foreach(AISituation situation in actionTable.Keys)
        {
            datalog += JsonUtility.ToJson(situation) + "\n";
            foreach (AIAction action in actionTable[situation].Values)
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
            foreach (AIAction action in actionTable[situation].Values)
            {
                datalog += action + "\n";
            }
            datalog += "~~~\n";
        }
        File.WriteAllText(filePath + "_readable.txt", datalog);

        Debug.Log(actionTable.Keys.Count());
    }

    public void LoadTable(string filePath)
    {
        actionTable = new Dictionary<AISituation, Dictionary<Action, AIAction>>();

        string contents = File.ReadAllText(filePath + ".txt");
        string[] situationStrings = contents.Split(new string[] { "~~~\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < situationStrings.Length; i++)
        {
            string[] dataStrings =  situationStrings[i].Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            AISituation situation = JsonUtility.FromJson<AISituation>(dataStrings[0]);
            if (!actionTable.ContainsKey(situation))
            {
                actionTable.Add(situation, new Dictionary<Action, AIAction>());

                for (int j = 1; j < dataStrings.Length; j++)
                {
                    AIAction action = JsonUtility.FromJson<AIAction>(dataStrings[j]);
                    actionTable[situation].Add(action.action, action);
                }
            }
            else
                Debug.Log("Duplicate entry found! Check for data mishandling");
        }
        Debug.Log("Loaded" + actionTable.Keys.Count());
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

public class AdviceLookupTable
{
    Dictionary<AISituation, Dictionary<Advice, Effectiveness>> adviceTable;
    List<Advice> availableAdvice;

    public AdviceLookupTable()
    {
        availableAdvice = new List<Advice>() { new Advice(Action.StandBlock, Result.Block), new Advice(Action.WalkLeft, Result.Whiffed) };
        adviceTable = new Dictionary<AISituation, Dictionary<Advice, Effectiveness>>();
    }

    public void UpdateAdvice(AISituation situation, Advice advice, bool successful)
    {
        if (!adviceTable.ContainsKey(situation))
        {
            adviceTable.Add(situation, new Dictionary<Advice, Effectiveness>());
            foreach (Advice a in availableAdvice)
                adviceTable[situation].Add(a, new Effectiveness());
        }

        if(successful)
            adviceTable[situation][advice].successes++;
        else
            adviceTable[situation][advice].failures++;
    }

    //Displays the success rate in readable format
    public float GetWeight(AISituation situation, Advice advice)
    {
        return adviceTable[situation][advice].Rate();
    }

    /// <summary>
    /// Picks a random advice from the set of advices in this situation
    /// </summary>
    /// <param name="situation"></param>
    /// <returns></returns>
    public Advice PickAdvice(AISituation situation)
    {
        //If we haven't seen this situation before, select a random action within our constraints
        if (!adviceTable.ContainsKey(situation))
        {
            adviceTable.Add(situation, new Dictionary<Advice, Effectiveness>());
            foreach (Advice a in availableAdvice)
                adviceTable[situation].Add(a, new Effectiveness());
        }


        return adviceTable[situation].ElementAt(Random.Range(0, adviceTable[situation].Count)).Key;
    }
}


[System.Serializable]
public class Transition
{
    public PerformedAction action;
    public AISituation result;

    public Transition(PerformedAction action, AISituation result)
    {
        this.action = action;
        this.result = result;
    }
}

[System.Serializable]
public class PerformedAction
{
    public Action action;
    public int duration; //Number of frames to do this action

    public PerformedAction(Action action, int duration)
    {
        this.action = action;
        this.duration = duration;
    }
}

[System.Serializable]
internal class Effectiveness
{
    public float successes;
    public float failures;
    
    public float Rate()
    {
        return successes / (successes + failures);
    }
}

[System.Serializable]
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
/// 
[System.Serializable]
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
                //cornered == situation.cornered &&
                //opponentCornered == situation.opponentCornered;// &&
                //status == situation.status &&
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