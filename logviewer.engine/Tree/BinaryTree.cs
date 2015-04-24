using System;
using System.Collections.Generic;

namespace logviewer.engine.Tree
{
    /// <summary>
    ///     Represents binary tree abstraction
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinaryTree<T>
    {
        private readonly IComparer<T> comparer;

        /// <summary>
        ///     Creates new empty tree with default comparere
        /// </summary>
        public BinaryTree() : this(Comparer<T>.Default)
        {
        }

        /// <summary>
        ///     Creates new empty tree with comparer specified
        /// </summary>
        /// <param name="comparer">Node value comparer</param>
        public BinaryTree(IComparer<T> comparer)
        {
            this.comparer = comparer;
            this.Root = null;
        }

        /// <summary>
        ///     Gets or sets tree root
        /// </summary>
        public BinaryTreeNode<T> Root { get; set; }

        /// <summary>
        ///     Clears tree
        /// </summary>
        public virtual void Clear()
        {
            this.Root = null;
        }

        /// <summary>
        ///     Preorder full tree traverse
        /// </summary>
        /// <param name="action">Action to execute for each node</param>
        public void PreorderTraversal(Action<BinaryTreeNode<T>> action)
        {
            this.PreorderTraversal(this.Root, action);
        }

        /// <summary>
        ///     Preorder custom tree traverse
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
        ///     Inorder full tree traverse
        /// </summary>
        /// <param name="action">Action to execute for each node</param>
        public void InorderTraversal(Action<BinaryTreeNode<T>> action)
        {
            this.InorderTraversal(this.Root, action);
        }

        /// <summary>
        ///     Inorder custom tree traverse
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
        ///     Postorder full tree traverse
        /// </summary>
        /// <param name="action">Action to execute for each node</param>
        public void PostorderTraversal(Action<BinaryTreeNode<T>> action)
        {
            this.PostorderTraversal(this.Root, action);
        }

        /// <summary>
        ///     Postorder custom tree traverse
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
        ///     Checks whether the tree contains value specified
        /// </summary>
        /// <param name="data">Value to find</param>
        /// <remarks>WARNING: Works correctly only in case of BST (binary search tree) i.e. if tree created using only Add method, not manually</remarks>
        /// <returns></returns>
        public bool Contains(T data)
        {
            var current = this.Root;
            while (current != null)
            {
                var result = this.comparer.Compare(current.Value, data);
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

        /// <summary>
        ///     Adds new node to the tree
        /// </summary>
        /// <param name="data">Node data</param>
        public virtual void Add(T data)
        {
            // create a new Node instance
            var n = new BinaryTreeNode<T>(data);
            int result;

            // now, insert n into the tree
            // trace down the tree until we hit a NULL
            BinaryTreeNode<T> current = this.Root, parent = null;
            while (current != null)
            {
                result = this.comparer.Compare(current.Value, data);
                if (result == 0)
                {
                    // they are equal - attempting to enter a duplicate - do nothing
                    return;
                }
                if (result > 0)
                {
                    // current.Value > data, must add n to current's left subtree
                    parent = current;
                    current = current.Left;
                }
                else if (result < 0)
                {
                    // current.Value < data, must add n to current's right subtree
                    parent = current;
                    current = current.Right;
                }
            }

            // We're ready to add the node!
            if (parent == null)
            {
                // the tree was empty, make n the root
                this.Root = n;
            }
            else
            {
                result = this.comparer.Compare(parent.Value, data);
                if (result > 0)
                {
                    // parent.Value > data, therefore n must be added to the left subtree
                    parent.Left = n;
                }
                else
                {
                    // parent.Value < data, therefore n must be added to the right subtree
                    parent.Right = n;
                }
            }
        }

        /// <summary>
        /// Removes node from the tree
        /// </summary>
        /// <param name="data">Node to remove</param>
        /// <returns>True if removing was successfuls false otherwise</returns>
        public bool Remove(T data)
        {
            // first make sure there exist some items in this tree
            if (this.Root == null)
            {
                return false; // no items to remove
            }

            // Now, try to find data in the tree
            BinaryTreeNode<T> current = this.Root, parent = null;
            int result = this.comparer.Compare(current.Value, data);
            while (result != 0)
            {
                if (result > 0)
                {
                    // current.Value > data, if data exists it's in the left subtree
                    parent = current;
                    current = current.Left;
                }
                else if (result < 0)
                {
                    // current.Value < data, if data exists it's in the right subtree
                    parent = current;
                    current = current.Right;
                }

                // If current == null, then we didn't find the item to remove
                if (current == null)
                {
                    return false;
                }
                result = this.comparer.Compare(current.Value, data);
            }

            // We now need to "rethread" the tree
            // CASE 1: If current has no right child, then current's left child becomes
            //         the node pointed to by the parent
            if (current.Right == null)
            {
                if (parent == null)
                {
                    this.Root = current.Left;
                }
                else
                {
                    result = this.comparer.Compare(parent.Value, current.Value);
                    if (result > 0)
                    {
                        // parent.Value > current.Value, so make current's left child a left child of parent
                        parent.Left = current.Left;
                    }
                    else if (result < 0)
                    {
                        // parent.Value < current.Value, so make current's left child a right child of parent
                        parent.Right = current.Left;
                    }
                }
            }
                // CASE 2: If current's right child has no left child, then current's right child
                //         replaces current in the tree
            else if (current.Right.Left == null)
            {
                current.Right.Left = current.Left;

                if (parent == null)
                {
                    this.Root = current.Right;
                }
                else
                {
                    result = this.comparer.Compare(parent.Value, current.Value);
                    if (result > 0)
                    {
                        // parent.Value > current.Value, so make current's right child a left child of parent
                        parent.Left = current.Right;
                    }
                    else if (result < 0)
                    {
                        // parent.Value < current.Value, so make current's right child a right child of parent
                        parent.Right = current.Right;
                    }
                }
            }
                // CASE 3: If current's right child has a left child, replace current with current's
                //          right child's left-most descendent
            else
            {
                // We first need to find the right node's left-most child
                BinaryTreeNode<T> leftmost = current.Right.Left, lmParent = current.Right;
                while (leftmost.Left != null)
                {
                    lmParent = leftmost;
                    leftmost = leftmost.Left;
                }

                // the parent's left subtree becomes the leftmost's right subtree
                lmParent.Left = leftmost.Right;

                // assign leftmost's left and right to current's left and right children
                leftmost.Left = current.Left;
                leftmost.Right = current.Right;

                if (parent == null)
                {
                    this.Root = leftmost;
                }
                else
                {
                    result = this.comparer.Compare(parent.Value, current.Value);
                    if (result > 0)
                    {
                        // parent.Value > current.Value, so make leftmost a left child of parent
                        parent.Left = leftmost;
                    }
                    else if (result < 0)
                    {
                        // parent.Value < current.Value, so make leftmost a right child of parent
                        parent.Right = leftmost;
                    }
                }
            }

            return true;
        }
    }
}