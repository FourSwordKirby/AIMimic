using UnityEngine;
using System.Collections;

public class SuspendState : State<Player>
{
    private Player player;

    private float duration;
    private float timer;

    private State<Player> hiddenState;

    public Vector3 origPos;
    public Quaternion origRot;

    public Vector3 origVel;
    public float origAngleVel;

    public SuspendState(Player playerInstance, StateMachine<Player> fsm, float duration, State<Player> state) : base(playerInstance, fsm)
    {
        player = playerInstance;

        this.duration = duration;
        this.hiddenState = state;

        origPos = player.transform.position;
        origRot = player.transform.rotation;
        origVel = player.selfBody.velocity;
        origAngleVel = player.selfBody.angularVelocity;
    }

    override public void Enter()
    {
        player.selfBody.isKinematic = true;
    }

    override public void Execute()
    {
        timer += Time.deltaTime;

        if (timer > duration)
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
        player.selfBody.isKinematic = false;
        player.selfBody.velocity = origVel;
        player.selfBody.angularVelocity = origAngleVel;
    }
}
