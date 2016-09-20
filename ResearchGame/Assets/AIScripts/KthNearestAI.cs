using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class KthNearestAI : MonoBehaviour {
    /// <summary>
    /// Parameter for how many neighbors to look at
    /// </summary>
    public int k;
    
    Player opponentPlayer;
    AIPlayer AIPlayer;

    private List<GameSnapshot> priorSnapshots;

    void Start()
    {
        opponentPlayer = GameManager.Players[0];
        AIPlayer = null;// FIX THIS GameManager.AIPlayers[0];

        priorSnapshots = KthNearestCollector.readFromLog();
    }

    public float actionResponseTime;
    private float counter;


    void Update()
    {
        //You know what, for now the AI does an action every second.
        counter += Time.deltaTime;
        if (counter >= actionResponseTime)
        {
            counter = 0.0f;

            List<GameSnapshot> closestNeighbors = new List<GameSnapshot>(k);
            float closestDistance = float.MaxValue;

            foreach (GameSnapshot snapshot in priorSnapshots)
            {
                float distance = snapshot.snapshotDistance(opponentPlayer, AIPlayer, GameManager.timeRemaining);

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNeighbors.Add(snapshot);
                }
            }

            Action chosenAction = closestNeighbors[closestNeighbors.Count-1].actionTaken;
            switch(chosenAction)
            {
                //HELP FIX THIS TOO
                //case Action.WalkLeft:
                //    AIPlayer.WalkLeft();
                //    break;
                //case Action.WalkRight:
                //    AIPlayer.WalkRight();
                //    break;
                //case Action.JumpNeutral:
                //    AIPlayer.JumpNeutral();
                //    break;
                //case Action.JumpLeft:
                //    AIPlayer.JumpLeft();
                //    break;
                //case Action.JumpRight:
                //    AIPlayer.JumpRight();
                //    break;
                //case Action.Attack:
                //    AIPlayer.Attack();
                //    break;
                //case Action.Block:
                //    AIPlayer.Block();
                //    break;
                //case Action.Idle:
                //    AIPlayer.Idle();
                //    break;
            }

            Debug.Log("Action: " + chosenAction + " was taken at time " + GameManager.timeRemaining);
        }
    }

    //Need to make the player actually do an action here.
}
