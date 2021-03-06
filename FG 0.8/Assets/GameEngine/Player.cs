﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Player : MonoBehaviour {

    public float maxHealth;
    public float maxMeter;
    public float walkSpeed;
    public float dashSpeed;
    public float airDashSpeed;
    public float backDashSpeed;
    public float airBackDashSpeed;
    public float neutralJumpHeight;
    public float directionJumpHeight;

    public float health;
    public float meter;
    public const int DEFAULT_STOCK_COUNT = 4;
    public const int COMBO_LIMIT = 0;

    public new bool enabled;
    public bool locked; //Indicates that the player is locked into this action for a while
    public bool suspended = false;
    public bool stunned = false;
    public bool knockedDown = false;
    public bool grounded = true;
    public bool chainable = false;
    public bool isCrouching;
    public bool isBlocking;

    public int airdashCount;
    public int comboCount;

    //Meta stuff for the data recorder
    public Player opponent;
    public bool isPlayer1;

    //Hacky bullshit that should be cleaned and purged
    public Sprite normalSprite;
    public Sprite hitSprite;

    public Vector2 effectivePosition { get; private set; }
    public Vector3 facingDirection { get; private set; }

    public StateMachine<Player> ActionFsm { get; private set; }

    //AI specific variables
    public PlayerStatus status;
    public bool AIControlled;
    public Action latestAction;

    //self references to various components
    public Rigidbody2D selfBody { get; private set; }
    public GameObject spriteContainer;
    public SpriteRenderer sprite;
    public GameObject shield;
    public List<GameObject> projectilePrefabs;
    public Collider2D ECB;
    public CollisionboxManager hitboxManager { get; private set; }

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

        Stand();
    }

    public void Pause()
    {
        this.ActionFsm.SuspendState(new PauseState(this, this.ActionFsm, this.ActionFsm.CurrentState));
    }

    public void Resume()
    {
        this.ActionFsm.ResumeState();
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
        //Maintaining consistency for data recording
        if(actionPreformed)
        {
            actionPreformed = false;
            EventManager.instance.RecordActionPerformed(latestAction, this);
        }


        if (!this.enabled)
            return;

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

        if (grounded && !stunned && !locked)
        {
            airdashCount = 0;
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

    //Used to indicate that an action has ended
    //This is for things like standing, blocking and crouching. Actions which could continue 
    //indefinitely but are interrupted by taking other actions
    bool actionEnded = false;
    public void EndAction()
    {
        actionEnded = true;
        //EventManager.instance.RecordEndAction(latestAction, this);
    }

    //Interface for doing actions in the game
    bool actionPreformed = true;
    public bool PerformAction(Action action, bool isAI = false)
    {
        //Can only perform actions when enabled
        if (!this.enabled)
            return false;

        if (!IsValidAction(action, isAI))
            return false;

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
            case Action.Overhead:
                this.Overhead();
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
            case Action.DashLeft:
                this.Dash(Parameters.InputDirection.W);
                break;
            case Action.DashRight:
                this.Dash(Parameters.InputDirection.E);
                break;
            case Action.AirdashLeft:
                this.Airdash(Parameters.InputDirection.W);
                break;
            case Action.AirdashRight:
                this.Airdash(Parameters.InputDirection.E);
                break;
            case Action.TechLeft:
                this.Tech(-1.0f);
                break;
            case Action.TechNeutral:
                this.Tech(0.0f);
                break;
            case Action.TechRight:
                this.Tech(1.0f);
                break;
            case Action.DP:
                this.DP();
                break;
            default:
                throw new Exception("Not implemented yet");
        }
        
        latestAction = action;
        if (actionEnded)
            EventManager.instance.RecordActionPerformed(latestAction, this);
        else
            actionPreformed = true;

        return true;
    }

    //Temp hacks to make the AI behave
    bool IsValidAction(Action action, bool isAI = false)
    {
        if (this.locked)
            return false;

        if(action != Action.TechLeft && action != Action.TechRight && action != Action.TechNeutral && action != Action.DP)
        {
            if (this.stunned || (this.ActionFsm.CurrentState is HitlagState))
                return false;
        }

        bool isValid = false;
        switch (action)
        {
            case Action.Stand:
                isValid = !isAI ? this.grounded : this.grounded && (status == PlayerStatus.Stand || status == PlayerStatus.Crouch || status == PlayerStatus.Moving || status == PlayerStatus.Highblock);
                break;
            case Action.Crouch:
                isValid = !isAI ? this.grounded : this.grounded && (status == PlayerStatus.Stand || status == PlayerStatus.Crouch || status == PlayerStatus.Moving || status == PlayerStatus.Lowblock);
                break;
            case Action.Attack:
                isValid = this.grounded && !this.isCrouching && (!(this.ActionFsm.CurrentState is AttackState) || this.chainable);
                break;
            case Action.Overhead:
                isValid = this.grounded && !this.isCrouching && (!(this.ActionFsm.CurrentState is AttackState) || this.chainable);
                break;
            case Action.LowAttack:
                isValid = this.grounded && this.isCrouching && (!(this.ActionFsm.CurrentState is AttackState) || this.chainable);
                break;
            case Action.AirAttack:
                isValid = !this.grounded && (this.ActionFsm.CurrentState is JumpState || this.ActionFsm.CurrentState is DashState);
                break;
            case Action.StandBlock:
                isValid = this.grounded && !(this.isBlocking && !this.isCrouching) && !(this.ActionFsm.CurrentState is AttackState);
                break;
            case Action.CrouchBlock:
                isValid = this.grounded && !(this.isBlocking && this.isCrouching) && !(this.ActionFsm.CurrentState is AttackState);
                break;
            case Action.JumpNeutral:
                isValid = this.grounded && !(this.ActionFsm.CurrentState is AttackState);
                break;
            case Action.JumpLeft:
                isValid = this.grounded && !(this.ActionFsm.CurrentState is AttackState);
                break;
            case Action.JumpRight:
                isValid = this.grounded && !(this.ActionFsm.CurrentState is AttackState);
                break;
            case Action.WalkLeft:
                isValid = this.grounded && !(this.ActionFsm.CurrentState is AttackState) && !this.isBlocking;// && !this.isCrouching;
                break;
            case Action.WalkRight:
                isValid = this.grounded && !(this.ActionFsm.CurrentState is AttackState) && !this.isBlocking;// && !this.isCrouching;
                break;
            case Action.DashLeft:
                isValid = this.grounded && !(this.ActionFsm.CurrentState is AttackState) && !this.isBlocking;// && !this.isCrouching;
                break;
            case Action.DashRight:
                isValid = this.grounded && !(this.ActionFsm.CurrentState is AttackState) && !this.isBlocking;// && !this.isCrouching;
                break;
            case Action.AirdashLeft:
                isValid = !this.grounded && !(this.ActionFsm.CurrentState is AttackState) && airdashCount < 1;// && !this.isCrouching;
                break;
            case Action.AirdashRight:
                isValid = !this.grounded && !(this.ActionFsm.CurrentState is AttackState) && airdashCount < 1;// && !this.isCrouching;
                break;
            case Action.TechNeutral:
                isValid = this.grounded && this.knockedDown;
                break;
            case Action.TechLeft:
                isValid = this.grounded && this.knockedDown;
                break;
            case Action.TechRight:
                isValid = this.grounded && this.knockedDown;
                break;
            case Action.DP:
                isValid = this.grounded && ((!(this.ActionFsm.CurrentState is AttackState) || this.chainable) || this.knockedDown);
                break;
            default:
                throw new Exception("Not implemented yet");
        }

        return isValid;
    }

    void Walk(Parameters.InputDirection dir)
    {
        if (!this.grounded)
            return;

        this.Stand();
        
        Vector2 movementVector = Vector2.zero;
        if (dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE || dir == Parameters.InputDirection.NE)
            movementVector = Vector2.right;
        else if (dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.NW)
            movementVector = Vector2.left;
        else
            return;
        this.ActionFsm.ChangeState(new MovementState(this, this.ActionFsm, movementVector.x));
    }

    void Dash(Parameters.InputDirection dir)
    {
        if (!this.grounded)
            return;

        this.Stand();

        Vector2 movementVector = Vector2.zero;
        if (dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE || dir == Parameters.InputDirection.NE)
            movementVector = Vector2.right;
        else if (dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.NW)
            movementVector = Vector2.left;
        else
            return;
        this.ActionFsm.ChangeState(new DashState(this, this.ActionFsm, movementVector.x, false));
    }

    void Airdash(Parameters.InputDirection dir)
    {
        if (this.grounded)
            return;

        Vector2 movementVector = Vector2.zero;
        if (dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE || dir == Parameters.InputDirection.NE)
            movementVector = Vector2.right;
        else if (dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.NW)
            movementVector = Vector2.left;
        else
            return;
        this.ActionFsm.ChangeState(new DashState(this, this.ActionFsm, movementVector.x, true));
    }

    //Invincible to lows. Forward hop goes 2 spaces. Will jump over 1 space close opponents
    void Jump(Parameters.InputDirection dir)
    {
        if (!this.grounded)
            return;

        this.Stand();

        Vector2 movementVector = Vector2.zero;       
        if (dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE)
            movementVector = Vector2.right * walkSpeed;
        else if (dir == Parameters.InputDirection.NW || dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW)
            movementVector = Vector2.left * walkSpeed;
        else
            movementVector = Vector2.zero;
        this.ActionFsm.ChangeState(new JumpState(this, this.ActionFsm, ((Vector2)(transform.position)+ 2.5f * movementVector.normalized)));
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

    void Overhead()
    {
        if (!this.grounded)
            return;

        this.ActionFsm.ChangeState(new OverheadState(this, this.ActionFsm, this.comboCount));
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

        this.isCrouching = false;
        StandAnim();

        this.ActionFsm.ChangeState(new IdleState(this, this.ActionFsm));
    }

    public void Crouch()
    {
        if (!this.grounded)
            return;

        this.isCrouching = true;
        CrouchAnim();

        this.ActionFsm.ChangeState(new IdleState(this, this.ActionFsm));
    }


    public void DP()
    {
        if (!this.grounded)
            return;

        this.ActionFsm.ChangeState(new DPState(this, this.ActionFsm));
    }


    public void Tech(float dir)
    {
        print("Record how long a player waits to tech as well as account for DPs etc.");
        print("Also add visual cue as the player is teching so you can more easily follow the tech direction");

        this.ActionFsm.ChangeState(new TechState(this, this.ActionFsm, dir));
        EventManager.instance.RecordRecovery(this);
    }

    public void EnterHitstun(float hitlag, float hitstun, Vector2 knockback, bool knockdown)
    {
        this.ActionFsm.ChangeState(new HitState(this, hitlag, hitstun, knockback, knockdown, this.ActionFsm));
    }

    public void ExitHitstun()
    {
        GameManager.EndCombo(this.opponent);

        this.stunned = false;
        this.locked = false;

        //Wakeup options for the player
        Parameters.InputDirection dir = Controls.getInputDirection(this);

        if (dir == Parameters.InputDirection.S || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.SE)
            this.PerformAction(Action.Crouch);
        else
            this.PerformAction(Action.Stand);

        if (Controls.shieldInputHeld(this))
        {
            if (dir == Parameters.InputDirection.S || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.SE)
                this.PerformAction(Action.CrouchBlock);
            else
                this.PerformAction(Action.StandBlock);
        }
    }


    public void StartInvuln()
    {
        if(!invulnerable)
        {
            invulnerable = true;
            hitboxManager.deactivateHitBox("Hurtbox");
            StartCoroutine(Flash());
        }
    }

    public void EndInvuln()
    {
        if(invulnerable)
        {
            invulnerable = false;
            hitboxManager.activateHitBox("Hurtbox");
        }
    }

    private bool invulnerable;
    private float flashPeriod = 0.15f;

    public IEnumerator Flash()
    {
        bool flashUp = true;
        float timer = 0.0f;

        Color originalColor = this.sprite.color;

        while (invulnerable)
        {
            float scale = 10*Time.deltaTime;
            this.sprite.color = flashUp ? Color.Lerp(originalColor, originalColor-Color.black * 0.5f, (timer) / flashPeriod)
                                        : Color.Lerp(originalColor-Color.black * 0.5f, originalColor, (timer) / flashPeriod);
            timer += Time.deltaTime;
            if (timer > flashPeriod)
            {
                timer = 0.0f;
                flashUp = !flashUp;
            }
            yield return new WaitForSeconds(0.01f);
        }
        this.sprite.color = originalColor;
        yield return null;
    }
}
