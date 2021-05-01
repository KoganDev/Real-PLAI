using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork
{
    public class Node
    {
        private static int serializedID = 1;  // Serialized Number of the class - gives ID to nodes
        private int nodeID;
        public double nodeValue { get; set; }
        public List<Arc> outgoingArcs { get; set; }
        public List<Arc> ingoingArcs { get; set; }

        /// <summary>
        /// Constructor: The value creates a new Node object from a given node value.
        /// </summary>
        /// <param name="value">The value of the node.</param>
        public Node(double value)
        {
            this.nodeValue = value;
            outgoingArcs = new List<Arc>();
            ingoingArcs = new List<Arc>();
            nodeID = serializedID;
            serializedID++;
        }

        /// <summary>
        /// Get a nodeID.
        /// </summary>
        /// <returns></returns>
        public int GetNodeID()
        {
            return nodeID;
        }

        /// <summary>
        /// Add a new outgoing arc.
        /// </summary>
        /// <param name="arc"></param>
        /// <returns>An ougoing arc.</returns>
        public Arc AddOutgoingArc(Arc arc)
        {
            outgoingArcs.Add(arc);
            return arc;
        }

        /// <summary>
        /// Add a new outgoing arc.
        /// </summary>
        /// <param name="arc"></param>
        /// <returns>The node the arc's goes to.</returns>
        public Arc AddOutgoingArc(Node toNode)
        {
            Arc arc = new Arc(this, toNode);
            outgoingArcs.Add(arc);
            return arc;
        }

        /// <summary>
        /// Add a new outgoing arc.
        /// </summary>
        /// <param name="toNode">The node the arc's goes to.</param>
        /// <param name="weight">The weight of the new arc.</param>
        /// <returns></returns>
        public Arc AddOutgoingArc(Node toNode, double weight)
        {
            Arc arc = new Arc(this, toNode, weight);
            outgoingArcs.Add(arc);
            return arc;
        }

        /// <summary>
        /// Remove an outgoin arc.
        /// </summary>
        /// <param name="arc">The arc we need to remove.</param>
        public void RemoveOutgoingArc(Arc arc)
        {
            outgoingArcs.Remove(arc);
        }

        /// <summary>
        /// Add an ingoing arc.
        /// </summary>
        /// <param name="arc"></param>
        /// <returns>The arc we need to add.</returns>
        public Arc AddIngoingArc(Arc arc)
        {
            ingoingArcs.Add(arc);
            return arc;
        }

        /// <summary>
        /// Add an ingoing arc.
        /// </summary>
        /// <param name="fromNode"></param>
        /// <returns>The node the arc starts from./returns>
        public Arc AddIngoingArc(Node fromNode)
        {
            Arc arc = new Arc(fromNode, this);
            ingoingArcs.Add(arc);
            return arc;
        }

        /// <summary>
        /// Add an ingoing arc.
        /// </summary>
        /// <param name="fromNode">The node the arc starts from.</param>
        /// <param name="weight">The weight of the new arc.</param>
        /// <returns></returns>
        public Arc AddIngoingArc(Node fromNode, double weight)
        {
            Arc arc = new Arc(fromNode, this, weight);
            ingoingArcs.Add(arc);
            return arc;
        }

        /// <summary>
        /// Remove an ingoing arc.
        /// </summary>
        /// <param name="arc">The arc we need to remove.</param>
        public void RemoveIngoingArc(Arc arc)
        {
            ingoingArcs.Remove(arc);
        }

        /// <summary>
        /// The function converts the node's values to a string.
        /// </summary>
        /// <returns>The nodes values as a string.</returns>
        public override string ToString()
        {
            string output = "Node: " + this.nodeID + ":" + this.nodeValue.ToString("F");
            output += "\nIn going Nodes: ";
            if (ingoingArcs != null)
            {
                foreach (Arc arc in ingoingArcs)
                {
                    output += arc.fromNode.nodeID.ToString() + ":" + arc.fromNode.nodeValue.ToString("F") + ":" + arc.weight.ToString("F") + ", ";
                }
            }
            output += "\nOut going Nodes: ";
            if (outgoingArcs != null)
            {
                foreach (Arc arc in outgoingArcs)
                {
                    output += arc.toNode.nodeID.ToString() + ":" + arc.toNode.nodeValue.ToString("F") + ":" + arc.weight.ToString("F") + ", ";
                }
            }
            return output;
        }
    }
}
