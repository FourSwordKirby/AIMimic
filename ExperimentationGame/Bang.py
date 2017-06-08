import getpass
import os
import time
import random
import argparse
from datetime import datetime
from functools import reduce

from OptimalPlayer import OptimalPlayer
from BangEnv import BangEnv

#moves are
#1 = load bullet
#2 = fire bullet
#3 = block
#4 = use powerful bullet

def ParseBangLogs(log_name):
    data_file = open(log_name)
    contents = data_file.read()
    entries = contents.split("\n")
    entries = map(lambda x: eval(x.rstrip()), entries)
    return entries

def BangLogToNgram(entries, n = 3):
    playerNgram = dict()
    for round in entries:
        history = [None]*n
        for action in round:
            action = int(action[0][8])
            history.append(action)
            if(len(history) > n):
                history.pop(0)
            if(not str(history) in playerNgram):
                playerNgram[str(history)] = 0
            playerNgram[str(history)] += 1
    return playerNgram

def ListToNgram(round, n = 3):
    playerNgram = dict()

    history = [None]*n
    for action in round:
        history.append(action)
        if(len(history) > n):
            history.pop(0)
        if(not str(history) in playerNgram):
            playerNgram[str(history)] = 0
        playerNgram[str(history)] += 1
    return playerNgram

def MergeNgrams(*dicts):
    f = lambda x, y: x+y
    return reduce(lambda d1, d2: reduce(lambda d, t:
                                        dict(list(d.items()) +
                                             [(t[0], f(d[t[0]], t[1])
                                               if t[0] in d else
                                               t[1])]),
                                        d2.items(), d1),
                  dicts, {})

def ComparePlaystyles(p1_hist, p2_hist):
    similarity = 1
    difference = 0
    
    keys = set(p1_hist.keys()).intersection(set(p2_hist.keys()))
    if len(keys) == 0:
        return 0
    normalize = 1.0/len(keys)

    for key in keys:
        p1_count = p1_hist[key] if key in p1_hist.keys() else 0
        p2_count = p2_hist[key] if key in p2_hist.keys() else 0

        if(p1_count + p2_count == 0):
            continue

        difference += float(abs(p1_count - p2_count)) / float((p1_count + p2_count))

    similarity -= normalize * difference

    return similarity


#Ways to get actions
def get_optimal_action(policy, state):
    best_action = None
    best_value = -1000000

    for action in policy[state].keys():
        if policy[state][action] > best_value:
            best_value = policy[state][action]
            best_action = action
    return best_action

def get_stochastic_optimal_action(policy, state):
    softmax = sum(map(lambda x: 1.5**x, policy[state].values()))
    rng = random.uniform(0.0, softmax)
    threshold = 0.0
    for action in policy[state].keys():
        threshold += 1.5**(policy[state][action])
        if rng < threshold:
            return action

def get_ngram_action(env, state, prior_hist, ngram):
    action_count = dict()
    for action in env.p1_actions(state):
        added_hist = ngram[1:] + [action]
        if not str(added_hist) in prior_hist:
            action_count[action] = 0
        else:
            action_count[action] = prior_hist[str(added_hist)]

    softmax = sum(action_count.values())
    rng = random.uniform(0.0, softmax)
    threshold = 0.0

    for action in action_count:
        threshold += action_count[action]
        if rng < threshold:
            return action

#Reasoning for similarity vs normal ngram: similarity provides a sort of logical direction for the Ai to follow that is
#not present in normal rng from ngram solutions.
def get_similar_action(env, state, current_hist, prior_hist, ngram):
    for key in prior_hist.keys():
        current_hist[key] = current_hist[key] if key in current_hist.keys() else 0
        
    most_similar = 0.0
    chosen_action = 1

    for action in env.p1_actions(state):
        added_hist = ngram[1:] + [action]
        if not str(added_hist) in current_hist:
            current_hist[str(added_hist)] = 0

        current_hist[str(added_hist)] += 1
        similarity = ComparePlaystyles(current_hist, prior_hist)

        if(similarity > most_similar):
            most_similar = similarity
            chosen_action = action
        current_hist[str(added_hist)] -= 1

    return chosen_action

