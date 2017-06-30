using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameUI : MonoBehaviour {

    public Text frameRateUI;
	
	// Update is called once per frame
	void Update () {
        frameRateUI.text = "Frame: " + GameManager.instance.currentFrame.ToString();
	}
}
