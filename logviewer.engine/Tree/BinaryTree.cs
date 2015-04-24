using System;
using System.Collections.Generic;

namespace logviewer.engine.Tree
{
    /// <summary>
    /// Represents binary tree abstraction
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinaryTree<T>
    {
        private readonly IComparer<T> comparer;

        /// <summary>
        /// Creates new empty tree with default comparere
        /// </summary>
        public BinaryTree() : this(Comparer<T>.Default)
        {
        }
        
        /// <summary>
        /// Creates new empty tree with comparer specified
        /// </summary>
        /// <param name="comparer">Node value comparer</param>
        public BinaryTree(IComparer<T> comparer)
        {
            this.comparer = comparer;
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

        /// <summary>
        /// Checks whether the tree contains value specified
        /// </summary>
        /// <param name="data">Value to find</param>
        /// <returns></returns>
        public bool Contains(T data)
        {
            // search the tree for a node that contains data
            var current = this.Root;
            while (current != null)
            {
                int result = comparer.Compare(current.Value, data);
                if (result == 0)
                {
                    return true;
                }
                if (result > 0)
                {
                    // current.Value > data, search current's left subtree
                    current = current.Left;
                }
                else if (result < 0)
                {
                    // current.Value < data, search current's right subtree
                    current = current.Right;
                }
            }

            return false;
        }
    }
}