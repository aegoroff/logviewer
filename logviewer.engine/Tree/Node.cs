// Created by: egr
// Created at: 13.07.2015
// © 2012-2015 Alexander Egorov

using System.Diagnostics;

namespace logviewer.engine.Tree
{
    /// <summary>
    /// Represents generic tree node
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("{Value}")]
    public class Node<T>
    {
        
        /// <summary>
        /// Creates new empty node instance
        /// </summary>
        public Node()
        {
            Neighbors = null;
        }

        /// <summary>
        /// Creates new node instance with data specified but without childs
        /// </summary>
        /// <param name="data"></param>
        public Node(T data) : this(data, null)
        {
        }

        /// <summary>
        /// Creates fully initilized node with data and neighbors
        /// </summary>
        /// <param name="data">Node data</param>
        /// <param name="neighbors">Node neighbors</param>
        public Node(T data, NodeList<T> neighbors)
        {
            this.Value = data;
            this.Neighbors = neighbors;
        }

        /// <summary>
        /// Gets or sets node data
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets node neighbors
        /// </summary>
        protected NodeList<T> Neighbors { get; set; }
    }
}