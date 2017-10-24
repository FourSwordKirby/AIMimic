using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TransitionProfile
{
    Dictionary<AISituation, List<Transition>> playerTransitions = new Dictionary<AISituation, List<Transition>>();

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
}
