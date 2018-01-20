from PythonParse import ParsePlayerLogs

from keras.models import Sequential
from keras.layers import Dense, Activation

import numpy as np
import pandas as pd

import os
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3'

data = ParsePlayerLogs("Jotaro")
snapshots = [snapshot for game in data for snapshot in game]

def process_elem(e):
    if e == False:
        return 0.0
    elif e == True:
        return 1.0
    elif str(e).isdigit() or isinstance(e, float):
        return float(e)
    else:
        return None

def flatten_dict(d):
    values = [(k,d[k]) for k in sorted(d.keys())]
    result = dict()
    for (key, elem) in values:
        if isinstance(elem, dict):
            elem = flatten_dict(elem)
            for sub_key in sorted(elem.keys()):
                result[key + "_" + sub_key] = elem[sub_key]
        else:
            new_elem = process_elem(elem)
            if new_elem != None:
                result[key] = new_elem
    return result

snapshots = pd.DataFrame([flatten_dict(snapshot) for snapshot in snapshots])

p1_snapshots = snapshots.loc[snapshots['initiatedPlayer'] == 0]
#p1_snapshots = p1_snapshots.loc[snapshots['p1Interrupt'] == 0.0]
#p1_snapshots = p1_snapshots.loc[snapshots['p2Interrupt'] == 0.0]

p2_snapshots = snapshots.loc[snapshots['initiatedPlayer'] == 1]
#p2_snapshots = p1_snapshots.loc[snapshots['p1Interrupt'] == 0.0]
#p2_snapshots = p1_snapshots.loc[snapshots['p2Interrupt'] == 0.0]

#Compiling the train and test data
from collections import deque

n = 1

# p1_x = p1_snapshots[[col for col in list(p1_snapshots) if col != 'p1Action']]
# p1_y  = p1_snapshots['p1Action']
p2_init_x = p2_snapshots[[col for col in list(p2_snapshots) if col != 'p2Action']]
p2_y = p2_snapshots['p2Action']

p2_x = []
history = deque([np.zeros(21) for i in range(n)])
prev_frame = -1
for index, x in p2_init_x.iterrows():
    frame = x['frameTaken']
    if(frame < prev_frame):
        history = deque([np.zeros(21) for i in range(n)])

    history.append(x)
    history.popleft()
    entry = np.array(list(history))
    p2_x.append(entry.astype('float64').flatten())

p2_x = np.array(p2_x)

new_p2_y = list(p2_y)
p2_y = []
for idx in new_p2_y:
    encoding = [0] * 12
    encoding[int(idx)] = 1.0
    p2_y.append(encoding)
p2_y = np.array(p2_y)

msk = np.array([True] * (len(p2_snapshots)-200) + [False] * 200) #np.random.rand(len(p2_snapshots)) < 0.8

p2_train_x = p2_x[msk]#.as_matrix()
p2_test_x = p2_x[~msk]#.as_matrix()
p2_train_y = p2_y[msk]#.as_matrix()
p2_test_y = p2_y[~msk]#.as_matrix()

print(len(p2_train_x))
print(len(p2_test_x))

#Running the neural net
import tensorflow as tf
def mean_huber_loss(y_true, y_pred, max_grad=1.):
    squared = 0.5 * abs(y_true - y_pred)**2
    linear = max_grad * abs(y_true - y_pred) - 0.5 * max_grad**2

    return tf.reduce_mean(tf.where(squared <= max_grad, squared, linear))

model = Sequential()
model.add(Dense(32, input_dim=(n * 21)))
model.add(Activation('sigmoid'))
# model.add(Dense(16))
# model.add(Activation('sigmoid'))
model.add(Dense(12))
model.add(Activation('sigmoid'))

model.compile(loss='mean_squared_error',
              optimizer='sgd',
              metrics=['accuracy'])

model.fit(p2_train_x, p2_train_y, nb_epoch=50, batch_size=32)
model.train_on_batch(p2_train_x, p2_train_y)

loss_and_metrics = model.evaluate(p2_test_x, p2_test_y, batch_size=128)
print("")
print("test_stats", loss_and_metrics)
loss_and_metrics = model.evaluate(p2_train_x, p2_train_y, batch_size=128)
print("train_stats", loss_and_metrics)

#Manual evaluation of accuracy
x_tests = list(p2_test_x)
y_tests = list(p2_test_y)

successes = 0
total_trials = 0

import random

for i in range(len(x_tests)):
    snapshot = np.array([x_tests[i]])
    target_action = np.where(y_tests[i]==1)[0][0]

    actions = model.predict(snapshot).squeeze()

    max_score = -10000
    selected_action = 0

    for action in range(len(actions)):
        if(actions[action] > max_score):
            max_score = actions[action]
            selected_action = action

    #print("actions", actions)
    #print(snapshot)

    # softmax = sum(map(lambda x: 40**x, actions))
    # rng = random.uniform(0.0, softmax)
    # threshold = 0.0
    # selected_action = 0

    # for idx in range(len(actions)):
    #     action = actions[idx]
    #     threshold += 40 ** action
    #     if(rng < threshold):
    #         selected_action = idx
    #         break


    if(selected_action == target_action):
        successes += 1

    total_trials += 1

print(successes/total_trials)
print("Completed")
# classes = model.predict(x_test, batch_size=128)