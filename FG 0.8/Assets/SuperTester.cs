using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperTester : MonoBehaviour {

    public string playerName;
    public int playerLog;
    public bool loadAll;

    public NgramAI ngram;
    public GhostAI ghost;
    public TransitionSolver AI;

    public Tester tester;

    public static int currentTimeStep = -1;

    private void Start()
    {
        currentTimeStep++;
        ngram.playerProfileName = playerName;
        ngram.loadAll = loadAll;
        ngram.logNumber = playerLog;
        ghost.playerProfileName = playerName;
        ghost.loadAll = loadAll;
        ghost.logNumber = playerLog;
        AI.playerName = playerName;
        AI.loadAll = loadAll;
        AI.logNumber = (playerLog+1);
    }

	// Update is called once per frame
	void Update () {
        if(currentTimeStep % 3 == 0)
        {
            tester.currentTestName = playerName + (loadAll ? "All" : "") + "ngram";
            ngram.gameObject.SetActive(true);
            ghost.gameObject.SetActive(false);
            AI.gameObject.SetActive(false);
        }
        else if (currentTimeStep % 3 == 1)
        {
            tester.currentTestName = playerName + (loadAll ? "All" : "") + "ghost";
            ngram.gameObject.SetActive(false);
            ghost.gameObject.SetActive(true);
            AI.gameObject.SetActive(false);
        }
        else if (currentTimeStep % 3 == 2)
        {
            tester.currentTestName = playerName + (loadAll ? "All" : "") + "AI";
            ngram.gameObject.SetActive(false);
            ghost.gameObject.SetActive(false);
            AI.gameObject.SetActive(true);
        }
    }
}
