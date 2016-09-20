using UnityEngine;
using System.Collections;

[System.Serializable]
public class GameSnapshot {

    public Action actionTaken;

    public int timeRemaining;

    public float p1Health;
    public float p2Health;

    public float xDistance;
    public float yDistance;

    public float p1CornerDistance;
    public float p2CornerDistance;


    //Planned additional features
    private float frameAdvantage;

    public GameSnapshot()
    {
        //Exists just for the xml serializer
    }

    public GameSnapshot(Player p1, Player p2, int timeRemaining, Action p2Action)
    {
        this.actionTaken = p2Action;

        this.timeRemaining = timeRemaining;

        p1Health = p1.health;
        p2Health = p2.health;


        //Remember, in this scenario we are player 2 reacting to the actions of player 1
        xDistance = p2.effectivePosition.x - p1.effectivePosition.x;
        yDistance = p2.effectivePosition.y - p1.effectivePosition.y;

        p1CornerDistance = getCornerDistance(p1.effectivePosition.x);
        p2CornerDistance = getCornerDistance(p2.effectivePosition.x);
    }

    override public string ToString()
    {
        return actionTaken.ToString();
    }

    //Insert helper functions here for getting various atributes
    public float getCornerDistance(float xPosition)
    {
        float leftCornerDist = Mathf.Abs(xPosition - (-10));
        float rightCornerDist = Mathf.Abs(10-xPosition);

        return Mathf.Min(leftCornerDist, rightCornerDist);
    }

    //Insert a way of comparing attributes;
    public float snapshotDistance(Player p1, AIPlayer p2, float timeRemaining)
    {
    //{
    //    float deltaTime = this.timeRemaining - timeRemaining;

    //    float deltaP1Health = this.p1Health - p1.health;
    //    float deltaP2Health = this.p2Health - p2.health;


    //    //Remember, in this scenario we are player 2 reacting to the actions of player 1
    //    float deltaXDistance = this.xDistance - (p2.effectivePosition.x - p1.effectivePosition.x);
    //    float deltaYDistance = this.yDistance - (p2.effectivePosition.y - p1.effectivePosition.y);

    //    float deltaP1CornerDistance = this.p1CornerDistance - getCornerDistance(p1.effectivePosition.x);
    //    float deltaP2CornerDistance = this.p2CornerDistance - getCornerDistance(p2.effectivePosition.x);

        //return Mathf.Pow(Mathf.Pow(deltaTime, 2.0f),  0.5f);
        return 0;
    }
}