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

    private List<GameEvent> priorSnapshots;

    void Start()
    {
        controlledPlayer = GameManager.instance.p1;
        AIPlayer = GameManager.instance.p2;
        AIPlayer.AIControlled = true;

        AIPlayer.sprite.color = Color.green;

        priorSnapshots = Session.RetrievePlayerSession(playerProfileName);
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

            List<GameEvent> closestNeighbors = new List<GameEvent>(k);
            float closestDistance = float.MaxValue;

            foreach (GameEvent snapshot in priorSnapshots)
            {
                float distance = snapshot.snapshotDistance(controlledPlayer, AIPlayer, GameManager.instance.timeRemaining);

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNeighbors.Add(snapshot);
                }
            }

            Action chosenAction = Action.Stand;
            if (closestDistance < distanceThreshold)
            {
                chosenAction = closestNeighbors[closestNeighbors.Count - 1].p2Action;
            }

            AIPlayer.PerformAction(chosenAction);

            Debug.Log("Action: " + chosenAction + " was taken at time " + GameManager.instance.timeRemaining);
        }
    }

    //Need to make the player actually do an action here.
}
