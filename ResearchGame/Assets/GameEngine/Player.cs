using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

    public float maxHealth;
    public float maxMeter;
    public float movementSpeed;
    public float neutralJumpHeight;
    public float directionJumpHeight;

    public float health { get; private set; }
    public float meter { get; private set; }
    public const int DEFAULT_STOCK_COUNT = 4;

    public bool stunned = false;
    public bool knockedDown = false;
    public bool grounded = true;
    public bool chainable = false;
    public bool isCrouching;
    public bool isBlocking;

    public int comboCount;

    //Meta stuff for the data recorder
    public Player opponent;
    public bool isPlayer1;


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
    private EventRecorder eventRecorder;

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

        this.Stand();
    }

    public void Reset()
    {
        health = maxHealth;
        meter = 0.0f;
        comboCount = 0;

        ActionFsm.ClearStates();
        this.stunned = false;
        this.knockedDown = false;
        this.grounded = true;
        this.chainable = false;

        this.selfBody.gravityScale = 1.0f;
        this.selfBody.velocity = Vector3.zero;
        this.hitboxManager.deactivateAllHitboxes();
        this.hitboxManager.activateHitBox("Hurtbox");

        this.spriteContainer.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
        this.spriteContainer.transform.localPosition = -0.25f * Vector3.up;
        this.Stand();

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

    //Interface for doing actions in the game
    public bool PerformAction(Action action)
    {
        if (!IsValidAction(action))
            return false;

        if (eventRecorder != null)
            eventRecorder.RecordAction(action, this.isPlayer1);

        switch (action) {
            case Action.Stand:
                this.Stand();
                break;
            case Action.Crouch:
                this.Crouch();
                break;
            case Action.Attack:
                this.Attack(false);
                break;
            case Action.LowAttack:
                this.Attack(true);
                break;
            case Action.AirAttack:
                this.AirAttack();
                break;
            case Action.StandBlock:
                this.Block(false);
                break;
            case Action.CrouchBlock:
                this.Block(true);
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
                this.Stand();
                break;
        }
        return true;
    }

    //Temp hacks to make the AI behave
    bool IsValidAction(Action action)
    {
        //Temp hacks to make the AI behave
        if (this.stunned || (this.ActionFsm.CurrentState is SuspendState))
            return false;

        switch (action)
        {
            case Action.Stand:
                return this.grounded;// ? !this.AIControlled : this.grounded && !(this.ActionFsm.CurrentState is AttackState);
            case Action.Crouch:
                return this.grounded;// ? !this.AIControlled : this.grounded && !(this.ActionFsm.CurrentState is AttackState);
            case Action.Attack:
                //Temp hacks to make the AI behave
                return this.grounded && !this.isCrouching && (!(this.ActionFsm.CurrentState is AttackState) || this.chainable);
            case Action.LowAttack:
                //Temp hacks to make the AI behave
                return this.grounded && this.isCrouching && (!(this.ActionFsm.CurrentState is AttackState) || this.chainable);
            case Action.AirAttack:
                return !this.grounded && (this.ActionFsm.CurrentState is JumpState);
            case Action.StandBlock:
                return this.grounded && (!this.isCrouching || this.isBlocking) && !(this.ActionFsm.CurrentState is AttackState);
            case Action.CrouchBlock:
                return this.grounded && (this.isCrouching || this.isBlocking) && !(this.ActionFsm.CurrentState is AttackState);
            case Action.JumpNeutral:
                return this.grounded && !(this.ActionFsm.CurrentState is AttackState);
            case Action.JumpLeft:
                return this.grounded && !(this.ActionFsm.CurrentState is AttackState);
            case Action.JumpRight:
                return this.grounded && !(this.ActionFsm.CurrentState is AttackState);
            case Action.WalkLeft:
                return this.grounded && !(this.ActionFsm.CurrentState is AttackState) && !this.isBlocking;// && !this.isCrouching;
            case Action.WalkRight:
                return this.grounded && !(this.ActionFsm.CurrentState is AttackState) && !this.isBlocking;// && !this.isCrouching;
            default:
                return false;
        }
    }

    void Walk(Parameters.InputDirection dir)
    {
        if (!this.grounded)
            return;

        this.Stand();

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
        if (!this.grounded)
            return;

        this.Stand();

        Vector2 movementVector = Vector2.zero;       
        if (dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE)
            movementVector = Vector2.right * movementSpeed;
        else if (dir == Parameters.InputDirection.NW || dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW)
            movementVector = Vector2.left * movementSpeed;
        else
            movementVector = Vector2.zero;
        this.ActionFsm.ChangeState(new JumpState(this, this.ActionFsm, ((Vector2)(transform.position)+ 2 * movementVector.normalized)));
    }

    void Attack(bool isCrouching)
    {
        if (!this.grounded)
            return;

        if (isCrouching)
            this.Crouch();
        else
            this.Stand();
        this.ActionFsm.ChangeState(new AttackState(this, this.ActionFsm, this.comboCount));
    }

    void AirAttack()
    {
        if (this.grounded)
            return;

        this.ActionFsm.ChangeState(new AirAttackState(this, this.ActionFsm));
    }

    void Block(bool isCrouching)
    {
        if (!this.grounded)
            return;

        if (isCrouching)
            this.Crouch();
        else
            this.Stand();
        this.ActionFsm.ChangeState(new BlockState(this, this.ActionFsm));
    }

    public void Stand()
    {
        if (!this.grounded)
            return;

        this.ActionFsm.ChangeState(new IdleState(this, this.ActionFsm));

        this.isCrouching = false;
        StandAnim();
    }

    public void Crouch()
    {
        if (!this.grounded)
            return;

        this.ActionFsm.ChangeState(new IdleState(this, this.ActionFsm));

        this.isCrouching = true;
        CrouchAnim();
    }

    public void EnterHitstun(float hitlag, float hitstun, Vector2 knockback, bool knockdown)
    {
        //When you get hit, the data recorder should note that you were not successful in completing the last attempted action
        if (eventRecorder != null)
            eventRecorder.InterruptAction(this.isPlayer1);

        this.ActionFsm.ChangeState(new HitState(this, hitlag, hitstun, knockback, knockdown, this.ActionFsm));
    }

    public void ExitHitstun()
    {
        //Wakeup options for the player
        Parameters.InputDirection dir = Controls.getInputDirection(this);

        if (dir == Parameters.InputDirection.S || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.SE)
            this.Crouch();
        else
            this.Stand();

        if (Controls.shieldInputHeld(this))
            this.Block(isCrouching);

        //Upon exiting hitstun, the recorder should start recording your actions once more (it notes what your wakeup option was)
        if (eventRecorder != null)
            eventRecorder.ResumeRecording(this.isPlayer1, isCrouching, isBlocking);
    }

    //TODO: Implement forward and back techs
    public void Tech()
    {
        this.ActionFsm.ChangeState(new TechState(this, this.ActionFsm));
    }

    public void SetRecorder(EventRecorder eventRecorder)
    {
        this.eventRecorder = eventRecorder;
    }
}
