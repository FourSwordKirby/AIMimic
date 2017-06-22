from keras.models import Sequential
from keras.layers import Dense, Activation
from PythonParse import ParsePlayerLogs


model = Sequential()

model.add(Dense(10))
model.add(Activation('relu'))
model.add(Dense(1))

model.compile(loss='categorical_crossentropy',
              optimizer='sgd',
              metrics=['accuracy'])

data = ParsePlayerLogs("Jotaro")
print(data)

# x_train and y_train are Numpy arrays --just like in the Scikit-Learn API.
# model.fit(x_train, y_train, epochs=5, batch_size=32)
# loss_and_metrics = model.evaluate(x_test, y_test, batch_size=128)
# classes = model.predict(x_test, batch_size=128)