using UnityEngine;
using System.Collections;

public class ButtonLoadLevel : MonoBehaviour {

	public void LoadLevelButton (int index) {
		Application.LoadLevel (index);
	}

	public void LoadLevelButton (string name) {
		Application.LoadLevel (name);
	}
}
