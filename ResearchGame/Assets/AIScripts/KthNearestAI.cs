using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class KthNearestAI : MonoBehaviour {
    /// <summary>
    /// Parameter for how many neighbors to look at
    /// </summary>
    public int k;
    public string playerProfileName;

    Player controlledPlayer;
    Player AIPlayer;

    private List<GameSnapshot> priorSnapshots;

    void Start()
    {
        controlledPlayer = GameManager.Players[0];
        AIPlayer = GameManager.Players[1];

        AIPlayer.sprite.color = Color.green;

        priorSnapshots = Session.readFromLog(playerProfileName);
    }

    public float actionResponseTime;
    private float counter;
    private float distanceThreshold = 2;//Very termporary. may need to datamine????

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
                float distance = snapshot.snapshotDistance(controlledPlayer, AIPlayer, GameManager.timeRemaining);

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNeighbors.Add(snapshot);
                }
            }

            Action chosenAction = Action.Idle;
            if (closestDistance < distanceThreshold)
            {
                chosenAction = closestNeighbors[closestNeighbors.Count - 1].actionTaken;
            }


            switch(chosenAction)
            {
                case Action.WalkLeft:
                    AIPlayer.Walk(Parameters.InputDirection.W);
                    break;
                case Action.WalkRight:
                    AIPlayer.Walk(Parameters.InputDirection.E);
                    break;
                case Action.JumpNeutral:
                    AIPlayer.Jump(Parameters.InputDirection.N);
                    break;
                case Action.JumpLeft:
                    AIPlayer.Jump(Parameters.InputDirection.W);
                    break;
                case Action.JumpRight:
                    AIPlayer.Jump(Parameters.InputDirection.E);
                    break;
                case Action.Attack:
                    AIPlayer.Attack();
                    break;
                case Action.Block:
                    AIPlayer.Block();
                    break;
                case Action.Idle:
                    AIPlayer.Idle();
                    break;
            }

            Debug.Log("Action: " + chosenAction + " was taken at time " + GameManager.timeRemaining);
        }
    }

    //Need to make the player actually do an action here.
}