#This will be used to pick an action based on balancing between optimality and playing like the target player
#An idea we can implement is picking the optimal action when there is an optimal choice to take, and
#otherwise take the action with the highest similarity.
#We can also approach this by putting contraints on how optimal we should behave or how similar we should behave
def get_mixed_action():
    pass

#Meta stuff for the start and end of the game when in its playable version
def StartGame():
    print("Game Start")

def FinishGame():
    print("Game End")
    quit()


#Preloaded Data
prior_hist = BangLogToNgram(ParseBangLogs("p1_action.txt"))
#Making the prior arbitrarily large to accommodate for the current history data overflowing
for key in prior_hist:
    prior_hist[key] *=  200

#System parameters
n = 3
num_trials = 1000
player1 = OptimalPlayer(1)
player2 = OptimalPlayer(2)

#Game parameters
starting_life = 3
starting_bullets = 0

def main():
    parser = argparse.ArgumentParser(description='Run the game Bang in various modes')
    parser.add_argument('--playable', default="False", help='Determines whether the game is human-playable')
    parser.add_argument('--test', default="False", help='Describes if we are currently testing AI discernability')
    parser.add_argument('--AI', default="player", help='Describes the mode that the AI runs in')
    parser.add_argument('--profile', default='Roger', help='Directory to save data to')
    args = parser.parse_args()

    p1_wins = 0
    p2_wins = 0
    ties = 0
    current_hist = dict()
    true_current_hist = dict()

    env = BangEnv(starting_life, starting_bullets)

    #Used to randomly select between some sets of AI
    if(args.test == "True"): 
        rng = random.uniform(0.0, 0.9)
        print(rng)
        if(rng < 0.5): args.AI = "player"
        elif (rng < 0.6): args.AI = "ngram"
        elif (rng < 0.7): args.AI = "sim"
        elif (rng < 0.8): args.AI = "opt"
        elif (rng < 0.9): args.AI = "opt-rand"

    if(args.playable):
        StartGame()

        #Logging
        if not os.path.exists(args.profile):
            os.makedirs(args.profile)
        time_str = datetime.now().strftime('%Y%m%d%H%M%S')
        log_file_name = args.profile + "/" + time_str + "_" + args.AI + ".log"
        my_log = open(log_file_name, 'a', 1)

    if(args.AI == "opt" or args.AI == "opt-rand"):
        gamma = 0.9
        value_func = player1.value_iteration(env, gamma, max_iterations=int(1e4), tol=1e-5)
        policy = player1.value_function_to_policy(env, gamma, value_func)

    for i in range(num_trials):
        p1_actions = []
        p2_actions = []
        ngram = [None]*n
        round = 0

        state = env.reset()

        while True:
            # Code for when we want to have player input
            if(args.playable == "True"):
                print("p1: Health ", state[0], "| Bullets ", state[1])
                print("p2: Health ", state[2], "| Bullets ", state[3])
                my_log.write("p1: Health " + str(state[0]) + "| Bullets " + str(state[1]) + "\n")
                my_log.write("p2: Health " + str(state[2]) + "| Bullets " + str(state[3]) + "\n")

                os.system("stty -echo")
                p1_valid = False
                p2_valid = False

                while(not p1_valid):
                    p1_move = int(input("Enter p1 attack: "))
                    print("\n")
                    p1_valid = p1_move in env.valid_p1_actions(state)
                    if(not p1_valid):
                        print("Please choose an valid action")

                while(not p2_valid):
                    p2_move = int(input("Enter p2 attack: "))
                    print("\n")
                    p2_valid = p2_move in env.valid_p1_actions(env.mirror_state(state))
                    if(not p2_valid):
                        print("Please choose an valid action")
                
                os.system("stty echo")


                if(args.AI == "ngram"): p1_move = get_ngram_action(env, state, prior_hist, ngram)
                if(args.AI == "sim"): p1_move = get_similar_action(env, state, current_hist, prior_hist, ngram)
                if(args.AI == "opt"): p1_move = get_optimal_action(policy, state)
                if(args.AI == "opt-rand"): p1_move = get_stochastic_optimal_action(policy, state)
            else:
                if(args.AI == "ngram"): p1_move = get_ngram_action(env, state, prior_hist, ngram)
                if(args.AI == "sim"): p1_move = get_similar_action(env, state, current_hist, prior_hist, ngram)
                if(args.AI == "opt"): p1_move = get_optimal_action(policy, state)
                if(args.AI == "opt-rand"): p1_move = get_stochastic_optimal_action(policy, state)

                p2_move = random.choice(env.valid_p1_actions(env.mirror_state(state)))

            new_state, p1_reward, p2_reward, is_terminal = env.step(p1_move, p2_move)
            
            #results of a round when human playable
            if(args.playable == "True"):
                print("p1 action: ", p1_move)
                print("p2 action: ", p2_move)
                print("\n")

                my_log.write("p1 action: " + str(p1_move) + "\n")
                my_log.write("p2 action: " + str(p2_move) + "\n")
                my_log.write("~~~\n")

            p1_actions.append(p1_move)
            p2_actions.append(p2_move)
            
            #Upadte the ngram
            ngram.append(p1_move)
            if(len(ngram) > n):
                ngram.pop(0)

            if(not str(ngram) in current_hist):
                current_hist[str(ngram)] = 0
            current_hist[str(ngram)] += 1

            if(not str(ngram) in true_current_hist):
                true_current_hist[str(ngram)] = 0
            true_current_hist[str(ngram)] += 1

            if is_terminal:
                #Final display of victory
                if(args.playable == "True"):
                    print("p1: Health ", new_state[0], "| Bullets ", new_state[1])
                    print("p2: Health ", new_state[2], "| Bullets ", new_state[3])
        
                    if env.p1_life == 0 and env.p2_life == 0:
                        print("TIE GAME")
                        my_log.write("Result: TIE GAME")
                    elif env.p2_life == 0:
                        print("P1 VICTORY")
                        my_log.write("Result: P1 GAME")
                    else:
                        print("P2 VICTORY")
                        my_log.write("RESULT: P2 GAME")
                    
                    #User questions whether its the AI or a player
                    if(args.test == "True"):
                        valid_answer = False
                        while(not valid_answer):
                            is_AI = input("Were you playing against an AI (y/n)? ")
                            valid_answer = (is_AI == "y" or is_AI == "n")
                            if(not valid_answer):
                                print("Please choose an valid option")
                        my_log.write("Player Decision: " + str(is_AI))
                    FinishGame()

                #If not human playable, silently count wins and losses
                if env.p1_life == 0 and env.p2_life == 0:
                    ties += 1
                elif env.p2_life == 0:
                    p1_wins += 1
                else:
                    p2_wins += 1
                break
            else:
                state = new_state
                round += 1

            # result = process_action(p1_move, p2_move, p1_life, p1_bullets, p2_life, p2_bullets)
            # if result != None:
            #     p1_life, p1_bullets, p2_life, p2_bullets = result[0], result[1], result[2], result[3]
            # else:
            #     continue


            #Used for logging
            # recorded_p1_move = "action: " + str(p1_move) + " health: ", str(p1_life), " bullets: ", str(p1_bullets)
            # recorded_p2_move = "action: " + str(p2_move) + " health: ", str(p2_life), " bullets: ", str(p2_bullets)

            # p1_actions.append(recorded_p1_move)
            # p2_actions.append(recorded_p2_move)

    print("p1 wins: ", p1_wins, " | ", "p2 wins: ", p2_wins, " | ", "ties: ", ties)

    for key in true_current_hist:
        true_current_hist[key] /=  num_trials
    for key in prior_hist:
        prior_hist[key] /=  len(list(ParseBangLogs("p1_action.txt")))
        
    print("Similarity: ", ComparePlaystyles(true_current_hist, prior_hist))

    """    
    #Logging
    f = open('p1_action.txt', 'r')
    past_p1_actions = f.read()
    f.close()

    f = open('p2_action.txt', 'r')
    past_p2_actions = f.read()
    f.close()

    f = open('p1_action.txt', 'w')
    f.write(past_p1_actions + '\n' + str(p1_actions)) 
    f.close()

    f = open('p2_action.txt', 'w')
    f.write(past_p2_actions + '\n' + str(p2_actions)) 
    f.close()
    """

if __name__ == '__main__':
    main()