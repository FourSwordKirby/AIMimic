using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ForwardSimulation : MonoBehaviour {

    public GameObject positionTracker;
    public Rigidbody2D p1Dummy;
    public Rigidbody2D p2Dummy;

    float frameLength;
    float predictionLength = 0.3f;

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

    void LateUpdate()
    {
        //predict(GameManager.instance.p1, GameManager.instance.p2, predictionLength);
    }
    
    //This will take a single starting snapshot andd an action and return some estimates of the result of taking said action
    void predict(Player p1, Player p2, float time = 1)
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
        State<Player> p1State = p1.ActionFsm.CurrentState;
        //TODO, make this copy function actually work
        p1.ActionFsm.SubstituteState(p1State.Copy());

        //Player2
        bool p2Simulated = p2.selfBody.simulated;
        Vector2 p2Vel = p2.selfBody.velocity;
        float p2GravScale = p2.selfBody.gravityScale;
        float p2Drag = p2.selfBody.drag;
        float p2Mass = p2.selfBody.mass;
        bool p2Grounded = p2.grounded;

        Vector2 p2pos = p2.transform.position;
        State<Player> p2State = p2.ActionFsm.CurrentState;
        //TODO, make this copy function actually work
        p2.ActionFsm.SubstituteState(p2State.Copy());

        //maintaining the camera physics
        Vector3 cameraPos = GameManager.instance.Camera.transform.position;
        Vector3 cameraVel = GameManager.instance.Camera.selfBody.velocity;

        Physics2D.autoSimulation = false;
        for(int i  = 0; i < time/frameLength; i++)
        {
            Physics2D.Simulate(frameLength);

            //Spawn a tracker to see where the player ended up
            p1Trackers[i].transform.position = p1.transform.position;
            p2Trackers[i].transform.position = p2.transform.position;
        }

        //Reapplying stored physics
        //p1
        p1.selfBody.simulated = p1Simulated;
        p1.selfBody.velocity = p1Vel;
        p1.selfBody.gravityScale = p1GravScale;
        p1.selfBody.drag = p1Drag;
        p1.selfBody.mass = p1Mass;
        p1.grounded = p1Grounded;
        
        p1.transform.position = p1pos;
        p1.ActionFsm.SubstituteState(p1State);

        //p2
        p2.selfBody.simulated = p2Simulated;
        p2.selfBody.velocity = p2Vel;
        p2.selfBody.gravityScale = p2GravScale;
        p2.selfBody.drag = p2Drag;
        p2.selfBody.mass = p2Mass;
        p2.grounded = p2Grounded;

        p2.transform.position = p2pos;
        p2.ActionFsm.SubstituteState(p2State);

        //Reapplying camera physics
        GameManager.instance.Camera.transform.position = cameraPos;
        GameManager.instance.Camera.selfBody.velocity = cameraVel;

        Physics2D.autoSimulation = true;
    }

    Vector3 SimulatePosition(Transform t)
    {
        return Vector3.zero;
    }
}
