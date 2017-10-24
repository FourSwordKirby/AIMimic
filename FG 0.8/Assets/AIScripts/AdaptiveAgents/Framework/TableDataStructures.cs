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
            if (actionTable[situation][a].weight > highestWeight)
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

        foreach (AISituation situation in actionTable.Keys)
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
            string[] dataStrings = situationStrings[i].Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

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

        if (minWeight < 0)
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

        if (successful)
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