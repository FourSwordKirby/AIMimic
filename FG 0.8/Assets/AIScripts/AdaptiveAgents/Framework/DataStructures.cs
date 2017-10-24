using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

[System.Serializable]
public class Transition
{
    public AISituation prior;
    public PerformedAction action;
    public AISituation result;

    public Transition(AISituation prior, PerformedAction action, AISituation result)
    {
        this.prior = prior;
        this.action = action;
        this.result = result;
    }

    public override string ToString()
    {
        return "(" + this.action + ") " + "[" + this.prior + "]" + this.result;
    }
}

[System.Serializable]
public class PerformedAction : System.IEquatable<PerformedAction>
{
    public Action action;
    public int duration; //Number of frames to do this action

    public PerformedAction(Action action, int duration)
    {
        this.action = action;
        this.duration = duration;
    }

    public override int GetHashCode()
    {
        return (int)action + (int)duration;
    }

    public override string ToString()
    {
        return action + " " + duration;
    }

    public bool Equals(PerformedAction performedAction)
    {
        return performedAction.action == this.action &&
                performedAction.duration == this.duration;
    }
}

[System.Serializable]
internal class Effectiveness
{
    public float successes;
    public float failures;
    
    public float Rate()
    {
        return successes / (successes + failures);
    }
}

[System.Serializable]
public class AIAction
{
    public Action action;
    public int freq;
    public float weight;

    public AIAction(Action a)
    {
        action = a;
        freq = 0;
        weight = 1.0f;
    }

    public override string ToString()
    {
        return action + " frequency: " + freq + " weight: " + weight;
    }
}

