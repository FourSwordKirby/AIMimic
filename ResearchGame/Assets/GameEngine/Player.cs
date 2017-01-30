using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

    public float maxHealth;
    public float maxMeter;
    public float movementSpeed;
    public float neutralJumpHeight;
    public float directionJumpHeight;
    private float baseKnockdownThreshold;
    
    public float health { get; private set; }
    public float meter { get; private set; }
    public const int DEFAULT_STOCK_COUNT = 4;

    public Player opponent;
    public bool stunned = false;
    public bool knockedDown = false;
    public bool grounded = true;
    public bool chainable = false;
    public bool isCrouching;
    public bool isBlocking;

    public int comboCount;

    //Hacky bullshit that should be cleaned and purged
    public Sprite normalSprite;
    public Sprite hitSprite;

    public bool AIControlled;

    public Vector2 effectivePosition { get; private set; }
    public Vector3 facingDirection { get; private set; }

    public StateMachine<Player> ActionFsm { get; private set; }

    //self references to various components
    public Rigidbody2D selfBody { get; private set; }
    public GameObject spriteContainer;
    public SpriteRenderer sprite;
    public GameObject shield;
    public List<GameObject> projectilePrefabs;
    public CollisionboxManager hitboxManager { get; private set; }

    //Player keeps track of recording data bc it's impossible to record merely from observation tbh
    public DataRecorder dataRecorder;

    //Used for the initialization of internal, non-object variables
    void Awake()
    {
        health = maxHealth;
        meter = 0.0f;
    }

    // Use this for initialization of variables that rely on other objects
    void Start()
    {
        //Initializing components
        selfBody = this.GetComponent<Rigidbody2D>();
        hitboxManager = this.GetComponent<CollisionboxManager>();

        ActionFsm = new StateMachine<Player>(this);
        State<Player> startState = new IdleState(this, this.ActionFsm);
        ActionFsm.InitialState(startState);
    }

    public void Reset()
    {
        health = maxHealth;
        meter = 0.0f;

        ActionFsm.ClearStates();
        this.selfBody.gravityScale = 1.0f;
        this.selfBody.velocity = Vector3.zero;
        this.hitboxManager.deactivateAllHitboxes();
        this.hitboxManager.activateHitBox("Hurtbox");

        this.spriteContainer.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
        this.spriteContainer.transform.localPosition = -0.25f * Vector3.up;
        this.Idle();

        State<Player> startState = new IdleState(this, this.ActionFsm);
        ActionFsm.InitialState(startState);
    }

    // Update is called once per frame
    void Update()
    {
        this.ActionFsm.Execute();
        if (opponent.transform.position.x > this.transform.position.x)
        {
            this.sprite.flipX = false;
            facingDirection = Vector3.right;
        }
        else
        {
            this.sprite.flipX = true;
            facingDirection = Vector3.left;
        }

        if(grounded && !stunned)
        {
            if (isCrouching)
            {
                CrouchAnim();
            }
            else
            {
                StandAnim();
            }
        }
    }

    void FixedUpdate()
    {
        this.effectivePosition = new Vector2(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.y));
        this.ActionFsm.FixedExecute();
    }

    public void GainMeter(float meterGain)
    {
        if (meterGain > 0)
            this.meter += meterGain;
    }

    public void LostHealth(float damage)
    {
        if (damage > 0)
            this.health -= damage;
    }

    public void StandAnim()
    {
        this.spriteContainer.transform.localScale = Vector3.one;
        this.spriteContainer.transform.localPosition = -0.25f * Vector3.up;
        this.sprite.transform.localPosition = 0.25f * Vector3.up;
    }

    public void CrouchAnim()
    {
        this.spriteContainer.transform.localScale = Vector3.one - Vector3.up * 0.5f;
        this.spriteContainer.transform.localPosition = -0.5f * Vector3.up;
        this.sprite.transform.localPosition = 0.5f * Vector3.up;
    }

    public void Die()
    {
    }

    public void performAction(Action action)
    {
        //Recording our action
        Debug.Log("recorded " + action + "frame " + GameManager.currentFrame);
        if (dataRecorder.enabled)
            dataRecorder.RecordAction(action);

        switch(action) {
            case Action.Stand:
                this.Stand();
                break;
            case Action.Crouch:
                this.Crouch();
                break;
            case Action.Attack:
                this.Attack();
                break;
            case Action.AirAttack:
                this.AirAttack();
                break;
            case Action.Block:
                this.Block();
                break;
            case Action.Idle:
                this.Idle();
                break;
            case Action.JumpNeutral:
                this.Jump(Parameters.InputDirection.N);
                break;
            case Action.JumpLeft:
                this.Jump(Parameters.InputDirection.W);
                break;
            case Action.JumpRight:
                this.Jump(Parameters.InputDirection.E);
                break;
            case Action.WalkLeft:
                this.Walk(Parameters.InputDirection.W);
                break;
            case Action.WalkRight:
                this.Walk(Parameters.InputDirection.E);
                break;
            default:
                this.Idle();
                break;
        }

    }

    //Interface for doing actions in the game
    void Walk(Parameters.InputDirection dir)
    {
        //Vector2 originalPositon = this.transform.position; 
        Vector2 movementVector = Vector2.zero;
        if (dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE || dir == Parameters.InputDirection.NE)
            movementVector = Vector2.right;
        else if (dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.NW)
            movementVector = Vector2.left;
        else
            return;
        this.ActionFsm.ChangeState(new MovementState(this, this.ActionFsm, movementVector.x));
    }

    //Invincible to lows. Forward hop goes 2 spaces. Will jump over 1 space close opponents
    void Jump(Parameters.InputDirection dir)
    {
        Vector2 movementVector = Vector2.zero;
        this.Idle();

        if (dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE)
            movementVector = Vector2.right * movementSpeed;
        else if (dir == Parameters.InputDirection.NW || dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW)
            movementVector = Vector2.left * movementSpeed;
        else
            movementVector = Vector2.zero;
        this.ActionFsm.ChangeState(new JumpState(this, this.ActionFsm, ((Vector2)(transform.position)+ 2 * movementVector.normalized)));
    }

    void Attack()
    {
        this.ActionFsm.ChangeState(new AttackState(this, this.ActionFsm, this.comboCount));
    }

    void AirAttack()
    {
        this.ActionFsm.ChangeState(new AirAttackState(this, this.ActionFsm));
    }

    void Block()
    {
        this.ActionFsm.ChangeState(new BlockState(this, this.ActionFsm));
    }

    public void Idle()
    {
        this.ActionFsm.ChangeState(new IdleState(this, this.ActionFsm));
    }

    public void Stand()
    {
        this.isCrouching = false;
        StandAnim();
    }

    public void Crouch()
    {
        this.isCrouching = true;
        CrouchAnim();
    }
}
