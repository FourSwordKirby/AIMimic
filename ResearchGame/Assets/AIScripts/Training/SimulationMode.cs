using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationMode : MonoBehaviour
{
    public int simulationCount;
    public float simulationLength;

    float frameLength;

    public AIAgent bot;
    public Scenario testScenario;
    public List<Result> simulationResults;

    private void Start()
    {
        frameLength = 1.0f / Application.targetFrameRate;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
            MassSimulate(testScenario, bot);
    }

    //This should produce a list of results that occured after this simulation
    List<Result> Simulate(Scenario scenario, AIAgent bot)
    {
        bot.Reset();
        GameManager.instance.LoadRound(testScenario);

        Player p1 = GameManager.instance.p1;
        Player p2 = GameManager.instance.p2;
        
        Physics2D.autoSimulation = false;

        p1.AIControlled = true;
        p2.AIControlled = true;

        for (int i = 0; i < simulationLength / frameLength; i++)
        {
            Physics2D.Simulate(frameLength);
            GameManager.instance.AdvanceTime();

            GameRecorder.instance.CaptureFrame();
            bot.ObserveState();
            Action a = bot.GetAction();
            bot.PerformAction(a);

            p1.ActionFsm.CurrentState.Execute();
            p2.ActionFsm.CurrentState.Execute();
        }

        return simulationResults;
    }

    public void Hit(Hitbox hitbox)
    {
        if(hitbox.owner.isPlayer1 == bot.AIPlayer.isPlayer1)
            simulationResults.Add(Result.Hit);
        else
            simulationResults.Add(Result.Landed);
        p1Attacked = false;
        p2Attacked = false;
    }

    public void Block(Hitbox hitbox)
    {
        if (hitbox.owner.isPlayer1 == bot.AIPlayer.isPlayer1)
            simulationResults.Add(Result.Block);
        else
            simulationResults.Add(Result.Blocked);
        p1Attacked = false;
        p2Attacked = false;
    }

    bool p1Attacked = false;
    bool p2Attacked = false;

    bool p1JustAttacked = false;
    bool p2JustAttacked = false;
    public void ActionPerformed(KeyValuePair<Action, bool> pair)
    {
        Action action = pair.Key;
        bool isPlayer1 = pair.Value;
        
        if (isPlayer1)
        {
            if (action == Action.AirAttack || action == Action.LowAttack || action == Action.Attack)
            {
                p1Attacked = true;
                p1JustAttacked = true;
            }
            else
                p1JustAttacked = false;
        }
        else
        {
            if (action == Action.AirAttack || action == Action.LowAttack || action == Action.Attack)
            {
                p2Attacked = true;
                p2JustAttacked = true;
            }
            else
                p2JustAttacked = false;
        }

        if (p1Attacked && !p1JustAttacked)
        {
            if(bot.AIPlayer.isPlayer1)
                simulationResults.Add(Result.Dodged);
            else
                simulationResults.Add(Result.Whiffed);
            p1Attacked = false;
        }
        if (p2Attacked && !p2JustAttacked)
        {
            if (bot.AIPlayer.isPlayer1)
                simulationResults.Add(Result.Whiffed);
            else
                simulationResults.Add(Result.Dodged);
            p2Attacked = false;
        }
    }

    // Update is called once per frame
    void MassSimulate(Scenario scenario, AIAgent bot)
    {
        for (int i = 0; i < simulationCount; i++)
        {
            Simulate(scenario, bot);
        }

        Physics2D.autoSimulation = true;
        GameManager.instance.p1.AIControlled = false;
        GameManager.instance.p2.AIControlled = false;
    }
}
