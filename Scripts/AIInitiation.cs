using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NeuralNetwork;

public class AIInitiation : MonoBehaviour
{
    // ------------------------ General Variables ---------------------------

    static bool activated;  // Check if the class functions have been executed already

    private List<int> neuronsInLayers;

    public static bool QNetworkExists = false;
    public static bool TargetNetworkExists = false;

    // ------------------------ Text File Paths ---------------------------

    public static string QNetworkFilePath;
    public static string targetNetworkFilePath;
    public static string memoriesFilePath;

    /// <summary>
    /// The function chooses a new log file name for the current game.
    /// </summary>
    private void SetLogAddress()
    {
        AIController.logPath = Application.dataPath + "/Logs/log";
        int index = 1;
        // Check the file name didn't exists and if it exists check it's not empty - if it's empty we can write there
        // that way if go back to the main menu we'll not get a new log file - it's a waist.
        while (File.Exists(AIController.logPath + index + ".txt") && File.ReadAllText(AIController.logPath + index + ".txt") != "")
        {
            index++;
        }
        AIController.logPath += index + ".txt";
    }

    /// <summary>
    /// The function creates a new Graph (a new neural network) with the values from the given input.
    /// </summary>
    /// <param name="lines"> All the lines from a text file that contains the values of a neural network.</param>
    /// <returns>A new neural network with the values from the given input</returns>
    private Graph Decode(string[] lines)
    {
        Graph network;
        List<int> neurons = new List<int>();  // Init the neurons list
        // Create the structure of the neural network
        string[] structure = lines[0].Split(' ');
        for(int index=0; index < structure.Length-1; index++)  // structure.Length-1 because we get an extra string for some reason
        {
            neurons.Add(int.Parse(structure[index]));
        }
        network = new Graph();
        network.CreateNeuralNetwork(neurons);
        network.InitNeuralNetworkWeightsAndBiases();

        // Give the weights to the new neural network
        int lineNumber = 1;
        string[] weights;
        foreach (Layer layer in network.layers)
        {
            foreach (Node neuron in layer.nodesInLayer)
            {
                weights = lines[lineNumber].Split(' ');
                for (int ingoingArcIndex = 0; ingoingArcIndex < neuron.ingoingArcs.Count; ingoingArcIndex++)
                {
                    neuron.ingoingArcs[ingoingArcIndex].weight = double.Parse(weights[ingoingArcIndex]);
                }

                lineNumber++;
                weights = lines[lineNumber].Split(' ');
                for (int outgoingArcIndex = 0; outgoingArcIndex < neuron.outgoingArcs.Count; outgoingArcIndex++)
                {
                    neuron.outgoingArcs[outgoingArcIndex].weight = double.Parse(weights[outgoingArcIndex]);
                }
                lineNumber++;
            }
        }

        // Give bias values to the new neural network
        string[] biasValues = lines[lineNumber].Split(' ');
        for (int biasIndex = 0; biasIndex < biasValues.Length-1; biasIndex++)
        {
            // biasIndex is also the layer index
            network.layers[biasIndex].SetBias(double.Parse(biasValues[biasIndex]));
        }
        return network;
    }

    /// <summary>
    /// The function initiates the Q-Network from the AIController script.
    /// </summary>
    /// <param name="filePath">The file name that contains the neural network's values.</param>
    private void InitQNetworkFromFile(string filePath)
    {
        if(!File.Exists(filePath) || File.ReadAllText(filePath) == "")
        {
            // Create the txt file if it doesn't exists
            File.WriteAllText(filePath, "");
            // Init the Q-Network
            AIController.QNetwork = new Graph();
            AIController.QNetwork.CreateNeuralNetwork(neuronsInLayers);
            AIController.QNetwork.InitNeuralNetworkWeightsAndBiases();
        }
        else
        {
            QNetworkExists = true;
            string[] lines = File.ReadAllLines(filePath);
            AIController.QNetwork = Decode(lines);
        }
    }

    /// <summary>
    /// The function initiates the Target-Network from the AIController script.
    /// </summary>
    /// <param name="filePath">The file name that contains the neural network's values.</param>
    private void InitTargetNetworkFromFile(string filePath)
    {
        if (!File.Exists(filePath) || File.ReadAllText(filePath) == "")
        {
            // Create the txt file if it doesn't exists
            File.WriteAllText(filePath, "");
            // Init the Target-Network
            AIController.TargetNetwork = new Graph();
            AIController.TargetNetwork.CreateNeuralNetwork(neuronsInLayers);
            AIController.TargetNetwork.CopyWeightsAndBiases(AIController.QNetwork);
        }
        else
        {
            TargetNetworkExists = true;
            string[] lines = File.ReadAllLines(filePath);
            AIController.TargetNetwork = Decode(lines);
        }

    }