//The original AI situation class and corresponding situation change class is stored here, 
//we're currently trying to migrate the state representation to using the specific x and y values rather than generalizations
/*
/// <summary>
/// A simplified version of the game state used by the AI. Simplifies things by removing the continuous elements
/// </summary>
/// 
[System.Serializable]
public class AISituation : System.IEquatable<AISituation>
{
    public Side side;
    public xDistance deltaX;
    public yDistance deltaY;

    public xMovement xVel;
    public yMovement yVel;

    public xMovement opponentXVel;
    public yMovement opponentYVel;

    public Health health;
    public Health opponentHealth;

    public Cornered cornered;
    public Cornered opponentCornered;
    public PlayerStatus status;
    public PlayerStatus opponentStatus;

    public AISituation(Snapshot snapshot, bool isPlayer1)
    {
        float xDist;
        float yDist;

        if (isPlayer1)
        {
            xDist = snapshot.xDistance;
            yDist = snapshot.yDistance;
        }
        else
        {
            xDist = -snapshot.xDistance;
            yDist = -snapshot.yDistance;
        }

        //Velocity
        if(isPlayer1)
        {
            if (snapshot.p1Status == PlayerStatus.Moving || snapshot.p1Status == PlayerStatus.Dashing || snapshot.p1Status == PlayerStatus.Tech)
            {
                if (snapshot.p1Vel.x < 0)
                    xVel = xMovement.Left;
                else if (snapshot.p1Vel.x > 0)
                    xVel = xMovement.Right;
                else
                    xVel = xMovement.Neutral;
                yVel = yMovement.Neutral;
            }
            else if (snapshot.p1Status == PlayerStatus.Air)
            {
                if (snapshot.p1Vel.x < 0)
                    xVel = xMovement.Left;
                else if (snapshot.p1Vel.x > 0)
                    xVel = xMovement.Right;
                else
                    xVel = xMovement.Neutral;

                if (snapshot.p1Vel.y < 0)
                    yVel = yMovement.Down;
                else if (snapshot.p1Vel.y > 0)
                    yVel = yMovement.Up;
                else
                    yVel = yMovement.Neutral;
            }
            else
            {
                xVel = xMovement.Neutral;
                yVel = yMovement.Neutral;
            }

            if (snapshot.p2Status == PlayerStatus.Moving || snapshot.p2Status == PlayerStatus.Dashing || snapshot.p2Status == PlayerStatus.Tech)
            {
                if (snapshot.p2Vel.x < 0)
                    opponentXVel = xMovement.Left;
                else if (snapshot.p2Vel.x > 0)
                    opponentXVel = xMovement.Right;
                else
                    opponentXVel = xMovement.Neutral;
                opponentYVel = yMovement.Neutral;
            }
            else if (snapshot.p2Status == PlayerStatus.Air)
            {
                if (snapshot.p2Vel.x < 0)
                    opponentXVel = xMovement.Left;
                else if (snapshot.p2Vel.x > 0)
                    opponentXVel = xMovement.Right;
                else
                    opponentXVel = xMovement.Neutral;

                if (snapshot.p2Vel.y < 0)
                    opponentYVel = yMovement.Down;
                else if (snapshot.p2Vel.y > 0)
                    opponentYVel = yMovement.Up;
                else
                    opponentYVel = yMovement.Neutral;
            }
            else
            {
                opponentXVel = xMovement.Neutral;
                opponentYVel = yMovement.Neutral;
            }
        }
        else
        {
            if (snapshot.p2Status == PlayerStatus.Moving || snapshot.p2Status == PlayerStatus.Dashing || snapshot.p2Status == PlayerStatus.Tech)
            {
                if (snapshot.p2Vel.x < -0.25f)
                    xVel = xMovement.Left;
                else if (snapshot.p2Vel.x > 0.25f)
                    xVel = xMovement.Right;
                else
                    xVel = xMovement.Neutral;
                yVel = yMovement.Neutral;
            }
            else if (snapshot.p2Status == PlayerStatus.Air)
            {
                if (snapshot.p2Vel.x < -0.25f)
                    xVel = xMovement.Left;
                else if (snapshot.p2Vel.x > 0.25f)
                    xVel = xMovement.Right;
                else
                    xVel = xMovement.Neutral;

                if (snapshot.p2Vel.y < -0.25f)
                    yVel = yMovement.Down;
                else if (snapshot.p2Vel.y > 0.25f)
                    yVel = yMovement.Up;
                else
                    yVel = yMovement.Neutral;
            }
            else
            {
                xVel = xMovement.Neutral;
                yVel = yMovement.Neutral;
            }

            if (snapshot.p1Status == PlayerStatus.Moving || snapshot.p1Status == PlayerStatus.Dashing || snapshot.p1Status == PlayerStatus.Tech)
            {
                if (snapshot.p1Vel.x < -0.25f)
                    opponentXVel = xMovement.Left;
                else if (snapshot.p1Vel.x > 0.25f)
                    opponentXVel = xMovement.Right;
                else
                    opponentXVel = xMovement.Neutral;
                opponentYVel = yMovement.Neutral;
            }
            else if (snapshot.p1Status == PlayerStatus.Air)
            {
                if (snapshot.p1Vel.x < -0.25f)
                    opponentXVel = xMovement.Left;
                else if (snapshot.p1Vel.x > 0.25f)
                    opponentXVel = xMovement.Right;
                else
                    opponentXVel = xMovement.Neutral;

                if (snapshot.p1Vel.y < -0.25f)
                    opponentYVel = yMovement.Down;
                else if (snapshot.p1Vel.y > 0.25f)
                    opponentYVel = yMovement.Up;
                else
                    opponentYVel = yMovement.Neutral;
            }
            else
            {
                opponentXVel = xMovement.Neutral;
                opponentYVel = yMovement.Neutral;
            }
        }


        //Side
        if (xDist < 0)
            side = Side.Left;
        else
            side = Side.Right;
        //xDistance
        if (Mathf.Abs(xDist) < 2)
            deltaX = xDistance.Adjacent;
        else if (Mathf.Abs(xDist) < 5)
            deltaX = xDistance.Near;
        else
            deltaX = xDistance.Far;

        //yDistance
        //TODO Fix dumb positioning values with respect to the player rotating while attacking :|
        if (yDist <= -0.8f)
            deltaY = yDistance.Below;
        else if (-0.8f < yDist && yDist <= 0.8f)
            deltaY = yDistance.Level;
        else 
            deltaY = yDistance.Above;

        //Health
        if (0 < snapshot.p1Health && snapshot.p1Health <= 30)
            health = Health.Low;
        else if (30 < snapshot.p1Health && snapshot.p1Health <= 70)
            health = Health.Med;
        else if (70 < snapshot.p1Health && snapshot.p1Health <= 100)
            health = Health.High;

        if (0 < snapshot.p2Health && snapshot.p2Health <= 30)
            opponentHealth = Health.Low;
        else if (30 < snapshot.p2Health && snapshot.p2Health <= 70)
            opponentHealth = Health.Med;
        else if (70 < snapshot.p2Health && snapshot.p2Health <= 100)
            opponentHealth = Health.High;

        //Cornered
        if (isPlayer1)
        {
            cornered =  snapshot.p1CornerDistance < 3 ? Cornered.yes : Cornered.no;
            opponentCornered = snapshot.p2CornerDistance < 3 ? Cornered.yes : Cornered.no;
        }
        else
        {
            cornered = snapshot.p1CornerDistance < 3 ? Cornered.yes : Cornered.no;
            opponentCornered = snapshot.p2CornerDistance < 3 ? Cornered.yes : Cornered.no;
        }

        //Status
        if (isPlayer1)
        {
            status = snapshot.p1Status;
            opponentStatus = snapshot.p2Status;
        }
        else
        {
            status = snapshot.p2Status;
            opponentStatus = snapshot.p1Status;
        }
    }


    //Obsolete code used to not break other pieces of code
    public AISituation(GameEvent gameEvent)
    {
    }

    //Empty constructor used for copying an AISituation
    private AISituation()
    {
    }

    //Filler to stop an angry compiler
    public AISituation(Snapshot currentState)
    {
    }

    public bool Equals(AISituation situation)
    {
        return side == situation.side &&
                deltaX == situation.deltaX &&
                deltaY == situation.deltaY &&
                //Don't account for health for now bc it isn't pertient towards making the AI navigate neutral effectively
                //health == situation.health &&
                //opponentHealth == situation.opponentHealth &&

                xVel == situation.xVel &&
                yVel == situation.yVel &&
                opponentXVel == situation.opponentXVel &&
                opponentYVel == situation.opponentYVel &&
                cornered == situation.cornered &&
                opponentCornered == situation.opponentCornered &&
                status == situation.status &&
                opponentStatus == situation.opponentStatus;
    }

    public override int GetHashCode()
    {
        return (104743 * (int)side + 
                229 * (int)deltaX + 
                541 * (int)deltaY + 
                821 * (int)status +
                821 * (int)opponentStatus);
    }

    public override string ToString()
    {
        return side + " " + 
                deltaX + " " + deltaY + " " +
                xVel + " " + yVel + " " +
                opponentXVel + " " + opponentYVel + " " +
                cornered + " " + opponentCornered + " " +
                status + " " + opponentStatus;
    }

    public AISituation Copy()
    {
        AISituation newSituation = new AISituation();
        newSituation.side = side;
        newSituation.deltaX = deltaX;
        newSituation.deltaY = deltaY;

        newSituation.xVel = xVel;
        newSituation.yVel = yVel;

        newSituation.opponentXVel = opponentXVel;
        newSituation.opponentYVel = opponentYVel;

        newSituation.health = health;
        newSituation.opponentHealth = opponentHealth;
        newSituation.cornered = cornered;
        newSituation.opponentCornered = opponentCornered;
        newSituation.status = status;
        newSituation.opponentStatus = opponentStatus;
        return newSituation;
    }

    //Need to be adjusted with the new variablesa added
    internal static float Similarity(AISituation x, AISituation y)
    {
        SituationChange diff = new SituationChange(x, y);
        return (((diff.sideChange == 0 ? 1 : 0)
            + (diff.deltaXChange == 0 ? 1 : 0)
            + (diff.deltaYChange == 0 ? 1 : 0)
            + (diff.xVelChange == 0 ? 1 : 0)
            + (diff.yVelChange == 0 ? 1 : 0)
            + (diff.opponentXVelChange == 0 ? 1 : 0)
            + (diff.opponentYVelChange == 0 ? 1 : 0)
            + (diff.corneredChange == 0 ? 1 : 0)
            + (diff.opponentCorneredChange == 0 ? 1 : 0)
            + (x.status == y.status ? 1 : 0)
            + (x.opponentStatus == y.opponentStatus ? 1 : 0)) + 1.0f) / 12.0f; //The +1 is used to prevent divide by 0 errors
    }
}

[System.Serializable]
public class SituationChange : System.IEquatable<SituationChange>
{
    public AISituation prior;
    public AISituation result;

    public int sideChange;
    public int deltaXChange;
    public int deltaYChange;

    public int xVelChange;
    public int yVelChange;

    public int opponentXVelChange;
    public int opponentYVelChange;

    public int healthChange;
    public int opponentHealthChange;

    public int corneredChange;
    public int opponentCorneredChange;

    public PlayerStatus resultingStatus;
    public PlayerStatus opponentResultingStatus;

    //public int statusChange;
    //public int opponentStatusChange;

    public override int GetHashCode()
    {
        return (int)Mathf.Sign((float)sideChange - 0.5f) * ((int)deltaXChange + (int)deltaYChange);
    }

    public override string ToString()
    {
        return sideChange + " " + deltaXChange + " " + deltaYChange + " " + resultingStatus + " " + opponentResultingStatus;
    }

    public bool Equals(SituationChange change)
    {
        return sideChange == change.sideChange &&
                deltaXChange == change.deltaXChange &&
                deltaYChange == change.deltaYChange &&
                healthChange == change.healthChange &&
                xVelChange == change.xVelChange && 
                yVelChange == change.yVelChange &&
                opponentXVelChange == change.opponentXVelChange &&
                opponentYVelChange == change.opponentYVelChange &&
                opponentHealthChange == change.opponentHealthChange &&
                corneredChange == change.corneredChange &&
                opponentCorneredChange == change.opponentCorneredChange &&
                resultingStatus == change.resultingStatus &&
                opponentResultingStatus == change.opponentResultingStatus;
    }

    public SituationChange(AISituation prior, AISituation result)
    {
        this.prior = prior;
        this.result = result;

        sideChange = (int)result.side - (int)prior.side;
        deltaXChange = (int)result.deltaX - (int)prior.deltaX;
        deltaYChange = (int)result.deltaY - (int)prior.deltaY;

        xVelChange = (int)result.xVel - (int)prior.xVel;
        yVelChange = (int)result.yVel - (int)prior.yVel;

        opponentXVelChange = (int)result.opponentXVel - (int)prior.opponentXVel;
        opponentYVelChange = (int)result.opponentYVel - (int)prior.opponentYVel;

        healthChange = (int)result.health - (int)prior.health;
        opponentHealthChange = (int)result.health - (int)prior.health;

        corneredChange = (int)result.cornered - (int)prior.cornered;
        opponentCorneredChange = (int)result.opponentCornered - (int)prior.opponentCornered;

        resultingStatus = result.status;
        opponentResultingStatus = result.opponentStatus;
    }

    public static AISituation ApplyChange(AISituation prior, SituationChange change)
    {
        AISituation newSituation = prior.Copy();

        int sideMax = System.Enum.GetValues(typeof(Side)).Cast<int>().Max();
        int xMax = System.Enum.GetValues(typeof(xDistance)).Cast<int>().Max();
        int yMax = System.Enum.GetValues(typeof(yDistance)).Cast<int>().Max();
        int healthMax = System.Enum.GetValues(typeof(Health)).Cast<int>().Max();
        int corneredMax = System.Enum.GetValues(typeof(Cornered)).Cast<int>().Max();
        int statusMax = System.Enum.GetValues(typeof(PlayerStatus)).Cast<int>().Max();

        newSituation.side = (Side)Mathf.Clamp((int)prior.side + (int)change.sideChange, 0, sideMax);
        newSituation.deltaX = (xDistance)Mathf.Clamp((int)prior.deltaX + (int)change.deltaXChange, 0, xMax);
        newSituation.deltaY = (yDistance)Mathf.Clamp((int)prior.deltaY + (int)change.deltaYChange, 0, yMax);
        newSituation.health = (Health)Mathf.Clamp((int)prior.health + (int)change.healthChange, 0, healthMax);
        newSituation.opponentHealth = (Health)Mathf.Clamp((int)prior.opponentHealth + (int)change.opponentHealthChange, 0, healthMax);
        newSituation.cornered = (Cornered)Mathf.Clamp((int)prior.cornered + (int)change.corneredChange, 0, corneredMax);
        newSituation.opponentCornered = (Cornered)Mathf.Clamp((int)prior.opponentCornered + (int)change.opponentCorneredChange, 0, corneredMax);

        newSituation.status = change.resultingStatus;
        newSituation.opponentStatus = change.opponentResultingStatus;
        //You can't apply change on the status bc that's jank and doesn't really work lol
        //newSituation.status = (PlayerStatus)Mathf.Clamp((int)prior.status + (int)change.statusChange, 0, statusMax);
        //newSituation.opponentStatus = (PlayerStatus)Mathf.Clamp((int)prior.opponentStatus + (int)change.opponentStatusChange, 0, statusMax);

        return newSituation;
    }
}
*/


