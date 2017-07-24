using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingMode : MonoBehaviour {

    public Scenario testScenario;

    void Start()
    {
        GameManager.instance.currentScenario = testScenario;
    }

    void LateUpdate ()
    {

	}
}
