using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

public class TransitionSolver : MonoBehaviour
{
    public bool usingActionEffects;
    public bool searchSpecificTarget;
    public bool randomPredecessor;
    public bool repeatedPlanning;
    public bool debugLogging;

    public int perceptionDelay;
    public int perceptionLeeway;
    public float timeout;

    //Used to keep track of the last x frames, allows the planner to be more lenient w.r.t choosing when to abort and try a new action
    //TODO: May also want to look at creating a better metric for state similarity
    private List<AISituation> perceptionBuffer;

    public string playerName;
    public Player controlledPlayer;

    TransitionProfile profile;
    Dictionary<AISituation, List<Transition>> playerTransitions = new Dictionary<AISituation, List<Transition>>();
    Dictionary<PerformedAction, List<SituationChange>> actionEffects = new Dictionary<PerformedAction, List<SituationChange>>();
    public List<AISituation> goalSituations = new List<AISituation>();

    public List<Transition> desiredTransitions;

    public void Awake()
    {
        print("Started awakening masters");
        LoadTransitions();
        perceptionBuffer = new List<AISituation>(perceptionLeeway);
        print("Finished awakening masters");
    }

    public void LoadTransitions()
    {
        profile = TransitionProfile.LoadTransitions(playerName);

        playerTransitions = profile.getPlayerTransitions();
        actionEffects = profile.getActionEffects();
        goalSituations = profile.getGoalSituations(isPrimaryGoal);
    }

    public Dictionary<PlayerStatus, List<Action>> ValidMoveTable = 
        new Dictionary<PlayerStatus, List<Action>>()
        { {PlayerStatus.Stand, new List<Action>() { Action.Stand, Action.Crouch, Action.WalkLeft, Action.WalkRight, Action.JumpLeft, Action.JumpRight, Action.JumpNeutral, Action.Attack, Action.Overhead,  Action.StandBlock, Action.DashLeft, Action.DashRight, Action.DP} },
            {PlayerStatus.Crouch, new List<Action>() {Action.Stand, Action.Crouch, Action.LowAttack, Action.JumpLeft, Action.JumpRight, Action.JumpNeutral, Action.CrouchBlock}  },
            {PlayerStatus.Air, new List<Action>() { Action.Stand, Action.Crouch, Action.AirAttack, Action.AirdashLeft, Action.AirdashRight, Action.AirdashLeft }  },
            {PlayerStatus.Highblock, new List<Action>() {Action.Stand, Action.StandBlock, Action.CrouchBlock, Action.JumpLeft, Action.JumpRight, Action.JumpNeutral }  },
            {PlayerStatus.Lowblock, new List<Action>() { Action.Crouch, Action.StandBlock, Action.CrouchBlock, Action.JumpLeft, Action.JumpRight, Action.JumpNeutral }  },
            {PlayerStatus.Hit, new List<Action>() {}  },
            {PlayerStatus.KnockdownHit, new List<Action>() { Action.TechLeft, Action.TechNeutral, Action.TechRight, Action.DP}  },
            {PlayerStatus.Tech, new List<Action>() {Action.Stand, Action.Crouch, Action.StandBlock, Action.CrouchBlock}  },
            {PlayerStatus.Moving, new List<Action>() { Action.Stand, Action.Crouch, Action.StandBlock, Action.JumpLeft, Action.JumpNeutral, Action.JumpRight, Action.DashLeft, Action.DashRight, Action.Attack, Action.Overhead, Action.DP}  },
            {PlayerStatus.Dashing, new List<Action>() { Action.Stand, Action.Crouch}  },
            {PlayerStatus.AirDashing,new List<Action>() {Action.AirAttack }  },
            {PlayerStatus.StandAttack, new List<Action>() {Action.Stand, Action.Crouch, Action.Attack }  }, 
            {PlayerStatus.LowAttack, new List<Action>() {Action.Stand, Action.Crouch, Action.LowAttack }  },
            {PlayerStatus.OverheadAttack,new List<Action>() {Action.Stand }  },
            {PlayerStatus.AirAttack, new List<Action>() {Action.Stand }  },
            {PlayerStatus.DP, new List<Action>() {Action.Stand }  },
            {PlayerStatus.Recovery, new List<Action>() {Action.Stand, Action.Crouch, Action.Attack, Action.LowAttack }  },
        };


