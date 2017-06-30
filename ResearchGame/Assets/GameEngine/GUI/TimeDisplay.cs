﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimeDisplay : MonoBehaviour {

    public Text currentText;
	
	// Update is called once per frame
	void Update () {
        int time = (int)(GameManager.instance.timeRemaining);
        if (time < 10)
            currentText.text = "0" + time.ToString();
        else
            currentText.text = time.ToString();
	}
}