    /// <summary>
    /// The function saves the given neural network to a text file in the given path.
    /// </summary>
    /// <param name="neuralNetwork">The neural network we want to save.</param>
    /// <param name="filePath">The path to the text file where we want to save the neural network.</param>
    public static void SaveNetwork(Graph neuralNetwork, string filePath)
    {
        File.WriteAllText(filePath, "");  // Create the text file/Delete all the information from the text file
        // Save the neural network's structre
        foreach (Layer layer in neuralNetwork.layers)
        {
            File.AppendAllText(filePath, layer.nodesInLayer.Count.ToString() + " ");
        }
        File.AppendAllText(filePath, "\n");
        // Save the neural network's weights
        foreach (Layer layer in neuralNetwork.layers)
        {
            foreach (Node neuron in layer.nodesInLayer)
            {
                foreach (Arc ingoingArc in neuron.ingoingArcs)
                {
                    File.AppendAllText(filePath, ingoingArc.weight.ToString() + " ");
                }
                File.AppendAllText(filePath, "\n");
                foreach (Arc outgoingArc in neuron.outgoingArcs)
                {
                    File.AppendAllText(filePath, outgoingArc.weight.ToString() + " ");
                }
                File.AppendAllText(filePath, "\n");
            }
        }
        // Save the neural network's biases
        foreach (Layer layer in neuralNetwork.layers)
        {
            if (layer.hasBias)
            {
                File.AppendAllText(filePath, layer.GetBias().ToString() + " ");
            }
        }
    }

    /// <summary>
    /// The function initiates the Experience Replay memories from the AIController script.
    /// </summary>
    /// <param name="filePath">The file name that contains the Experience Replay memories.</param>
    private void InitMemories(string filePath)
    {
        // Init all the variables needed to receives all the information from a given set of memories
        double[] state = new double[AIController.ySize * AIController.xSize];
        double reward;
        int action;
        double[] nextState = new double[AIController.ySize * AIController.xSize];
        bool isTerminal;

        AIController.memories = new ReplayMemory();
        string[] lines = File.ReadAllLines(filePath);
        int lineIndex = 0;
        foreach(string line in lines)
        {
            string[] parts = line.Split(' ');
            // Get state s
            string[] stateString = parts[0].Split(',');
            for (int tileIndex=0; tileIndex < state.Length; tileIndex++)
            {
                state[tileIndex] = double.Parse(stateString[tileIndex]);
            }
            // Get action a
            action = int.Parse(parts[1]);
            // Get reward r
            reward = double.Parse(parts[2]);
            // Get state s'
            stateString = parts[3].Split(',');
            for (int tileIndex = 0; tileIndex < nextState.Length; tileIndex++)
            {
                nextState[tileIndex] = double.Parse(stateString[tileIndex]);
            }
            // Get is state s' terminal
            isTerminal = bool.Parse(parts[4]);
            AIController.memories.Enqueue(new Experience(state, action, reward, nextState, isTerminal));
            lineIndex++;
        }
    }

    /// <summary>
    /// The function saves the given Experience Replay memories to a text file in the given path.
    /// </summary>
    /// <param name="memory">The memories we need to save.</param>
    /// <param name="filePath">The path of the file where we need to save the memories.</param>
    public static void SaveMemories(ReplayMemory memory, string filePath)
    {
        File.WriteAllText(filePath, "");  // Empty the contents of the file
        foreach(Experience experience in memory.GetMiniBatch(ReplayMemory.maxCapacity))
        {
            string line = "";
            foreach(double tileCode in experience.s)
            {
                line += tileCode.ToString("F") + ",";
            }
            line += " ";
            line += experience.a.ToString();
            line += " ";
            line += experience.r.ToString("F");
            line += " ";
            foreach (double tileCode in experience.nextS)
            {
                line += tileCode.ToString("F") + ",";
            }
            line += " ";
            line += experience.isSTerminal.ToString();
            File.AppendAllText(filePath, line + "\n");
        }
    }

    /// <summary>
    /// Initiate all the neural networks and the memories the AI needs. Start is called before the first frame update
    /// </summary>
    void Start()
    {
        if (activated)
        {
            // If we already loaded all the information no need to do it again so exit the function
            return;
        }
        // Hardcoded neural network size
        neuronsInLayers = new List<int>();
        neuronsInLayers.Add(AIController.ySize * AIController.xSize);
        neuronsInLayers.Add(16);
        neuronsInLayers.Add(16);
        neuronsInLayers.Add(16);
        neuronsInLayers.Add(5); // ***

        // Create the initaiation foldre if it doesn't exists
        if(!Directory.Exists(Application.dataPath + "/InitiationFolder"))
        {
            Directory.CreateDirectory(Application.dataPath + "/InitiationFolder");
        }

        // Create the log folder if it doesn't exists
        if (!Directory.Exists(Application.dataPath + "/Logs"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Logs");
        }


        // Set the file path of the neural networks
            QNetworkFilePath = Application.dataPath + "/InitiationFolder/QNetwork.txt";
        targetNetworkFilePath  = Application.dataPath + "/InitiationFolder/TargetNetwork.txt";
        memoriesFilePath = Application.dataPath + "/InitiationFolder/Memories.txt";

        AIController.specialLog = Application.dataPath + "/InitiationFolder/LOG.txt";
        File.WriteAllText(AIController.specialLog, "");

        // Set the new log file path 
        SetLogAddress();  // After the function is done we can remove the function from the AI controller
        // Initialize the networks
        InitQNetworkFromFile(QNetworkFilePath);  // After the functions are done we can remove the function from the AI controller
        InitTargetNetworkFromFile(targetNetworkFilePath);

        InitMemories(memoriesFilePath);

        activated = true;
    }
}