    int actionTracker = 0;
    int startFrame = 0;
    int attemptStartFrame = 0;
    int attemptLimit = 5;
    bool actionCompleted = false;

    public void WritePlan(List<Transition> plan)
    {
        string directoryPath = Application.streamingAssetsPath;
        string filePath = directoryPath + "/InitialPlan" + ".txt";

        // serialize
        string datalog = "";
        for (int i = 0; i < plan.Count; i++)
        {
            datalog += plan[i].action + "\n";
        }
        File.WriteAllText(filePath, datalog);
    }

    public void WriteExploration(List<KeyValuePair<AISituation, float>> states)
    {
        string directoryPath = Application.streamingAssetsPath;
        string filePath = directoryPath + "/ExploredStates" + ".txt";

        // serialize
        string datalog = "";
        for (int i = 0; i < states.Count; i++)
        {
            datalog += states[i].Key + " " + states[i].Value + "\n";
        }
        File.WriteAllText(filePath, datalog);
    }

    public void Update()
    {
        perceptionBuffer.Add(new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1));
        if (perceptionBuffer.Count > perceptionLeeway)
            perceptionBuffer.RemoveAt(0);

        while (GameManager.instance.currentFrame < 50)
            return;

        controlledPlayer.AIControlled = true;
        if (desiredTransitions == null)
        {
            desiredTransitions = usingActionEffects ? FindTarget(playerTransitions, actionEffects) : FindTarget(playerTransitions);
        }

        if (actionTracker >= desiredTransitions.Count && repeatedPlanning)
        {
            print("creating new plan");
            desiredTransitions = usingActionEffects ? FindTarget(playerTransitions, actionEffects) : FindTarget(playerTransitions);
            actionTracker = 0;

            actionCompleted = false;
            startFrame = GameManager.instance.currentFrame;
            attemptStartFrame = GameManager.instance.currentFrame;
        }

