using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The goal of this AI agent is to look at a previous set of playsessions between  the plaer Test and an unknown other player
/// The AI's behavior patterns should mimic Test using ngrams. That is, it should do the same moves that Test does in certain situations
/// given some history.
/// </summary>
public class NgramAI : MonoBehaviour
{
    public string playerProfileName;
    public int logNumber;
    public EventRecorder dataRecorder;

    public Player AIPlayer;
    public Player Opponent;

    private List<GameEvent> priorSnapshots;

    //Currently using most basic kind of ngram, the kind where the player does a certain kind of move repeatedly
    //The key is the string version of an array of previous moves. The value is a the set of all actions that have been done with that history
    //That list of actions is essentially a frequency table.
    private Dictionary<string, List<Action>> ngramHistory = new Dictionary<string, List<Action>>();
    
    void Start()
    {
        AIPlayer.AIControlled = true;

        AIPlayer.sprite.color = Color.green;

        priorSnapshots = Session.RetrievePlayerSession(playerProfileName, logNumber);
        priorSnapshots = priorSnapshots.OrderBy(x => x.frameTaken).ToList();

        Debug.Log(priorSnapshots.Count);

        Action[] currentHistory = new Action[2] { Action.Stand, Action.Stand }; //Dummy 2 gram model used
        for(int i = 0; i < priorSnapshots.Count; i++)
        {
            GameEvent snapshot = priorSnapshots[i];
            string historyString = currentHistory[0] + " " + currentHistory[1];
            if(!ngramHistory.ContainsKey(historyString))
                ngramHistory.Add(historyString, new List<Action>());
            ngramHistory[historyString].Add(snapshot.p1Action);
            currentHistory[0] = currentHistory[1];
            currentHistory[1] = snapshot.p1Action;
        }
    }

    int frameInterval = 5;
    Action[] currentHistory = new Action[2] { Action.Stand, Action.Stand }; //Dummy 2 gram model used

    public Action lastAction;
    void Update()
    {
        if (GameManager.instance.currentFrame % frameInterval == 0)
        {
            GameEvent currentState = getGameState();
            if (currentState == null)
                return;

            currentHistory[0] = currentHistory[1];
            currentHistory[1] = currentState.p1Action;
            string historyString = currentHistory[0] + " " + currentHistory[1];
            if (!ngramHistory.ContainsKey(historyString))
                return;

            List<Action> freqTable = ngramHistory[historyString];
            Action action;
            if (freqTable.Count == 0)
                action = Action.StandBlock;
            else
                action = freqTable[Random.Range(0, freqTable.Count)];

            if(action == Action.Stand || action == Action.Crouch || action == Action.WalkLeft || action == Action.WalkRight)
            {
                if (action == lastAction)
                    return;
            }
            PerformAction(action);
        }
    }


    //Encapsulate the state of the opponent player, reduced to easily identifiable enums
    GameEvent getGameState()
    {
        return dataRecorder.currentSession.snapshots.FindLast(x => true);
    }

    public void PerformAction(Action action)
    {
        //Edge case which is not covered by the base system due to how we're tracking player actions
        //Prevents the AI from standing or crouching once commiting itself to an attack
        if (AIPlayer.ActionFsm.CurrentState is AttackState)
        {
            if (action == Action.Stand || action == Action.Crouch)
                return;
        }

        bool actionSucceeded = AIPlayer.PerformAction(action);
        if (actionSucceeded)
            lastAction = action;
    }
}