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
    public AISituation prior;
    public PerformedAction action;
    public AISituation result;

    public Transition(AISituation prior, PerformedAction action, AISituation result)
    {
        this.prior = prior;
        this.action = action;
        this.result = result;
    }

    public override string ToString()
    {
        return "(" + this.action + ") " + "[" + this.prior + "]" + this.result;
    }
}

[System.Serializable]
public class PerformedAction : System.IEquatable<PerformedAction>
{
    public Action action;
    public int duration; //Number of frames to do this action

    public PerformedAction(Action action, int duration)
    {
        this.action = action;
        this.duration = duration;
    }

    public override int GetHashCode()
    {
        return (int)action + (int)duration;
    }

    public override string ToString()
    {
        return action + " " + duration;
    }

    public bool Equals(PerformedAction performedAction)
    {
        return performedAction.action == this.action &&
                performedAction.duration == this.duration;
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
public class AIAction
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

    public xMovement xVel;
    public yMovement yVel;

    public xMovement opponentXVel;
    public yMovement opponentYVel;

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

        //Velocity
        if(isPlayer1)
        {
            if (snapshot.p1Status == PlayerStatus.Moving || snapshot.p1Status == PlayerStatus.Dashing || snapshot.p1Status == PlayerStatus.Tech)
            {
                if (snapshot.p1Vel.x < 0)
                    xVel = xMovement.Left;
                else if (snapshot.p1Vel.x > 0)
                    xVel = xMovement.Right;
                else
                    xVel = xMovement.Neutral;
                yVel = yMovement.Neutral;
            }
            else if (snapshot.p1Status == PlayerStatus.Air)
            {
                if (snapshot.p1Vel.x < 0)
                    xVel = xMovement.Left;
                else if (snapshot.p1Vel.x > 0)
                    xVel = xMovement.Right;
                else
                    xVel = xMovement.Neutral;

                if (snapshot.p1Vel.y < 0)
                    yVel = yMovement.Down;
                else if (snapshot.p1Vel.y > 0)
                    yVel = yMovement.Up;
                else
                    yVel = yMovement.Neutral;
            }
            else
            {
                xVel = xMovement.Neutral;
                yVel = yMovement.Neutral;
            }

            if (snapshot.p2Status == PlayerStatus.Moving || snapshot.p2Status == PlayerStatus.Dashing || snapshot.p2Status == PlayerStatus.Tech)
            {
                if (snapshot.p2Vel.x < 0)
                    opponentXVel = xMovement.Left;
                else if (snapshot.p2Vel.x > 0)
                    opponentXVel = xMovement.Right;
                else
                    opponentXVel = xMovement.Neutral;
                opponentYVel = yMovement.Neutral;
            }
            else if (snapshot.p2Status == PlayerStatus.Air)
            {
                if (snapshot.p2Vel.x < 0)
                    opponentXVel = xMovement.Left;
                else if (snapshot.p2Vel.x > 0)
                    opponentXVel = xMovement.Right;
                else
                    opponentXVel = xMovement.Neutral;

                if (snapshot.p2Vel.y < 0)
                    opponentYVel = yMovement.Down;
                else if (snapshot.p2Vel.y > 0)
                    opponentYVel = yMovement.Up;
                else
                    opponentYVel = yMovement.Neutral;
            }
            else
            {
                opponentXVel = xMovement.Neutral;
                opponentYVel = yMovement.Neutral;
            }
        }
        else
        {
            if (snapshot.p2Status == PlayerStatus.Moving || snapshot.p2Status == PlayerStatus.Dashing || snapshot.p2Status == PlayerStatus.Tech)
            {
                if (snapshot.p2Vel.x < -0.25f)
                    xVel = xMovement.Left;
                else if (snapshot.p2Vel.x > 0.25f)
                    xVel = xMovement.Right;
                else
                    xVel = xMovement.Neutral;
                yVel = yMovement.Neutral;
            }
            else if (snapshot.p2Status == PlayerStatus.Air)
            {
                if (snapshot.p2Vel.x < -0.25f)
                    xVel = xMovement.Left;
                else if (snapshot.p2Vel.x > 0.25f)
                    xVel = xMovement.Right;
                else
                    xVel = xMovement.Neutral;

                if (snapshot.p2Vel.y < -0.25f)
                    yVel = yMovement.Down;
                else if (snapshot.p2Vel.y > 0.25f)
                    yVel = yMovement.Up;
                else
                    yVel = yMovement.Neutral;
            }
            else
            {
                xVel = xMovement.Neutral;
                yVel = yMovement.Neutral;
            }

            if (snapshot.p1Status == PlayerStatus.Moving || snapshot.p1Status == PlayerStatus.Dashing || snapshot.p1Status == PlayerStatus.Tech)
            {
                if (snapshot.p1Vel.x < -0.25f)
                    opponentXVel = xMovement.Left;
                else if (snapshot.p1Vel.x > 0.25f)
                    opponentXVel = xMovement.Right;
                else
                    opponentXVel = xMovement.Neutral;
                opponentYVel = yMovement.Neutral;
            }
            else if (snapshot.p1Status == PlayerStatus.Air)
            {
                if (snapshot.p1Vel.x < -0.25f)
                    opponentXVel = xMovement.Left;
                else if (snapshot.p1Vel.x > 0.25f)
                    opponentXVel = xMovement.Right;
                else
                    opponentXVel = xMovement.Neutral;

                if (snapshot.p1Vel.y < -0.25f)
                    opponentYVel = yMovement.Down;
                else if (snapshot.p1Vel.y > 0.25f)
                    opponentYVel = yMovement.Up;
                else
                    opponentYVel = yMovement.Neutral;
            }
            else
            {
                opponentXVel = xMovement.Neutral;
                opponentYVel = yMovement.Neutral;
            }
        }


        //Side
        if (xDist < 0)
            side = Side.Left;
        else
            side = Side.Right;
        //xDistance
        if (Mathf.Abs(xDist) < 2)
            deltaX = xDistance.Adjacent;
        else if (Mathf.Abs(xDist) < 5)
            deltaX = xDistance.Near;
        else
            deltaX = xDistance.Far;

        //yDistance
        //TODO Fix dumb positioning values with respect to the player rotating while attacking :|
        if (yDist <= -0.8f)
            deltaY = yDistance.Below;
        else if (-0.8f < yDist && yDist <= 0.8f)
            deltaY = yDistance.Level;
        else 
            deltaY = yDistance.Above;

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
            cornered =  snapshot.p1CornerDistance < 3 ? Cornered.yes : Cornered.no;
            opponentCornered = snapshot.p2CornerDistance < 3 ? Cornered.yes : Cornered.no;
        }
        else
        {
            cornered = snapshot.p1CornerDistance < 3 ? Cornered.yes : Cornered.no;
            opponentCornered = snapshot.p2CornerDistance < 3 ? Cornered.yes : Cornered.no;
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


    //Obsolete code used to not break other pieces of code
    public AISituation(GameEvent gameEvent)
    {
    }

    //Empty constructor used for copying an AISituation
    private AISituation()
    {
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

                xVel == situation.xVel &&
                yVel == situation.yVel &&
                opponentXVel == situation.opponentXVel &&
                opponentYVel == situation.opponentYVel &&
                cornered == situation.cornered &&
                opponentCornered == situation.opponentCornered &&
                status == situation.status &&
                opponentStatus == situation.opponentStatus;
    }

    public override int GetHashCode()
    {
        return (int)Mathf.Sign((float)side-0.5f) * 
                (229 * (int)deltaX + 
                541 * (int)deltaY + 
                821 * (int)status +
                821 * (int)opponentStatus);
    }

    public override string ToString()
    {
        return side + " " + 
                deltaX + " " + deltaY + " " +
                xVel + " " + yVel + " " +
                opponentXVel + " " + opponentYVel + " " +
                cornered + " " + opponentCornered + " " +
                status + " " + opponentStatus;
    }

    public AISituation Copy()
    {
        AISituation newSituation = new AISituation();
        newSituation.side = side;
        newSituation.deltaX = deltaX;
        newSituation.deltaY = deltaY;

        newSituation.xVel = xVel;
        newSituation.yVel = yVel;

        newSituation.opponentXVel = opponentXVel;
        newSituation.opponentYVel = opponentYVel;

        newSituation.health = health;
        newSituation.opponentHealth = opponentHealth;
        newSituation.cornered = cornered;
        newSituation.opponentCornered = opponentCornered;
        newSituation.status = status;
        newSituation.opponentStatus = opponentStatus;
        return newSituation;
    }

    //Need to be adjusted with the new variablesa added
    internal static float Similarity(AISituation x, AISituation y)
    {
        SituationChange diff = new SituationChange(x, y);
        return ((diff.sideChange == 0 ? 1 : 0)
            + (diff.deltaXChange == 0 ? 1 : 0)
            + (diff.deltaYChange == 0 ? 1 : 0)
            + (diff.xVelChange == 0 ? 1 : 0)
            + (diff.yVelChange == 0 ? 1 : 0)
            + (diff.opponentXVelChange == 0 ? 1 : 0)
            + (diff.opponentYVelChange == 0 ? 1 : 0)
            + (diff.corneredChange == 0 ? 1 : 0)
            + (diff.opponentCorneredChange == 0 ? 1 : 0)
            + (x.status == y.status ? 1 : 0)
            + (x.opponentStatus == y.opponentStatus ? 1 : 0)) + 1 / 12.0f; //The +1 is used to prevent divide by 0 errors
    }
}

[System.Serializable]
public class SituationChange : System.IEquatable<SituationChange>
{
    public AISituation prior;
    public AISituation result;

