using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingMode : MonoBehaviour {

    public Player recordedPlayer;
    public Scenario testScenario;
    public Color recordingColor;
    public bool applyPattern;

    public ActionSequence testSequence = new ActionSequence();
    public string sequenceName;
    bool recording = false;
    int startFrame;

    public SequenceAI patternAI;

    void Start()
    {
        GameManager.instance.currentScenario = testScenario;
    }

    void LateUpdate ()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            if (recording)
                StopRecording();
            else
                StartRecording();
        }

        if(Input.GetKeyDown(KeyCode.T))
        {
            if (!recording && applyPattern)
            {
                testSequence.RestartSequence();
                patternAI.enabled = true;
                patternAI.sequence = testSequence;
            }
            GameManager.instance.LoadRound(testScenario);
        }

        if(Input.GetKeyDown(KeyCode.Y))
        {
            testSequence.SaveSequence(sequenceName);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            testSequence.LoadSequence(sequenceName);
        }
    }

    void ActionPerformed(KeyValuePair<Action, bool> pair)
    {
        if (recording)
        {
            Action a = pair.Key;
            bool isPlayer1 = pair.Value;
            if(isPlayer1 == recordedPlayer.isPlayer1)
                testSequence.AddAction(a, GameManager.instance.currentFrame - startFrame);
        }
    }

    void StartRecording()
    {
        testSequence = new ActionSequence();
        startFrame = GameManager.instance.currentFrame;
        recording = true;
        recordedPlayer.sprite.color = recordingColor;
    }

    void StopRecording()
    {
        recording = false;
        print(testSequence);
        recordedPlayer.sprite.color = Color.white;
    }
}
