using UnityEngine;
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

    //Implementation of the ghost AI
    //Basically, given the game's state, we look at what the player we're imitating did in that state
    //It will then do the action appropriate to the situation
    //That list of actions is essentially a frequency table.
    private Dictionary<GhostAISituation, List<Action>> frequencyTable
        = new Dictionary<GhostAISituation, List<Action>>();

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
            GhostAISituation state = new GhostAISituation(snapshot);
            if(!frequencyTable.ContainsKey(state))                      
                frequencyTable.Add(state, new List<Action>());
            frequencyTable[state].Add(snapshot.p2Action);
        }
    }

    int frameInterval = 5;
    Action[] currentHistory = new Action[2] { Action.Idle, Action.Idle }; //Dummy 2 gram model used
    void Update()
    {
        if (GameManager.currentFrame % frameInterval == 0)
        {
            GameSnapshot currentState = getGameState();
            Debug.Log(currentState);
            if (currentState == null)
                return;
            GhostAISituation currentSituation = new GhostAISituation(currentState);

            Action action;
            if (frequencyTable.ContainsKey(currentSituation))
            {
                List<Action> sampleActions = frequencyTable[currentSituation];
                if (sampleActions.Count == 0)
                    action = Action.Block;
                else
                    action = sampleActions[Random.Range(0, sampleActions.Count)];
            }
            else
            {
                //action = Action.Block;
                action = (Action)Random.Range(0, System.Enum.GetValues(typeof(Action)).Length);
            }
            AIPlayer.performAction(action);
        }
    }


    //Encapsulate the state of the opponent player, reduced to easily identifiable enums
    GameSnapshot getGameState()
    {
        return dataRecorder.currentSession.snapshots.FindLast(x => true);
    }
}

public class GhostAISituation
{
    public xDistance deltaX;
    public yDistance deltaY;

    public PlayerStatus opponentStatus;

    public GhostAISituation(GameSnapshot snapshot)
    {
        //xDistance
        if(snapshot.xDistance < 2)
            deltaX = xDistance.Adjacent;
        else if (snapshot.xDistance < 4)
            deltaX = xDistance.Near;
        else
            deltaX = xDistance.Far;

        //yDistance
        if (snapshot.xDistance < 0.5f)
            deltaY = yDistance.Level;
        else if (snapshot.xDistance < 1)
            deltaY = yDistance.Near;
        else
            deltaY = yDistance.Far;

        //Status
        opponentStatus = snapshot.p1Status;
    }

    public override bool Equals(System.Object obj)
    {
        // If parameter cannot be cast to ThreeDPoint return false:
        GhostAISituation situation = obj as GhostAISituation;
        if ((object)situation == null)
        {
            return false;
        }

        // Return true if the fields match:
        return deltaX == situation.deltaX &&
                deltaY == situation.deltaY &&
                opponentStatus == situation.opponentStatus;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}