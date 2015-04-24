using System.Collections.ObjectModel;
using System.Linq;

namespace logviewer.engine.Tree
{
    /// <summary>
    /// Represents node collection abstraction
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NodeList<T> : Collection<Node<T>>
    {
        /// <summary>
        /// Creates empty node list
        /// </summary>
        public NodeList()
        {
        }

        /// <summary>
        /// Creates list with size specified. All neighbors will be defaults (null)
        /// </summary>
        /// <param name="initialSize">Initial size</param>
        public NodeList(int initialSize)
        {
            for (int i = 0; i < initialSize; i++)
            {
                this.Items.Add(default(Node<T>));
            }
        }

        /// <summary>
        /// Find a node in the collection by value
        /// </summary>
        /// <param name="value">Node to file value of</param>
        /// <returns>Result node or null</returns>
        public Node<T> FindByValue(T value)
        {
            return this.Items.FirstOrDefault(node => node.Value.Equals(value));
        }
    }
}