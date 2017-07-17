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


        for (int i = 0; i < (predictionLength / frameLength) + 1; i++)
        {
            GameObject marker1 = Instantiate(positionTracker);
            GameObject marker2 = Instantiate(positionTracker);

            p1Trackers.Add(marker1);
            p2Trackers.Add(marker2);
        }
    }

    void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            if (Physics2D.autoSimulation)
                predict(GameManager.instance.p1, GameManager.instance.p2, predictionLength);
            else
                Physics2D.autoSimulation = true;
        }
    }
    
    //This will take a single starting snapshot andd an action and return some estimates of the result of taking said action
    void predict(Player p1, Player p2, float time = 1)
    {
        //Copying and storing the physics
        //TODO: Get this to work more consistently.
        System.Reflection.FieldInfo[] fields = p1.selfBody.GetType().GetFields();
        print(fields);
        foreach (System.Reflection.FieldInfo field in fields)
        {
            print(field);
            field.SetValue(p1Dummy, field.GetValue(p1.selfBody));
        }
        bool p1Simulated = p1.selfBody.simulated;
        p1Dummy.simulated = false;

        Vector2 p1pos = p1.transform.position;
        Vector2 p2pos = p2.transform.position;

        Physics2D.autoSimulation = false;
        for(int i  = 0; i < time/frameLength; i++)
        {
            Physics2D.Simulate(frameLength);

            //Spawn a tracker to see where the player ended up
            p1Trackers[i].transform.position = p1.transform.position;
            p2Trackers[i].transform.position = p2.transform.position;
        }

        p1.transform.position = p1pos;
        p2.transform.position = p2pos;

        //Reapplying stored physics
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(field.GetValue(p1.selfBody), p1Dummy);
        }
        p1.selfBody.simulated = p1Simulated;
        p1Dummy.transform.position = Vector3.zero;

        //p1.selfBody.velocity = p1vel;
        //p2.selfBody.velocity = p2vel;

        //Physics2D.autoSimulation = true;
    }

    Vector3 SimulatePosition(Transform t)
    {
        return Vector3.zero;
    }
}
