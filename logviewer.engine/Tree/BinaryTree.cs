using System;

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

        /// <summary>
        /// Preorder full tree traverse
        /// </summary>
        /// <param name="action">Action to execute for each node</param>
        public void PreorderTraversal(Action<BinaryTreeNode<T>> action)
        {
            this.PreorderTraversal(this.Root, action);
        }
        
        /// <summary>
        /// Preorder custom tree traverse
        /// </summary>
        /// <param name="current">Node to start travese from</param>
        /// <param name="action">Action to execute for each node</param>
        public void PreorderTraversal(BinaryTreeNode<T> current, Action<BinaryTreeNode<T>> action)
        {
            if (current == null)
            {
                return;
            }
            action(current);
            this.PreorderTraversal(current.Left, action);
            this.PreorderTraversal(current.Right, action);
        }

        /// <summary>
        /// Inorder full tree traverse
        /// </summary>
        /// <param name="action">Action to execute for each node</param>
        public void InorderTraversal(Action<BinaryTreeNode<T>> action)
        {

            this.InorderTraversal(Root, action);
        }
        
        /// <summary>
        /// Inorder custom tree traverse
        /// </summary>
        /// <param name="current">Node to start travese from</param>
        /// <param name="action">Action to execute for each node</param>
        public void InorderTraversal(BinaryTreeNode<T> current, Action<BinaryTreeNode<T>> action)
        {
            if (current == null)
            {
                return;
            }
            this.InorderTraversal(current.Left, action);
            action(current);
            this.InorderTraversal(current.Right, action);
        }
        
        /// <summary>
        /// Postorder full tree traverse
        /// </summary>
        /// <param name="action">Action to execute for each node</param>
        public void PostorderTraversal(Action<BinaryTreeNode<T>> action)
        {

            this.PostorderTraversal(Root, action);
        }
        
        /// <summary>
        /// Postorder custom tree traverse
        /// </summary>
        /// <param name="current">Node to start travese from</param>
        /// <param name="action">Action to execute for each node</param>
        public void PostorderTraversal(BinaryTreeNode<T> current, Action<BinaryTreeNode<T>> action)
        {
            if (current == null)
            {
                return;
            }
            this.PostorderTraversal(current.Left, action);
            this.PostorderTraversal(current.Right, action);
            action(current);
        }
    }
}