/// <summary>
/// A simplified version of the game state used by the AI. Simplifies things by removing the continuous elements
/// </summary>
/// 
[System.Serializable]
public class AISituation : System.IEquatable<AISituation>
{
    public int xPos;
    public int yPos;

    public int opponentXPos;
    public int opponentYPos;

    public xMovement xVel;
    public yMovement yVel;

    public xMovement opponentXVel;
    public yMovement opponentYVel;

    public Health health;
    public Health opponentHealth;

    public PlayerStatus status;
    public PlayerStatus opponentStatus;

    public AISituation(Snapshot snapshot, bool isPlayer1)
    {
        if (isPlayer1)
        {
            xPos = (int)(snapshot.p1Position.x * 2);
            yPos = (int)(snapshot.p1Position.y);

            opponentXPos = (int)(snapshot.p2Position.x * 2);
            opponentYPos = (int)(snapshot.p2Position.y);
        }
        else
        {

            xPos = (int)(snapshot.p2Position.x * 2);
            yPos = (int)(snapshot.p2Position.y);

            opponentXPos = (int)(snapshot.p1Position.x * 2);
            opponentYPos = (int)(snapshot.p1Position.y);
        }

        //Velocity
        if (isPlayer1)
        {
            if (snapshot.p1Status == PlayerStatus.Moving || snapshot.p1Status == PlayerStatus.Dashing || snapshot.p1Status == PlayerStatus.Tech)
            {
                if (snapshot.p1Vel.x < 0)
                    xVel = xMovement.Left;
                else if (snapshot.p1Vel.x > 0)
                    xVel = xMovement.Right;
                else
                    xVel = xMovement.Neutral;
                yVel = yMovement.Neutral;
            }
            else if (snapshot.p1Status == PlayerStatus.Air)
            {
                if (snapshot.p1Vel.x < 0)
                    xVel = xMovement.Left;
                else if (snapshot.p1Vel.x > 0)
                    xVel = xMovement.Right;
                else
                    xVel = xMovement.Neutral;

                if (snapshot.p1Vel.y < 0)
                    yVel = yMovement.Down;
                else if (snapshot.p1Vel.y > 0)
                    yVel = yMovement.Up;
                else
                    yVel = yMovement.Neutral;
            }
            else
            {
                xVel = xMovement.Neutral;
                yVel = yMovement.Neutral;
            }

            if (snapshot.p2Status == PlayerStatus.Moving || snapshot.p2Status == PlayerStatus.Dashing || snapshot.p2Status == PlayerStatus.Tech)
            {
                if (snapshot.p2Vel.x < 0)
                    opponentXVel = xMovement.Left;
                else if (snapshot.p2Vel.x > 0)
                    opponentXVel = xMovement.Right;
                else
                    opponentXVel = xMovement.Neutral;
                opponentYVel = yMovement.Neutral;
            }
            else if (snapshot.p2Status == PlayerStatus.Air)
            {
                if (snapshot.p2Vel.x < 0)
                    opponentXVel = xMovement.Left;
                else if (snapshot.p2Vel.x > 0)
                    opponentXVel = xMovement.Right;
                else
                    opponentXVel = xMovement.Neutral;

                if (snapshot.p2Vel.y < 0)
                    opponentYVel = yMovement.Down;
                else if (snapshot.p2Vel.y > 0)
                    opponentYVel = yMovement.Up;
                else
                    opponentYVel = yMovement.Neutral;
            }
            else
            {
                opponentXVel = xMovement.Neutral;
                opponentYVel = yMovement.Neutral;
            }
        }
        else
        {
            if (snapshot.p2Status == PlayerStatus.Moving || snapshot.p2Status == PlayerStatus.Dashing || snapshot.p2Status == PlayerStatus.Tech)
            {
                if (snapshot.p2Vel.x < -0.25f)
                    xVel = xMovement.Left;
                else if (snapshot.p2Vel.x > 0.25f)
                    xVel = xMovement.Right;
                else
                    xVel = xMovement.Neutral;
                yVel = yMovement.Neutral;
            }
            else if (snapshot.p2Status == PlayerStatus.Air)
            {
                if (snapshot.p2Vel.x < -0.25f)
                    xVel = xMovement.Left;
                else if (snapshot.p2Vel.x > 0.25f)
                    xVel = xMovement.Right;
                else
                    xVel = xMovement.Neutral;

                if (snapshot.p2Vel.y < -0.25f)
                    yVel = yMovement.Down;
                else if (snapshot.p2Vel.y > 0.25f)
                    yVel = yMovement.Up;
                else
                    yVel = yMovement.Neutral;
            }
            else
            {
                xVel = xMovement.Neutral;
                yVel = yMovement.Neutral;
            }

            if (snapshot.p1Status == PlayerStatus.Moving || snapshot.p1Status == PlayerStatus.Dashing || snapshot.p1Status == PlayerStatus.Tech)
            {
                if (snapshot.p1Vel.x < -0.25f)
                    opponentXVel = xMovement.Left;
                else if (snapshot.p1Vel.x > 0.25f)
                    opponentXVel = xMovement.Right;
                else
                    opponentXVel = xMovement.Neutral;
                opponentYVel = yMovement.Neutral;
            }
            else if (snapshot.p1Status == PlayerStatus.Air)
            {
                if (snapshot.p1Vel.x < -0.25f)
                    opponentXVel = xMovement.Left;
                else if (snapshot.p1Vel.x > 0.25f)
                    opponentXVel = xMovement.Right;
                else
                    opponentXVel = xMovement.Neutral;

                if (snapshot.p1Vel.y < -0.25f)
                    opponentYVel = yMovement.Down;
                else if (snapshot.p1Vel.y > 0.25f)
                    opponentYVel = yMovement.Up;
                else
                    opponentYVel = yMovement.Neutral;
            }
            else
            {
                opponentXVel = xMovement.Neutral;
                opponentYVel = yMovement.Neutral;
            }
        }

        //Health
        if (0 < snapshot.p1Health && snapshot.p1Health <= 30)
            health = Health.Low;
        else if (30 < snapshot.p1Health && snapshot.p1Health <= 70)
            health = Health.Med;
        else if (70 < snapshot.p1Health && snapshot.p1Health <= 100)
            health = Health.High;

        if (0 < snapshot.p2Health && snapshot.p2Health <= 30)
            opponentHealth = Health.Low;
        else if (30 < snapshot.p2Health && snapshot.p2Health <= 70)
            opponentHealth = Health.Med;
        else if (70 < snapshot.p2Health && snapshot.p2Health <= 100)
            opponentHealth = Health.High;

        //Status
        if (isPlayer1)
        {
            status = snapshot.p1Status;
            opponentStatus = snapshot.p2Status;
        }
        else
        {
            status = snapshot.p2Status;
            opponentStatus = snapshot.p1Status;
        }
    }


