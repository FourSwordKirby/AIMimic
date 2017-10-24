using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

public class TransitionSolver : MonoBehaviour
{
    public bool usingActionEffects;
    public bool searchSpecificTarget;
    public bool repeatedPlanning;

    public int perceptionDelay;
    public int perceptionLeeway;

    //Used to keep track of the last x frames, allows the planner to be more lenient w.r.t choosing when to abort and try a new action
    //TODO: May also want to look at creating a better metric for state similarity
    private List<AISituation> perceptionBuffer;

    public string playerName;
    public Player controlledPlayer;
    
    Dictionary<AISituation, List<Transition>> playerTransitions = new Dictionary<AISituation, List<Transition>>();
    Dictionary<PerformedAction, List<SituationChange>> actionEffects = new Dictionary<PerformedAction, List<SituationChange>>();

    List<Transition> desiredTransitions;

    public void Start()
    {
        LoadTransitions();
        perceptionBuffer = new List<AISituation>(perceptionLeeway);
    }

    public void LoadTransitions()
    {
        TransitionProfile profile = TransitionProfile.LoadTransitions(playerName);

        playerTransitions = profile.getPlayerTransitions();
        actionEffects = profile.getActionEffects();
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
    int attemptLimit = 150;
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
            WritePlan(desiredTransitions);
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
                    print("Completed" + currentAction.action);
                    startFrame = GameManager.instance.currentFrame;
                }

                if (GameManager.instance.currentFrame - attemptStartFrame > currentAction.duration + attemptLimit)
                {
                    print("attempt failed " + currentAction + ", replanned" + 
                            new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1));

