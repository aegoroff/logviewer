namespace logviewer.engine.Tree
{
    /// <summary>
    /// Represents binary tree node abstraction
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinaryTreeNode<T> : Node<T>
    {
        /// <summary>
        /// Initializes new empty node
        /// </summary>
        public BinaryTreeNode()
        {
        }

        /// <summary>
        /// Initializes node with data specified but without childs
        /// </summary>
        /// <param name="data">Node data</param>
        public BinaryTreeNode(T data) : base(data, null)
        {
        }

        /// <summary>
        /// Creates fully initialized node
        /// </summary>
        /// <param name="data">Node data</param>
        /// <param name="left">Left child</param>
        /// <param name="right">Right child</param>
        public BinaryTreeNode(T data, Node<T> left, Node<T> right)
        {
            this.Value = data;
            var children = new NodeList<T>(2);
            children[0] = left;
            children[1] = right;

            this.Neighbors = children;
        }

        /// <summary>
        /// Gets or sets left child node
        /// </summary>
        public BinaryTreeNode<T> Left
        {
            get
            {
                if (this.Neighbors == null)
                {
                    return null;
                }
                return (BinaryTreeNode<T>) this.Neighbors[0];
            }
            set
            {
                if (this.Neighbors == null)
                {
                    this.Neighbors = new NodeList<T>(2);
                }

                this.Neighbors[0] = value;
            }
        }

        /// <summary>
        /// Gets or sets right child node
        /// </summary>
        public BinaryTreeNode<T> Right
        {
            get
            {
                if (this.Neighbors == null)
                {
                    return null;
                }
                return (BinaryTreeNode<T>) this.Neighbors[1];
            }
            set
            {
                if (this.Neighbors == null)
                {
                    this.Neighbors = new NodeList<T>(2);
                }

                this.Neighbors[1] = value;
            }
        }
    }
}