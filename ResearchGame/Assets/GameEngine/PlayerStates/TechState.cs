using UnityEngine;
using System.Collections;

public class TechState : State<Player>
{

    private Player player;

    private float timer = 100;
    private float animTime = 0.5f;

    public TechState(Player playerInstance, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        player = playerInstance;
    }

    override public void Enter()
    {
        player.hitboxManager.deactivateHitBox("Hurtbox");
        player.selfBody.mass = 20;

    }

    private float techVel = 5.0f;
    private float techScale = 20.0f;
    override public void Execute()
    {
        if(timer < animTime)
        {
            timer += Time.deltaTime;
            if(0 <= timer && timer < 0.2f * animTime)
                player.spriteContainer.transform.rotation = Quaternion.Lerp(Quaternion.AngleAxis(90.0f * player.facingDirection.x, Vector3.forward), Quaternion.AngleAxis(0, Vector3.forward), timer / (animTime/5));
            else if (0.2f * animTime <= timer && timer < 0.4f * animTime)
                player.spriteContainer.transform.rotation = Quaternion.Lerp(Quaternion.AngleAxis(0, Vector3.forward), Quaternion.AngleAxis(-90.0f * player.facingDirection.x, Vector3.forward), (timer-0.2f * animTime) / (animTime / 5));
            else if (0.4f * animTime <= timer && timer < 0.6f * animTime)
                player.spriteContainer.transform.rotation = Quaternion.Lerp(Quaternion.AngleAxis(-90.0f * player.facingDirection.x, Vector3.forward), Quaternion.AngleAxis(-180.0f * player.facingDirection.x, Vector3.forward), (timer - 0.4f * animTime) / (animTime / 5));
            else if (0.6f * animTime <= timer && timer < 0.8f * animTime)
                player.spriteContainer.transform.rotation = Quaternion.Lerp(Quaternion.AngleAxis(-180.0f * player.facingDirection.x, Vector3.forward), Quaternion.AngleAxis(90.0f * player.facingDirection.x, Vector3.forward), (timer - 0.6f * animTime) / (animTime / 5));
            else if (0.8f * animTime <= timer && timer < animTime)
                player.spriteContainer.transform.rotation = Quaternion.Lerp(Quaternion.AngleAxis(90.0f * player.facingDirection.x, Vector3.forward), Quaternion.AngleAxis(0, Vector3.forward), (timer - 0.8f * animTime) / (animTime / 5));

            player.spriteContainer.transform.position += (Vector3.up * techVel * Time.deltaTime);
            techVel += (Physics2D.gravity * techScale * Time.deltaTime).y;

            if (timer >= animTime)
            {
                player.spriteContainer.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
                player.spriteContainer.transform.localPosition = -0.25f * Vector3.up;
                player.Stand();
            }
        }
    }

    override public void FixedExecute()
    {
        if (player.grounded && timer >= animTime)
        {
            timer = 0.0f;
        }
    }

    override public void Exit()
    {
        player.knockedDown = false;
        player.stunned = false;
        player.hitboxManager.activateHitBox("Hurtbox");
        player.selfBody.mass = 1;
    }
}