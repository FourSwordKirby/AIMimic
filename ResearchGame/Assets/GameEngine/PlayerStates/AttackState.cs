using UnityEngine;
using System.Collections;

public class AttackState : State<Player>
{
    private Player player;
    private GameObject meleeHitbox;

    private float attackDistance;

    public float startup;
    public float duration;
    public float endlag;
    
    private float frameCounter;

    private Vector3 startPosition;
    private Vector3 endPosition;

    public AttackState(Player playerInstance, StateMachine<Player> fsm, int comboCount = 0) : base(playerInstance, fsm)
    {
        player = playerInstance;
        player.comboCount = comboCount;

        meleeHitbox = player.hitboxManager.getHitbox("MeleeHitbox").gameObject;

        attackDistance = 0.55f;

        startup = 0.05f * Application.targetFrameRate;
        duration = 0.05f * Application.targetFrameRate; ;
        endlag = 0.25f * Application.targetFrameRate; ;

        frameCounter = 0;
    }

    override public void Enter()
    {
        GameManager.instance.playSound("PunchWiff");

        this.player.selfBody.drag = 20.0f;
        this.player.selfBody.velocity = Vector2.zero;

        if (!player.isCrouching)
        {
            startPosition = Vector3.zero;
            endPosition = player.facingDirection * 1.375f * attackDistance;
        }
        else
        {
            startPosition = Vector3.down * 0.25f;
            endPosition = startPosition + player.facingDirection * attackDistance;
        }


        if (player.comboCount >= 2)
        {
            player.chainable = false;
            meleeHitbox.GetComponent<Hitbox>().knockdown = true;
        }

        if (player.isCrouching)
            meleeHitbox.GetComponent<Hitbox>().type = Hitbox.hitType.low;
        else
            meleeHitbox.GetComponent<Hitbox>().type = Hitbox.hitType.mid;

        meleeHitbox.transform.localPosition = startPosition;
        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.white;
    }

    override public void Execute()
    {
        if(player.chainable)
        {
            if (Controls.attackInputDown(player))
                player.PerformAction(Action.Attack);
        }

        frameCounter++;
        if (frameCounter < startup)
        {
            meleeHitbox.transform.localPosition = Vector3.Lerp(startPosition, endPosition, frameCounter / startup);
        }
        else if (frameCounter < startup + duration)
        {
            if (frameCounter - 1 < startup)
                player.hitboxManager.activateHitBox("MeleeHitbox");

            meleeHitbox.transform.localPosition = endPosition;
        }
        else if (frameCounter - 1 < startup + duration + endlag)
        {
            if (frameCounter - 1 < startup + duration)
                player.hitboxManager.deactivateHitBox("MeleeHitbox");

            meleeHitbox.transform.localPosition = Vector3.Lerp(endPosition, startPosition, (frameCounter - startup - duration) / endlag);
        }
        else
        {
            meleeHitbox.transform.localPosition = Vector2.zero;
            if (!player.isCrouching)
                player.PerformAction(Action.Stand);
            else
                player.PerformAction(Action.Crouch);
        }
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
        this.player.selfBody.drag = 0.0f;
        player.chainable = false;

        meleeHitbox.GetComponent<Hitbox>().knockdown = false;
        player.hitboxManager.deactivateHitBox("MeleeHitbox");
        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;
    }
}
