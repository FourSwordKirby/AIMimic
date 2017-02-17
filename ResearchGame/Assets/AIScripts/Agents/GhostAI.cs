using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//The goal of this AI agent is to look at a previous set of playsessions between  the plaer Test and an unknown other player
//The AI's behavior patterns should mimic Test using ngrams. That is, it should do the same moves that Test does in certain situations
//given some history.

public class GhostAI : MonoBehaviour
{
    public string playerProfileName;
    public DataRecorder dataRecorder;

    Player AIPlayer;
    Player Opponent;

    private List<GameSnapshot> priorSnapshots;

    public Text DebugText;

    //Implementation of the ghost AI
    //Basically, given the game's state, we look at what the player we're imitating did in that state
    //It will then do the action appropriate to the situation
    //That list of actions is essentially a frequency table.
    private Dictionary<GhostAISituation, ActionLookupTable> frequencyTable
        = new Dictionary<GhostAISituation, ActionLookupTable>();

    void Start()
    {
        //controlledPlayer = GameManager.Players[0];
        AIPlayer = GameManager.instance.p2;
        AIPlayer.AIControlled = true;

        AIPlayer.sprite.color = Color.magenta;

        priorSnapshots = Session.RetrievePlayerSession(playerProfileName);
        priorSnapshots = priorSnapshots.OrderBy(x => x.frameTaken).ToList();

        Debug.Log(priorSnapshots.Count);

        for(int i = 0; i < priorSnapshots.Count; i++)
        {
            GameSnapshot snapshot = priorSnapshots[i];
            GhostAISituation situation = new GhostAISituation(snapshot);

            if (!frequencyTable.ContainsKey(situation))
                frequencyTable.Add(situation, new ActionLookupTable());
            frequencyTable[situation].IncreaseFrequency(snapshot.p2Action);
        }
    }

    int frameInterval = 5;

    GameSnapshot pastState = null;
    Action pastAction;
    GhostAISituation pastSituation = null;

    void Update()
    {
        if (!AIPlayer.enabled)
            return; 

        if (GameManager.currentFrame % frameInterval == 0)
        {
            //Get the current game snapshot
            GameSnapshot currentState = getGameState();
            if (currentState == null)
                return;

            //Get the reward if applicable
            if(pastState != null && pastSituation != null)
            {
                float reward = GetReward(pastState, currentState);

                //Hacky fix to prevent the agent from crashing if it's in an unfamiliar situation
                //Should really make the AI have a handle on some kind of strategy for all situations
                if (frequencyTable.ContainsKey(pastSituation))
                {
                    frequencyTable[pastSituation].IncreaseWeight(pastAction, reward);
                    DebugText.text = "Last action: " + pastAction + "\n" + "Current Weight: " + frequencyTable[pastSituation].GetValue(pastAction);
                }
            }

            GhostAISituation currentSituation = new GhostAISituation(currentState);
            
            Action action;
            if (frequencyTable.ContainsKey(currentSituation))
            {
                ActionLookupTable sampleActions = frequencyTable[currentSituation];
                action = sampleActions.GetRandomAction();
            }
            else
            {
                Debug.Log("SUPER RANDOM");
                action = (Action)Random.Range(0, System.Enum.GetValues(typeof(Action)).Length);
            }

            bool actionSucceeded = AIPlayer.performAction(action);

            //If we successfully did the action, update the past action and past situation
            if (actionSucceeded)
            {
                pastAction = action;
                pastSituation = currentSituation;
            }
            pastState = currentState;
        }
    }

    private float GetReward(GameSnapshot pastState, GameSnapshot currentState)
    {
        return (pastState.p1Health - currentState.p1Health) + (currentState.p2Health - pastState.p2Health);
    }

    //Encapsulate the state of the opponent player, reduced to easily identifiable enums
    GameSnapshot getGameState()
    {
        return dataRecorder.currentSession.snapshots.FindLast(x => true);
    }
}

public class ActionLookupTable
{
    List<AIAction> actionTable;

    public ActionLookupTable()
    {
        actionTable = new List<AIAction>();
        foreach(Action action in System.Enum.GetValues(typeof(Action)))
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
        actionTable[(int)action].weight = Mathf.Max(0, actionTable[(int)action].weight+reward);
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
}


public class GhostAISituation : System.IEquatable<GhostAISituation>
{
    public xDistance deltaX;
    public yDistance deltaY;

    public PlayerStatus opponentStatus;

    public GhostAISituation(GameSnapshot snapshot)
    {
        //xDistance
        if(snapshot.xDistance < 1)
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

    public bool Equals(GhostAISituation situation)
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