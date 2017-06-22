from keras.models import Sequential
from keras.layers import Dense, Activation

def main():
    print("========")
    print("Neural net here for all of your neural net needs")
    print("Top level yomi predictor which will know exactly what you want to do in this situation >:P")
    print("========")

    model = Sequential()
    model.add(Dense(units=64, input_dim=100))
    model.add(Activation('relu'))
    model.add(Dense(units=10))
    model.add(Activation('softmax'))

    model.compile(loss='categorical_crossentropy',
              optimizer='sgd',
              metrics=['accuracy'])

    # x_train and y_train are Numpy arrays --just like in the Scikit-Learn API.
    #model.fit(x_train, y_train, epochs=5, batch_size=32)

    #loss_and_metrics = model.evaluate(x_test, y_test, batch_size=128)
    #classes = model.predict(x_test, batch_size=128)

if __name__ == '__main__':
    main()