using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NeuralNetwork
{
    public class Graph
    {
        /// <summary>
        /// A propery that contains all the nodes and the arcs of the graph divided by layers.
        /// </summary>
        public List<Layer> layers { get; set; }

        private const double defaultValueOfBias = 0.1;

        private static Random random = new Random();

        /// <summary>
        /// Constructor: The function builds a new empty Graph object.
        /// </summary>
        public Graph()
        {
            layers = new List<Layer>();
        }

        /// <summary>
        /// Constructor: The function builds a new Graph object with given layers of nodes.
        /// </summary>
        /// <param name="layers"></param>
        public Graph(List<Layer> layers)
        {
            this.layers = layers;
        }

        /// <summary>
        /// The function returns the value of the bias of the previous layer. 
        /// If the current layer index isn't valid the function will return -1.
        /// </summary>
        /// <param name="currentLayerIndex">The index of the current layer of the graph we are working on</param>
        /// <returns>the value of the bias of the previous layer.</returns>
        private double GetPreviousLayerBias(int currentLayerIndex)
        {
            if (currentLayerIndex >= 1)
            {
                return layers[currentLayerIndex - 1].GetBias();
            }
            return -1;
        }

        /// <summary>
        /// The function Sets the value of the bias in the previous layer to the given bias value.
        /// </summary>
        /// <param name="currentLayerIndex">The index of the current layer of the graph we are working on</param>
        /// <param name="value">A bias value</param>
        private void SetPreviousLayerBias(int currentLayerIndex, double value)
        {
            if (currentLayerIndex >= 1)
            {
                layers[currentLayerIndex - 1].SetBias(value);
            }
        }

        /// <summary>
        /// The function adds a value to the value of the bias from the layer before the layer we got its index.
        /// </summary>
        /// <param name="currentLayerIndex">The index of the layer we are working on.</param>
        /// <param name="value">The value we need to add to the bias from the previous layer.</param>
        private void AddToPreviousLayerBias(int currentLayerIndex, double value)
        {
            if (currentLayerIndex >= 1)
            {
                layers[currentLayerIndex - 1].SetBias(value + this.GetPreviousLayerBias(currentLayerIndex));
            }
        }


        /// <summary>
        /// The function creates the layers and the nodes of a graph according to the given list.
        /// </summary>
        /// <param name="neuronsInLayers">A list of numbers that contains how many layers should be in our new 
        /// graph and how many nodes should be in each layer</param>
        private void CreateNodes(List<int> neuronsInLayers)
        {
            foreach (int neuronsInLayer in neuronsInLayers)
            {
                Layer layer = new Layer();
                for (int index = 0; index < neuronsInLayer; index++)
                {
                    layer.AddNode(new Node(0));  // the value of the node doesn't matter here - we'll initialize it later
                }
                layer.SetBias(0);  // We'll set a bias for every layer in the neural network - the value of the node doesn't matter here
                this.layers.Add(layer);
            }
            // We added a bias to every layer but we don't need a bias in the output layer so we'll delete it
            this.layers[neuronsInLayers.Count - 1].RemoveBias();
        }

        /// <summary>
        /// The function connects each neuron in every layer to each neuron in the adjacent layers.
        /// </summary>
        private void CreateArcs()
        {
            // We start at 1 because we are working on two adjacent layers at a time
            for (int layerIndex = 1; layerIndex < this.layers.Count; layerIndex++)
            {
                Layer currentLayer = this.layers[layerIndex];

                for (int currentLayerNeuronIndex = 0; currentLayerNeuronIndex < currentLayer.nodesInLayer.Count; currentLayerNeuronIndex++)
                {
                    Node currentLayerNeuron = currentLayer.nodesInLayer[currentLayerNeuronIndex];

                    Layer previousLayer = this.layers[layerIndex - 1];
                    for (int previousLayerNeuronIndex = 0; previousLayerNeuronIndex < previousLayer.nodesInLayer.Count; previousLayerNeuronIndex++)
                    {
                        Node previousLayerNeuron = previousLayer.nodesInLayer[previousLayerNeuronIndex];
                        Arc arc = previousLayerNeuron.AddOutgoingArc(currentLayerNeuron, 0);
                        currentLayerNeuron.AddIngoingArc(arc);
                    }
                }
            }
        }

        /// <summary>
        /// The function creates the desired graph and connects each layer to each adjacent layer in the graph.
        /// The function also adds bias units in each layer except the output layer.
        /// </summary>
        /// <param name="neuronsInLayers">A list of numbers that contains how many layers should be in our new graph 
        /// and how many nodes should be in each layer</param>
        public void CreateNeuralNetwork(List<int> neuronsInLayers)
        {
            CreateNodes(neuronsInLayers);
            CreateArcs();
        }

        /// <summary>
        /// The function returns a number that initalizes the arc's weight according to the He Uniform initialization.
        /// </summary>
        /// <param name="incomingArcs">The number of ingoing arcs to a specific node</param>
        /// <returns>A number that initalizes the arc's weight according to the He Uniform initialization</returns>
        private static double WeightInit(int incomingArcs)
        {
            double maxLimit = Math.Sqrt(6.0 / incomingArcs);
            double minLimit = -maxLimit;
            return (random.NextDouble() * (maxLimit - minLimit)) + minLimit;  // generates a number between the min limit and the max limit
        }

        /// <summary>
        /// The function initializes every arc in the graph that we activated the function on according to the 
        /// He Uniform Initialization.
        /// </summary>
        public void InitNeuralNetworkWeightsAndBiases()
        {
            bool firstLayer = true;
            foreach (Layer layer in this.layers)
            {
                if(layer.hasBias)
                {
                    layer.SetBias(defaultValueOfBias);
                }
                if (firstLayer)
                {
                    // We start initiating the arc weights from the second layer - the first layer neurons don't have incoming arcs
                    firstLayer = false;
                    continue;
                }
                foreach (Node neuron in layer.nodesInLayer)
                {
                    foreach (Arc arc in neuron.ingoingArcs)
                    {
                        arc.weight = WeightInit(neuron.ingoingArcs.Count);
                    }
                }
            }
        }

        /// <summary>
        /// Entry: The function receives a number.
        /// Exit: The function retuns the given number after the Leaky ReLU function changed its value.
        /// </summary>
        /// <param name="number">A number.</param>
        /// <returns>The given number after the Leaky ReLU function changed its value</returns>
        private static double LeakyReLU(double number)
        {
            if (number > 0)
                return number;
            else
                return number * 0.01;
        }


        /// <summary>
        /// The function copies the weights of the arcs from the given graph to the graph we activated the function on.
        /// The given graph and the graph we activated the function on must conatin the same number of nodes and arcs 
        /// in each node.
        /// </summary>
        /// <param name="copyFromNetwork">A graph.</param>
        public void CopyWeightsAndBiases(Graph copyFromNetwork)
        {
            // Move on every layer in the graph we need to copy from
            for (int layerIndex = 0; layerIndex < copyFromNetwork.layers.Count; layerIndex++)
            {
                Layer layer = copyFromNetwork.layers[layerIndex];
                int numberOfNodesInLayer = layer.nodesInLayer.Count;
                for (int nodeIndex = 0; nodeIndex < numberOfNodesInLayer; nodeIndex++)
                {
                    Node neuron = layer.nodesInLayer[nodeIndex];

                    // Copy the weights from the ingoing arcs
                    int numberOfIngoingArcs = neuron.ingoingArcs.Count;
                    for (int ingoingArcIndex = 0; ingoingArcIndex < numberOfIngoingArcs; ingoingArcIndex++)
                    {
                        this.layers[layerIndex].nodesInLayer[nodeIndex].ingoingArcs[ingoingArcIndex].weight = neuron.ingoingArcs[ingoingArcIndex].weight;
                    }
                    // Copy the weights from the outgoing arcs
                    int numberOfOutgoingArcs = neuron.outgoingArcs.Count;
                    for (int outgoingArcIndex = 0; outgoingArcIndex < numberOfOutgoingArcs; outgoingArcIndex++)
                    {
                        this.layers[layerIndex].nodesInLayer[nodeIndex].outgoingArcs[outgoingArcIndex].weight = neuron.outgoingArcs[outgoingArcIndex].weight;
                    }
                }
                // Copy the bias value as well
                if (layer.hasBias)
                {
                    this.layers[layerIndex].SetBias(layer.GetBias());
                }
            }
        }

        /// <summary>
        /// The function uses forward propagation to calculate the value of each node in the graph.
        /// </summary>
        public void ForwardPropagation()
        {
            double sum;
            // We are skipping the input layer because we don't change the neuron values there - the value is the input
            for (int layerIndex = 1; layerIndex < this.layers.Count; layerIndex++)
            {
                Layer layer = this.layers[layerIndex];
                foreach (Node neuron in layer.nodesInLayer)
                {
                    sum = 0.0;
                    sum += GetPreviousLayerBias(layerIndex);  // We add the bias of the previous layer
                    foreach (Arc ingoingArc in neuron.ingoingArcs)
                    {
                        sum += ingoingArc.fromNode.nodeValue * ingoingArc.weight;
                    }
                    sum = LeakyReLU(sum);
                    neuron.nodeValue = sum;
                }
            }
        }

        /// <summary>
        /// Exit: The function returns a list that contains the number of neurons in each layer in the Graph we are working on.
        /// </summary>
        /// <returns>A list that contains the number of neurons in each layer in the Graph we are working on</returns>
        private List<int> GetNetworkProperties()
        {
            List<int> neuronsInEachLayer = new List<int>();
            foreach (Layer layer in this.layers)
            {
                neuronsInEachLayer.Add(layer.nodesInLayer.Count);
            }
            return neuronsInEachLayer;
        }

        /// <summary>
        /// The function passes the state in the given array to the input layer in a given neural network.
        /// The length of the given array is supposed to be the same as the length of the input layer in the given neural network.
        /// </summary>
        /// <param name="inputState">A state - a double array.</param>
        public void PassStateToQNetwork(double[] inputState)
        {
            for (int index = 0; index < inputState.Length; index++)
            {
                this.layers[0].nodesInLayer[index].nodeValue = inputState[index];
            }
        }

        /// <summary>
        /// The function returns the biggest value (the Q-Value) in the output layer of the graph we are working on.
        /// </summary>
        /// <returns>The biggest value (the Q-Value) in the output layer of the graph we are working on</returns>
        private double BiggestQValue()
        {
            double biggestQValue = double.MinValue;
            foreach (Node neuron in this.layers[this.layers.Count - 1].nodesInLayer)
            {
                if (biggestQValue < neuron.nodeValue)
                {
                    biggestQValue = neuron.nodeValue;
                }
            }
            return biggestQValue;
        }

        /// <summary>
        /// The function calculates the loss function.
        /// </summary>
        /// <param name="targetValue">The value we attempt to get closer to. The r we recives for preforming action a in state s and the biggest q value
        /// we can get from state s'.</param>
        /// <param name="actualValue">The actual q value we got from preforming action a in state s.</param>
        /// <returns>The function returns the loss function on the given values.</returns>
        private double LossFunction(double targetValue, double actualValue)
        {
            return Math.Pow(targetValue - actualValue, 2);
        }

        /// <summary>
        /// The function returns the derivative of the acivation function on the given neuron value.
        /// Disclimaer: We actually need to receive the neuron's value before the activation function changed it (z)
        /// but in LeakyReLU I can use also the value of the neuron after the activation function changed it.
        /// </summary>
        /// <param name="neuronValue">A neuron value - a of index j in layer l.</param>
        /// <returns>The function returns the derivative of the acivation function on the given neuron value.</returns>
        private double ActivationFunctionDerivative(double neuronValue)
        {
            if(neuronValue > 0)
            {
                return 1;
            }
            else
            {
                return 0.01;
            }
        }

        /// <summary>
        /// The fucntion calculates and returns the derivative of the loss function on the given values.
        /// </summary>
        /// <param name="actualValue">The actual q value we got from preforming action a in state s.</param>
        /// <param name="targetValue">The value we attempt to get closer to.</param>
        /// <returns></returns>
        private double LossFunctionDerivative(double actualValue, double targetValue)
        {
            return 2 * (actualValue - targetValue);
        }

        /// <summary>
        /// The function updates the partial derivatives of a layer. It adds to them because they can already contain values in them.
        /// </summary>
        /// <param name="differenceGraph">The graph the contains the sum of the partial derivatives.</param>
        /// <param name="currentDelta">The delta from the previos layer. The delta that we just caclculated.</param>
        /// <param name="layerIndex">The layer we update its values.</param>
        private void UpdateADerivativeLayerWeights(Graph differenceGraph, double currentDelta, int layerIndex)
        {
            int neuronsInCurrentLayer = this.layers[layerIndex].nodesInLayer.Count;
            for (int currentLayerNeuronIndex = 0; currentLayerNeuronIndex < neuronsInCurrentLayer; currentLayerNeuronIndex++)
            {
                int numberOfIngoingArcs = this.layers[layerIndex].nodesInLayer[currentLayerNeuronIndex].ingoingArcs.Count;
                for (int ingoingArcIndex = 0; ingoingArcIndex < numberOfIngoingArcs; ingoingArcIndex++)
                {
                    double neuronValue = this.layers[layerIndex].nodesInLayer[currentLayerNeuronIndex].ingoingArcs[ingoingArcIndex].fromNode.nodeValue;
                    differenceGraph.layers[layerIndex].nodesInLayer[currentLayerNeuronIndex].ingoingArcs[ingoingArcIndex].weight += currentDelta * neuronValue;
                }
            }
        }

        /// <summary>
        /// The function preforms backpropagation on the graph that we are working on.
        /// The function adds the partial derivative of each weight and bias in the network to the differenceGraph graph.
        /// </summary>
        /// <param name="differenceGraph">The graph that saves the sum of the partial deriviatives.</param>
        /// <param name="actualValue">The actual value we received from the output layer.</param>
        /// <param name="targetValue">The value we try to get closer to.</param>
        /// <param name="action">The action we just preformed.</param>
        private void Backpropagation(Graph differenceGraph, double actualValue, double targetValue, int action)
        {
            double currentDelta, previousDelta;
            // Working on the arcs thats connected to the output layer
            // The layer we are working at - the output layer
            Layer QNetworkCurrentLayer = this.layers[this.layers.Count - 1];
            // The node with the q value of the action a the AI chose
            Node QNetworkCurrentNode = QNetworkCurrentLayer.nodesInLayer[action];

            currentDelta = LossFunctionDerivative(actualValue, targetValue);
            currentDelta *= ActivationFunctionDerivative(QNetworkCurrentNode.nodeValue);

            // Update the bias of the previous layer
            differenceGraph.AddToPreviousLayerBias(differenceGraph.layers.Count - 1, currentDelta);

            Node differenceCurrentNode = differenceGraph.layers[differenceGraph.layers.Count - 1].nodesInLayer[action];
            for (int ingoingArcIndex = 0; ingoingArcIndex < QNetworkCurrentNode.ingoingArcs.Count; ingoingArcIndex++)
            {
                differenceCurrentNode.ingoingArcs[ingoingArcIndex].weight += currentDelta * QNetworkCurrentNode.ingoingArcs[ingoingArcIndex].fromNode.nodeValue;
            }
            previousDelta = currentDelta;


            // Work on the layer before
            // Init the currentDelta variable - we need to accumulate a new current delta value 
            currentDelta = 0;
            foreach (Arc ingoingArc in QNetworkCurrentNode.ingoingArcs)
            {
                double accumalator = previousDelta * ingoingArc.weight;
                //accumalator *= ActivationFunctionDerivative(ingoingArc.fromNode.nodeValue);
                currentDelta += accumalator;
            }
            // Set the index of the layer we are working on
            int currentlayerIndex = this.layers.Count - 2;
            // Update the bias of the previous layer
            differenceGraph.AddToPreviousLayerBias(currentlayerIndex, currentDelta);

            UpdateADerivativeLayerWeights(differenceGraph, currentDelta, this.layers.Count - 2);
            previousDelta = currentDelta;

            // Now we are starting to work on the other arcs
            for (int layerIndex = this.layers.Count - 3; layerIndex >= 1; layerIndex--)
            {
                QNetworkCurrentLayer = this.layers[layerIndex];
                currentDelta = 0;

                // Move on the neurons of our current layer
                for (int neuronIndex = 0; neuronIndex < QNetworkCurrentLayer.nodesInLayer.Count; neuronIndex++)
                {
                    double accumalator = 0;
                    QNetworkCurrentNode = QNetworkCurrentLayer.nodesInLayer[neuronIndex];

                    foreach (Arc outgoingArc in QNetworkCurrentNode.outgoingArcs)
                    {
                        accumalator += previousDelta * outgoingArc.weight;
                    }
                    accumalator *= ActivationFunctionDerivative(QNetworkCurrentNode.nodeValue);
                    currentDelta += accumalator;
                }

                differenceGraph.AddToPreviousLayerBias(layerIndex, currentDelta);

                UpdateADerivativeLayerWeights(differenceGraph, currentDelta, layerIndex);
                previousDelta = currentDelta;
            }
        }

        /// <summary>
        /// The function updates the weights and biases of the graph that we are working on accoring to the 
        /// partial derivatives that we have in the differenceGraph graph.
        /// </summary>
        /// <param name="differenceGraph">The graph that saves the sum of the partial deriviatives.</param>
        /// <param name="numberOfExperiences">Number of experiences that we used in backpropagation.</param>
        /// <param name="learningRate">The learning rate of the network.</param>
        private void UpdateWeights(Graph differenceGraph, int numberOfExperiences, double learningRate)
        {
            int numberOfLayers = this.layers.Count;
            for (int layerIndex = 0; layerIndex < numberOfLayers-1; layerIndex++)
            {
                int numberOfNeurons = this.layers[layerIndex].nodesInLayer.Count;
                for (int neuronIndex = 0; neuronIndex < numberOfNeurons; neuronIndex++)
                {
                    int numberOfoutgoingArcs = this.layers[layerIndex].nodesInLayer[neuronIndex].outgoingArcs.Count;
                    for (int outgoingArcIndex = 0; outgoingArcIndex < numberOfoutgoingArcs; outgoingArcIndex++)
                    {
                        Arc QNetworkArc = this.layers[layerIndex].nodesInLayer[neuronIndex].outgoingArcs[outgoingArcIndex];
                        Arc differenceGraphkArc = differenceGraph.layers[layerIndex].nodesInLayer[neuronIndex].outgoingArcs[outgoingArcIndex];

                        QNetworkArc.weight = QNetworkArc.weight - learningRate * (differenceGraphkArc.weight / numberOfExperiences);
                    }
                }
                if(this.layers[layerIndex].hasBias)
                {
                    this.layers[layerIndex].SetBias(this.layers[layerIndex].GetBias() - learningRate * (differenceGraph.layers[layerIndex].GetBias() / numberOfExperiences));
                }
            }
        }

        /// <summary>
        /// The function preforms stochastic gradient descent of the graph that we are workig on and changes its values
        /// according to the algorithm.
        /// </summary>
        /// <param name="targetNetwork">The Target-Network.</param>
        /// <param name="memories">The experience replay object that saves the network's memories.</param>
        /// <param name="minibatchSize">The size of the minibatch we are training the network on.</param>
        /// <param name="gama">A discount factor.</param>
        /// <param name="learningRate">The learning rate of the network.</param>
        /// <param name="logFileAddress">A log path that we can save data to.</param>
        public void StochasticGradientDescent(Graph targetNetwork, ReplayMemory memories, int minibatchSize, float gama, double learningRate, string logFileAddress)
        {
            // Create a temporary neural network that will contain the changes to the weights and biases in the neural network
            // we are working on - the Q-Network.
            Graph differenceGraph = new Graph();
            differenceGraph.CreateNeuralNetwork(this.GetNetworkProperties());

            // Save the original q network so we can save it in the log file later
            Graph temp = new Graph();
            temp.CreateNeuralNetwork(this.GetNetworkProperties());
            temp.CopyWeightsAndBiases(this);

            // Create a minibatch of memories
            List<Experience> minibatch = memories.GetMiniBatch(minibatchSize);

            double lossFunctionLog = 0.0;

            File.AppendAllText(logFileAddress, "\n\nBefore Updating: " + "\n");

            // Train on each experience in the minibatch
            foreach (Experience expereince in minibatch)
            {
                double targetValue;
                // Calculate the targetValue The value the network should get closer to. If s is terminal s' doesn't exists.
                if (!expereince.isSTerminal)
                {
                    targetNetwork.PassStateToQNetwork(expereince.nextS);
                    targetNetwork.ForwardPropagation();
                    targetValue = expereince.r + gama * targetNetwork.BiggestQValue();
                }
                else
                {
                    targetValue = expereince.r;
                }

                // Calculate the actual value we got from the network we are working on
                this.PassStateToQNetwork(expereince.s);
                this.ForwardPropagation();
                double actualValue = this.layers[this.layers.Count - 1].nodesInLayer[expereince.a].nodeValue;

                File.AppendAllText(logFileAddress, "\n\nTarget value: " + targetValue.ToString() + " Actual Value: " + actualValue.ToString() + "\n");
                // calcualte the loss function
                double lossFunction = LossFunction(targetValue, actualValue);

                lossFunctionLog += lossFunction;

                // Preform backpropagation
                this.Backpropagation(differenceGraph, actualValue, targetValue, expereince.a);
            }
            this.UpdateWeights(differenceGraph, minibatch.Count, learningRate);

            File.AppendAllText(logFileAddress, "\n\nLoss function before: " + (lossFunctionLog / minibatch.Count).ToString() + "\n");

            double lossBefore = lossFunctionLog / minibatch.Count;

            // --------- Check --------------

            File.AppendAllText(logFileAddress, "\nAfter Updating: " + "\n");

            lossFunctionLog = 0.0;

            foreach (Experience expereince in minibatch)
            {
                double targetValue;
                // If s is terminal s' doesn't exists
                if (!expereince.isSTerminal)
                {
                    targetNetwork.PassStateToQNetwork(expereince.nextS);
                    targetNetwork.ForwardPropagation();
                    targetValue = expereince.r + gama * targetNetwork.BiggestQValue();
                }
                else
                {
                    targetValue = expereince.r;
                }

                this.PassStateToQNetwork(expereince.s);
                this.ForwardPropagation();
                double actualValue = this.layers[this.layers.Count - 1].nodesInLayer[expereince.a].nodeValue;

                File.AppendAllText(logFileAddress, "\n\nTarget value: " + targetValue.ToString() + " Actual Value: " + actualValue.ToString() + "\n");
                double lossFunction = LossFunction(targetValue, actualValue);

                lossFunctionLog += lossFunction;
            }
            File.AppendAllText(logFileAddress, "\n\nLoss function after : " + (lossFunctionLog / minibatch.Count).ToString() + "\n");

            if(lossBefore < (lossFunctionLog / minibatch.Count))
            {
                File.AppendAllText(logFileAddress, "\nError\nThe graph before updating: \n\n" + temp.ToString());

                File.AppendAllText(logFileAddress, "\nThe graph after updating: \n\n" + this.ToString());
            }

            // ------------------------------
        }

        /// <summary>
        /// The function returns the graph we activated the function on as a string.
        /// </summary>
        /// <returns>Returns the graph we activated the function on as a string</returns>
        public override string ToString()
        {
            string output = "";
            int count = 1;
            output += "\nNodeID:NodeValue:ArcWeight(opt)\n";
            foreach (Layer layer in this.layers)
            {
                output += "\n\nLayer number: " + count + "\n";
                if (layer.hasBias)
                {
                    output += "Layer bias value: " + layer.GetBias().ToString("F") + "\n";
                }
                foreach (Node node in layer.nodesInLayer)
                {
                    output += "\n" + node.ToString();
                }
                count++;
            }
            return output;
        }
    }
}