    public int sideChange;
    public int deltaXChange;
    public int deltaYChange;

    public int xVelChange;
    public int yVelChange;

    public int opponentXVelChange;
    public int opponentYVelChange;

    public int healthChange;
    public int opponentHealthChange;

    public int corneredChange;
    public int opponentCorneredChange;

    public PlayerStatus resultingStatus;
    public PlayerStatus opponentResultingStatus;

    //public int statusChange;
    //public int opponentStatusChange;

    public override int GetHashCode()
    {
        return (int)Mathf.Sign((float)sideChange - 0.5f) * ((int)deltaXChange + (int)deltaYChange);
    }

    public override string ToString()
    {
        return sideChange + " " + deltaXChange + " " + deltaYChange + " " + resultingStatus + " " + opponentResultingStatus;
    }

    public bool Equals(SituationChange change)
    {
        return sideChange == change.sideChange &&
                deltaXChange == change.deltaXChange &&
                deltaYChange == change.deltaYChange &&
                healthChange == change.healthChange &&
                xVelChange == change.xVelChange && 
                yVelChange == change.yVelChange &&
                opponentXVelChange == change.opponentXVelChange &&
                opponentYVelChange == change.opponentYVelChange &&
                opponentHealthChange == change.opponentHealthChange &&
                corneredChange == change.corneredChange &&
                opponentCorneredChange == change.opponentCorneredChange &&
                resultingStatus == change.resultingStatus &&
                opponentResultingStatus == change.opponentResultingStatus;
    }

