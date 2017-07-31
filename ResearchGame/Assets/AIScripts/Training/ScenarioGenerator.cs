using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioGenerator : MonoBehaviour {
    
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Tab))
        {
            GameRecorder.instance.CaptureFrame();
            Snapshot snapshot = GameRecorder.instance.snapshots[0];
            print(JsonUtility.ToJson(snapshot));
        }
	}
}
