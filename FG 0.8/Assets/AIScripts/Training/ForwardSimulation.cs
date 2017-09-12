using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ForwardSimulation : MonoBehaviour {

    public GameObject positionTracker;
    public Rigidbody2D p1Dummy;
    public Rigidbody2D p2Dummy;

    float frameLength;
    float predictionLength = 1.0f;

    List<GameObject> p1Trackers;
    List<GameObject> p2Trackers;


    private void Start()
    {
        frameLength = 1.0f / Application.targetFrameRate;
        p1Trackers = new List<GameObject>();
        p2Trackers = new List<GameObject>();


        for (int i = 0; i < (predictionLength / frameLength); i++)
        {
            GameObject marker1 = Instantiate(positionTracker);
            GameObject marker2 = Instantiate(positionTracker);

            p1Trackers.Add(marker1);
            p2Trackers.Add(marker2);
        }
    }

    //TODO, scrap and move this class into its own module for mass testing of various scenarios
    bool makePrediction;
    void ActionPerformed(KeyValuePair<Action, bool> pair)
    {
        Action a = pair.Key;
        if (!makePrediction && a != Action.Stand)
            makePrediction = true;
    }

    void Hit(Hitbox h)
    {
        if (makePrediction)
        {
            print("hit");
        }
    }

    void Update()
    {
        Player p1 = GameManager.instance.p1;
        Player p2 = GameManager.instance.p2;
        
        if (makePrediction)
        {
            if (!(p1.suspended || p1.stunned || !p1.enabled || p2.suspended || p2.stunned || !p2.enabled))
                Predict(GameManager.instance.p1, GameManager.instance.p2, predictionLength);
            makePrediction = false;
        }
    }

    //This will take a single starting snapshot andd an action and return some estimates of the result of taking said action
    void Predict(Player p1, Player p2, float time = 1)
    {
        //Copying and storing the physics
        //Player1
        bool p1Simulated = p1.selfBody.simulated;
        Vector2 p1Vel = p1.selfBody.velocity;
        float p1GravScale = p1.selfBody.gravityScale;
        float p1Drag = p1.selfBody.drag;
        float p1Mass = p1.selfBody.mass;
        Vector2 p1pos = p1.transform.position;

        float p1Health = p1.health;
        float p1Meter = p1.meter;
        int p1ComboCount = p1.comboCount;
        bool p1Stunned = p1.stunned;
        bool p1KnockedDown = p1.knockedDown;
        bool p1Grounded = p1.grounded;
        bool p1Chainable = p1.chainable;

        State<Player> p1State = p1.ActionFsm.CurrentState;
        p1.ActionFsm.SubstituteState(p1State.Copy());

        //Player2
        bool p2Simulated = p2.selfBody.simulated;
        Vector2 p2Vel = p2.selfBody.velocity;
        float p2GravScale = p2.selfBody.gravityScale;
        float p2Drag = p2.selfBody.drag;
        float p2Mass = p2.selfBody.mass;
        Vector2 p2pos = p2.transform.position;

        float p2Health = p2.health;
        float p2Meter = p2.meter;
        int p2ComboCount = p2.comboCount;
        bool p2Stunned = p2.stunned;
        bool p2KnockedDown = p2.knockedDown;
        bool p2Grounded = p2.grounded;
        bool p2Chainable = p2.chainable;

        State<Player> p2State = p2.ActionFsm.CurrentState;
        p2.ActionFsm.SubstituteState(p2State.Copy());

        //maintaining the camera physics
        Vector3 cameraPos = GameManager.instance.Camera.transform.position;
        Vector3 cameraVel = GameManager.instance.Camera.selfBody.velocity;

        Physics2D.autoSimulation = false;
        for(int i  = 0; i < time/frameLength; i++)
        {
            Physics2D.Simulate(frameLength);

            p1.ActionFsm.CurrentState.Execute();
            p2.ActionFsm.CurrentState.Execute();
            //Spawn a tracker to see where the player ended up
            p1Trackers[i].transform.position = p1.transform.position;
            p2Trackers[i].transform.position = p2.transform.position;

            if (p1.ActionFsm.CurrentState.ToString() != p1State.ToString() 
                || p2.ActionFsm.CurrentState.ToString() != p2State.ToString())
                break;
        }

        //Restoring State
        //p1
        //Physics
        p1.selfBody.simulated = p1Simulated;
        p1.selfBody.velocity = p1Vel;
        p1.selfBody.gravityScale = p1GravScale;
        p1.selfBody.drag = p1Drag;
        p1.selfBody.mass = p1Mass;
        p1.transform.position = p1pos;

        //stats
        p1.health = p1Health;
        p1.meter = p1Meter;
        p1.comboCount = p1ComboCount;
        p1.stunned = p1Stunned;
        p1.knockedDown = p1KnockedDown;
        p1.grounded = p1Grounded;
        p1.chainable = p1Chainable;

        p1.ActionFsm.SubstituteState(p1State);

        //p2
        //Physics
        p2.selfBody.simulated = p2Simulated;
        p2.selfBody.velocity = p2Vel;
        p2.selfBody.gravityScale = p2GravScale;
        p2.selfBody.drag = p2Drag;
        p2.selfBody.mass = p2Mass;
        p2.transform.position = p2pos;

        //stats
        p2.health = p2Health;
        p2.meter = p2Meter;
        p2.comboCount = p2ComboCount;
        p2.stunned = p2Stunned;
        p2.knockedDown = p2KnockedDown;
        p2.grounded = p2Grounded;
        p2.chainable = p2Chainable;

        p2.ActionFsm.SubstituteState(p2State);


        //Reapplying camera physics
        GameManager.instance.Camera.transform.position = cameraPos;
        GameManager.instance.Camera.selfBody.velocity = cameraVel;

        Physics2D.autoSimulation = true;
    }

    IEnumerator ReplaySituation(Player p1, Player p2, float time = 1)
    {
        //Copying and storing the physics
        //Player1
        bool p1Simulated = p1.selfBody.simulated;
        Vector2 p1Vel = p1.selfBody.velocity;
        float p1GravScale = p1.selfBody.gravityScale;
        float p1Drag = p1.selfBody.drag;
        float p1Mass = p1.selfBody.mass;
        bool p1Grounded = p1.grounded;

        Vector2 p1pos = p1.transform.position;
        //TODO, make this copy function actually work
        State<Player> p1State = p1.ActionFsm.CurrentState.Copy();
        p1.ActionFsm.SubstituteState(p1State);

        //Player2
        bool p2Simulated = p2.selfBody.simulated;
        Vector2 p2Vel = p2.selfBody.velocity;
        float p2GravScale = p2.selfBody.gravityScale;
        float p2Drag = p2.selfBody.drag;
        float p2Mass = p2.selfBody.mass;
        bool p2Grounded = p2.grounded;

        Vector2 p2pos = p2.transform.position;
        //TODO, make this copy function actually work
        State<Player> p2State = p2.ActionFsm.CurrentState.Copy();

        //maintaining the camera physics
        Vector3 cameraPos = GameManager.instance.Camera.transform.position;
        Vector3 cameraVel = GameManager.instance.Camera.selfBody.velocity;

        //Replay the situation on keypress
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                //Reapplying stored physics
                //p1
                p1.selfBody.simulated = p1Simulated;
                p1.selfBody.velocity = p1Vel;
                p1.selfBody.gravityScale = p1GravScale;
                p1.selfBody.drag = p1Drag;
                p1.selfBody.mass = p1Mass;
                p1.grounded = p1Grounded;

                p1.transform.position = p1pos;
                p1.ActionFsm.SubstituteState(p1State.Copy());

                //p2
                p2.selfBody.simulated = p2Simulated;
                p2.selfBody.velocity = p2Vel;
                p2.selfBody.gravityScale = p2GravScale;
                p2.selfBody.drag = p2Drag;
                p2.selfBody.mass = p2Mass;
                p2.grounded = p2Grounded;

                p2.transform.position = p2pos;
                p2.ActionFsm.SubstituteState(p2State.Copy());

                //Reapplying camera physics
                GameManager.instance.Camera.transform.position = cameraPos;
                GameManager.instance.Camera.selfBody.velocity = cameraVel;


                makePrediction = false;
                yield return null;
            }
            else
                yield return new WaitForSeconds(frameLength);
        }
    }
}
