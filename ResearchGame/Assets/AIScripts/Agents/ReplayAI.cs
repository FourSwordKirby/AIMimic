using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class ReplayAI : MonoBehaviour
{
    public string playerProfileName;

    //Player controlledPlayer;
    Player AIPlayer;

    private List<GameSnapshot> priorSnapshots;

    void Start()
    {
        //controlledPlayer = GameManager.Players[0];
        AIPlayer = GameManager.instance.p2;
        AIPlayer.AIControlled = true;

        AIPlayer.sprite.color = Color.gray;

        priorSnapshots = Session.RetrievePlayerSession(playerProfileName);
        priorSnapshots = priorSnapshots.OrderBy(x => x.frameTaken).ToList();

        priorSnapshots = priorSnapshots.FindAll(x => x.initiatedPlayer == 1);

        Debug.Log(priorSnapshots.Count);
    }

    int actionCount = 0;
    void Update()
    {
        if (actionCount < priorSnapshots.Count && GameManager.currentFrame == priorSnapshots[actionCount].frameTaken)
        {
            Action chosenAction = priorSnapshots[actionCount].p2Action;
            Vector3 AIPosition = priorSnapshots[actionCount].p2Position;

            actionCount++;
            AIPlayer.transform.position = AIPosition;
            AIPlayer.performAction(chosenAction);
        }
    }

    //Need to make the player actually do an action here.
}
