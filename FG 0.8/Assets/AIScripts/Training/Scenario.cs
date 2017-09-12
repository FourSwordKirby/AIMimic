using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scenario : MonoBehaviour {

    public float p1Health;
    public float p2Health;
    public float timeLimit;
    public int roundToWin;

    public GameObject spawnPoint1;
    public GameObject spawnPoint2;

    public bool isTraining;

    public State<Player> p1State;
    public State<Player> p2State;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
