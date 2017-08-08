using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TransitionSolver : MonoBehaviour
{
    public string playerName;
    public Player controlledPlayer;

    Dictionary<AISituation, int> heuristics = new Dictionary<AISituation, int>();
    Dictionary<AISituation, List<Transition>> playerTransitions = new Dictionary<AISituation, List<Transition>>();
    List<Transition> desiredTransitions;

    public void Start()
    {
        LoadTransitions();
    }

    int actionTracker = -1;
    int startFrame = 0;
    public void Update()
    {
        controlledPlayer.AIControlled = true;
        if(desiredTransitions == null)
            desiredTransitions = FindTarget(playerTransitions);
        
        if(actionTracker < desiredTransitions.Count-1)
        {
            //Hackiness to take care of doing the 1st action flawlessly
            if (actionTracker == -1)
            {
                actionTracker = 0;
                startFrame = GameManager.instance.currentFrame;

                PerformedAction firstAction = desiredTransitions[actionTracker].action;

                controlledPlayer.PerformAction(firstAction.action);
                print(controlledPlayer.isPlayer1 + "Action: " + firstAction.action + ": " + firstAction.duration + ": " + GameManager.instance.currentFrame);
            }

            PerformedAction currentAction = desiredTransitions[actionTracker].action;
            AISituation desiredSituation = desiredTransitions[actionTracker].result;

            if (GameManager.instance.currentFrame - startFrame >= currentAction.duration)
            {
                //If the transition was to an unexpected place, replan
                if(!(desiredSituation.Equals(new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1))))
                {
                    print("replanned");
                    print(desiredSituation + " : " + new AISituation(GameRecorder.instance.LatestFrame(),controlledPlayer.isPlayer1));
                    actionTracker = -1;
                    desiredTransitions = FindTarget(playerTransitions);
                    return;
                }

                actionTracker++;
                startFrame = GameManager.instance.currentFrame;

                currentAction = desiredTransitions[actionTracker].action;
                controlledPlayer.PerformAction(currentAction.action);

                print("Action: " + currentAction.action + ": " + currentAction.duration + ": " + GameManager.instance.currentFrame);
            }
        }
        //Restart the process once we've gone through all the actions
        else
        {
            actionTracker = -1;
            desiredTransitions = FindTarget(playerTransitions);
        }
    }

    public void LoadTransitions()
    {
        string directoryPath = Application.streamingAssetsPath + "/TransitionTables/";
        string filePath = directoryPath + playerName + ".txt";

        // deserialize
        playerTransitions = new Dictionary<AISituation, List<Transition>>();

        string contents = File.ReadAllText(filePath);
        string[] serializeObjects = contents.Split(new string[] { "~~~~" }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < serializeObjects.Length - 1; i++)
        {
            string situationContents = serializeObjects[i];
            string[] objects = situationContents.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            AISituation situation = JsonUtility.FromJson<AISituation>(objects[0]);
            playerTransitions.Add(situation, new List<Transition>());

            for (int j = 1; j < objects.Length; j++)
            {
                Transition transition = JsonUtility.FromJson<Transition>(objects[j]);
                playerTransitions[situation].Add(transition);
            }
        }
    }

    public void InitHeuristics()
    {
        foreach(AISituation situation in playerTransitions.Keys)
        {
            //Do some initialization of herustics here ;_;
        }
    }

    public List<Transition> FindTarget(Dictionary<AISituation, List<Transition>> transitions)
    {
        AISituation currentSituation = new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1);

        
        PriorityQueue<KeyValuePair<AISituation, List<Transition>>> pendingSituations = new PriorityQueue<KeyValuePair<AISituation, List<Transition>>>();

        Dictionary<AISituation, List<Transition>> observedSituations = new Dictionary<AISituation, List<Transition>>();

        List<Transition> currentTransitions = new List<Transition>();
        pendingSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(currentSituation, currentTransitions), currentTransitions.Count);

        while (pendingSituations.Count > 0)
        {
            KeyValuePair<AISituation, List<Transition>> pair = pendingSituations.Dequeue();
            currentSituation = pair.Key;
            currentTransitions = pair.Value;

            if (isTargetSituation(currentSituation))
                return currentTransitions;

            if (!transitions.ContainsKey(currentSituation))
                continue;

            if (!observedSituations.ContainsKey(currentSituation))
            {
                observedSituations.Add(currentSituation, currentTransitions);

                List<Transition> availableTransitions = transitions[currentSituation];
                foreach (Transition transition in availableTransitions)
                {
                    if (!observedSituations.ContainsKey(transition.result))
                    {
                        List<Transition> latestTransitions = new List<Transition>();
                        latestTransitions.AddRange(currentTransitions);
                        latestTransitions.Add(transition);

                        pendingSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(transition.result, latestTransitions), latestTransitions.Count);
                    }
                }
            }
        }
        //print("we didn't find the situation");
        //print(currentSituation);

        //Do a random move
        Action randomAction = (Action)Random.Range(0, System.Enum.GetValues(typeof(Action)).Length);
        return new List<Transition>() { new Transition(new PerformedAction(randomAction, 1), currentSituation) };
    }


    public AISituation targetSituation;
    //Used to show the proof of concept. That is, the agent can navigate to the desired state
    //public bool isTargetSituation(AISituation situation)
    //{
    //    return situation.Equals(targetSituation);
    //}

    //We just want to get to a state where we hit the opponent
    public bool isTargetSituation(AISituation situation)
    {
        return situation.opponentStatus == PlayerStatus.Hit;
    }
}