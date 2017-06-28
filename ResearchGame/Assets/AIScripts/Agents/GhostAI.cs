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
            GameSnapshot snapshot = priorSnapshots[i];
            AISituation situation = new AISituation(snapshot);

            if (!frequencyTable.ContainsKey(situation))
                frequencyTable.Add(situation, new ActionLookupTable());
            frequencyTable[situation].IncreaseFrequency(snapshot.p2Action);
        }
    }

    int frameInterval = 5;

    GameSnapshot pastState = null;
    AISituation pastSituation = null;
    Action pastAction;
    
    void Update()
    {
        if (!AIPlayer.enabled)
            return; 

        if (GameManager.currentFrame % frameInterval == 0)
        {
            //Get the current game snapshot
            GameSnapshot currentState = GetGameState();
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

    private float GetReward(GameSnapshot pastState, GameSnapshot currentState)
    {
        return (pastState.p1Health - currentState.p1Health) + (currentState.p2Health - pastState.p2Health);
    }

    //Encapsulate the state of the opponent player, reduced to easily identifiable enums
    GameSnapshot GetGameState()
    {
        return dataRecorder.currentSession.snapshots.FindLast(x => true);
    }
}