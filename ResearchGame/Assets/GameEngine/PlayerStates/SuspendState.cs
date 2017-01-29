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
        if(!player.grounded)
            player.selfBody.simulated = false;
        player.selfBody.isKinematic = true;
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
        //player.transform.position = origPos;
        //player.transform.rotation = origRot;
    }

    override public void Exit()
    {
        if(!player.grounded)
            player.selfBody.simulated = true;
        player.selfBody.isKinematic = false;
        player.selfBody.velocity = origVel;
        player.selfBody.angularVelocity = origAngleVel;
    }
}