        if (actionTracker < desiredTransitions.Count)
        {
            PerformedAction currentAction = desiredTransitions[actionTracker].action;
            AISituation desiredSituation = desiredTransitions[actionTracker].result;

            if (!actionCompleted)
            {
                actionCompleted = controlledPlayer.PerformAction(currentAction.action, true);
                if(actionCompleted)
                {
                    print(GameManager.instance.currentFrame + " Completed " + currentAction.action + new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1));
                    startFrame = GameManager.instance.currentFrame;
                }

                if (GameManager.instance.currentFrame - attemptStartFrame > currentAction.duration + attemptLimit)
                {
                    print(GameManager.instance.currentFrame + " attempt failed " + currentAction + ", replanned" + 
                            new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1));

                    desiredTransitions = usingActionEffects ? FindTarget(playerTransitions, actionEffects) : FindTarget(playerTransitions);
                    actionTracker = 0;

                    actionCompleted = false;
                    startFrame = GameManager.instance.currentFrame;
                    attemptStartFrame = GameManager.instance.currentFrame;
                    return;
                }
            }
            else if(GameManager.instance.currentFrame - startFrame >= currentAction.duration)
            {
                //If the transition was to an unexpected place, replan
                //TODO: make the replanning more lenient, so it doesn't need a 1 to 1 match with regards to it's planned situation
                //TODO: make it learn that it needs to crouch -> attack in order to do low attacks and Jump -> attack to do air attacks
                //When determining what action to do next, it should make sure that the action effect that it takes has minimal friction between the states?
                //Basically, have the priority system take into account the feasibility of the action when comparing 2 states
                //The key idea is to make it favor doing stand -> jump -> air attack over stand -> air attack

                //Adding a buffer window so that we say that our state must have been within the last x frames we've seen
                if (!perceptionBuffer.Contains(desiredSituation))
                {
                    //If we haven't seen the desired situation, provide some leeway to see if the desired situation ends up developing
                    if (GameManager.instance.currentFrame - startFrame < currentAction.duration + perceptionLeeway)
                        return;

                    //If after waiting we still have't seen the situation, then replan
                    print("action" + currentAction + "replanned " + desiredSituation + " : " + new AISituation(GameRecorder.instance.LatestFrame(),controlledPlayer.isPlayer1));

                    desiredTransitions = usingActionEffects ? FindTarget(playerTransitions, actionEffects) : FindTarget(playerTransitions);
                    actionTracker = 0;

                    actionCompleted = false;
                    startFrame = GameManager.instance.currentFrame;
                    attemptStartFrame = GameManager.instance.currentFrame;

                    return;
                }
                else
                {
                    attemptStartFrame = GameManager.instance.currentFrame;
                    actionCompleted = false;
                    actionTracker++;
                    if (actionTracker >= desiredTransitions.Count)
                    {
                        print("Completed plan of length: " + desiredTransitions.Count);
                        return;
                    }
                    currentAction = desiredTransitions[actionTracker].action;
                }
            }
        }
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

            if (isGoal(currentSituation))
            {
                return currentTransitions;
            }

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
                        {
                            if (!(currentSituation.grounded && (transition.action.action == Action.Stand || transition.action.action == Action.Crouch)))
                                continue;
                        }

                        if (observedSituations.ContainsKey(transition.result))
                            continue;

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

            if (isGoal(currentSituation) && currentTransitions.Count > 0)
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


    //TODO: Fix this issue
    //Right now, if we want the AI to get to the state where it has walked up to the opponent, we may not be able to do it
    //This is because we want the AI to perform the stand action in a state where it has never used this action before. 
    //Right now, the planner will only look at action effects if there are no more known states that it can expand. This is
    //Incorrect behavior, as it may run out of states at some point on the search tree which is completely unrelated to the state we want
    //it to take an action effect on. Ideally, at each step it should look at both normal transitions and the action effects, but 
    //this will lead to lots of slowdown. Maybe it'd be easier if we detect that we're close to the goal (heuristic would be needed).
    //Maybe we shouldn't care about status at all and assume that the actions will impose whatever status we want? This is 
    //A difficult thing that needs to be considered
    public List<Transition> FindTarget(Dictionary<AISituation, List<Transition>> transitions, Dictionary<PerformedAction, List<SituationChange>> actionEffects)
    {
        //Yay different seeds work
        int episodeSeed = 5;

        AISituation originalSituation = new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1);
        AISituation currentSituation = originalSituation;
        
        //Set the predecessor target if we're in that mode
        if (randomPredecessor)
        {
            targetSituation = PickTarget(goalSituations, currentSituation);//[Random.Range(0, predecessorSituations.Count)];
            //List<AISituation> predecessorSituations = profile.getPredecessorSituations(targetSituation);
            //targetSituation = predecessorSituations[Random.Range(0, predecessorSituations.Count)];
            //targetSituation = AdjustTarget(targetSituation, currentSituation);
        }

        if (debugLogging)
        {
            print("State is currently" + originalSituation);
            print("Target State: " + targetSituation);
        }

        //Stuff to handle timing out
        Stopwatch sw = Stopwatch.StartNew();

        //Setting ustff up for A* search
        PriorityQueue<AISituation> pendingKnownSituations = new PriorityQueue<AISituation>();
        PriorityQueue<AISituation> pendingUnknownSituations = new PriorityQueue<AISituation>();

        Dictionary<AISituation, AISituation> observedKnownSituations = new Dictionary<AISituation, AISituation>();
        Dictionary<AISituation, AISituation> observedUnknownSituations = new Dictionary<AISituation, AISituation>();

        if(transitions.ContainsKey(currentSituation))
            pendingKnownSituations.Enqueue(currentSituation, 
                                            0 + heuristic(currentSituation, targetSituation));
        pendingUnknownSituations.Enqueue(currentSituation,
                                            0 + heuristic(currentSituation, targetSituation));

        //Mostly used for debugging which states have been explored
        //And keeping track of the most viable states
        List<KeyValuePair<AISituation, float>> exploredStates = new List<KeyValuePair<AISituation, float>>();
        List<Transition> plan;

        while ((pendingKnownSituations.Count > 0 || pendingUnknownSituations.Count > 0))
        {
            print(sw.ElapsedMilliseconds/100);
            if (sw.ElapsedMilliseconds >= timeout)
            {
                break;
            }

            //Dequeuing the lowest priority state
            PriorityQueue<AISituation>.Node node;
            if (pendingKnownSituations.Count > 0)
                node = pendingKnownSituations.Dequeue();
            else
                node = pendingUnknownSituations.Dequeue();

            float priority = node.Priority;
            currentSituation = node.Value;
            priority -= heuristic(currentSituation, targetSituation);


            //Debugging what was explored
            if(debugLogging)
                exploredStates.Add(new KeyValuePair<AISituation, float>(currentSituation, priority + heuristic(currentSituation, targetSituation)));

            //print("Looking at state: " + currentSituation + " with priority " + priority);
            if (isGoal(currentSituation))    //Might want to re-add in the check that the current plan is non-empty
            {
                // Backtrack        
                plan = new List<Transition>();
                while (currentSituation.parentAction != null)
                {
                    plan.Add(new Transition(currentSituation.parentSituation, currentSituation.parentAction, currentSituation));
                    currentSituation = currentSituation.parentSituation;
                }

                plan.Reverse();
                if(debugLogging)
                {
                    print("Time elapsed to plan: " + sw.ElapsedMilliseconds);
                    WriteExploration(exploredStates);
                    WritePlan(plan);
                }

                return plan;
            }

            //Used to differentiate between looking at the known transitions for this state or looking at the action effects on this state
            bool isKnown = pendingKnownSituations.Count > 0;

            if (isKnown)
            {
                if (observedKnownSituations.ContainsKey(currentSituation))
                    continue;

                observedKnownSituations.Add(currentSituation, currentSituation);

                List<Transition> availableTransitions = transitions[currentSituation];
                foreach (Transition transition in availableTransitions)
                {
                    //TODO hack to deal with faulty data
                    if (transition.action.duration > 200)
                        continue;

                    if (!ValidMoveTable[currentSituation.status].Contains(transition.action.action))
                        continue;
                    if (!currentSituation.grounded && !(transition.action.action == Action.AirdashLeft 
                        || transition.action.action == Action.AirdashRight || transition.action.action == Action.AirAttack))
                        continue;

                    if (observedKnownSituations.ContainsKey(transition.result) && observedUnknownSituations.ContainsKey(transition.result))
                        continue;

                    AISituation newState = transition.result.Copy();
                    newState.parentAction = transition.action;
                    newState.parentSituation = currentSituation;

                    if (transitions.ContainsKey(newState))
                        pendingKnownSituations.Enqueue(newState,   
                                                        priority + 1.0f + heuristic(newState, targetSituation));
                    //No matter what, this state shows up on our list of states that will be examined via action effects
                    pendingUnknownSituations.Enqueue(newState,
                                                        priority + 1.0f + heuristic(newState, targetSituation));
                }
            }
            else
            {
                if(!observedUnknownSituations.ContainsKey(currentSituation))
                {
                    observedUnknownSituations.Add(currentSituation, currentSituation);
                    //Debug
                    //print("Looking at situation " + currentSituation);
                    //print("target situation is " + targetSituation);

                    foreach (PerformedAction action in actionEffects.Keys)
                    {
                        //TODO hack to deal with faulty data
                        if (action.duration > 200)
                            continue;

                        //Makes sure that the action is valid for our current state
                        if (!ValidMoveTable[currentSituation.status].Contains(action.action))
                            continue;
                        if (!currentSituation.grounded && !(action.action == Action.AirdashLeft
                            || action.action == Action.AirdashRight || action.action == Action.AirAttack))
                            continue;


                        //**********************Testing using a real predictor vs our primitive table maneuvering.
                        //TODO: Speed up the calculations
                        KeyValuePair<AISituation, float> predictionPair = profile.predictResult(currentSituation, action, actionEffects);
                        AISituation newSituation = predictionPair.Key;
                        float confidence = predictionPair.Value;

                        //Debug
                        //print("Action is " + action + " From " + currentSituation + " Results in " + newSituation);

                        AISituation newState = newSituation.Copy();
                        newState.parentAction = action;
                        newState.parentSituation = currentSituation;

                        //Priority here for taking a new action is 
                        //(1+success rate of action) * (1+similarity of other situation to current situation)
                        //Focusing primarily on the state similarity seems to kind of sort of work.

                        float lambda = confidence;// + GetPriority(currentSituation, change.prior, episodeSeed);
                        float pendingPriority = priority + 1.0f + confidence;

                        //Right now we're not exploring the unknown transitions from this state, which could be bad?
                        //Hard to say if we'll get significantly more cases where we pick a random action
                        if (transitions.ContainsKey(newState))
                            pendingKnownSituations.Enqueue(newState,
                                                            pendingPriority + heuristic(newState, targetSituation));
                        //No matter what, this state shows up on our list of states that will be examined via action effects
                        pendingUnknownSituations.Enqueue(newState,
                                                            pendingPriority + heuristic(newState, targetSituation));

                        //**********************Old way of using action effects by iterating over a table*************************/
                        /*
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
                            float lambda = GetPriority(currentSituation, change.prior, episodeSeed);
                            float pendingPriority = priority + 1.0f + lambda/AISituation.Similarity(currentSituation, change.prior);

                            //Right now we're not exploring the unknown transitions from this state, which could be bad?
                            //Hard to say if we'll get significantly more cases where we pick a random action
                            if (transitions.ContainsKey(transition.result))
                            {
                                pendingKnownSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(transition.result, latestTransitions),
                                                                pendingPriority + heuristic(transition.result, targetSituation));
                                //Comment this out bc it leads to bad early terminations? break;
                            }
                            //No matter what, this state shows up on our list of states that will be examined via action effects
                            pendingUnknownSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(transition.result, latestTransitions),
                                                                pendingPriority + heuristic(transition.result, targetSituation));
                        }
                        //***********************************************/
                    }
                }
            }
        }

        //If we timed out, return the best plan we have so far.
        if(debugLogging)
            WriteExploration(exploredStates);

        exploredStates.Sort((x, y) => (int)(x.Value - y.Value));
        AISituation bestSituation = exploredStates[0].Key;

        // Backtrack from the best Situation
        plan = new List<Transition>();
        currentSituation = bestSituation;
        while (currentSituation.parentAction != null)
        {
            plan.Add(new Transition(currentSituation.parentSituation, currentSituation.parentAction, currentSituation));
            currentSituation = currentSituation.parentSituation;
        }

        plan.Reverse();

        if (debugLogging)
        {
            print("Following best available situation");
            WritePlan(plan);
        }

        return plan;

        /*
        Action randomAction = ValidMoveTable[originalSituation.status][Random.Range(0, ValidMoveTable[originalSituation.status].Count)];
        print("random move" + randomAction + originalSituation.status + "Search time " + sw.ElapsedMilliseconds);

        return new List<Transition>() { new Transition(originalSituation, new PerformedAction(randomAction, 4), originalSituation) };
        */
    }

    //TODO: Need to make picking a target balance between choosing a situation that is easier to achieve and one that is more applicable
    //to this situation. A real life example is going for the atomic collider -> rapid cancel combo to side switch out of the corner
    //and get the big corner damage versus not doing that combo when it would put us in the corner and not lead to the same massive damage.
    private AISituation PickTarget(List<AISituation> candidateSituations, AISituation currentSituation)
    {
        float minDist = float.MaxValue;
        AISituation bestSituation = candidateSituations[0];
        //foreach(AISituation sit in candidateSituations)
        //{
        //    float dist = AISituation.Similarity(sit, currentSituation);
        //    if (dist < minDist)
        //    {
        //        minDist = dist;
        //        bestSituation = sit;
        //    }
        //}
        
        ////Random selection code
        bestSituation = candidateSituations[Random.Range(0, candidateSituations.Count)];
        return bestSituation;

        //Variation code to use after debugging is done
        /*
        float distTotal = candidateSituations.Select(x => AISituation.Similarity(x, currentSituation)).Sum();
        float rng = Random.Range(0, distTotal);
        float runningSum = 0.0f;
        foreach (AISituation sit in candidateSituations)
        {
            float dist = AISituation.Similarity(sit, currentSituation);
            print(dist);
            print(distTotal);
            runningSum += dist;
            if (rng < runningSum)
                return sit;
        }
        print("shoudn't be here");
        return candidateSituations[0];
        */
    }

    //We need to do something like this to better direct the search towards one of many viable goals.
    //Main issue lies in the fact that the player and the target move alot and the situation we want could occur anywhere on the battle field. There are 
    //countless variations with the one constant between all of them being that the opponent got hit.

    //Potential Solution: Primary and Derivative state variables
    //An example of a primary variable is the x and y position of the opponent, a secondary variable can be the x distance between the players
    //Action effects work solely on the primary variables, whereas the state comparison could work on a combination of the 2.
    private AISituation AdjustTarget(AISituation targetSituation, AISituation currentSituation)
    {
        return targetSituation;
    }

    float epsilon = 10.0f;
    public float heuristic(AISituation s, AISituation goal = null)
    {
        if (goal != null)
        {
            //Obsolete
            //return epsilon * AISituation.Distance(s, goal);
            int totalDistance = 0;

            int xDist = s.xPos - s.opponentXPos;
            int yDist = s.yPos - s.opponentYPos;

            int xDistGoal = goal.xPos - goal.opponentXPos;
            int yDistGoal = goal.yPos - goal.opponentYPos;

            totalDistance += Mathf.Abs(xDist - xDistGoal);
            totalDistance += Mathf.Abs(yDist - yDistGoal);
            totalDistance += s.grounded == goal.grounded ? 0 : 1;
            totalDistance += s.opponentGrounded == goal.opponentGrounded ? 0 : 1;
            totalDistance += s.status == goal.status ? 0 : 1;
            totalDistance += s.opponentStatus == goal.opponentStatus ? 0 : 1;
            return totalDistance * epsilon;
        }
        return 0.0f;
    }

    public float GetPriority(AISituation s1, AISituation s2, int randomSeed = 1)
    {
        float u1 = ((s1.GetHashCode() * randomSeed)% 1000) / 1000.0f;
        float u2 = ((s2.GetHashCode() * randomSeed)% 1000) / 1000.0f;

        //print(s1.GetHashCode());
        //print(s2.GetHashCode());
        //Generates a std normal distribution from the 2 above uniform distributions
        float BoxMullerNormal = Mathf.Sqrt(-2 * Mathf.Log(u1)) * Mathf.Cos(2 * Mathf.PI * u2);
        return Mathf.Abs(BoxMullerNormal)+1.0f;
    }

    public AISituation targetSituation;
    public bool isGoal(AISituation situation)
    {
        return heuristic(situation, targetSituation) == 0;
        //return isPrimaryGoal(situation);
    }

    public bool isPrimaryGoal(AISituation situation)
    {
        if (searchSpecificTarget)
            return situation.Equals(targetSituation);    //Used to show the proof of concept. That is, the agent can navigate to the desired state
        else
            return situation.opponentStatus == PlayerStatus.FreshHit;    //We just want to get to a state where we hit the opponent
    }
}