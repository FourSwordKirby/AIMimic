using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationMode : MonoBehaviour
{
    int simulationCount = 100;
    int failures = 0;

    float frameLength;
    float predictionLength;
    
    public Scenario testScenario;
    public string sequenceName;
    ActionSequence testSequence = new ActionSequence();

    private void Start()
    {
        testSequence.LoadSequence(sequenceName);
        frameLength = 1.0f / Application.targetFrameRate;
        predictionLength = 10.0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            MassSimulate(testScenario, testSequence);
    }

    // Update is called once per frame
    void MassSimulate(Scenario scenario, ActionSequence sequence)
    {
        for (int i = 0; i < simulationCount; i++)
        {
            Simulate(scenario, sequence);
        }

        Physics2D.autoSimulation = true;
        GameManager.instance.p1.AIControlled = false;
        GameManager.instance.p2.AIControlled = false;

        print("Failures" + failures);
    }

    void Simulate(Scenario scenario, ActionSequence sequence)
    {
        testSequence.RestartSequence();
        GameManager.instance.LoadRound(testScenario);

        Player p1 = GameManager.instance.p1;
        Player p2 = GameManager.instance.p2;

        int currentFrame = 0;
        Physics2D.autoSimulation = false;

        p1.AIControlled = true;
        p2.AIControlled = true;

        for (int i = 0; i < predictionLength / frameLength; i++)
        {
            Physics2D.Simulate(frameLength);
            currentFrame++;

            Action a = sequence.GetAction(currentFrame);
            if(Random.Range(0.0f, 1.0f) > 0.5f)
                p1.PerformAction(Action.StandBlock);
            else
                p1.PerformAction(Action.CrouchBlock);
            p2.PerformAction(a);

            p1.ActionFsm.CurrentState.Execute();
            p2.ActionFsm.CurrentState.Execute();
        }
        if (p1.health < 3)
        {
            failures++;
        }
    }
}
