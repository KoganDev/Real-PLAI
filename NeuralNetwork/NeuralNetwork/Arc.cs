using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork
{
    public class Arc
    {
        public Node fromNode { get; set; }
        public Node toNode { get; set; }
        public double weight { get; set; }

        /// <summary>
        /// Constructor: The function creates a new Arc object from the function's inputs.
        /// </summary>
        /// <param name="fromNode">The node the arc starts from.</param>
        /// <param name="toNode">The node the arc is going to.</param>
        /// <param name="weight">The weight of the arc.</param>
        public Arc(Node fromNode, Node toNode, double weight = 1.0)
        {
            this.fromNode = fromNode;
            this.toNode = toNode;
            this.weight = weight;
        }

        /// <summary>
        /// The function converts the arc's values to a string.
        /// </summary>
        /// <returns>The arcs values as a string.</returns>
        public override string ToString()
        {
            return "Weight: " + weight + " " + fromNode.ToString() + "->" + toNode.ToString();
        }
    }
}
