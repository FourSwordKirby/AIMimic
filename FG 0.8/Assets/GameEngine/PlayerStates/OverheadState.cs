using UnityEngine;
using System.Collections;

//Program this up
public class OverheadState : State<Player>
{
    private Player player;
    private GameObject meleeHitbox;

    private float direction;
    private float attackDistance;

    public float startup;
    public float duration;
    public float endlag;
    
    private float frameCounter;

    private Vector3 startPosition;
    private Vector3 midPosition;
    private Vector3 endPosition;

    private Quaternion startRotation;
    private Quaternion endRotation;

    private Vector3 startSpriteSize;
    private Vector3 endSpriteSize;

    private Vector3 startSpritePosition;
    private Vector3 endSpritePosition;

    public OverheadState(Player playerInstance, StateMachine<Player> fsm, int comboCount = 0) : base(playerInstance, fsm)
    {
        player = playerInstance;
        player.comboCount = comboCount;

        meleeHitbox = player.hitboxManager.getHitbox("OverheadHitbox").gameObject;

        direction = Mathf.Sign(player.facingDirection.x);
        attackDistance = 0.8f;

        startup = 0.4f * Application.targetFrameRate;
        duration = 0.1f * Application.targetFrameRate; ;
        endlag = 0.6f * Application.targetFrameRate; ;



        startPosition = Vector3.zero;
        midPosition = new Vector3(-0.254f * direction, 0.20f, 0);
        endPosition = new Vector3(attackDistance * direction, 0, 0);

        startRotation = Quaternion.Euler(0, 0, 120*direction);
        endRotation = Quaternion.Euler(0, 0, 0);

        //Dumb size animations
        startSpriteSize = new Vector3(1.0f, 0.7f, 1.0f);
        endSpriteSize = Vector3.one;

        startSpritePosition = new Vector3(0, 0.1f, 0);
        endSpritePosition = new Vector3(0, 0.25f, 0);
        frameCounter = 0;
    }

    override public void Enter()
    {
        GameManager.instance.PlaySound("Overhead");

        this.player.selfBody.drag = 200.0f;
        this.player.selfBody.velocity = Vector2.zero;

        //Maybe spawn some aethetic charge effect
        meleeHitbox.transform.localPosition = startPosition;
        meleeHitbox.transform.localRotation = startRotation;

        meleeHitbox.GetComponent<SpriteRenderer>().flipX = player.sprite.flipX;
        meleeHitbox.GetComponent<Collider2D>().offset = new Vector2(direction, meleeHitbox.GetComponent<Collider2D>().offset.y);

        player.sprite.transform.localPosition = Vector2.up;

        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.white;

        //Player state managing
        player.locked = true;

        //Keeping track of player status
        player.status = PlayerStatus.OverheadAttack;
    }

    override public void Execute()
    {
        //Keeping track of player status
        if (frameCounter < startup + duration)
            player.status = PlayerStatus.OverheadAttack;
        else
            player.status = PlayerStatus.Recovery;

        frameCounter++;
        if (frameCounter < startup)
        {
            meleeHitbox.transform.localPosition = Vector3.Lerp(startPosition, midPosition, frameCounter / startup);

            player.spriteContainer.transform.localScale = Vector3.Lerp(endSpriteSize, startSpriteSize, frameCounter / startup);
            player.sprite.transform.localPosition = Vector3.Lerp(endSpritePosition, startSpritePosition, frameCounter / startup);
        }
        else if (frameCounter < startup + duration)
        {
            if (frameCounter - 1 < startup)
            {
                GameManager.instance.PlaySound("PunchWiff");
                player.hitboxManager.activateHitBox("OverheadHitbox");
            }

            meleeHitbox.transform.localPosition = Vector3.Lerp(midPosition, endPosition, (frameCounter - startup) / duration);
            meleeHitbox.transform.localRotation = Quaternion.Lerp(startRotation, endRotation, (frameCounter - startup) / duration);

            player.spriteContainer.transform.localScale = Vector3.Lerp(startSpriteSize, endSpriteSize, (frameCounter - startup) / duration);
            player.sprite.transform.localPosition = Vector3.Lerp(startSpritePosition, endSpritePosition, (frameCounter - startup) / duration);
        }
        else if (frameCounter - 1 < startup + duration + endlag)
        {

            if (frameCounter - 1 < startup + duration)
                player.hitboxManager.deactivateHitBox("OverheadHitbox");

            meleeHitbox.transform.localRotation = endRotation;
            meleeHitbox.transform.localPosition = Vector3.Lerp(endPosition, startPosition, (frameCounter - startup - duration) / endlag);
            meleeHitbox.GetComponent<SpriteRenderer>().color = Color.white - Color.black / 2;


            player.spriteContainer.transform.localScale = endSpriteSize;
            player.sprite.transform.localPosition = endSpritePosition;
        }
        else
        {
            player.locked = false;
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

        //COMMENTED OUT FOR RESEARCH meleeHitbox.GetComponent<Hitbox>().knockdown = false;
        player.hitboxManager.deactivateHitBox("MeleeHitbox");
        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;

        //Player state managing
        player.locked = false;
    }

    override public State<Player> Copy()
    {
        OverheadState attackCopy = new OverheadState(this.Owner, this.Owner.ActionFsm, player.comboCount);
        attackCopy.frameCounter = frameCounter;
        attackCopy.startPosition = this.startPosition;
        attackCopy.endPosition = this.endPosition;

        attackCopy.player.chainable = player.chainable;
        attackCopy.meleeHitbox.GetComponent<Hitbox>().knockdown = meleeHitbox.GetComponent<Hitbox>().knockdown;
        attackCopy.meleeHitbox.GetComponent<Hitbox>().type = meleeHitbox.GetComponent<Hitbox>().type;
        attackCopy.meleeHitbox.transform.localPosition = meleeHitbox.transform.localPosition;
        return attackCopy;
    }
}
