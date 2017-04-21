import getpass
import time
import random
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

def LogToNgram(entries, n = 3):
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

def comparePlaystyles(p1_hist, p2_hist):
    similarity = 1
    difference = 0
    
    keys = set(p1_hist.keys()).intersection(set(p2_hist.keys()))
    if len(keys) == 0:
        return 0
    normalize = 1.0/len(keys)
    #print("Nomal", normalize)

    for key in keys:
        p1_count = p1_hist[key] if key in p1_hist.keys() else 0
        p2_count = p2_hist[key] if key in p2_hist.keys() else 0

        if(p1_count + p2_count == 0):
            continue

        difference += float(abs(p1_count - p2_count)) / float((p1_count + p2_count))

    similarity -= normalize * difference

    return similarity


def get_p2_action(p1_life, p1_bullets, p2_life, p2_bullets):
    actions = []
    if(p2_bullets >= 0):
        actions.append(1)
    if(p2_bullets > 0):
        actions.append(2)
    if(p1_bullets > 0):
        actions.append(3)
    if(p2_bullets >= 3):
        actions.append(4)
    return random.choice(actions)

current_history = []

player_ngram = LogToNgram(ParseBangLogs("p1_action.txt"))

def get_similar_p2_action(p1_life, p1_bullets, p2_life, p2_bullets, p2_log, p2_actions):
    current_ngram = LogToNgram(p2_log)
    for key in player_ngram.keys():
        current_ngram[key] = current_ngram[key] if key in current_ngram.keys() else 0

    most_similar = 0.0
    chosen_action = 1

    for action in range(4):
        added_ngram = p2_actions[1:] + [action]
        if not str(added_ngram) in current_ngram:
            current_ngram[str(added_ngram)] = 0

        current_ngram[str(added_ngram)] += 1
        similarity = comparePlaystyles(player_ngram, current_ngram)
        print(action)
        print(similarity)

        if(similarity > most_similar):
            most_similar = similarity
            chosen_action = action
        current_ngram[str(added_ngram)] -= 1

    return chosen_action
    #comparePlaystyles(current_history, p1_hist)



player1 = OptimalPlayer(1)
player2 = OptimalPlayer(2)


def main():
    starting_life = 3
    starting_bullets = 0
    num_trials = 100
    p1_wins = 0
    p2_wins = 0

    env = BangEnv(3, 0)

    gamma = 0.9
    value_func = player1.value_iteration(env, gamma, max_iterations=int(1e4), tol=1e-5)
    policy = player1.value_function_to_policy(env, gamma, value_func)

    for i in range(num_trials):
        p1_actions = []
        p2_actions = []
        round = 0
        state = env.reset()

        #specific stuff used for similarity
        p2_log = [[]]
        p2_recent_actions = [None]*3

        while True:
            #print("===========[ROUND " + str(round) + " OF BANG]===========")
            # print("P1 health: ", p1_life, " bullets ", p1_bullets)
            # print("P2 health: ", p2_life, " bullets ", p2_bullets)

            #print(policy[state])
            softmax = sum(map(lambda x: 1.5**x, policy[state].values()))
            rng = random.uniform(0.0, softmax)
            threshold = 0.0
            for action in policy[state].keys():
                threshold += 1.5**(policy[state][action])
                if rng < threshold:
                    p1_move = action
                    break
            p2_move = random.choice(env.p1_actions(env.mirror_state(state)))
            # p1_move = input("Enter p1 attack: ") #getpass.getpass("Enter p1 attack: ")
            # if(not p1_move.isdigit()):
            #     print ("enter a valid action")
            #     continue
            # p1_move = int(p1_move)
            # if(p1_move != 1 and p1_move != 2 and p1_move != 3 and p1_move != 4):
            #     print ("enter a valid action")
            #     continue

            # p2_move = getpass.getpass("Enter p2 attack: ")
            # if(not p2_move.isdigit()):
            #     print ("enter a valid action")
            #     continue
            # p2_move = int(p2_move)
            # if(p2_move != 1 and p2_move != 2 and p2_move != 3 and p2_move != 4):
            #     print ("enter a valid action")
            #     continue

            # p2_move = get_similar_p2_action(p1_life, p1_bullets, p2_life, p2_bullets, p2_log, p2_recent_actions)
            # p2_move = player2.get_action((p1_life, p1_bullets, p2_life, p2_bullets))

            # p2_recent_actions.pop(0)
            # p2_recent_actions.append(p2_move)

            print("state", state)
            print("policy", policy[state])
            print("p1_move", p1_move)
            print("p2_move", p2_move)            
            new_state, p1_reward, p2_reward, is_terminal = env.step(p1_move, p2_move)

            #train players on the reward
            #print(env.p1_life, env.p1_bullets)
            #print(env.p2_life, env.p2_bullets)
            
            if is_terminal:
                #p1_move = player1.get_p1_action(env, env.current_state())
                #p2_move = player2.get_p2_action(env, env.current_state())
                #player1.update_policy(env)
                if env.p2_life == 0:
                    p1_wins += 1
                else:
                    p2_wins += 1
                print(p1_wins, p2_wins)
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
    print(p1_wins)
    print(p2_wins)
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

if __name__ == '__main__':
    main()