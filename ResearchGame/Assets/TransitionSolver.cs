using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TransitionSolver : MonoBehaviour
{
    public string playerName;
    public Player controlledPlayer;


    Dictionary<AISituation, List<Transition>> playerTransitions = new Dictionary<AISituation, List<Transition>>();
    List<PerformedAction> desiredActions;
    public AISituation targetSituation;

    public void Start()
    {
        LoadTransitions();
    }

    int actionTracker = 0;
    int startFrame = -200;
    public void Update()
    {
        controlledPlayer.AIControlled = true;
        if(desiredActions == null)
            desiredActions = FindTarget(playerTransitions, targetSituation);

        if(actionTracker < desiredActions.Count)
        {
            PerformedAction currentAction = desiredActions[actionTracker];
            if (currentAction.duration < GameManager.instance.currentFrame - startFrame)
            {
                actionTracker++;
                startFrame = GameManager.instance.currentFrame;

                print("Action: " + currentAction.action + ": " + currentAction.duration);
            }
            controlledPlayer.PerformAction(currentAction.action);
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

    public List<PerformedAction> FindTarget(Dictionary<AISituation, List<Transition>> transitions, AISituation targetSituation)
    {
        AISituation currentSituation = new AISituation(GameRecorder.instance.CaptureFrame(), controlledPlayer.isPlayer1);

        Queue<KeyValuePair<AISituation, List<PerformedAction>>> pendingSituations = new Queue<KeyValuePair<AISituation, List<PerformedAction>>>();
        Dictionary<AISituation, List<PerformedAction>> observedSituations = new Dictionary<AISituation, List<PerformedAction>>();

        List<PerformedAction> currentActions = new List<PerformedAction>();
        pendingSituations.Enqueue(new KeyValuePair<AISituation, List<PerformedAction>>(currentSituation, currentActions));

        while (pendingSituations.Count > 0)
        {
            KeyValuePair<AISituation, List<PerformedAction>> pair = pendingSituations.Dequeue();
            currentSituation = pair.Key;
            currentActions = pair.Value;

            if (currentSituation.Equals(targetSituation))
                return currentActions;
            
            if (!observedSituations.ContainsKey(currentSituation))
            {
                observedSituations.Add(currentSituation, currentActions);

                List<Transition> availableTransitions = transitions[currentSituation];
                foreach (Transition transition in availableTransitions)
                {
                    if (!observedSituations.ContainsKey(transition.result))
                    {
                        List<PerformedAction> performedActions = new List<PerformedAction>();
                        performedActions.AddRange(currentActions);
                        performedActions.Add(transition.action);

                        pendingSituations.Enqueue(new KeyValuePair<AISituation, List<PerformedAction>>(transition.result, performedActions));
                    }
                }
            }
        }
        print("we didn't find the situation");
        return null;
    }
}