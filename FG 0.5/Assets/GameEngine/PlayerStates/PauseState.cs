using UnityEngine;
using System.Collections;

public class PauseState : State<Player>
{
    private Player player;


    private State<Player> hiddenState;

    public bool origSim;
    public Vector3 origPos;
    public Quaternion origRot;

    public Vector3 origVel;
    public float origAngleVel;

    public PauseState(Player playerInstance, StateMachine<Player> fsm, State<Player> state) : base(playerInstance, fsm)
    {
        player = playerInstance;
        this.hiddenState = state;

        origSim = player.selfBody.simulated;

        origVel = player.selfBody.velocity;
        origAngleVel = player.selfBody.angularVelocity;
    }


    override public void Enter()
    {
        player.selfBody.simulated = false;
        Debug.Log(hiddenState);
        Debug.Log(origSim);
    }

    override public void Execute()
    {
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
        player.selfBody.simulated = origSim;

        player.selfBody.velocity = origVel;
        player.selfBody.angularVelocity = origAngleVel;
    }
}
