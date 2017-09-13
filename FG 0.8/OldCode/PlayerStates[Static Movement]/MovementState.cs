using UnityEngine;
using System.Collections;

public class MovementState : State<Player> {

    private Player player;
    private Vector3 targetLocation;

    //Used primarily by the datarecorder
    public int moveDir;

    //TODO: INPUT BUFFER FOR NEXT MOVE
    private Action nextAction = Action.Stand;


    public MovementState(Player playerInstance, StateMachine<Player> fsm, Vector3 targetLocation)
        : base(playerInstance, fsm)
    {
        float displacement = targetLocation.x - playerInstance.effectivePosition.x;
        moveDir = displacement < 0 ? -1 : displacement > 0 ? 1 : 0;

        player = playerInstance;
        this.targetLocation = new Vector3(Mathf.Clamp(targetLocation.x, - 10, 10), Mathf.Clamp(targetLocation.y, 0, 10), 0);
    }

    override public void Enter()
    {
        Vector2 movementVector = (targetLocation - player.transform.position).normalized * player.movementSpeed;
        player.selfBody.velocity = movementVector;
    }

    override public void Execute()
    {
        Parameters.InputDirection dir = Controls.getInputDirection(player);
        //Only activate this near the end
        //Also encapsulate all of this in some generic everyclass thing lol
        //Change this to be a jump button
        nextAction = Action.Stand;
        if (Controls.jumpInputHeld(player))
        {
            if (dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE)
                nextAction = Action.JumpRight;
            else if (dir == Parameters.InputDirection.NW || dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW)
                nextAction = Action.JumpLeft;
            else
                nextAction = Action.JumpNeutral;
            return;
        }

        if (dir != Parameters.InputDirection.None)
        {
            if (dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE)
                nextAction = Action.WalkRight;
            else if (dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW)
                nextAction = Action.WalkLeft;
            return;
        }

        if (Controls.attackInputDown(player))
        {
            nextAction = Action.Attack;
        }
    }

    override public void FixedExecute()
    {
        /* There's an awful input delay thing I think but I don't know D:
         * Cross test with blazblue plz
        if(nextAction != Action.WalkLeft || nextAction != Action.WalkRight)
        {
            player.selfBody.velocity = Vector2.zero;
            player.performAction(nextAction);
            return;
        }*/


        if (Mathf.Sign((targetLocation - player.transform.position).x) != Mathf.Sign(player.selfBody.velocity.x))
        {
            targetLocation = player.effectivePosition;
        }
        if (Vector2.Distance(player.transform.position, targetLocation) < Parameters.positionLeeway || player.selfBody.velocity.x == 0)
        {
            player.selfBody.velocity = Vector2.zero;
            player.transform.position = targetLocation;
            player.performAction(nextAction);
        }
    }

    override public void Exit()
    {
        return;
    }
}
