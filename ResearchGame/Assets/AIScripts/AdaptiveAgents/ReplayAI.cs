using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// An AI which will replay the agent's moves from the latest set of player logs
/// </summary>
public class ReplayAI : MonoBehaviour
{
    public string playerProfileName;
    
    public Player AIPlayer;

    private List<GameEvent> priorSnapshots;

    void Start()
    {
        AIPlayer.AIControlled = true;

        AIPlayer.sprite.color = Color.gray;

        priorSnapshots = Session.RetrievePlayerSession(playerProfileName);
        priorSnapshots = priorSnapshots.OrderBy(x => x.frameTaken).ToList();

        priorSnapshots = priorSnapshots.FindAll(x => AIPlayer.isPlayer1 ? (x.initiatedPlayer == 0) : (x.initiatedPlayer == 1));

        Debug.Log(priorSnapshots.Count);
    }

    int actionCount = 0;
    void Update()
    {
        if (priorSnapshots.Count == 0)
            return;

        if(AIPlayer.enabled)
        {
            Action chosenAction;
            Vector3 AIPosition;

            if (actionCount == 0)
            {
                if (AIPlayer.isPlayer1)
                {
                    chosenAction = priorSnapshots[actionCount].p1Action;
                    AIPosition = priorSnapshots[actionCount].p1Position;
                }
                else
                {
                    chosenAction = priorSnapshots[actionCount].p2Action;
                    AIPosition = priorSnapshots[actionCount].p2Position;
                }

                AIPlayer.transform.position = AIPosition;
                AIPlayer.PerformAction(chosenAction);
            }

            if (actionCount < priorSnapshots.Count-1 && GameManager.instance.currentFrame == priorSnapshots[actionCount].frameTaken)
            {
                //Getting the correct position of the player
                if (AIPlayer.isPlayer1)
                    AIPosition = priorSnapshots[actionCount].p1Position;
                else
                    AIPosition = priorSnapshots[actionCount].p2Position;
                AIPlayer.transform.position = AIPosition;

                //Making the player do the right action
                actionCount++;
                if (AIPlayer.isPlayer1)
                    chosenAction = priorSnapshots[actionCount].p1Action;
                else
                    chosenAction = priorSnapshots[actionCount].p2Action;
                AIPlayer.PerformAction(chosenAction);
            }
        }
    }
}
