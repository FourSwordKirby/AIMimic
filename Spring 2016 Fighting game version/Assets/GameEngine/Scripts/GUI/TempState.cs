using UnityEngine;
using System.Collections;

public class TempState : MonoBehaviour {

	public static TempState Instance = null;

    public string playerOne;
    public string playerTwo;
    public string playerThree;
    public string playerFour;

	void Awake () {
		// Checks for conflicting instances
		if (Instance == null) {
			Instance = this;
		}

		DontDestroyOnLoad (gameObject);
	}

	public void setPlayerOne (string name) {
		playerOne = name;
		print(playerOne);
	}

    public void setPlayerTwo (string name)
    {
        playerTwo = name;
        print(playerTwo);
    }

    public void setPlayerThree (string name)
    {
        playerThree = name;
        print(playerThree);
    }

    public void setPlayerFour (string name)
    {
        playerFour = name;
        print(playerFour);
    }
    public string getPlayerOne () { return playerOne; }
    public string getPlayerTwo() { return playerTwo; }
    public string getPlayerThree() { return playerThree; }
    public string getPlayerFour() { return playerFour; }

}
