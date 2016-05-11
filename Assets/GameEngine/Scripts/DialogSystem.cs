 using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogSystem : MonoBehaviour {
    public Dictionary<string, string> DialogTriggers;
   
    //Actually do this at some point it's too hard

	// Use this for initialization
	void Start () {
        DialogTriggers = new Dictionary<string, string>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
