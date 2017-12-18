using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tester : MonoBehaviour {

    public GameObject p1;
    public GameObject p2;

    private float startHealth;
    public string currentTestName;
    private static List<float> healthRecordings = new List<float>();
    public int roundLength = 1250;
    public int sampleCount = 5;
    private static int counter;

    private void Awake()
    {
        startHealth = p2.GetComponent<Player>().health;
    }

	// Update is called once per frame
	void Update () {
        if(counter < sampleCount)
        {
            if (GameManager.instance.currentFrame < 50)
            {
                p1.SetActive(false);
                p2.SetActive(false);
            }
            else if (GameManager.instance.currentFrame > roundLength)
            {
                counter++;

                float healthDifference = startHealth - p2.GetComponent<Player>().health;
                healthRecordings.Add(healthDifference);

                TransitionRecorder.instance.SaveTransitions(currentTestName, counter);
                EventRecorder.instance.WriteToLog(currentTestName);
                if(counter == sampleCount)
                {
                    string filePath = Application.streamingAssetsPath + "/" + currentTestName + "_health.txt";
                    string datalog = "";//"Metadata";

                    for (int i = 0; i < healthRecordings.Count; i++)
                        datalog += healthRecordings[i].ToString() + "\n";

                    File.WriteAllText(filePath, datalog);
                    SceneManager.LoadScene("MainMenu");
                    return;
                }

                SceneManager.LoadScene("MainGame");
            }
            else
            {
                p1.SetActive(true);
                p2.SetActive(true);
            }
        }
    }
}
