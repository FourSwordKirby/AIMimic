using UnityEngine;
using System.Collections;

public class HitlagState : State<Player>
{
    private Player player;

    private float frameDuration;
    private float frameCounter;

    private State<Player> hiddenState;

    public Vector3 origPos;
    public Quaternion origRot;

    public Vector3 origVel;
    public float origAngleVel;

    public HitlagState(Player playerInstance, StateMachine<Player> fsm, float duration, State<Player> state) : base(playerInstance, fsm)
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
        player.suspended = true;
        if (!player.grounded)
            player.selfBody.simulated = false;
        else
            player.selfBody.drag = 0.0f;

        if (player.transform.position.y < 0)
            player.transform.position = player.transform.position - Vector3.up * player.transform.position.y;
    }

    override public void Execute()
    {
        frameCounter++;

        if (frameCounter > frameDuration)
        {
            player.ActionFsm.ResumeState();
            Debug.Assert(player.ActionFsm.CurrentState == hiddenState);
        }
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
        player.suspended = false;
        if (!player.grounded)
            player.selfBody.simulated = true;
        else
            player.selfBody.drag = 20.0f;

        player.selfBody.velocity = origVel;
        player.selfBody.angularVelocity = origAngleVel;
    }
}