    //Obsolete code used to not break other pieces of code
    public AISituation(GameEvent gameEvent)
    {
    }

    //Empty constructor used for copying an AISituation
    private AISituation()
    {
    }

    //Filler to stop an angry compiler
    public AISituation(Snapshot currentState)
    {
    }

    public bool Equals(AISituation situation)
    {
        return xPos == situation.xPos &&
                yPos == situation.yPos &&
                opponentXPos == situation.opponentXPos &&
                opponentYPos == situation.opponentYPos &&
                //Don't account for health for now bc it isn't pertient towards making the AI navigate neutral effectively
                //health == situation.health &&
                //opponentHealth == situation.opponentHealth &&

                xVel == situation.xVel &&
                yVel == situation.yVel &&
                opponentXVel == situation.opponentXVel &&
                opponentYVel == situation.opponentYVel &&
                status == situation.status &&
                opponentStatus == situation.opponentStatus;
    }

    public override int GetHashCode()
    {
        return (229 * (int)Mathf.Abs(xPos) +
                541 * (int)Mathf.Abs(xPos) +
                17 * (int)Mathf.Abs(opponentXPos) +
                23 * (int)Mathf.Abs(opponentYPos) +
                821 * (int)status +
                821 * (int)opponentStatus);
    }

    public override string ToString()
    {
        return xPos + " " + yPos + " " +
                opponentXPos + " " + opponentYPos + " " +
                xVel + " " + yVel + " " +
                opponentXVel + " " + opponentYVel + " " +
                status + " " + opponentStatus;
    }

