using UnityEngine;
using System.Collections;

[System.Serializable]
public class GameSnapshot {

    public Action actionTaken;

    public int timeRemaining;

    private float p1Health;
    private float p2Health;

    private float xDistance;
    private float yDistance;

    private float p1CornerDistance;
    private float p2CornerDistance;


    //Planned additional features
    private float frameAdvantage;

    public GameSnapshot(Player p1, Player p2, int timeRemaining, Action p2Action)
    {
        this.actionTaken = p2Action;

        this.timeRemaining = timeRemaining;

        p1Health = p1.health;
        p2Health = p2.health;


        //Remember, in this scenario we are player 2 reacting to the actions of player 1
        xDistance = p2.transform.position.x - p1.transform.position.x;
        yDistance = p2.transform.position.y - p1.transform.position.y;

        p1CornerDistance = getCornerDistance(p1.transform.position.x);
        p2CornerDistance = getCornerDistance(p2.transform.position.x);
    }

    override public string ToString()
    {
        return actionTaken.ToString();
    }

    //Insert helper functions here for getting various atributes
    public float getCornerDistance(float xPosition)
    {
        float leftCornerDist = Mathf.Abs(xPosition - (-13));
        float rightCornerDist = Mathf.Abs(13-xPosition);

        return Mathf.Min(leftCornerDist, rightCornerDist);
    }

    //Insert a way of comparing attributes;
    public float snapshotDistance(Player p1, AIPlayer p2, float timeRemaining)
    {
        float deltaTime = this.timeRemaining - timeRemaining;

        float deltaP1Health = this.p1Health - p1.health;
        float deltaP2Health = this.p2Health - p2.health;


        //Remember, in this scenario we are player 2 reacting to the actions of player 1
        float deltaXDistance = this.xDistance - (p2.transform.position.x - p1.transform.position.x);
        float deltaYDistance = this.yDistance - (p2.transform.position.y - p1.transform.position.y);

        float deltaP1CornerDistance = this.p1CornerDistance - getCornerDistance(p1.transform.position.x);
        float deltaP2CornerDistance = this.p2CornerDistance - getCornerDistance(p2.transform.position.x);

        return Mathf.Pow(Mathf.Pow(deltaTime, 2.0f),  0.5f);
    }
}