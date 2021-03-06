﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Player : MonoBehaviour {

    public float maxHealth;
    public float maxMeter;
    public float movementSpeed;
    public float neutralJumpHeight;
    public float directionJumpHeight;

    public float health;
    public float meter;
    public const int DEFAULT_STOCK_COUNT = 4;

    public new bool enabled;
    public bool suspended = false;
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

    public Vector2 effectivePosition { get; private set; }
    public Vector3 facingDirection { get; private set; }

    public StateMachine<Player> ActionFsm { get; private set; }

    //AI specific variables
    public bool AIControlled;
    public Action latestAction;

    //self references to various components
    public Rigidbody2D selfBody { get; private set; }
    public GameObject spriteContainer;
    public SpriteRenderer sprite;
    public GameObject shield;
    public List<GameObject> projectilePrefabs;
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

        this.Stand();
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
        
        EventManager.instance.RecordActionPerformed(action, this);

        return true;
    }

    //Temp hacks to make the AI behave
    bool IsValidAction(Action action)
    {
        //Temp hacks to make the AI behave
        if (this.stunned || (this.ActionFsm.CurrentState is HitlagState))
            return false;

        bool isValid = false;
        switch (action)
        {
            case Action.Stand:
                isValid = this.grounded;// ? !this.AIControlled : this.grounded && !(this.ActionFsm.CurrentState is AttackState);
                break;
            case Action.Crouch:
                isValid = this.grounded;// ? !this.AIControlled : this.grounded && !(this.ActionFsm.CurrentState is AttackState);
                break;
            case Action.Attack:
                isValid = this.grounded && !this.isCrouching && (!(this.ActionFsm.CurrentState is AttackState) || this.chainable);
                break;
            case Action.LowAttack:
                isValid = this.grounded && this.isCrouching && (!(this.ActionFsm.CurrentState is AttackState) || this.chainable);
                break;
            case Action.AirAttack:
                isValid = !this.grounded && (this.ActionFsm.CurrentState is JumpState);
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
            default:
                isValid = false; break;
        }

        if (isValid)
            latestAction = action;
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
        this.ActionFsm.ChangeState(new HitState(this, hitlag, hitstun, knockback, knockdown, this.ActionFsm));
    }

    public void ExitHitstun()
    {
        this.stunned = false;

        //Wakeup options for the player
        Parameters.InputDirection dir = Controls.getInputDirection(this);

        if (dir == Parameters.InputDirection.S || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.SE)
            this.Crouch();
        else
            this.Stand();

        if (Controls.shieldInputHeld(this))
            this.Block(isCrouching);
    }

    //TODO: Implement forward and back techs
    public void Tech()
    {
        this.ActionFsm.ChangeState(new TechState(this, this.ActionFsm));
        EventManager.instance.RecordRecovery(this);
    }
}