    public AISituation Copy()
    {
        AISituation newSituation = new AISituation();

        newSituation.xPos = xPos;
        newSituation.yPos = yPos;
        newSituation.opponentXPos = opponentXPos;
        newSituation.opponentYPos = opponentYPos;
        newSituation.xVel = xVel;
        newSituation.yVel = yVel;
        newSituation.opponentXVel = opponentXVel;
        newSituation.opponentYVel = opponentYVel;

        newSituation.health = health;
        newSituation.opponentHealth = opponentHealth;

        newSituation.status = status;
        newSituation.opponentStatus = opponentStatus;
        return newSituation;
    }

    //TODO: Need to be adjusted with the new variablesa added
    //Specifically, add the xPos and yPos variables
    internal static float Similarity(AISituation x, AISituation y)
    {
        SituationChange diff = new SituationChange(x, y);
        return ((
            (diff.xPosChange == 0 ? 1 : 0)
            + (diff.yPosChange == 0 ? 1 : 0)
            + (diff.opponentXPosChange == 0 ? 1 : 0)
            + (diff.opponentYPosChange == 0 ? 1 : 0)
            + (diff.xVelChange == 0 ? 1 : 0)
            + (diff.yVelChange == 0 ? 1 : 0)
            + (diff.opponentXVelChange == 0 ? 1 : 0)
            + (diff.opponentYVelChange == 0 ? 1 : 0)
            + (x.status == y.status ? 1 : 0)
            + (x.opponentStatus == y.opponentStatus ? 1 : 0)) + 1.0f) / 11.0f; //The +1 is used to prevent divide by 0 errors
    }

