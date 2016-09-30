using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class ReplayAI : MonoBehaviour
{
    /// <summary>
    /// Parameter for how many neighbors to look at
    /// </summary>
    Player controlledPlayer;
    Player AIPlayer;

    private List<GameSnapshot> priorSnapshots;

    void Start()
    {
        controlledPlayer = GameManager.Players[0];
        AIPlayer = GameManager.Players[1];

        AIPlayer.sprite.color = Color.gray;

        priorSnapshots = KthNearestCollector.readFromLog();
        priorSnapshots = priorSnapshots.OrderBy(x => -x.timeRemaining).ToList();

        Debug.Log(priorSnapshots.Count);
    }

    int actionCount = 0;
    void Update()
    {
        if (actionCount < priorSnapshots.Count && GameManager.timeRemaining < priorSnapshots[actionCount].timeRemaining)
        {
            Action chosenAction = priorSnapshots[actionCount].actionTaken;
            actionCount++;

            switch (chosenAction)
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
                    AIPlayer.Walk(Parameters.InputDirection.W);
                    break;
                case Action.JumpRight:
                    AIPlayer.Walk(Parameters.InputDirection.E);
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
        }
    }

    //Need to make the player actually do an action here.
}
