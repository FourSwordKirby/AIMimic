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
    private AISituation currentSituation;
    public bool currentDuration;
    public int lastCapturedFrame;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            SaveTransitions();
        if (Input.GetKeyDown(KeyCode.M))
            LoadTransitions();

        if(!GameManager.instance.roundOver)
        {
            //Edge case for game start
            if(GameManager.instance.currentFrame == 0)
            {
                startFrame = 0;
                lastAction = Action.Stand;
                lastSituation = new AISituation(GameRecorder.instance.LatestFrame(), recordedPlayer.isPlayer1);
            }

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
                AISituation currentSituation = new AISituation(GameRecorder.instance.LatestFrame(), recordedPlayer.isPlayer1);

                //We need to denote that the player just got hit on this frame
                currentSituation.status = PlayerStatus.FreshHit;

                Transition transition = new Transition(lastSituation, performedAction, currentSituation);
                profile.ForceTransition(lastSituation, transition);
                lastCapturedFrame = GameManager.instance.currentFrame;
            }

            startFrame = -1;
        }
        else
        {
            //Checking if the opponent was jsut hit by an attack
            int duration = GameManager.instance.currentFrame - startFrame;

            PerformedAction performedAction = new PerformedAction(lastAction, duration);
            AISituation currentSituation = new AISituation(GameRecorder.instance.LatestFrame(), recordedPlayer.isPlayer1);

            //We need to denote that the player just got hit on this frame
            currentSituation.opponentStatus = PlayerStatus.FreshHit;

            Transition transition = new Transition(lastSituation, performedAction, currentSituation);
            profile.ForceTransition(lastSituation, transition);
            lastCapturedFrame = GameManager.instance.currentFrame;
        }
    }

    //This needs to record the frame after the action ended
    public void ActionEnded(KeyValuePair<Action, bool> pair)
    {
        //Action currentAction = pair.Key;
        //bool isPlayer1 = pair.Value;

        //if (isPlayer1 == recordedPlayer.isPlayer1)
        //{
        //    if (startFrame == -1 || lastSituation == null || currentAction != lastAction)
        //    {
        //        print("Something got messed up");
        //        return;
        //    }

        //    int duration = GameManager.instance.currentFrame - startFrame;
        //    if (duration != 0)
        //    {
        //        PerformedAction performedAction = new PerformedAction(lastAction, duration);
        //        AISituation currentSituation = new AISituation(GameRecorder.instance.LatestFrame(), isPlayer1);
        //        Transition transition = new Transition(lastSituation, performedAction, currentSituation);

        //        if (lastCapturedFrame != GameManager.instance.currentFrame)
        //            profile.LogTransition(lastSituation, transition);
        //        lastCapturedFrame = GameManager.instance.currentFrame;
        //    }
        //}

        //Debug.Break();

        //startFrame = -1;
        //lastSituation = null;
    }

    public void ActionPerformed(KeyValuePair<Action, bool> pair)
    {
        Action currentAction = pair.Key;
        bool isPlayer1 = pair.Value;

        if (isPlayer1 == recordedPlayer.isPlayer1)
        {
            if (startFrame == -1 || lastSituation == null)
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

                if (lastCapturedFrame != GameManager.instance.currentFrame)
                    profile.LogTransition(lastSituation, transition);
                lastCapturedFrame = GameManager.instance.currentFrame;
            }

            lastSituation = currentSituation;
            lastAction = currentAction;
            startFrame = GameManager.instance.currentFrame;
        }
    }
    
    public void StateChanged(AISituation newSituation)
    {
        if (lastCapturedFrame == GameManager.instance.currentFrame)
            return;

        int duration = GameManager.instance.currentFrame - startFrame;
        PerformedAction performedAction = new PerformedAction(lastAction, duration);
        Transition transition = new Transition(lastSituation, performedAction, newSituation);

        profile.LogTransition(lastSituation, transition);
        lastCapturedFrame = GameManager.instance.currentFrame;
    }
}