using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class GameEvent {

    public int initiatedPlayer; //-1 means not initiated by a player, 0 means p1 and 1 means p2

    public Action p1Action;
    public Action p2Action;

    public PlayerStatus p1Status;
    public PlayerStatus p2Status;

    public float p1Duration;
    public float p2Duration;
    public float frameTaken;

    public float p1Health;
    public float p2Health;

    public bool p1Interrupt;
    public bool p2Interrupt;

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

    public GameEvent()
    {
        //Exists just for the xml serializer
    }


    //Basically, whenever the player we're recording does a move or when the opponent does a move
    //We want to know what the player is doing
    //Example, say the recorded player is moving right, we need the player to move right for every
    //action that the opponent is doing in that time
    public GameEvent(int initiatedPlayer, float frameTaken,
                            Player p1, Player p2, 
                            float p1Duration, float p2Duration,
                            Action p1Action, Action p2Action, 
                            bool p1Interrupt = false, bool p2Interrupt = false)
    {
        this.initiatedPlayer = initiatedPlayer;
        this.frameTaken = frameTaken;

        this.p1Duration = p1Duration;
        this.p2Duration = p2Duration;
        this.p1Action = p1Action;
        this.p2Action = p2Action;

        this.p1Status = StateToStatus(p1);
        this.p2Status = StateToStatus(p2);

        this.p1Interrupt = p1Interrupt;
        this.p2Interrupt = p2Interrupt;

        p1Health = p1.health;
        p2Health = p2.health;

        p1Position = p1.transform.position;
        p2Position = p2.transform.position;

        //Remember, in this scenario we are player 2 reacting to the actions of player 1
        xDistance = Mathf.Abs(p2.effectivePosition.x - p1.effectivePosition.x);
        yDistance = p2.effectivePosition.y - p1.effectivePosition.y;

        p1CornerDistance = getCornerDistance(p1.effectivePosition.x);
        p2CornerDistance = getCornerDistance(p2.effectivePosition.x);

        this.labels = new List<string>();
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

    public PlayerStatus StateToStatus(Player p)
    {
        State<Player> state = p.ActionFsm.CurrentState;

        if (p.ActionFsm.StateStack.Count == 0)
            state = p.ActionFsm.CurrentState;
        else
            state = p.ActionFsm.StateStack.First(x => !(x is SuspendState));

        if (state is IdleState)
        {
            if (!p.isCrouching)
                return PlayerStatus.Stand;
            else
                return PlayerStatus.Crouch;
        }
        if (state is BlockState)
        {
            if (!p.isCrouching)
                return PlayerStatus.Highblock;
            else
                return PlayerStatus.Lowblock;
        }
        else if (state is MovementState)
        {
            return PlayerStatus.Stand;
        }
        else if (state is JumpState)
        {
            return PlayerStatus.Air;
        }
        else if (state is AttackState || state is AirAttackState)
        {
            return PlayerStatus.Attacking;
        }
        else if (state is HitState)
        {
            return PlayerStatus.Hit;
        }
        else if (state is TechState)
        {
            return PlayerStatus.Tech;
        }
        else
            return PlayerStatus.Other;
    }

    //A way to compare attributes
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