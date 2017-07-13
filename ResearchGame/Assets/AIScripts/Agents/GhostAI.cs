using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


//TODO: Migrate it ot he AIAgent framework
/// <summary>
/// Implementation of the ghost AI
/// Basically, given the game's state, we look at what the player we're imitating did in that state
/// It will then do the action appropriate to the situation
/// That list of actions is essentially a frequency table.
/// </summary>

public class GhostAI : MonoBehaviour
{
    public string playerProfileName;
    public EventRecorder dataRecorder;

    Player AIPlayer;
    Player Opponent;

    private List<GameEvent> priorSnapshots;

    public Text DebugText;

    private Dictionary<AISituation, ActionLookupTable> frequencyTable
        = new Dictionary<AISituation, ActionLookupTable>();

    //private AdaptiveActionSelector actionSelector;

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
            GameEvent snapshot = priorSnapshots[i];
            AISituation situation = new AISituation(snapshot);

            if (!frequencyTable.ContainsKey(situation))
                frequencyTable.Add(situation, new ActionLookupTable());
            frequencyTable[situation].IncreaseFrequency(snapshot.p2Action);
        }
    }

    int frameInterval = 5;

    GameEvent pastState = null;
    AISituation pastSituation = null;
    Action pastAction;
    
    void Update()
    {
        if (!AIPlayer.enabled)
            return; 

        if (GameManager.instance.currentFrame % frameInterval == 0)
        {
            //Get the current game snapshot
            GameEvent currentState = GetGameState();
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

            AISituation currentSituation = new AISituation(currentState);
            
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

            bool actionSucceeded = AIPlayer.PerformAction(action);

            //If we successfully did the action, update the past action and past situation
            if (actionSucceeded)
            {
                pastAction = action;
                pastSituation = currentSituation;
            }
            pastState = currentState;
        }
    }

    private float GetReward(GameEvent pastState, GameEvent currentState)
    {
        return (pastState.p1Health - currentState.p1Health) + (currentState.p2Health - pastState.p2Health);
    }

    //Encapsulate the state of the opponent player, reduced to easily identifiable enums
    GameEvent GetGameState()
    {
        return dataRecorder.currentSession.snapshots.FindLast(x => true);
    }
}