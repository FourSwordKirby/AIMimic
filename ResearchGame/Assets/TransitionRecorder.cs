using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TransitionRecorder : MonoBehaviour
{
    Dictionary<AISituation, List<Transition>> playerTransitions = new Dictionary<AISituation, List<Transition>>();
    public Player recordedPlayer;

    public AISituation lastSituation;
    public Action lastAction;
    
    public string playerName;

    int startFrame = -1;
    AISituation currentSituation;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            SaveTransitions();
        if (Input.GetKeyDown(KeyCode.M))
            LoadTransitions();

        if(!GameManager.instance.roundOver)
        {
            if(currentSituation == null)
                currentSituation = new AISituation(GameRecorder.instance.LatestFrame(), recordedPlayer.isPlayer1);
            AISituation newSituation = new AISituation(GameRecorder.instance.LatestFrame(), recordedPlayer.isPlayer1);
            if(!newSituation.Equals(currentSituation))
            {
                StateChanged(newSituation);
                currentSituation = newSituation;
            }
        }
    }

    public void SaveTransitions()
    {
        string directoryPath = Application.streamingAssetsPath + "/TransitionTables/";
        string filePath = directoryPath + playerName + ".txt";

        // serialize
        string datalog = "";
        foreach (AISituation situation in playerTransitions.Keys)
        {
            datalog += JsonUtility.ToJson(situation) + "\n";

            for(int i = 0; i < playerTransitions[situation].Count; i++)
            {
                datalog += JsonUtility.ToJson(playerTransitions[situation][i]) + "\n";
            }

            datalog += "~~~~\n";
        }
        File.WriteAllText(filePath, datalog);
        Debug.Log("wrote to log");
    }

    public void LoadTransitions()
    {
        string directoryPath = Application.streamingAssetsPath + "/TransitionTables/";
        string filePath = directoryPath + playerName + ".txt";

        // deserialize
        playerTransitions = new Dictionary<AISituation, List<Transition>>();

        string contents = File.ReadAllText(filePath);
        string[] serializeObjects = contents.Split(new string[] { "~~~~" }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < serializeObjects.Length-1; i++)
        {
            string situationContents = serializeObjects[i];
            string[] objects = situationContents.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            AISituation situation = JsonUtility.FromJson<AISituation>(objects[0]);
            playerTransitions.Add(situation, new List<Transition>());

            print(situation);

            for (int j = 1; j < objects.Length; j++)
            {
                Transition transition = JsonUtility.FromJson<Transition>(objects[j]);
                playerTransitions[situation].Add(transition);
            }
        }
    }

    public void Hit(Hitbox hitbox)
    {
        //The palyer we're recording has been hit and we need to end the last action they have done
        if (hitbox.owner.isPlayer1 != recordedPlayer.isPlayer1)
        {
            if(!recordedPlayer.stunned)
            {
                int duration = GameManager.instance.currentFrame - startFrame;

                PerformedAction performedAction = new PerformedAction(lastAction, duration);
                AISituation currentSituation = new AISituation(GameRecorder.instance.LatestFrame());

                Transition transition = new Transition(performedAction, currentSituation);
                playerTransitions[lastSituation].Add(transition);
            }

            startFrame = -1;
        }
    }

    public void ActionPerformed(KeyValuePair<Action, bool> pair)
    {
        bool isPlayer1 = pair.Value;

        if (isPlayer1 == recordedPlayer.isPlayer1)
        {
            if(startFrame == -1)
            {
                lastSituation = new AISituation(GameRecorder.instance.LatestFrame(), isPlayer1);
                lastAction = pair.Key;
                startFrame = GameManager.instance.currentFrame;
                return;
            }

            int duration = GameManager.instance.currentFrame - startFrame;

            if(duration != 0)
            {
                PerformedAction performedAction = new PerformedAction(lastAction, duration);
                AISituation currentSituation = new AISituation(GameRecorder.instance.LatestFrame(), isPlayer1);
                Transition transition = new Transition(performedAction, currentSituation);

                if (!playerTransitions.ContainsKey(lastSituation))
                    playerTransitions.Add(lastSituation, new List<Transition>());
                playerTransitions[lastSituation].Add(transition);
            }

            lastSituation = currentSituation;
            lastAction = pair.Key;
            startFrame = GameManager.instance.currentFrame;
        }
    }
    
    public void StateChanged(AISituation newSituation)
    {
        int duration = GameManager.instance.currentFrame - startFrame;
        PerformedAction performedAction = new PerformedAction(lastAction, duration);
        Transition transition = new Transition(performedAction, newSituation);

        if (!playerTransitions.ContainsKey(lastSituation))
            playerTransitions.Add(lastSituation, new List<Transition>());
        playerTransitions[lastSituation].Add(transition);
    }
}