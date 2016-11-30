using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    public Text currentText;
	
	// Update is called once per frame
	void Update () {
        int time = (int)(GameManager.timeRemaining);
        if (time < 10)
            currentText.text = "0" + time.ToString();
        else
            currentText.text = time.ToString();
	}
}
