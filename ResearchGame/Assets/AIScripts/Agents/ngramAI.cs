using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//The goal of this AI agent is to look at a previous set of playsessions between  the plaer Test and an unknown other player
//The AI's behavior patterns should mimic Test using ngrams. That is, it should do the same moves that Test does in certain situations
//given some history.

public class ngramAI : MonoBehaviour
{
    public string playerProfileName;
    public DataRecorder dataRecorder;

    Player AIPlayer;
    Player Opponent;

    private List<GameSnapshot> priorSnapshots;

    //Currently using most basic kind of ngram, the kind where the player does a certain kind of move repeatedly
    //The key is the string version of an array of previous moves. The value is a the set of all actions that have been done with that history
    //That list of actions is essentially a frequency table.
    private Dictionary<string, List<Action>> ngramHistory = new Dictionary<string, List<Action>>();

    void Start()
    {
        //controlledPlayer = GameManager.Players[0];
        AIPlayer = GameManager.instance.p2;
        AIPlayer.AIControlled = true;

        AIPlayer.sprite.color = Color.green;

        priorSnapshots = Session.RetrievePlayerSession(playerProfileName);
        priorSnapshots = priorSnapshots.OrderBy(x => x.frameTaken).ToList();

        Debug.Log(priorSnapshots.Count);

        Action[] currentHistory = new Action[2] { Action.Stand, Action.Stand }; //Dummy 2 gram model used
        for(int i = 0; i < priorSnapshots.Count; i++)
        {
            GameSnapshot snapshot = priorSnapshots[i];
            string historyString = currentHistory[0] + " " + currentHistory[1];
            if(!ngramHistory.ContainsKey(historyString))
                ngramHistory.Add(historyString, new List<Action>());
            ngramHistory[historyString].Add(snapshot.p2Action);
            currentHistory[0] = currentHistory[1];
            currentHistory[1] = snapshot.p2Action;
        }
    }

    int frameInterval = 5;
    Action[] currentHistory = new Action[2] { Action.Stand, Action.Stand }; //Dummy 2 gram model used
    void Update()
    {
        if (GameManager.currentFrame % frameInterval == 0)
        {
            GameSnapshot currentState = getGameState();
            if (currentState == null)
                return;

            currentHistory[0] = currentHistory[1];
            currentHistory[1] = currentState.p2Action;
            string historyString = currentHistory[0] + " " + currentHistory[1];
            List<Action> freqTable = ngramHistory[historyString];
            Action action;
            if (freqTable.Count == 0)
                action = Action.StandBlock;
            else
                action = freqTable[Random.Range(0, freqTable.Count)];
            AIPlayer.PerformAction(action);
        }
    }


    //Encapsulate the state of the opponent player, reduced to easily identifiable enums
    GameSnapshot getGameState()
    {
        return dataRecorder.currentSession.snapshots.FindLast(x => true);
    }
}