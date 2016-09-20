using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIPlayer : MonoBehaviour
{
    //NEEDS TO BE FIXED
    //public float maxHealth;
    //public float maxMeter;
    //private float baseKnockdownThreshold;

    //public float health { get; private set; }
    //public float meter { get; private set; }
    //public int stocks { get; private set; }

    //public float movementSpeed;
    //public float rollSpeed;
    //public float friction;
    //public float jumpHeight;
    //public float fallSpeed;
    //public float airMovementSpeed;

    //public float knockdownThreshold;

    //public bool grounded;

    //public int maxAirJumps;
    //public int maxAirDashes;

    //public int airJumps;
    //public int airDashes;

    //public Parameters.InputDirection direction { get; set; }

    ////Tells us the status of the player (things that affect the hitbox)
    //public Parameters.PlayerStatus status { get; set; }

    //public const int DEFAULT_STOCK_COUNT = 4;

    //public StateMachine<Player> ActionFsm { get; private set; }

    ////self references to various components
    ////private Collider selfCollider;
    //public Animator anim { get; private set; }
    //public Rigidbody2D selfBody { get; private set; }
    //public CollisionboxManager hitboxManager { get; private set; }
    //public ECB environmentCollisionBox;
    //public Shield shield;
    //public List<GameObject> projectilePrefabs;
    ///*private GameObject bodyVisual;
    //public PlayerSounds Sounds { get; private set; }
    //*/

    ////Used for the initialization of internal, non-object variables
    //void Awake()
    //{
    //    health = maxHealth;
    //    meter = 0.0f;
    //    stocks = DEFAULT_STOCK_COUNT;

    //    airJumps = 0;
    //    airDashes = 0;
    //}

    //// Use this for initialization of variables that rely on other objects
    //void Start()
    //{
    //    //Initializing components
    //    anim = this.GetComponent<Animator>();
    //    selfBody = this.GetComponent<Rigidbody2D>();
    //    hitboxManager = this.GetComponent<CollisionboxManager>();

    //    /*
    //    ActionFsm = new StateMachine<Player>(this);
    //    State<Player> startState = new IdleState(this, this.ActionFsm);
    //    ActionFsm.InitialState(startState);
    //     */
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    //this.ActionFsm.Execute();

    //    if (health <= 0)
    //    {
    //        stocks -= 1;
    //        if (stocks > 0)
    //            health = maxHealth;
    //    }

    //    if (stocks <= 0)
    //        Debug.Log("Player defeated");
    //}

    //void FixedUpdate()
    //{
    //    //this.ActionFsm.FixedExecute();
    //}

    //public void gainMeter(float meterGain)
    //{
    //    if (meterGain > 0)
    //        this.meter += meterGain;
    //}

    //public void loseHealth(float damage)
    //{
    //    if (damage > 0)
    //        this.health -= damage;
    //}

    //public void Die()
    //{
    //}

    ////Interface for the AI
    //public void WalkLeft()
    //{
    //    this.selfBody.velocity = new Vector2(-10.0f, 0);
    //}
    //public void WalkRight()
    //{
    //    this.selfBody.velocity = new Vector2(10.0f, 0);
    //}
    //public void JumpNeutral()
    //{
    //    this.selfBody.velocity = new Vector2(0.0f, 10.0f);
    //}
    //public void JumpLeft()
    //{
    //    this.selfBody.velocity = new Vector2(-10.0f, 10.0f);
    //}
    //public void JumpRight()
    //{
    //    this.selfBody.velocity = new Vector2(10.0f, 0);
    //}
    //public void Attack()
    //{
    //}
    //public void Block()
    //{
    //}
    //public void Idle()
    //{
    //}
}
