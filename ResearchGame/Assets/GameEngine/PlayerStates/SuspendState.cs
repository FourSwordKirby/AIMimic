using UnityEngine;
using System.Collections;

public class SuspendState : State<Player>
{
    private Player player;

    private float frameDuration;
    private float frameCounter;

    private State<Player> hiddenState;

    public Vector3 origPos;
    public Quaternion origRot;

    public Vector3 origVel;
    public float origAngleVel;

    public SuspendState(Player playerInstance, StateMachine<Player> fsm, float duration, State<Player> state) : base(playerInstance, fsm)
    {
        player = playerInstance;

        this.frameDuration = duration;
        this.hiddenState = state;

        origPos = player.transform.position;
        origRot = player.transform.rotation;
        origVel = player.selfBody.velocity;
        origAngleVel = player.selfBody.angularVelocity;
    }


    override public void Enter()
    {
        if (!player.grounded)
            player.selfBody.simulated = false;
        else
            player.selfBody.drag = 0.0f;
    }

    override public void Execute()
    {
        frameCounter++;

        if (frameCounter > frameDuration)
        {
            player.ActionFsm.ResumeState();
        }
    }

    override public void FixedExecute()
    {

        float xNew = Mathf.Max(0.0f, Mathf.Abs(player.selfBody.velocity.x) - 0.1f);
        player.selfBody.velocity = new Vector2(Mathf.Sign(player.selfBody.velocity.x) * xNew, player.selfBody.velocity.y);
    }

    override public void Exit()
    {
        if(!player.grounded)
            player.selfBody.simulated = true;
        else
            player.selfBody.drag = 20.0f;

        player.selfBody.velocity = origVel;
        player.selfBody.angularVelocity = origAngleVel;
    }
}
