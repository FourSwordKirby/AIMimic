using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;


public class GameSnapshot {

    public int initiatedPlayer; //-1 means not initiated by a player, 0 means p1 and 1 means p2

    public Action p1Action;
    public Action p2Action;

    public float frameDelay;
    public float frameTaken;

    public float p1Health;
    public float p2Health;

    public Vector3 p1Position;
    public Vector3 p2Position;

    public float xDistance;
    public float yDistance;

    public float p1CornerDistance;
    public float p2CornerDistance;

    //Used to label this action/sequence of actions
    public List<string> labels;

    //Planned additional features
    //private float frameAdvantage;

    public GameSnapshot()
    {
        //Exists just for the xml serializer
    }


    //Basically, whenever the player we're recording does a move or when the opponent does a move
    //We want to know what the player is doing
    //Example, say the recorded player is moving right, we need the player to move right for every
    //action that the opponent is doing in that time
    public GameSnapshot(int initiatedPlayer, Player p1, Player p2, float delay, float frameTaken, Action p1Action, Action p2Action)
    {
        this.initiatedPlayer = initiatedPlayer;

        p1Health = p1.health;
        p2Health = p2.health;

        p1Position = p1.transform.position;
        p2Position = p2.transform.position;

        //Remember, in this scenario we are player 2 reacting to the actions of player 1
        xDistance = p2.effectivePosition.x - p1.effectivePosition.x;
        yDistance = p2.effectivePosition.y - p1.effectivePosition.y;

        p1CornerDistance = getCornerDistance(p1.effectivePosition.x);
        p2CornerDistance = getCornerDistance(p2.effectivePosition.x);

        this.frameDelay = delay;
        this.frameTaken = frameTaken;
        this.p2Action = p2Action;

        this.labels = new List<string>();
        labels.Add("wtf");
    }

    override public string ToString()
    {
        return p2Action.ToString();
    }

    //Insert helper functions here for getting various atributes
    public float getCornerDistance(float xPosition)
    {
        float leftCornerDist = Mathf.Abs(xPosition - (-10));
        float rightCornerDist = Mathf.Abs(10-xPosition);

        return Mathf.Min(leftCornerDist, rightCornerDist);
    }

    //Insert a way of comparing attributes;
    public float snapshotDistance(Player p1, Player p2, float timeRemaining)
    {
     
        float deltaTime = this.frameTaken - timeRemaining;
     
        //float deltaP1Health = this.p1Health - p1.health;
        //float deltaP2Health = this.p2Health - p2.health;
     
     
        //Remember, in this scenario we are player 2 reacting to the actions of player 1
        float deltaXDistance = this.xDistance - (p2.effectivePosition.x - p1.effectivePosition.x);
        float deltaYDistance = this.yDistance - (p2.effectivePosition.y - p1.effectivePosition.y);
     
        float deltaP1CornerDistance = this.p1CornerDistance - getCornerDistance(p1.effectivePosition.x);
        float deltaP2CornerDistance = this.p2CornerDistance - getCornerDistance(p2.effectivePosition.x);

        return Mathf.Pow(Mathf.Pow(deltaTime, 2.0f),  0.5f) + deltaXDistance + deltaYDistance + deltaP1CornerDistance + deltaP2CornerDistance;
    }
}