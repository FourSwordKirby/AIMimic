import random
from BangEnv import BangEnv

class OptimalPlayer(object):
    def __init__(self, player_num = 1):
        self.player_num = player_num
        # self.policy = dict() #maps a state to a policy taken by the agent
        # self.value_func = dict() #maps a state to it's value

    def value_iteration(self, env, gamma, max_iterations=int(1e3), tol=1e-3):
        """Runs value iteration for a given gamma and environment.

        See page 90 (pg 108 pdf) of the Sutton and Barto Second Edition
        book.

        http://webdocs.cs.ualberta.ca/~sutton/book/bookdraft2016sep.pdf

        Parameters
        ----------
        env: gym.core.Environment
        The environment to compute value iteration for. Must have nS,
        nA, and P as attributes.
        gamma: float
        Discount factor, must be in range [0, 1)
        max_iterations: int
        The maximum number of iterations to run before stopping.
        tol: float
        Determines when value function has converged.

        Returns
        -------
        np.ndarray, iteration
        The value function and the number of iterations it took to converge.
        """
        all_states = env.all_states()
        values = dict()
        for state in all_states:
            values[state] = 0.0


        iterations = 0
        converged = False
        while not converged:
            delta = 0.0
            iterations += 1
            new_values = dict()
            for state in all_states:
                best_value = None
                for action in env.p1_actions(state):
                    possible_next_states = env.p1_action_states(state, action)
                    
                    value = 0.0
                    for (prob, next_state, reward, terminal) in possible_next_states:
                        if terminal:
                            value += prob * reward
                        else:
                            value += prob * (reward + gamma * values[next_state])

                    if best_value is None or best_value < value:
                        best_value = value
                    
                delta = max(delta, abs(best_value - values[state]))
                new_values[state] = best_value

            values = new_values
            converged = delta < tol
            
            if iterations >= max_iterations:
                break

        return values#, iterations

    def value_function_to_policy(self, env, gamma, value_function):
        """Output action numbers for each state in value_function.

        Parameters
        ----------
        env: gym.core.Environment
        Environment to compute policy for. Must have nS, nA, and P as
        attributes.
        gamma: float
        Discount factor. Number in range [0, 1)
        value_function: np.ndarray
        Value of each state.

        Returns
        -------
        np.ndarray
        An array of integers. Each integer is the optimal action to take
        in that state according to the environment dynamics and the
        given value function.
        """
        all_states = env.all_states()
        policy = dict()
        for state in all_states:
            policy[state] = dict()
            for action in env.p1_actions(state):
                policy[state][action] = 0.0

        for state in all_states:
            max_value = None
            best_action = None
            for action in env.p1_actions(state):
                possible_next_states = env.p1_action_states(state, action)
                for (prob, next_state, reward, terminal) in possible_next_states:
                    policy[state][action] += prob * value_function[next_state]
        return policy

    # def update_policy(self, env):
    #     tol = 0.05
    #     gamma = 0.9
    #     converged = False
    #     for state in self.policy.keys():
    #         if state not in self.value_func.keys():
    #             self.value_func[state] = 0.0

    #     #Update the value function with what we've seen
    #     while not converged:
    #         largest_delta = 0
    #         new_values = dict()
    #         for state in self.value_func.keys():
    #             old_value = self.value_func[state]

    #             action_table = self.policy[state]

    #             new_value = 0.0
    #             for action in env.get_p1_actions(state):
    #                 prob = action_table[action]
    #                 next_state = None
                    
    #                 next_states = env.p1_action_states(state, action)
    #                 min_val = float("+inf")
    #                 for s in next_states:
    #                     if s in self.value_func:
    #                         if self.value_func[s] < min_val:
    #                             min_val = self.value_func[s]
    #                             next_state = s
    #                     else:
    #                         if env.p1_state_reward(s) < min_val:
    #                             min_val = env.p1_state_reward(s)
    #                             next_state = s
    #                 reward = env.p1_state_reward(next_state)

    #                 terminal = env.is_terminal_state(next_state)

    #                 if terminal:
    #                     new_value += prob * reward
    #                 else:
    #                     if next_state in self.value_func:
    #                         new_value += prob * (reward + gamma * self.value_func[next_state])
    #                     else:
    #                         new_value += prob * reward

    #             largest_delta = max(largest_delta, abs(new_value - old_value))
    #             new_values[state] = new_value

    #         self.value_func = new_values
    #         converged = largest_delta < tol
    #         #print(converged)
    #         #print(self.value_func)

    #     #Update the policy accordingly
    #     for state in self.value_func.keys():
    #         self.policy[state] = dict()

    #         for action in env.get_p1_actions(state):
    #             if self.player_num == 1:
    #                 next_states = env.p1_action_states(state, action)
    #                 min_val = float("+inf")
    #                 for s in next_states:
    #                     if s in self.value_func:
    #                         if self.value_func[s] < min_val:
    #                             min_val = self.value_func[s]
    #             if min_val == float("+inf"):
    #                 self.policy[state][action] = 0
    #             self.policy[state][action] = min_val
            
    #         action_table_min = min(action_table.values())
    #         if action_table_min < 0:
    #             for key in action_table:
    #                 action_table[key] -= action_table_min
    #         action_table_sum = sum(action_table.values())

    #         if action_table_sum <= 0 :
    #             for action in env.get_p1_actions(state):
    #                 self.policy[state][action] = 1/len(action_table)
    #         else:
    #             for action in action_table.keys():
    #                 self.policy[state][action] = action_table[action]/action_table_sum

    #     #for state in self.policy:
    #         #print(state, self.policy[state], self.value_func[state])

    # def get_p1_action(self, bang_env, state):
    #     if state not in self.policy.keys():
    #         available_actions = bang_env.get_p1_actions(state)
    #         self.policy[state] = dict()

    #         for i in bang_env.actions:
    #             if i in available_actions:
    #                 self.policy[state][i] = 1.0/len(available_actions)
    #             else:
    #                 self.policy[state][i] = 0.0
    #     rng = random.uniform(0.0, 1.0)

    #     currentThreshold = 0
    #     action_table = self.policy[state]

    #     for a in action_table.keys():
    #         v = action_table[a]
    #         currentThreshold += v
    #         if rng < currentThreshold:
    #             return a

    # def get_p2_action(self, bang_env, state):
    #     if state not in self.policy.keys():
    #         available_actions = bang_env.get_p2_actions(state)
    #         self.policy[state] = dict()

    #         for i in bang_env.actions:
    #             if i in available_actions:
    #                 self.policy[state][i] = 1.0/len(available_actions)
    #             else:
    #                 self.policy[state][i] = 0.0
    #     rng = random.uniform(0.0, 1.0)

    #     currentThreshold = 0
    #     action_table = self.policy[state]
    #     for a in action_table.keys():
    #         v = action_table[a]
    #         currentThreshold += v
    #         if rng < currentThreshold:
    #             return a

