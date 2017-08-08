using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RLSimulationMode : MonoBehaviour
{
    public int simulationCount;
    public float simulationLength;

    float frameLength;

    public AIAgent bot1;
    public AIAgent bot2;

    public Scenario defaultScenario;

    private void Start()
    {
        frameLength = 1.0f / Application.targetFrameRate;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            MassSimulate(bot1, bot2, defaultScenario);
    }
    
    void Simulate(AIAgent bot1, AIAgent bot2, Scenario defaultScenario)
    {
        GameManager.instance.LoadRound(defaultScenario);

        Player p1 = GameManager.instance.p1;
        Player p2 = GameManager.instance.p2;

        Physics2D.autoSimulation = false;

        p1.AIControlled = true;
        p2.AIControlled = true;

        for (int i = 0; i < simulationLength / frameLength; i++)
        {
            Physics2D.Simulate(frameLength);
            GameManager.instance.AdvanceTime();

            GameRecorder.instance.LatestFrame();
            bot1.ObserveState();
            Action a1 = bot1.GetAction();
            bot1.PerformAction(a1);


            bot2.ObserveState();
            Action a2 = bot2.GetAction();
            bot2.PerformAction(a2);

            p1.ActionFsm.CurrentState.Execute();
            p2.ActionFsm.CurrentState.Execute();
        }
    }

    // Update is called once per frame
    void MassSimulate(AIAgent bot1, AIAgent bot2, Scenario defaultScenario)
    {
        bot1.Reset();
        bot2.Reset();
        for (int i = 0; i < simulationCount; i++)
        {
            Reset();
            Simulate(bot1, bot2, defaultScenario);
        }
        bot1.Reset();
        bot2.Reset();

        Physics2D.autoSimulation = true;
        GameManager.instance.p1.AIControlled = false;
        GameManager.instance.p2.AIControlled = false;
    }

    private void Reset()
    {
        ;
    }
}
