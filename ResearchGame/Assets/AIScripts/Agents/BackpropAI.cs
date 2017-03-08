using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//The goal of this AI agent is to look at a previous set of playsessions between  the plaer Test and an unknown other player
//The AI's behavior patterns should mimic Test using ngrams. That is, it should do the same moves that Test does in certain situations
//given some history.

public class BackpropAI : MonoBehaviour
{
    public string playerProfileName;
    public DataRecorder dataRecorder;

    public Player AIPlayer;
    public Player Opponent;

    public Text DebugText;

    public int backpropDepth = 3;

    //Implementation of the ghost AI
    //Basically, given the game's state, we look at what the player we're imitating did in that state
    //It will then do the action appropriate to the situation
    //That list of actions is essentially a frequency table.

    private AdaptiveActionSelector actionSelector = new AdaptiveActionSelector();

    void Start()
    {
        //controlledPlayer = GameManager.Players[0];
        AIPlayer = GameManager.instance.p2;
        AIPlayer.AIControlled = true;

        AIPlayer.sprite.color = Color.green;
    }

    int frameInterval = 5;

    GameSnapshot pastState = null;
    List<AISituation> pastSituations = new List<AISituation>();
    List<Action> pastActions = new List<Action>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            //actionSelector.loadTable(Application.streamingAssetsPath + "/ActionTables/TestTable");
            actionSelector.StoreTable(Application.streamingAssetsPath + "/ActionTables/TestTable");

        if (!AIPlayer.enabled)
            return; 

        if (GameManager.currentFrame % frameInterval == 0)
        {
            //Get the current game snapshot
            GameSnapshot currentState = getGameState();
            if (currentState == null)
                return;

            //Get the reward if applicable
            if(pastState != null && pastSituations.Count() > 0)
            {
                float reward = GetReward(pastState, currentState);
                float gamma = 0.9f;
                for (int i = 0; i < pastSituations.Count(); i++)
                {
                    AISituation pastSituation = pastSituations[pastSituations.Count() - 1 - i];
                    Action pastAction = pastActions[pastSituations.Count() - 1 - i];

                    //Hacky fix to prevent the agent from crashing if it's in an unfamiliar situation
                    //Should really make the AI have a handle on some kind of strategy for all situations
                    actionSelector.IncreaseWeight(pastSituation, pastAction, Mathf.Pow(gamma,i) * reward);
                    DebugText.text = "Last action: " + pastAction + "\n" + "Current Weight: " + actionSelector.GetWeight(pastSituation, pastAction);
                }
            }

            AISituation currentSituation = new AISituation(currentState);
            
            Action action = actionSelector.GetAction(currentSituation);
            bool actionSucceeded = AIPlayer.performAction(action);

            //If we successfully did the action, update the past action and past situation
            if (actionSucceeded)
            {
                pastActions.Add(action);
                pastSituations.Add(currentSituation);

                if(pastActions.Count > backpropDepth && pastSituations.Count > backpropDepth)
                {
                    pastActions.RemoveAt(0);
                    pastSituations.RemoveAt(0);
                }
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