    public SituationChange(AISituation prior, AISituation result)
    {
        this.prior = prior;
        this.result = result;

        sideChange = (int)result.side - (int)prior.side;
        deltaXChange = (int)result.deltaX - (int)prior.deltaX;
        deltaYChange = (int)result.deltaY - (int)prior.deltaY;

        xVelChange = (int)result.xVel - (int)prior.xVel;
        yVelChange = (int)result.yVel - (int)prior.yVel;

        opponentXVelChange = (int)result.opponentXVel - (int)prior.opponentXVel;
        opponentYVelChange = (int)result.opponentYVel - (int)prior.opponentYVel;

        healthChange = (int)result.health - (int)prior.health;
        opponentHealthChange = (int)result.health - (int)prior.health;

        corneredChange = (int)result.cornered - (int)prior.cornered;
        opponentCorneredChange = (int)result.opponentCornered - (int)prior.opponentCornered;

        resultingStatus = result.status;
        opponentResultingStatus = result.opponentStatus;
    }

    public static AISituation ApplyChange(AISituation prior, SituationChange change)
    {
        AISituation newSituation = prior.Copy();

        int sideMax = System.Enum.GetValues(typeof(Side)).Cast<int>().Max();
        int xMax = System.Enum.GetValues(typeof(xDistance)).Cast<int>().Max();
        int yMax = System.Enum.GetValues(typeof(yDistance)).Cast<int>().Max();
        int healthMax = System.Enum.GetValues(typeof(Health)).Cast<int>().Max();
        int corneredMax = System.Enum.GetValues(typeof(Cornered)).Cast<int>().Max();
        int statusMax = System.Enum.GetValues(typeof(PlayerStatus)).Cast<int>().Max();

        newSituation.side = (Side)Mathf.Clamp((int)prior.side + (int)change.sideChange, 0, sideMax);
        newSituation.deltaX = (xDistance)Mathf.Clamp((int)prior.deltaX + (int)change.deltaXChange, 0, xMax);
        newSituation.deltaY = (yDistance)Mathf.Clamp((int)prior.deltaY + (int)change.deltaYChange, 0, yMax);
        newSituation.health = (Health)Mathf.Clamp((int)prior.health + (int)change.healthChange, 0, healthMax);
        newSituation.opponentHealth = (Health)Mathf.Clamp((int)prior.opponentHealth + (int)change.opponentHealthChange, 0, healthMax);
        newSituation.cornered = (Cornered)Mathf.Clamp((int)prior.cornered + (int)change.corneredChange, 0, corneredMax);
        newSituation.opponentCornered = (Cornered)Mathf.Clamp((int)prior.opponentCornered + (int)change.opponentCorneredChange, 0, corneredMax);

        newSituation.status = change.resultingStatus;
        newSituation.opponentStatus = change.opponentResultingStatus;
        //You can't apply change on the status bc that's jank and doesn't really work lol
        //newSituation.status = (PlayerStatus)Mathf.Clamp((int)prior.status + (int)change.statusChange, 0, statusMax);
        //newSituation.opponentStatus = (PlayerStatus)Mathf.Clamp((int)prior.opponentStatus + (int)change.opponentStatusChange, 0, statusMax);

        return newSituation;
    }
}