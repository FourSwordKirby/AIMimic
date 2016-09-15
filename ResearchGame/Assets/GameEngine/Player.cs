using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

    public float maxHealth;
    public float maxMeter;
    public float movementSpeed;
    private float baseKnockdownThreshold;


    public float health { get; private set; }
    public float meter { get; private set; }
    public int stocks { get; private set; }
    public const int DEFAULT_STOCK_COUNT = 4;


    public Player opponent;
    public bool grounded = true;
    public bool isBlocking;


    public Vector2 effectivePosition { get; private set; }
    public Vector3 facingDirection { get; private set; }

    
    public StateMachine<Player> ActionFsm { get; private set; }

    //self references to various components
    public Rigidbody2D selfBody { get; private set; }
    public List<GameObject> projectilePrefabs;
    public CollisionboxManager hitboxManager { get; private set; }


    //Used for the initialization of internal, non-object variables
    void Awake()
    {
        health = maxHealth;
        meter = 0.0f;
        stocks = DEFAULT_STOCK_COUNT;
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

    // Update is called once per frame
    void Update()
    {
        this.ActionFsm.Execute();
        if (opponent.transform.position.x > this.transform.position.x)
            facingDirection = Vector3.right;
        else
            facingDirection = Vector3.left;
        facingDirection = facingDirection * 2;

        //if (health <= 0)
        //{
        //    stocks -= 1;
        //    if (stocks > 0)
        //        health = maxHealth;
        //}

        //if (stocks <= 0)
        //    Debug.Log("Player defeated");
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

    public void Die()
    {
    }

    //Interface for the AI
    //FIX THE WALKING
    public void Walk(Parameters.InputDirection dir)
    {
        Vector2 originalPositon = this.transform.position; 
        Vector2 movementVector = Vector2.zero;
        if (dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE)
            movementVector = Vector2.right * movementSpeed;
        else if (dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW)
            movementVector = Vector2.left * movementSpeed;
        this.ActionFsm.ChangeState(new MovementState(this, this.ActionFsm, (originalPositon + movementVector.normalized)));
    }

    //Invincible to lows. Forward hop goes 2 spaces. Will jump over 1 space close opponents
    public void Jump(Parameters.InputDirection dir)
    {
        Vector2 originalPositon = this.transform.position;
        Vector2 movementVector = Vector2.zero;

        if (dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE)
            movementVector = Vector2.right * movementSpeed;
        else if (dir == Parameters.InputDirection.NW || dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW)
            movementVector = Vector2.left * movementSpeed;
        else
            movementVector = Vector2.zero;
        this.ActionFsm.ChangeState(new JumpState(this, this.ActionFsm, (effectivePosition + 2 * movementVector.normalized)));
    }

    public void Attack()
    {
        this.ActionFsm.ChangeState(new AttackState(this, this.ActionFsm));
    }
    public void Block()
    {
        //Do this at some point
        //this.ActionFsm.ChangeState(new BlockState(this, this.ActionFsm))
    }

    public void Idle()
    {
        this.ActionFsm.ChangeState(new IdleState(this, this.ActionFsm));
    }
}
