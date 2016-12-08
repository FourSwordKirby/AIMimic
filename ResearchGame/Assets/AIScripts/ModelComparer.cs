using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//This script will look at the players current behavior and dynamically compare it to the previous set of actions
public class ModelComparer : MonoBehaviour {

    public string playerProfileName;

    public Text P1Info;
    public Text P2Info;

    Player controlledPlayer;
    Player opponentPlayer;

    private List<List<GameSnapshot>> priorSessions;

    void Start()
    {
        controlledPlayer = GameManager.instance.p1;
        opponentPlayer = GameManager.instance.p2;

        priorSessions = Session.RetrievePlayerHistory(playerProfileName);
    }

    void Update()
    {
        float distance = 0.0f;
        foreach(List<GameSnapshot> session in priorSessions)
        {
            foreach (GameSnapshot snapshot in session)
            {
                if (GameManager.timeRemaining - snapshot.timeRemaining < 1.0f)
                    distance += snapshot.snapshotDistance(controlledPlayer, opponentPlayer, GameManager.timeRemaining);
            }
        }

        P2Info.text = distance.ToString();
    }
}
