using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NeuralNetwork;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class AIController : MonoBehaviour
{
    // ------------------------ Log Variables ---------------------------
    
    public static string logPath;

    // ------------------------ Scene Names ---------------------------

    private string AIWinningScreen = "AIWinningScreen";
    private string currentSceneName;

    // ------------------------ AI's Character Variables ---------------------------

    private Animator AIAnimator;
    private Collider2D AICollider;
    private Rigidbody2D AIRigidBody;

    [SerializeField] private LayerMask ground;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float hurtForce = 10f;

    private enum State { idle, running, jumping, falling };
    private State AIState;

    // ------------------------ Environment Variables ---------------------------

    [SerializeField] private Tilemap foreground;
    [SerializeField] private Tilemap dangerLayer;

    private enum EnvironmentState { Air, Ground, StartBlock, EndBlock, Spikes };
    private const int numberOfStates = 5;

    // ------------------------ The AI's Input Variables ---------------------------

    public const int ySize = 12;
    public const int xSize = 14;
    const int rightOffest = 3;
    private double[,] states;
    private double[] inputNN;

    // ------------------------ The AI's Neural Networks ---------------------------

    public static Graph QNetwork;
    public static Graph TargetNetwork;
    public static ReplayMemory memories;
    private double sumOfRewards = 0.0;
    private enum Action { MoveLeft, MoveRight, Jump, RightJump, LeftJump};
    private const int numberOfActions = 5;

    // ------------------------ General AI Variables ---------------------------

    [SerializeField] private Rigidbody2D destination;
    private Vector2 lastCoordinates;
    private double[] stateS;
    private Vector2 endPosition;
    private Action chosenAction;
    private bool won;
    private bool lost;
    private bool firstState = true;
    public static int numberOfTries = 0;
    private static int numberOfIteration = 0;  // The number of frames since the beginning of the level
    private int numberOfEpochs = 0;  // The number of times we preformed an update on our Q-Network
    private int levelTime;
    private int secondsPerRectangle = 20;

    // ------------------------ UI Variables ---------------------------

    [SerializeField] private Text NumberOfTriesText;
    private string NumberOfTriesContent = "";

    [SerializeField] private Text RewardsSumText;
    private string RewardsSumTextContent = "";

    [SerializeField] private Text TryTimeText;
    private string TryTimeTextContent = "Time: ";

    public static System.Diagnostics.Stopwatch tryTime = new System.Diagnostics.Stopwatch();

    [SerializeField] private Text overallTimeText;
    private string overallTimeTextContent = "Overall Time: ";
    public static bool startedTime = false;

    public static System.Diagnostics.Stopwatch overallTime = new System.Diagnostics.Stopwatch();

    // ------------------------ AI Hyperparameters ---------------------------

    private int numberOfIterationsToAct = 5;  // How many iterations should pass every time until the AI can move
    private int numberOfEpochsToUpdateTarget = 45;  // How many epochs should pass every time until we update the Target-Network 
    private const int minibatchSize = 32;
    private float gama = 0.9f;
    private double learningRate = 0.01;

    // -------------- Epsilon Decay --------------------

    private const double minExpsilon = 0.075;
    private const double maxExpsilon = 1;
    private static double epsilon = maxExpsilon;
    private double decayPerEpoch = 0.01;


    /// <summary>
    /// The function caclualtes how much time to give to the AI to solve the current level.
    /// </summary>
    private void CalculateLevelTime()
    {
        levelTime = LevelControl.rectangles * secondsPerRectangle;  // Level time in seconds
    }

    /// <summary>
    ///The function initates the variables that are related to the character of the AI - how he looks like and moves.
    /// </summary>
    private void InitAICharacter()
    {
        AIRigidBody = GetComponent<Rigidbody2D>();
        AIAnimator = GetComponent<Animator>();
        AICollider = GetComponent<Collider2D>();
        AIState = State.idle;
        lastCoordinates = AIRigidBody.position;
    }

    /// <summary>
    /// The Start function is called before the first frame update and initializes the AI's variables.
    /// </summary>
    void Start()
    {
        // This matrix will contain the input I send to the neural network in a form of a two dimensional array
        states = new double[ySize, xSize];
        // This array will contain the input I send to the neural network in a form of a one dimensional array
        inputNN = new double[ySize * xSize];
        // Get the destination position as the end position of the level
        endPosition = destination.position;
        // Init the AI character
        InitAICharacter();
        // boolean values that contains values for whether the AI won or lost
        won = false;
        lost = false;
        // Set scene names so they can be used in case of a win/lost
        currentSceneName = SceneManager.GetActiveScene().name;
        // Calculate level time in seconds
        CalculateLevelTime();

        if (AIInitiation.QNetworkExists && AIInitiation.TargetNetworkExists)
        {
            // If the network exists no need to use epsilon decay
            Debug.Log("Constant Epsilon");
            epsilon = minExpsilon;
        }

        // Start the overall time if needed
        if(!startedTime)
        {
            overallTime.Start();
            startedTime = true;
        }

        // Start the level's time again
        tryTime.Restart();
        tryTime.Start();

        numberOfTries++;
        NumberOfTriesContent = "Number Of Tries: " + numberOfTries.ToString();
        NumberOfTriesText.text = NumberOfTriesContent;

        RewardsSumTextContent = "The Sum Of Rewards: " + sumOfRewards.ToString("F");
        RewardsSumText.text = RewardsSumTextContent;

        File.AppendAllText(logPath, "\n\n---------Starting----------\n\n");
    }

    /// <summary>
    /// The function initiates the global variable of the states matrix.
    /// </summary>
    private void InitState()
    {
        // Initialize the states matrix - 0 means air - no tile in the grid
        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                states[y, x] = (int)EnvironmentState.Air;
            }
        }
    }


    /// <summary>
    /// The function handles the AI collisions with the environment.
    /// The function is called when the AI collides with something.
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionEnter2D(Collision2D other)
    {
        // If the player touches the spikes restart level
        if (other.gameObject.tag == "Danger")
        {
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            lost = true;
        }
        else if (other.gameObject.tag == "Destination")
        {
            won = true;
        }
    }

    /// <summary>
    /// The function inserts the values from the states matrix to the inputNN array.
    /// </summary>
    private void ConvertToOneDimension()
    {
        for (int y = ySize - 1; y >= 0; y--)
        {
            for (int x = 0; x < xSize; x++)
            {
                inputNN[ySize * y + x] = states[y, x];
            }
        }
    }

    /// <summary>
    /// The function changes the given number to be in range of 0-1.
    /// </summary>
    /// <param name="value">A number that represnets a specific block in the environment.</param>
    /// <returns>A number in range of 0-1.</returns>
    private double SetValueInRange(double value)
    {
        return value / (numberOfStates - 1);
    }

    /// <summary>
    /// The function receives the current environemnt state from the environment and inserts the state to the inputNN array.
    /// </summary>
    private void GetEnvironmentState()
    {
        InitState();
        // Get AI's position in game
        Vector2 playerPosition = AIRigidBody.position;
        // Get the AI's position in the grid world
        Vector3Int currentCell = foreground.WorldToCell(playerPosition);
        int yDifference = ySize / 2;  // Half of the height of the AI's input state
        int xDifference = xSize / 2;  // Half of the width of the AI's input state

        int yCounter = 0;
        for (int y = currentCell.y - yDifference; y < currentCell.y + yDifference; y++)
        {
            int xCounter = 0;
            int xCellAfterOffset = currentCell.x + rightOffest;
            for (int x = xCellAfterOffset - xDifference; x < xCellAfterOffset + xDifference; x++)
            {
                TileBase foregroundTile = foreground.GetTile(new Vector3Int(x, y, currentCell.z));
                TileBase dangerousTile = dangerLayer.GetTile(new Vector3Int(x, y, currentCell.z));
                if (foregroundTile != null)
                {
                    if (foregroundTile.name == "Block")
                    {
                        states[yCounter, xCounter] = (int)EnvironmentState.Ground;
                    }
                    else if (foregroundTile.name == "StartBlock")
                    {
                        states[yCounter, xCounter] = (int)EnvironmentState.StartBlock;
                    }
                    else if (foregroundTile.name == "EndBlock")
                    {
                        states[yCounter, xCounter] = (int)EnvironmentState.EndBlock;
                    }
                    states[yCounter, xCounter] = SetValueInRange(states[yCounter, xCounter]);
                }
                else if (dangerousTile != null)
                {
                    if (dangerousTile.name == "Spikes")
                    {
                        states[yCounter, xCounter] = (int)EnvironmentState.Spikes;
                    }
                    states[yCounter, xCounter] = SetValueInRange(states[yCounter, xCounter]);
                }
                xCounter++;
            }
            yCounter++;
        }
        ConvertToOneDimension();
    }

    /// <summary>
    /// The function searches and returns the action with the biggest Q Value from the Q-Network output layer.
    /// </summary>
    /// <returns>The action with the biggest Q Value from the Q-Network output layer.</returns>
    private Action GetBiggestQValueAction(Graph neuralNetwork)
    {
        double biggestQvalue = double.MinValue;
        Action ActionWithBiggestQValue = 0;
        int numberOfOutputNeurons = neuralNetwork.layers[neuralNetwork.layers.Count - 1].nodesInLayer.Count;
        Layer outputLayer = neuralNetwork.layers[neuralNetwork.layers.Count - 1];

        for (int outputNeuronIndex = 0; outputNeuronIndex < numberOfOutputNeurons; outputNeuronIndex++)
        {
            double neuronValue = outputLayer.nodesInLayer[outputNeuronIndex].nodeValue;
            if (neuronValue > biggestQvalue)
            {
                biggestQvalue = neuronValue;
                ActionWithBiggestQValue = (Action)outputNeuronIndex;
            }
        }
        return ActionWithBiggestQValue;
    }

    /// <summary>
    /// The function returns an action according to the epsilon greedy policy.
    /// </summary>
    /// <returns>Action according to the epsilon greedy policy.</returns>
    private Action GetAction()
    {
        float chance = Random.Range(0f, 1f);  // Get a random float between 0-1
        if(chance < epsilon)  // We have a probability of epsilon to perform a random action
        {
            return (Action)Random.Range(0, numberOfActions);
        }
        else  // We have a probability of 1-epsilon to perform the action with the biggest Q-value
        {
            return GetBiggestQValueAction(QNetwork);
        }
    }

    /// <summary>
    /// The function changes the player's animation states according to its velocity.
    /// The function is called every frame by the Update function.
    /// </summary>
    private void AnimationState()
    {
        // Jump and fall animations have priority over running and idle animations because they are first in the if statements
        if (AIState == State.jumping)
        {
            // If the player stops jumping and starts falling
            if (AIRigidBody.velocity.y < 0.1f)
            {
                AIState = State.falling;
            }
        }
        // If the player isn't jumping and isn't touching the ground it falls
        else if (!AICollider.IsTouchingLayers(ground))  // Check if the player is falling
        {
            // If the player has velocity down change to falling animation 
            AIState = State.falling;
        }
        else if (AIState == State.falling)
        {
            // If we landed on the ground after falling change states to idle
            if (AICollider.IsTouchingLayers(ground))
            {
                AIState = State.idle;
            }
        }
        // If the player has velocity in the x axis change to running animation states
        else if (Mathf.Abs(AIRigidBody.velocity.x) > 2f)
        {
            AIState = State.running;
        }
        // If the player isn't moving change to idle states
        else
        {
            AIState = State.idle;
        }
    }

    /// <summary>
    /// The function makes the AI move left - it gives it velocity left.
    /// </summary>
    private void MoveLeft()
    {
        //Debug.Log("Move left");
        // If the AI is trying to stop falling by running on a surface on the side of the surface - don't move (not allowed)
        if (AIRigidBody.velocity.y < -0.3f && AICollider.IsTouchingLayers(ground))
        {
            return;
        }
        // We dont want to affect the y velocity so it stays the same
        AIRigidBody.velocity = new Vector2(-speed, AIRigidBody.velocity.y);
        // The x is -1 so the sprite will turn left (we don't touch the y) - the AI is facing left
        transform.localScale = new Vector3(-1, 1);
    }

    /// <summary>
    /// The function makes the AI move right - it gives it velocity right.
    /// </summary>
    private void MoveRight()
    {
        //Debug.Log("Move right");
        // If the AI is trying to stop falling by running on a surface on the side of the surface - don't move (not allowed)
        if (AIRigidBody.velocity.y < -0.3f && AICollider.IsTouchingLayers(ground))
        {
            return;
        }
        // We dont want to affect the y velocity so it stays the same
        AIRigidBody.velocity = new Vector2(speed, AIRigidBody.velocity.y);
        // The x is 1 so the sprite will turn right (we don't touch the y) - the AI is facing right
        transform.localScale = new Vector3(1, 1);
    }

    /// <summary>
    /// The function makes the AI jump - it gives it velocity upwards.
    /// </summary>
    private void Jump()
    {

        // We dont want to affect the x velocity
        AIRigidBody.velocity = new Vector2(AIRigidBody.velocity.x, jumpForce);
        AIState = State.jumping;
    }

    /// <summary>
    /// The function makes the AI to perform the given action.
    /// </summary>
    /// <param name="action">An action which the AI need to execute.</param>
    private void PerformAction(Action action)
    {
        switch(action)
        {
            case Action.MoveLeft:
                {
                    MoveLeft();
                    break;
                }
            case Action.MoveRight:
                {
                    MoveRight();
                    break;
                }
            case Action.Jump:
                {
                    if(AICollider.IsTouchingLayers(ground))
                    {
                        Jump();
                    }
                    break;
                }
            case Action.LeftJump:
                {
                    MoveLeft();
                    if (AICollider.IsTouchingLayers(ground))
                    {
                        Jump();
                    }
                    break;
                }
            case Action.RightJump:
                {
                    MoveRight();
                    if (AICollider.IsTouchingLayers(ground))
                    {
                        Jump();
                    }
                    break;
                }
        }
    }

    /// <summary>
    /// The function calculates and returns the reward the agent should get for the current iteration.
    /// </summary>
    /// <returns>The function returns the reward the agent gets for the current iteration.</returns>
    private double GetReward()
    {
        double mul = Mathf.Pow(gama, numberOfEpochs);
        // Every iteration the reward should get smaller - this way the AI will finish the level as fast as possible
        double sum = -1;
        if (won)
        {
            // If we got to the end give the largest prize
            sum = 10;
        }
        else if(lost)
        {
            lost = true;
            sum = -5;
        }
        else
        {
            double lastDistance = Mathf.Abs(endPosition.x - lastCoordinates.x);
            double currentDistance = Mathf.Abs(endPosition.x - AIRigidBody.position.x);
            if (lastDistance > currentDistance)
            {
                // If we got closer give a positive prize
                sum = 0.5;
            }
        }
        return sum * mul;
    }

    /// <summary>
    /// The function is in charge of changing the AI's character animation.
    /// </summary>
    private void AnimationManager()
    {
        // Change animation due to the AI's action
        AnimationState();
        // Set the new animation state
        AIAnimator.SetInteger("state", (int)AIState);
    }

    /// <summary>
    /// The function saves a new memory to the AI's memories - adds a memory to the experience replay buffer.
    /// </summary>
    /// <param name="r">A reward</param>
    /// <param name="a">An action</param>
    private void InsertANewMemory(double[] s, double r, Action a, double[] sNext)
    {
        // Check if the state s' is terminal
        bool isTerminal = (won || lost) ? true : false;
        memories.Enqueue(new Experience(s, (int)a, r, sNext, isTerminal));
    }

    /// <summary>
    /// The function saves the Q-Network, the Target-Network and the Expereince replay memories to text files.
    /// </summary>
    public static void SaveAll()
    {
        // Save the Q-Network weights and biases to a txt file
        AIInitiation.SaveNetwork(QNetwork, AIInitiation.QNetworkFilePath);
        // Save the Target-Network weights and biases to a txt file
        AIInitiation.SaveNetwork(TargetNetwork, AIInitiation.targetNetworkFilePath);
        // Save the AI's memories - the experience replay buffer
        AIInitiation.SaveMemories(memories, AIInitiation.memoriesFilePath);
    }

    /// <summary>
    /// The function saves the Q-Network, the Target-Network and the Expereince replay memories to text files when
    /// the game is closed in the middle of the AI's level.
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveAll();
    }

    /// <summary>
    /// Check if the AI's time to solve the level is over. If so it means that the AI lost.
    /// </summary>
    private void CheckIfTimeOver()
    {
        if(tryTime.Elapsed.TotalSeconds >= levelTime)
        {
            lost = true;
        }
    }

    /// <summary>
    /// The function shows the AI's time on screen - the overall time and the time for the current try.
    /// </summary>
    private void ShowTimeOnScreen()
    {
        // Measure overall time and show it on the screen
        System.TimeSpan time = overallTime.Elapsed;
        string elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
        overallTimeText.text = overallTimeTextContent + elapsedTime;

        // Measure time and show it on the screen
        time = tryTime.Elapsed;
        elapsedTime = System.String.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
        TryTimeText.text = TryTimeTextContent + elapsedTime;
    }


    /// <summary>
    /// The function is in charge of the AI. The function is called once per frame and recieves nothing.
    /// </summary>
    void Update()
    {
        ShowTimeOnScreen();

        if (firstState)
        {
            // Get state s
            GetEnvironmentState();
            // State s is in stateS
            stateS = (double[])inputNN.Clone();
            // Pass state s to the Q-Network
            QNetwork.PassStateToQNetwork(stateS);
            // Execute forward propagation in the Q-Network
            QNetwork.ForwardPropagation();
            // Get an action to perform according to the epsilon greedy policy
            chosenAction = GetAction();
            // Perform the given action
            PerformAction(chosenAction);
            lastCoordinates = AIRigidBody.position;
            firstState = false;
        }
        else if (numberOfIteration % numberOfIterationsToAct == 0)
        {
            // Receive a reward after performing the action
            double reward = GetReward();
            sumOfRewards += reward;
            // Print The Sum Of Rewards To The Game Screen
            RewardsSumTextContent = "The Sum Of Rewards: " + sumOfRewards.ToString("F");
            RewardsSumText.text = RewardsSumTextContent;

            // Get state s' inside inputNN
            GetEnvironmentState();
            double[] sNext = (double[])inputNN.Clone();
            // Inserts a new memory to the experience replay buffer
            InsertANewMemory(stateS, reward, chosenAction, sNext);

            // preform Stochastic Gradient Descent and Backpropagation
            QNetwork.StochasticGradientDescent(TargetNetwork, memories, minibatchSize, gama, learningRate, logPath);
            File.AppendAllText(logPath, "\n\nSum of Rewards: " + sumOfRewards.ToString("F") + "\n");
            if (numberOfIteration % numberOfEpochsToUpdateTarget == 0)
            {
                // Preform target network update
                TargetNetwork.CopyWeightsAndBiases(QNetwork);
            }

            stateS = (double[])sNext.Clone();
            numberOfEpochs++;

            // Epsilon Decay
            if(epsilon > minExpsilon)
            {
                epsilon = epsilon - decayPerEpoch;
                Debug.Log("Epsilon: " + epsilon.ToString("F"));
                if(epsilon < minExpsilon)
                {
                    epsilon = minExpsilon;
                    Debug.Log("Epsilon At Minimum. Epsilon: " + epsilon.ToString("F"));
                }
            }

            // Pass state s to the Q-Network
            QNetwork.PassStateToQNetwork(stateS);
            // Execute forward propagation in the Q-Network
            QNetwork.ForwardPropagation();
            // Get an action to perform according to the epsilon greedy policy
            chosenAction = GetAction();
            // Perform the given action
            PerformAction(chosenAction);
            lastCoordinates = AIRigidBody.position;
        }

        // Update the charcter's animation every frame no matter what
        AnimationManager();

        // Check if the AI's time is over
        CheckIfTimeOver();

        // Switch scenes in case the agent won/lost
        if (won)
        {
            // Stop meausring time
            tryTime.Stop();
            overallTime.Stop();
            // Save the networks and the memories
            SaveAll();
            SceneManager.LoadScene(AIWinningScreen);
        }
        else if (lost)
        {
            // Dont save here!
            SceneManager.LoadScene(currentSceneName);
        }
        numberOfIteration++;
    }
}