                    desiredTransitions = usingActionEffects ? FindTarget(playerTransitions, actionEffects) : FindTarget(playerTransitions);
                    actionTracker = 0;
                    return;
                }
            }
            else if(GameManager.instance.currentFrame - startFrame >= currentAction.duration)
            {
                attemptStartFrame = GameManager.instance.currentFrame;
                actionCompleted = false;
                actionTracker++;

                if (actionTracker >= desiredTransitions.Count)
                {
                    print("Completed plan of length: " + desiredTransitions.Count);
                    return;
                }

                //If the transition was to an unexpected place, replan
                //TODO: make the replanning more lenient, so it doesn't need a 1 to 1 match with regards to it's planned situation
                //TODO: make it learn that it needs to crouch -> attack in order to do low attacks and Jump -> attack to do air attacks
                //When determining what action to do next, it should make sure that the action effect that it takes has minimal friction between the states?
                //Basically, have the priority system take into account the feasibility of the action when comparing 2 states
                //The key idea is to make it favor doing stand -> jump -> air attack over stand -> air attack

                //Adding a buffer window so that we say that our state must have been within the last x frames we've seen
                if (!perceptionBuffer.Contains(desiredSituation))
                {
                    //print("action" + currentAction + "replanned " + desiredSituation + " : " + new AISituation(GameRecorder.instance.LatestFrame(),controlledPlayer.isPlayer1));

                    //desiredTransitions = usingActionEffects ? FindTarget(playerTransitions, actionEffects) : FindTarget(playerTransitions);
                    //actionTracker = 0;
                    return;
                }
                else
                {
                    currentAction = desiredTransitions[actionTracker].action;
                    //print("Trying to do action" + attemptStartFrame + " " + desiredTransitions[actionTracker].action.action + " " +
                    //                                desiredTransitions[actionTracker].action.duration);
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

            if (isTargetSituation(currentSituation))
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
                            print("Invalid action detected on table" + currentSituation.status + transition.action.action);
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
        //int episodeSeed = 111;
        int episodeSeed = 5;

        //Stuff to handle timing out
        Stopwatch sw = Stopwatch.StartNew();
        float timeout = 5000;

        AISituation originalSituation = new AISituation(GameRecorder.instance.LatestFrame(), controlledPlayer.isPlayer1);
        AISituation currentSituation = originalSituation;

        PriorityQueue<KeyValuePair<AISituation, List<Transition>>> pendingKnownSituations = new PriorityQueue<KeyValuePair<AISituation, List<Transition>>>();
        PriorityQueue<KeyValuePair<AISituation, List<Transition>>> pendingUnknownSituations = new PriorityQueue<KeyValuePair<AISituation, List<Transition>>>();

        Dictionary<AISituation, List<Transition>> observedKnownSituations = new Dictionary<AISituation, List<Transition>>();
        Dictionary<AISituation, List<Transition>> observedUnknownSituations = new Dictionary<AISituation, List<Transition>>();

        List<Transition> currentTransitions = new List<Transition>();
        if(transitions.ContainsKey(currentSituation))
            pendingKnownSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(currentSituation, currentTransitions), 
                                            0 + heuristic(currentSituation, targetSituation));
        pendingUnknownSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(currentSituation, currentTransitions),
                                            0 + heuristic(currentSituation, targetSituation));

        //Mostly used for debugging which states have been explored
        List<KeyValuePair<AISituation, float>> exploredStates = new List<KeyValuePair<AISituation, float>>();

        while ((pendingKnownSituations.Count > 0 || pendingUnknownSituations.Count > 0))
        {
            print(sw.ElapsedMilliseconds);
            if (sw.ElapsedMilliseconds >= timeout)
            {
                print("Timed out");
                break;
            }


            //Used to differentiate between looking at the known transitions for this state or looking at the action effects on this state
            bool isKnown = pendingKnownSituations.Count > 0;

            PriorityQueue<KeyValuePair<AISituation, List<Transition>>>.Node node;
            if (pendingKnownSituations.Count > 0)
                node = pendingKnownSituations.Dequeue();
            else
                node = pendingUnknownSituations.Dequeue();

            float priority = node.Priority;
            KeyValuePair<AISituation, List<Transition>> pair = node.Value;
            currentSituation = pair.Key;
            currentTransitions = pair.Value;
            priority -= heuristic(currentSituation, targetSituation);


            //Debugging what was explored
            if(!isKnown)
                exploredStates.Add(new KeyValuePair<AISituation, float>(currentSituation, priority + heuristic(currentSituation, targetSituation)));

            //print("Looking at state: " + currentSituation + " with priority " + priority);
            if (isTargetSituation(currentSituation))    //Might want to re-add in the check that the current plan is non-empty
            {
                print("Time elapsed to plan: " + sw.ElapsedMilliseconds);
                print("Plan is: " + currentTransitions[0].action);

                WriteExploration(exploredStates);
                return currentTransitions;
            }

            if (isKnown)
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

                        if (!ValidMoveTable[currentSituation.status].Contains(transition.action.action))
                        {
                            print("Invalid action detected on table" + currentSituation.status + transition.action.action);
                            continue;
                        }

                        if (observedKnownSituations.ContainsKey(transition.result) && observedUnknownSituations.ContainsKey(transition.result))
                            continue;

                        List<Transition> latestTransitions = new List<Transition>();
                        latestTransitions.AddRange(currentTransitions);
                        latestTransitions.Add(transition);

                        if (transitions.ContainsKey(transition.result))
                            pendingKnownSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(transition.result, latestTransitions),   
                                                            priority + 1.0f + heuristic(transition.result, targetSituation));
                        //No matter what, this state shows up on our list of states that will be examined via action effects
                        pendingUnknownSituations.Enqueue(new KeyValuePair<AISituation, List<Transition>>(transition.result, latestTransitions),
                                                            priority + 1.0f + heuristic(transition.result, targetSituation));
                    }
                }
            }
            else
            {
                if(!observedUnknownSituations.ContainsKey(currentSituation))
                {
                    observedUnknownSituations.Add(currentSituation, currentTransitions);

                    foreach (PerformedAction action in actionEffects.Keys)
                    {
                        //TODO hack to deal with faulty data
                        if (action.duration > 200)
                            continue;

                        //Makes sure that the action is valid for our current state
                        if (!ValidMoveTable[currentSituation.status].Contains(action.action))
                        {
                            //print("Invalid action detected on table: " + currentSituation.status + action.action);
                            continue;
                        }

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
                    }
                }
            }
        }

        //If EVERYTHING else fails, do a random move
        Action randomAction = ValidMoveTable[originalSituation.status][Random.Range(0, ValidMoveTable[originalSituation.status].Count)];
        print("random move" + randomAction + originalSituation.status + "Search time " + sw.ElapsedMilliseconds);

        WriteExploration(exploredStates);
        return new List<Transition>() { new Transition(originalSituation, new PerformedAction(randomAction, 4), originalSituation) };
    }

    float epsilon = 10.0f;
    public float heuristic(AISituation s, AISituation goal = null)
    {
        if (goal != null)
            //return 0.0f;
            return epsilon * AISituation.Distance(s, goal);
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
    public bool isTargetSituation(AISituation situation)
    {
        if(searchSpecificTarget)
            return situation.Equals(targetSituation);    //Used to show the proof of concept. That is, the agent can navigate to the desired state
        else
            return situation.opponentStatus == PlayerStatus.Hit;    //We just want to get to a state where we hit the opponent
    }
}