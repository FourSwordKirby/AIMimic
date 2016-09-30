using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    public Text currentText;
	
	// Update is called once per frame
	void Update () {
        currentText.text = ((int)(GameManager.timeRemaining)).ToString();
	}
}
