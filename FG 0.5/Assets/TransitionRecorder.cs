using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TransitionRecorder : MonoBehaviour
{
    TransitionProfile profile = new TransitionProfile();
    
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
        profile.SaveTransitions(playerName);
    }

    public void LoadTransitions()
    {
        profile = TransitionProfile.LoadTransitions(playerName);
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

                Transition transition = new Transition(lastSituation, performedAction, currentSituation);
                profile.LogTransition(lastSituation, transition);
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
                Transition transition = new Transition(lastSituation, performedAction, currentSituation);

                profile.LogTransition(lastSituation, transition);
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
        Transition transition = new Transition(lastSituation, performedAction, newSituation);

        profile.LogTransition(lastSituation, transition);
    }
}