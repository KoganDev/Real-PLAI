using System;
using System.Collections.Generic;
using System.Text;

namespace NeuralNetwork
{
    public class ReplayMemory
    {
        private List<Experience> buffer;
        public const int maxCapacity = 10000;
        private static Random random = new Random();

        /// <summary>
        /// Constructor: Create a new and empty experince replay object.
        /// </summary>
        public ReplayMemory()
        {
            buffer = new List<Experience>();
        }

        /// <summary>
        /// The function inserts the given Experience to the buffer.
        /// </summary>
        /// <param name="exp">An Experience</param>
        public void Enqueue(Experience exp)
        {
            if (buffer.Count == maxCapacity)
            {
                // Remove the oldest experience - the experience in the maximum index
                buffer.RemoveAt(maxCapacity - 1);
            }
            buffer.Insert(0, exp);
        }

        /// <summary>
        /// The function returns a list of Experiences in the given size. 
        /// If the function doesn't have enough Experiences the function will return the number of Experiences it contains.
        /// </summary>
        /// <param name="minibatchSize">The size of the number of Experiences the function should return</param>
        /// <returns></returns>
        public List<Experience> GetMiniBatch(int minibatchSize)
        {
            List<Experience> minibatch = new List<Experience>();
            if (minibatchSize > buffer.Count)
            {
                foreach (Experience exp in buffer)
                {
                    minibatch.Add(new Experience(exp.s, exp.a, exp.r, exp.nextS, exp.isSTerminal));
                }
                return minibatch;
            }
            List<int> minibatchIndexes = new List<int>();
            while (minibatchSize != 0)
            {
                int index = random.Next(0, buffer.Count);
                if (!minibatchIndexes.Contains(index))
                {
                    // If we didn't take the experience already add it to our minibatch.
                    minibatch.Add(buffer[index]);
                    minibatchIndexes.Add(index);
                    minibatchSize--;
                }
            }
            return minibatch;
        }
    }
}