    //Gives the raw difference between the 2 states. Used as a lowerbound for the number of actions that needs to be done to 
    //Go between 2 states.
    internal static float Distance(AISituation x, AISituation y)
    {
        SituationChange diff = new SituationChange(x, y);
        return 
            (Mathf.Abs(diff.xPosChange) + Mathf.Abs(diff.yPosChange)
            //(diff.xPosChange == 0 ? 0 : 1)
            // + (diff.yPosChange == 0 ? 0 : 1)
            + (diff.opponentXPosChange == 0 ? 0 : 1)
            + (diff.opponentYPosChange == 0 ? 0 : 1)
            + (diff.xVelChange == 0 ? 0 : 1)
            + (diff.yVelChange == 0 ? 0 : 1)
            + (diff.opponentXVelChange == 0 ? 0 : 1)
            + (diff.opponentYVelChange == 0 ? 0 : 1)
            + (x.status == y.status ? 0 : 1)
            + (x.opponentStatus == y.opponentStatus ? 0 : 1));
    }
}

[System.Serializable]
public class SituationChange : System.IEquatable<SituationChange>
{
    public AISituation prior;
    public AISituation result;

    public int xPosChange;
    public int yPosChange;
    public int opponentXPosChange;
    public int opponentYPosChange;

    public int xVelChange;
    public int yVelChange;

