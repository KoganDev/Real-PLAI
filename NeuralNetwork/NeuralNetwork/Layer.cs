using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork
{
    public class Layer
    {
        public List<Node> nodesInLayer { get; set; }
        public bool hasBias { get; set; }

        private Node bias;

        /// <summary>
        /// Constructor: The function creates a new and empty Layer object.
        /// </summary>
        public Layer()
        {
            nodesInLayer = new List<Node>();
            bias = null;
            hasBias = false;
        }

        /// <summary>
        /// Set a new bais.
        /// </summary>
        /// <param name="value">The value of the new bias we are adding to the layer.</param>
        public void SetBias(double value)
        {
            bias = new Node(value);
            hasBias = true;
        }

        /// <summary>
        /// Get the bias from the layer we are working on.
        /// </summary>
        /// <returns>The bias of the layer.</returns>
        public double GetBias()
        {
            return bias.nodeValue;
        }

        /// <summary>
        /// Remove the bias from the layer object we are working on.
        /// </summary>
        public void RemoveBias()
        {
            bias = null;
            hasBias = false;
        }

        /// <summary>
        /// Add a new node to the layer we are working on.
        /// </summary>
        /// <param name="node">The node we need to add.</param>
        public void AddNode(Node node)
        {
            nodesInLayer.Add(node);
        }

        /// <summary>
        /// Remove a node from the layer we are working on.
        /// </summary>
        /// <param name="node">The node that we need to remove.</param>
        public void RemoveNode(Node node)
        {
            nodesInLayer.Remove(node);
        }
    }
}
