using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class TransitionSolver : MonoBehaviour
{
    public bool usingActionEffects;
    public bool searchSpecificTarget;

    public string playerName;
    public Player controlledPlayer;

    Dictionary<AISituation, int> heuristics = new Dictionary<AISituation, int>();
    Dictionary<AISituation, List<Transition>> playerTransitions = new Dictionary<AISituation, List<Transition>>();
    Dictionary<PerformedAction, List<SituationChange>> actionEffects = new Dictionary<PerformedAction, List<SituationChange>>();

    List<Transition> desiredTransitions;

    public void Start()
    {
        LoadTransitions();
    }

    public Dictionary<PlayerStatus, List<Action>> ValidMoveTable = 
        new Dictionary<PlayerStatus, List<Action>>()
        { {PlayerStatus.Stand, new List<Action>() { Action.Stand, Action.Crouch, Action.WalkLeft, Action.WalkRight, Action.JumpLeft, Action.JumpRight, Action.JumpNeutral, Action.Attack, Action.Overhead,  Action.StandBlock, Action.DashLeft, Action.DashRight, Action.DP} },
            {PlayerStatus.Crouch, new List<Action>() {Action.Stand, Action.Crouch, Action.LowAttack, Action.JumpLeft, Action.JumpRight, Action.JumpNeutral, Action.CrouchBlock}  },
            {PlayerStatus.Air, new List<Action>() {Action.Stand, Action.Crouch, Action.AirAttack, Action.AirdashLeft, Action.AirdashRight, Action.AirdashLeft }  },
            {PlayerStatus.Highblock, new List<Action>() {Action.Stand, Action.StandBlock, Action.CrouchBlock, Action.JumpLeft, Action.JumpRight, Action.JumpNeutral }  },
            {PlayerStatus.Lowblock, new List<Action>() { Action.Crouch, Action.StandBlock, Action.CrouchBlock, Action.JumpLeft, Action.JumpRight, Action.JumpNeutral }  },
            {PlayerStatus.Hit, new List<Action>() {}  },
            {PlayerStatus.KnockdownHit, new List<Action>() { Action.TechLeft, Action.TechNeutral, Action.TechRight, Action.DP}  },
            {PlayerStatus.Tech, new List<Action>() {Action.Stand, Action.Crouch, Action.StandBlock, Action.CrouchBlock}  },
            {PlayerStatus.Moving, new List<Action>() { Action.Stand, Action.Crouch, Action.StandBlock, Action.JumpLeft, Action.JumpNeutral, Action.JumpRight, Action.DashLeft, Action.DashRight, Action.Attack, Action.Overhead, Action.DP}  },
            {PlayerStatus.Dashing, new List<Action>() { Action.Stand, Action.Crouch}  },
            {PlayerStatus.AirDashing,new List<Action>() {Action.AirAttack }  },
            {PlayerStatus.StandAttack, new List<Action>() {Action.Stand }  },
            {PlayerStatus.LowAttack, new List<Action>() {Action.Crouch }  },
            {PlayerStatus.OverheadAttack,new List<Action>() {Action.Stand }  },
            {PlayerStatus.AirAttack, new List<Action>() {Action.Stand }  },
            {PlayerStatus.DP, new List<Action>() {Action.Stand }  },
            {PlayerStatus.Recovery, new List<Action>() {Action.Stand, Action.Crouch }  },
        };


    int actionTracker = 0;
    int startFrame = 0;
    int attemptStartFrame = 0;
    int attemptLimit = 150;
    bool actionCompleted = false;

    int delay = 0;
    public void Update()
    {
        //Minor delay in order to see what's actually going on
        if(delay > 0)
        {
            UnityEngine.Debug.Break();
            delay--;
        }


        controlledPlayer.AIControlled = true;
        if (desiredTransitions == null)
        {
            desiredTransitions = usingActionEffects ? FindTarget(playerTransitions, actionEffects) : FindTarget(playerTransitions);
            //desiredTransitions = usingActionEffects ? FindTarget(actionEffects) : FindTarget(playerTransitions);
        }

        if (actionTracker < desiredTransitions.Count)
        {
            PerformedAction currentAction = desiredTransitions[actionTracker].action;
            AISituation desiredSituation = desiredTransitions[actionTracker].result;

            if (!actionCompleted)
            {
                startFrame = GameManager.instance.currentFrame;
                actionCompleted = controlledPlayer.PerformAction(currentAction.action, true);
                if(actionCompleted)
                    attemptStartFrame = GameManager.instance.currentFrame;
            }

            if(GameManager.instance.currentFrame - attemptStartFrame > currentAction.duration + attemptLimit && !actionCompleted)
            {
                print("attempt failed " + currentAction + ", replanned" + new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1));
                actionTracker = 0;

                desiredTransitions = usingActionEffects ? FindTarget(playerTransitions, actionEffects) : FindTarget(playerTransitions);
                //desiredTransitions = usingActionEffects ? FindTarget(actionEffects) : FindTarget(playerTransitions);
                return;
            }

            if (GameManager.instance.currentFrame - startFrame >= currentAction.duration)
            {
                actionCompleted = false;
                actionTracker++;

                //If the transition was to an unexpected place, replan
                //TODO: make the replanning more lenient, so it doesn't need a 1 to 1 match with regards to it's planned situation
                //TODO: make it learn that it needs to crouch -> attack in order to do low attacks and Jump -> attack to do air attacks
                //When determining what action to do next, it should make sure that the action effect that it takes has minimal friction between the states?
                //Basically, have the priority system take into account the feasibility of the action when comparing 2 states
                //The key idea is to make it favor doing stand -> jump -> air attack over stand -> air attack
                if (!(desiredSituation.Equals(new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1))))
                {
                    //print("action" + currentAction + "replanned " + desiredSituation + " : " + new AISituation(GameRecorder.instance.LatestFrame(),controlledPlayer.isPlayer1));
                    actionTracker = 0;

                    desiredTransitions = usingActionEffects ? FindTarget(playerTransitions, actionEffects) : FindTarget(playerTransitions);
                    //desiredTransitions = usingActionEffects ? FindTarget(actionEffects) : FindTarget(playerTransitions);
                    return;
                }

                if (!(actionTracker < desiredTransitions.Count))
                {
                    print("we're done. Plan length: " + desiredTransitions.Count);
                    return;
                }

                currentAction = desiredTransitions[actionTracker].action;
            }
        }
        //Restart the process once we've gone through all the actions
        else
        {
            actionTracker = 0;
            desiredTransitions = usingActionEffects ? FindTarget(playerTransitions, actionEffects) : FindTarget(playerTransitions);
            //desiredTransitions =  usingActionEffects ? FindTarget(actionEffects):FindTarget(playerTransitions);
        }
    }

    public void LoadTransitions()
    {
        TransitionProfile profile = TransitionProfile.LoadTransitions(playerName);

        playerTransitions = profile.getPlayerTransitions();
        actionEffects = profile.getActionEffects();
    }

    public List<Transition> FindTarget(Dictionary<AISituation, List<Transition>> transitions)
    {
        AISituation originalSituation = new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1);
        AISituation currentSituation = originalSituation;

        PriorityQueue<KeyValuePair<AISituation, List<Transition>>> pendingSituations = new PriorityQueue<KeyValuePair<AISituation, List<Transition>>>();
        Dictionary<AISituation, List<Transition>> observedSituations = new Dictionary<AISituation, List<Transition>>();

        List<Transition> currentTransitions = new List<Transition>();
        pendingSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(currentSituation, currentTransitions), currentTransitions.Count);

        while (pendingSituations.Count > 0)
        {
            KeyValuePair<AISituation, List<Transition>> pair = pendingSituations.Dequeue().Value;
            currentSituation = pair.Key;
            currentTransitions = pair.Value;

            if (isTargetSituation(currentSituation) && currentTransitions.Count > 0)
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
                        if (!ValidMoveTable[currentSituation.status].Contains(transition.action.action))
                            print("Invalid action detected on table" + currentSituation.status + transition.action.action);

                        List<Transition> latestTransitions = new List<Transition>();
                        latestTransitions.AddRange(currentTransitions);
                        latestTransitions.Add(transition);

                        pendingSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(transition.result, latestTransitions), latestTransitions.Count);
                    }
                }
            }
        }

        //Do a random move
        //If EVERYTHING else fails, do a random move
        Action randomAction = ValidMoveTable[originalSituation.status][Random.Range(0, ValidMoveTable[originalSituation.status].Count)];
        print("random move" + randomAction + originalSituation.status);
        return new List<Transition>() { new Transition(originalSituation, new PerformedAction(randomAction, 1), originalSituation) };
    }

    public List<Transition> FindTarget(Dictionary<PerformedAction, List<SituationChange>> actionEffects)
    {
        AISituation originalSituation = new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1);
        AISituation currentSituation = originalSituation;

        PriorityQueue<KeyValuePair<AISituation, List<Transition>>> pendingSituations = new PriorityQueue<KeyValuePair<AISituation, List<Transition>>>();

        Dictionary<AISituation, List<Transition>> observedSituations = new Dictionary<AISituation, List<Transition>>();

        List<Transition> currentTransitions = new List<Transition>();
        pendingSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(currentSituation, currentTransitions), currentTransitions.Count);

        while (pendingSituations.Count > 0)
        {
            PriorityQueue<KeyValuePair<AISituation, List<Transition>>>.Node node = pendingSituations.Dequeue();
            float priority = node.Priority;
            KeyValuePair<AISituation, List<Transition>> pair = node.Value;
            currentSituation = pair.Key;
            currentTransitions = pair.Value;

            if (isTargetSituation(currentSituation) && currentTransitions.Count > 0)
            {
                print("Plan priority: " + priority);
                return currentTransitions;
            }

            if (!observedSituations.ContainsKey(currentSituation))
            {
                observedSituations.Add(currentSituation, currentTransitions);

                foreach (PerformedAction action in actionEffects.Keys)
                {
                    //TODO hack to deal with faulty data
                    if (action.duration > 200)
                        continue;

                    List<SituationChange> situationChanges = actionEffects[action];
                    float totalCount = situationChanges.Count();

                    var changeDistribution = situationChanges.GroupBy(x => x);

                    foreach (var grp in changeDistribution)
                    {
                        float count = (float)grp.Count();

                        SituationChange change = grp.Key;
                        AISituation newSituation = SituationChange.ApplyChange(currentSituation, change);
                        Transition transition = new Transition(currentSituation, action, newSituation);
                        
                        List<Transition> latestTransitions = new List<Transition>();
                        latestTransitions.AddRange(currentTransitions);
                        latestTransitions.Add(transition);

                        //Priority here for taking a new action is 
                        //(1+success rate of action) * (1+similarity of other situation to current situation)
                        float pendingPriority = priority + (2 - (count / totalCount)) * (2 - AISituation.Similarity(currentSituation, change.prior));

                        pendingSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(transition.result, latestTransitions), 
                            pendingPriority);
                    }
                }
            }
        }
        print("Time to implement probability of the transitions");

        //Do a random move
        //If EVERYTHING else fails, do a random move
        Action randomAction = ValidMoveTable[originalSituation.status][Random.Range(0, ValidMoveTable[originalSituation.status].Count)];
        return new List<Transition>() { new Transition(originalSituation, new PerformedAction(randomAction, 1), originalSituation) };
    }

    public List<Transition> FindTarget(Dictionary<AISituation, List<Transition>> transitions, Dictionary<PerformedAction, List<SituationChange>> actionEffects)
    {
        //Stuff to handle timing out
        Stopwatch sw = Stopwatch.StartNew();
        float timeout = 500;

        AISituation originalSituation = new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1);
        AISituation currentSituation = originalSituation;

        PriorityQueue<KeyValuePair<AISituation, List<Transition>>> pendingKnownSituations = new PriorityQueue<KeyValuePair<AISituation, List<Transition>>>();
        PriorityQueue<KeyValuePair<AISituation, List<Transition>>> pendingUnknownSituations = new PriorityQueue<KeyValuePair<AISituation, List<Transition>>>();

        Dictionary<AISituation, List<Transition>> observedKnownSituations = new Dictionary<AISituation, List<Transition>>();
        Dictionary<AISituation, List<Transition>> observedUnknownSituations = new Dictionary<AISituation, List<Transition>>();

        List<Transition> currentTransitions = new List<Transition>();
        if(transitions.ContainsKey(currentSituation))
            pendingKnownSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(currentSituation, currentTransitions), currentTransitions.Count);
        else
            pendingUnknownSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(currentSituation, currentTransitions), currentTransitions.Count);

        while ((pendingKnownSituations.Count > 0 || pendingUnknownSituations.Count > 0) && sw.ElapsedMilliseconds < timeout)
        {
            float priority = 0;
            if (pendingKnownSituations.Count > 0)
            {
                PriorityQueue<KeyValuePair<AISituation, List<Transition>>>.Node node = pendingKnownSituations.Dequeue();
                priority = node.Priority;
                KeyValuePair<AISituation, List<Transition>> pair = node.Value;
                currentSituation = pair.Key;
                currentTransitions = pair.Value;
            }
            else
            {
                PriorityQueue<KeyValuePair<AISituation, List<Transition>>>.Node node = pendingUnknownSituations.Dequeue();
                priority = node.Priority;
                KeyValuePair<AISituation, List<Transition>> pair = node.Value;
                currentSituation = pair.Key;
                currentTransitions = pair.Value;
            }

            if (isTargetSituation(currentSituation) && currentTransitions.Count > 0)
            {
                print(sw.ElapsedMilliseconds);
                return currentTransitions;
            }

            if (transitions.ContainsKey(currentSituation))
            {
                if (!observedKnownSituations.ContainsKey(currentSituation))
                {
                    observedKnownSituations.Add(currentSituation, currentTransitions);

                    List<Transition> availableTransitions = transitions[currentSituation];
                    foreach (Transition transition in availableTransitions)
                    {
                        //TODO hack to deal with faulty data
                        if (transition.action.duration > 200)
                            continue;

                        List<Transition> latestTransitions = new List<Transition>();
                        latestTransitions.AddRange(currentTransitions);
                        latestTransitions.Add(transition);

                        if(transitions.ContainsKey(transition.result))
                            pendingKnownSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(transition.result, latestTransitions),   priority + 1.0f);
                        else
                            pendingUnknownSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(transition.result, latestTransitions), priority + 1.0f);
                    }
                }
            }
            else
            {
                //print("Looking at action effects");
                if(!observedUnknownSituations.ContainsKey(currentSituation))
                {
                    observedUnknownSituations.Add(currentSituation, currentTransitions);

                    foreach (PerformedAction action in actionEffects.Keys)
                    {
                        //TODO hack to deal with faulty data
                        if (action.duration > 200)
                            continue;

                        //Makes sure that the action is valid for our current state
                        if (!ValidMoveTable.ContainsKey(currentSituation.status))
                            print(currentSituation.status + "Debug this later!!");
                        else if (!ValidMoveTable[currentSituation.status].Contains(action.action))
                            continue;

                        List<SituationChange> situationChanges = actionEffects[action];
                        float totalCount = situationChanges.Count();

                        var changeDistribution = situationChanges.GroupBy(x => x);

                        foreach (var grp in changeDistribution)
                        {
                            float count = (float)grp.Count();

                            SituationChange change = grp.Key;
                            AISituation newSituation = SituationChange.ApplyChange(currentSituation, change);

                            Transition transition = new Transition(currentSituation, action, newSituation);

                            List<Transition> latestTransitions = new List<Transition>();
                            latestTransitions.AddRange(currentTransitions);
                            latestTransitions.Add(transition);

                            //Priority here for taking a new action is 
                            //(1+success rate of action) * (1+similarity of other situation to current situation)
                            //Focusing primarily on the state similarity seems to kind of sort of work.
                            float pendingPriority = priority + /*(2 - (count / totalCount)) **/ 10*(2 - AISituation.Similarity(currentSituation, change.prior));

                            //Right now we're not exploring the unknown transitions from this state, which could be bad?
                            //Hard to say if we'll get significantly more cases where we pick a random action
                            if (transitions.ContainsKey(transition.result))
                            {
                                pendingKnownSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(transition.result, latestTransitions), pendingPriority);
                                //Comment this out bc it leads to bad early terminations? break;
                            }
                            else
                                pendingUnknownSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(transition.result, latestTransitions), pendingPriority);
                        }
                    }
                }
            }
        }

        //If EVERYTHING else fails, do a random move
        Action randomAction = ValidMoveTable[originalSituation.status][Random.Range(0, ValidMoveTable[originalSituation.status].Count)];
        print("random move" + randomAction + originalSituation.status + "Search time " + sw.ElapsedMilliseconds);
        return new List<Transition>() { new Transition(originalSituation, new PerformedAction(randomAction, 1), originalSituation) };
    }

    public AISituation targetSituation;
    public bool isTargetSituation(AISituation situation)
    {
        if(searchSpecificTarget)
            return situation.Equals(targetSituation);    //Used to show the proof of concept. That is, the agent can navigate to the desired state
        else
            return situation.opponentStatus == PlayerStatus.Hit;    //We just want to get to a state where we hit the opponent
    }
}