    public int opponentXVelChange;
    public int opponentYVelChange;

    public int healthChange;
    public int opponentHealthChange;

    //public int corneredChange;
    //public int opponentCorneredChange;

    public PlayerStatus resultingStatus;
    public PlayerStatus opponentResultingStatus;

    //public int statusChange;
    //public int opponentStatusChange;

    public override int GetHashCode()
    {
        return (13*(int)xPosChange + 47*(int)yPosChange + 37 * (int)opponentXPosChange + 23 * (int)opponentYPosChange);
    }

    public override string ToString()
    {
        return xPosChange + " " + yPosChange + " " + opponentXPosChange + " " + opponentYPosChange + " " + resultingStatus + " " + opponentResultingStatus;
    }                                          

    public bool Equals(SituationChange change)
    {
        return xPosChange == change.xPosChange &&
                yPosChange == change.yPosChange &&
                opponentXPosChange == change.opponentXPosChange &&
                opponentYPosChange == change.opponentYPosChange &&
                healthChange == change.healthChange &&
                xVelChange == change.xVelChange && 
                yVelChange == change.yVelChange &&
                opponentXVelChange == change.opponentXVelChange &&
                opponentYVelChange == change.opponentYVelChange &&
                opponentHealthChange == change.opponentHealthChange &&
                resultingStatus == change.resultingStatus &&
                opponentResultingStatus == change.opponentResultingStatus;
    }

