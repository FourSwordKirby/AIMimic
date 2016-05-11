using UnityEngine;
using System.Collections;

public class MovementStartupState : State<Player> {

    private Player player;

    private float duration;

    private const float startup_duration = 0.2f; //Around 6 frames for a60 fps game?

    public MovementStartupState(Player playerInstance, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        duration = startup_duration;
        player = playerInstance;
    }

    override public void Enter()
    {
        player.anim.SetBool("Walking", true);
        Debug.Log("entered MovementStart state");
        return;
    }

    override public void Execute()
    {
        duration -= Time.deltaTime;
        if (duration < 0)
            player.ActionFsm.ChangeState(new MovementState(player, player.ActionFsm));

        Parameters.InputDirection potential_direction = Controls.getInputDirection(player);
        /*
        if(potential_direction != Parameters.InputDirection.Stop)
            player.direction = Controls.getInputDirection(player);
         */
    }

    override public void FixedExecute()
    {
        if (player.GetComponent<Rigidbody2D>().velocity.magnitude >= player.movementSpeed)
            player.GetComponent<Rigidbody2D>().velocity = player.GetComponent<Rigidbody2D>().velocity.normalized * player.movementSpeed;
        else
        {
            switch (player.direction)
            {
                    /*
                case Parameters.InputDirection.North:
                    player.GetComponent<Rigidbody2D>().velocity += new Vector2(0, 1) * player.movementSpeed * 0.2f;
                    break;                                      
                case Parameters.InputDirection.NorthEast:
                    player.GetComponent<Rigidbody2D>().velocity += new Vector2(Mathf.Sin(Mathf.PI / 2), Mathf.Sin(Mathf.PI / 2)) * player.movementSpeed * 0.2f;
                    break;                                      
                case Parameters.InputDirection.East:
                    player.GetComponent<Rigidbody2D>().velocity += new Vector2(1, 0) * player.movementSpeed * 0.2f;
                    break;                                      
                case Parameters.InputDirection.SouthEast:
                    player.GetComponent<Rigidbody2D>().velocity += new Vector2(Mathf.Sin(Mathf.PI / 2), Mathf.Sin(3 * Mathf.PI / 2)) * player.movementSpeed * 0.2f;
                    break;                                      
                case Parameters.InputDirection.South:
                    player.GetComponent<Rigidbody2D>().velocity += new Vector2(0, -1) * player.movementSpeed * 0.2f;
                    break;                                      
                case Parameters.InputDirection.SouthWest:
                    player.GetComponent<Rigidbody2D>().velocity += new Vector2(Mathf.Sin(3 * Mathf.PI / 2), Mathf.Sin(3 * Mathf.PI / 2)) * player.movementSpeed * 0.2f;
                    break;                                      
                case Parameters.InputDirection.West:
                    player.GetComponent<Rigidbody2D>().velocity += new Vector2(-1, 0) * player.movementSpeed * 0.2f;
                    break;                                      
                case Parameters.InputDirection.NorthWest:
                    player.GetComponent<Rigidbody2D>().velocity += new Vector2(Mathf.Sin(3 * Mathf.PI / 2), Mathf.Sin(Mathf.PI / 2)) * player.movementSpeed * 0.2f;
                    break;                  
                     */
            }
        }
    }

    override public void Exit()
    {
        Debug.Log("exited Movement Startup state");
        return;
    }
}
