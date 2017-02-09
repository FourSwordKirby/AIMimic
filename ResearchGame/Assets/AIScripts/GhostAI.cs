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
            GhostAISituation situation = new GhostAISituation(snapshot);
            print(situation);


            if (!frequencyTable.ContainsKey(situation))                      
                frequencyTable.Add(situation, new List<Action>());
            frequencyTable[situation].Add(snapshot.p2Action);
        }
    }

    int frameInterval = 5;
    void Update()
    {
        if (!AIPlayer.enabled)
            return; 

        if (GameManager.currentFrame % frameInterval == 0)
        {
            GameSnapshot currentState = getGameState();
            if (currentState == null)
                return;
            GhostAISituation currentSituation = new GhostAISituation(currentState);
            print(currentSituation);


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
                Debug.Log("SUPER RANDOM");
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
        Debug.Log("called");
        return deltaX == situation.deltaX &&
                deltaY == situation.deltaY &&
                opponentStatus == situation.opponentStatus;
    }
    
    /*
    public static bool operator !=(GhostAISituation a, GhostAISituation b)
    {
        //Debug.Log("??");
        //// If parameter cannot be cast to ThreeDPoint return false:
        //GhostAISituation situation = obj as GhostAISituation;
        //if ((object)situation == null)
        //{
        //    return false;
        //}

        // Return true if the fields match:
        return !(a.deltaX == b.deltaX &&
                a.deltaY == b.deltaY &&
                a.opponentStatus == b.opponentStatus);
    }
    */

    public override int GetHashCode()
    {
        return ((int)deltaX + (int)deltaY + (int)opponentStatus);
    }

    public override string ToString()
    {
        return deltaX + " " + deltaY + " " + opponentStatus;
    }
}