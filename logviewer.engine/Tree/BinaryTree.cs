namespace logviewer.engine.Tree
{
    /// <summary>
    /// Represents binary tree abstraction
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinaryTree<T>
    {
        /// <summary>
        /// Creates new empty tree
        /// </summary>
        public BinaryTree()
        {
            this.Root = null;
        }

        /// <summary>
        /// Gets or sets tree root
        /// </summary>
        public BinaryTreeNode<T> Root { get; set; }

        /// <summary>
        /// Clears tree
        /// </summary>
        public virtual void Clear()
        {
            this.Root = null;
        }
    }
}