    public SituationChange(AISituation prior, AISituation result)
    {
        this.prior = prior;
        this.result = result;

        xPosChange = (int)result.xPos - (int)prior.xPos;
        yPosChange = (int)result.yPos - (int)prior.yPos;

        opponentXPosChange = (int)result.opponentXPos - (int)prior.opponentXPos;
        opponentYPosChange = (int)result.opponentYPos - (int)prior.opponentYPos;

        xVelChange = (int)result.xVel - (int)prior.xVel;
        yVelChange = (int)result.yVel - (int)prior.yVel;

        opponentXVelChange = (int)result.opponentXVel - (int)prior.opponentXVel;
        opponentYVelChange = (int)result.opponentYVel - (int)prior.opponentYVel;

        healthChange = (int)result.health - (int)prior.health;
        opponentHealthChange = (int)result.health - (int)prior.health;

        resultingStatus = result.status;
        opponentResultingStatus = result.opponentStatus;
    }

    public static AISituation ApplyChange(AISituation prior, SituationChange change)
    {
        AISituation newSituation = prior.Copy();

        int sideMax = System.Enum.GetValues(typeof(Side)).Cast<int>().Max();
        int xMax = 20;
        int yMax = 20;
        int xVelMax = System.Enum.GetValues(typeof(xMovement)).Cast<int>().Max();
        int yVelMax = System.Enum.GetValues(typeof(yMovement)).Cast<int>().Max();
        int healthMax = System.Enum.GetValues(typeof(Health)).Cast<int>().Max();
        
        newSituation.xPos = Mathf.Clamp((int)prior.xPos + (int)change.xPosChange, -xMax, xMax);
        newSituation.yPos = Mathf.Clamp((int)prior.yPos + (int)change.yPosChange, 0, yMax);
        newSituation.opponentXPos = Mathf.Clamp((int)prior.opponentXPos + (int)change.opponentXPosChange, -xMax, xMax);
        newSituation.opponentYPos = Mathf.Clamp((int)prior.opponentYPos + (int)change.opponentYPosChange, 0, yMax);

        newSituation.xVel = (xMovement)Mathf.Clamp((int)prior.xVel + (int)change.xVelChange, 0, xVelMax);
        newSituation.yVel = (yMovement)Mathf.Clamp((int)prior.yVel + (int)change.yVelChange, 0, yVelMax);
        newSituation.opponentXVel = (xMovement)Mathf.Clamp((int)prior.opponentXVel + (int)change.opponentXVelChange, 0, xVelMax);
        newSituation.opponentYVel = (yMovement)Mathf.Clamp((int)prior.opponentYVel + (int)change.opponentYVelChange, 0, yVelMax);

        newSituation.health = (Health)Mathf.Clamp((int)prior.health + (int)change.healthChange, 0, healthMax);
        newSituation.opponentHealth = (Health)Mathf.Clamp((int)prior.opponentHealth + (int)change.opponentHealthChange, 0, healthMax);

        newSituation.status = change.resultingStatus;
        newSituation.opponentStatus = change.opponentResultingStatus;
        //You can't apply change on the status bc that's jank and doesn't really work lol
        //newSituation.status = (PlayerStatus)Mathf.Clamp((int)prior.status + (int)change.statusChange, 0, statusMax);
        //newSituation.opponentStatus = (PlayerStatus)Mathf.Clamp((int)prior.opponentStatus + (int)change.opponentStatusChange, 0, statusMax);

        return newSituation;
    }
}