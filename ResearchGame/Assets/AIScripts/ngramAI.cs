using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class ngramAI : MonoBehaviour
{
    public string playerProfileName;

    //Player controlledPlayer;
    Player AIPlayer;
    Player Opponent;

    private List<GameSnapshot> priorSnapshots;

    void Start()
    {
        //controlledPlayer = GameManager.Players[0];
        AIPlayer = GameManager.instance.p2;
        AIPlayer.AIControlled = true;

        AIPlayer.sprite.color = Color.gray;

        priorSnapshots = Session.RetrievePlayerSession(playerProfileName);
        priorSnapshots = priorSnapshots.OrderBy(x => x.frameTaken).ToList();

        Debug.Log(priorSnapshots.Count);
    }

    int actionCount = 0;
    void Update()
    {
    }


    //Encapsulate the state of the opponent player, reduced to easily identifiable enums
    void getPlayerState()
    {
        
    }
}

public enum xDistance
{
    Far,
    Near,
    Adjacent
}

public enum yDistance
{
    Far,
    Near,
    Level
}

public enum Health
{
    High,
    Med,
    Low
}

public enum Cornered
{
    yes,
    no
}