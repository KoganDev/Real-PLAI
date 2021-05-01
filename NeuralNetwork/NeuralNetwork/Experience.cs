using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork
{
    public class Experience
    {
        public double[] s { get; set; }
        public int a { get; set; }
        public double r { get; set; }
        public double[] nextS { get; set; }

        public bool isSTerminal { get; set; }

        /// <summary>
        /// Constructor: Create a new set of memories - an experience.
        /// </summary>
        /// <param name="s">The state the AI were in.</param>
        /// <param name="a">The action the AI preformed.</param>
        /// <param name="r">The reward the AI received after preforming action a in state s.</param>
        /// <param name="nextS">The next state after the AI preformed action a in state s.</param>
        /// <param name="isTerminal"></param>
        public Experience(double[] s, int a, double r, double[] nextS, bool isTerminal)
        {
            this.s = s;
            this.a = a;
            this.r = r;
            this.nextS = nextS;
            this.isSTerminal = isTerminal;
        }
    }
}
