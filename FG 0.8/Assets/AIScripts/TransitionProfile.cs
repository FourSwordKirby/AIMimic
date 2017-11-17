using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TransitionProfile
{
    Dictionary<AISituation, List<Transition>> playerTransitions = new Dictionary<AISituation, List<Transition>>();

    //This logs the last transition but ensures that if there was another transition logged on this frame, that other transition is overwritten
    public void ForceTransition(AISituation prior, Transition transition)
    {
        if (!playerTransitions.ContainsKey(prior))
            playerTransitions.Add(prior, new List<Transition>());
        else if (playerTransitions[prior][playerTransitions[prior].Count - 1].action.action == transition.action.action
            && playerTransitions[prior][playerTransitions[prior].Count - 1].action.duration == transition.action.duration)
            playerTransitions[prior].RemoveAt(playerTransitions[prior].Count - 1);

        playerTransitions[prior].Add(transition);
    }


    public void LogTransition(AISituation prior, Transition transition)
    {
        if (!playerTransitions.ContainsKey(prior))
            playerTransitions.Add(prior, new List<Transition>());

        playerTransitions[prior].Add(transition);
    }

    public void SaveTransitions(string playerName)
    {
        string directoryPath = Application.streamingAssetsPath + "/TransitionTables/";
        string filePath = directoryPath + playerName + ".txt";

        // serialize
        string datalog = "";
        foreach (AISituation situation in playerTransitions.Keys)
        {
            datalog += JsonUtility.ToJson(situation) + "\n";

            for (int i = 0; i < playerTransitions[situation].Count; i++)
            {
                datalog += JsonUtility.ToJson(playerTransitions[situation][i]) + "\n";
            }

            datalog += "~~~~\n";
        }
        File.WriteAllText(filePath, datalog);

        //Writes a readable version
        datalog = "";
        string readableFilePath = directoryPath + playerName + "Readable.txt";
        foreach (AISituation situation in playerTransitions.Keys)
        {
            datalog += JsonUtility.ToJson(situation) + "\n";

            for (int i = 0; i < playerTransitions[situation].Count; i++)
            {
                datalog += playerTransitions[situation][i].ToString() + "\n";
            }

            datalog += "~~~~\n";
        }
        File.WriteAllText(readableFilePath, datalog);

        Debug.Log("wrote to log");
    }

    public static TransitionProfile LoadTransitions(string playerName)
    {
        string directoryPath = Application.streamingAssetsPath + "/TransitionTables/";
        string filePath = directoryPath + playerName + ".txt";

        // deserialize
        Dictionary<AISituation, List<Transition>> playerTransitions = new Dictionary<AISituation, List<Transition>>();

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
                transition.prior = situation;
                playerTransitions[situation].Add(transition);
            }
        }

        TransitionProfile profile = new TransitionProfile();
        profile.playerTransitions = playerTransitions;

        return profile;
    }


    public delegate bool IsGoal(AISituation x);
    public List<AISituation> getGoalSituations(IsGoal isGoal)
    {
        List<AISituation> goalStituations = new List<AISituation>();
        foreach (AISituation situation in playerTransitions.Keys)
        {
            List<Transition> transitions = playerTransitions[situation];
            foreach (Transition transition in transitions)
            {
                //Quick hacky check to ensure its the 1st time the opponent gets hit
                if (isGoal(transition.result) && !isGoal(transition.prior) && transition.result.status != PlayerStatus.Recovery)
                    goalStituations.Add(transition.result);
            }
        }
        return goalStituations;
    }


    public List<AISituation> getPredecessorSituations(IsGoal isGoal)
    {
        List<AISituation> predStituations = new List<AISituation>();
        foreach (AISituation situation in playerTransitions.Keys)
        {
            List<Transition> transitions = playerTransitions[situation];
            foreach (Transition transition in transitions)
            {
                if (isGoal(transition.result))
                    predStituations.Add(situation);
            }
        }
        return predStituations;
    }

    public List<AISituation> getPredecessorSituations(AISituation goalState)
    {
        List<AISituation> predStituations = new List<AISituation>();
        foreach (AISituation situation in playerTransitions.Keys)
        {
            List<Transition> transitions = playerTransitions[situation];
            foreach (Transition transition in transitions)
            {
                if (transition.result.Equals(goalState))
                {
                    predStituations.Add(situation);
                    break;
                }
            }
        }
        return predStituations;
    }


    public Dictionary<AISituation, List<Transition>> getPlayerTransitions()
    {
        return playerTransitions;
    }

    public Dictionary<PerformedAction, List<SituationChange>> getActionEffects()
    {
        Dictionary<PerformedAction, List<SituationChange>> actionEffects = new Dictionary<PerformedAction, List<SituationChange>>();

        //***********TODO: delete once we've gotten the player to move and stop next to the opponent.*******************
        //PerformedAction action1 = new PerformedAction(Action.Stand, 1);

        //AISituation prior = new AISituation(GameRecorder.instance.LatestFrame());
        //prior.xPos = 7;
        //prior.yPos = 0;
        //prior.opponentXPos = 0;
        //prior.opponentYPos = 4;
        //prior.xVel = xMovement.Right;
        //prior.yVel = yMovement.Neutral;
        //prior.status = PlayerStatus.Moving;

        //AISituation result = new AISituation(GameRecorder.instance.LatestFrame());
        //result.xPos = 7;
        //result.yPos = 0;
        //result.opponentXPos = 0;
        //result.opponentYPos = 4;
        //result.xVel = xMovement.Neutral;
        //result.yVel = yMovement.Neutral;
        //result.status = PlayerStatus.Stand;

        //actionEffects.Add(action1, new List<SituationChange>() { new SituationChange(prior, result) });
        //return actionEffects;
        //********Comment out the above once debugged*********/
        
        foreach (AISituation situation in playerTransitions.Keys)
        {
            List<Transition> transitions = playerTransitions[situation];
            foreach(Transition transition in transitions)
            {
                PerformedAction action = transition.action;
                SituationChange change = new SituationChange(transition.prior, transition.result);

                if (!actionEffects.ContainsKey(action))
                    actionEffects.Add(action, new List<SituationChange>());
                actionEffects[action].Add(change);
            }
        }

        return actionEffects;
    }

    public float SimilarityWeight(AISituation x, AISituation y)
    {
        return Mathf.Pow(AISituation.Similarity(x, y), 2.0f);
    }

    public KeyValuePair<AISituation, float> predictResult(AISituation currentSituation, PerformedAction action, 
                                                            Dictionary<PerformedAction, List<SituationChange>> actionEffects)
    {
        List<SituationChange> situationChanges = actionEffects[action];

        //Insights becuase I'm not really getting anywhere
        //The purpose of these action effects is to get the AI to be able to guess the results of taking an action
        //in some state that it has never seen before.
        //Essentially, we want it to say something like "Walking forward -> x increases by 2" 
        //or "Attack -> if !opponentBlock and close(AI, opponent), then opponentHit"
        //The question is how to learn these sorts of rules on the fly/from previous data
        //As in, how can we figure out the players internal flowchart
        //For every variable in the resulting situation, it relies on some variables from the previous situation to function
        //Maybe do weighting on the prior situations. For example, If the current situation has the opponent close to you, and 
        //the prior experience had you successfully hitting the opponent when they were close to you, boost that instance
        //in the sampling data.


        //We are going to assume that the status change is the mode of the status changes seen for the player

        float situationWeightTotal = situationChanges.Select(x => SimilarityWeight(x.prior, currentSituation)).Aggregate(0.0f, (x,y)=>x+y);

        float xAvg = situationChanges.Select(x => (float)x.xPosChange * SimilarityWeight(x.prior, currentSituation) / situationWeightTotal).Sum();
        float xStd = Mathf.Sqrt(situationChanges.Select(x => x.xPosChange).Average(v => Mathf.Pow(v - xAvg, 2)));

        float yAvg = situationChanges.Select(x => (float)x.yPosChange * SimilarityWeight(x.prior, currentSituation) / situationWeightTotal).Sum();
        float yStd = Mathf.Sqrt(situationChanges.Select(x => x.yPosChange).Average(v => Mathf.Pow(v - yAvg, 2)));

        float xVelAvg = situationChanges.Select(x => (float)x.xVelChange * SimilarityWeight(x.prior, currentSituation) / situationWeightTotal).Sum();
        //float xVelStd = Mathf.Sqrt(situationChanges.Select(x => x.xVelChange).Average(v => Mathf.Pow(v - xVelAvg, 2)));

        float yVelAvg = situationChanges.Select(x => (float)x.yVelChange * SimilarityWeight(x.prior, currentSituation) / situationWeightTotal).Sum();
        //float yVelStd = Mathf.Sqrt(situationChanges.Select(x => x.yVelChange).Average(v => Mathf.Pow(v - yVelAvg, 2)));

        bool bestGrounded = situationChanges.Select(x => (x.resultingGrounded ? 1.0f : 0.0f) * SimilarityWeight(x.prior, currentSituation) / situationWeightTotal).Average() > 0.5f;

        //Implement this by taking the status with the highest weight
        Dictionary<PlayerStatus, float> statusWeights = new Dictionary<PlayerStatus, float>();
        foreach(SituationChange change in situationChanges)
        {
            if (!statusWeights.ContainsKey(change.resultingStatus))
                statusWeights[change.resultingStatus] = 0.0f;
            statusWeights[change.resultingStatus] += SimilarityWeight(change.prior, currentSituation);
        }

        PlayerStatus bestStatus = PlayerStatus.Stand;
        float bestWeight = 0.0f;
        foreach (PlayerStatus status in statusWeights.Keys)
        {
            if(statusWeights[status] > bestWeight)
            {
                bestWeight = statusWeights[status];
                bestStatus = status;
            }
        }


        //Not really sure with what to do for the opponent readings
        //Maybe make opponent readings something close to our prior experiences??
        //Not really sure, for now it changes using the same distribution as the player
        float oppXAvg = situationChanges.Select(x => (float)x.opponentXPosChange * SimilarityWeight(x.prior, currentSituation) / situationWeightTotal).Sum();
        float oppXStd = Mathf.Sqrt(situationChanges.Select(x => x.opponentXPosChange).Average(v => Mathf.Pow(v - oppXAvg, 2)));

        float oppYAvg = situationChanges.Select(x => (float)x.opponentYPosChange * SimilarityWeight(x.prior, currentSituation) / situationWeightTotal).Sum();
        float oppYStd = Mathf.Sqrt(situationChanges.Select(x => x.opponentYPosChange).Average(v => Mathf.Pow(v - oppYAvg, 2)));

        float oppXVelAvg = situationChanges.Select(x => (float)x.opponentXVelChange * SimilarityWeight(x.prior, currentSituation) / situationWeightTotal).Sum();
        //float oppXVelStd = Mathf.Sqrt(situationChanges.Select(x => x.opponentXVelChange).Average(v => Mathf.Pow(v - xVelAvg, 2)));

        float oppYVelAvg = situationChanges.Select(x => (float)x.opponentYVelChange * SimilarityWeight(x.prior, currentSituation) / situationWeightTotal).Sum();
        //float oppYVelStd = Mathf.Sqrt(situationChanges.Select(x => x.opponentYVelChange).Average(v => Mathf.Pow(v - oppYVelAvg, 2)));

        bool oppBestGrounded = situationChanges.Select(x => (x.opponentResultingGrounded ? 1.0f : 0.0f) * SimilarityWeight(x.prior, currentSituation) / situationWeightTotal).Average() > 0.5f;

        Dictionary<PlayerStatus, float> oppStatusWeights = new Dictionary<PlayerStatus, float>();
        foreach (SituationChange change in situationChanges)
        {
            if (!oppStatusWeights.ContainsKey(change.opponentResultingStatus))
                oppStatusWeights[change.opponentResultingStatus] = 0.0f;
            oppStatusWeights[change.opponentResultingStatus] += SimilarityWeight(change.prior, currentSituation);
        }

        PlayerStatus oppBestStatus = PlayerStatus.Stand;
        float oppBestWeight = 0.0f;
        foreach (PlayerStatus status in oppStatusWeights.Keys)
        {
            if (oppStatusWeights[status] > oppBestWeight)
            {
                oppBestWeight = oppStatusWeights[status];
                oppBestStatus = status;
            }
        }

        SituationChange predictedChange = new SituationChange();
        predictedChange.xPosChange = Mathf.RoundToInt(xAvg);
        predictedChange.yPosChange = Mathf.RoundToInt(yAvg);

        predictedChange.opponentXPosChange = Mathf.RoundToInt(oppXAvg);
        predictedChange.opponentYPosChange = Mathf.RoundToInt(oppYAvg);

        //Debug.Log(situationWeightTotal);

        //foreach (SituationChange change in situationChanges)
        //{
        //    Debug.Log(action);
        //    Debug.Log(change.xVelChange);
        //    Debug.Log(SimilarityWeight(change.prior, currentSituation));
        //}

        //Debug.Log(Mathf.RoundToInt(xVelAvg));

        predictedChange.xVelChange = Mathf.RoundToInt(xVelAvg);
        predictedChange.yVelChange = Mathf.RoundToInt(yVelAvg);

        predictedChange.opponentXVelChange = Mathf.RoundToInt(oppXVelAvg);
        predictedChange.opponentYVelChange = Mathf.RoundToInt(oppYVelAvg);

        //predictedChange.healthChange = 0;
        //predictedChange.opponentHealthChange = 0;

        predictedChange.resultingGrounded = bestGrounded;
        predictedChange.opponentResultingGrounded = oppBestGrounded;

        predictedChange.resultingStatus = bestStatus;
        predictedChange.opponentResultingStatus = oppBestStatus;


        AISituation newSituation = SituationChange.ApplyChange(currentSituation, predictedChange);
        float confidence = 1.0f; //?????TODO: not sure how to assign a confidence to our prediction

        KeyValuePair<AISituation, float> pair = new KeyValuePair<AISituation, float>(newSituation, confidence);
        return pair;
    }
}
