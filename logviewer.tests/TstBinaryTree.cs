using System.Collections.Generic;
using logviewer.engine.Tree;
using Xunit;

namespace logviewer.tests
{
    public class TstBinaryTree
    {
        [Fact]
        public void EmptyTree()
        {
            var tree = new BinaryTree<int>();
            Assert.Null(tree.Root);
        }
        
        [Fact]
        public void InitChilds()
        {
            var tree = new BinaryTree<int>
            {
                Root = new BinaryTreeNode<int>(1, new BinaryTreeNode<int>(2), new BinaryTreeNode<int>(3))
            };
            Assert.NotNull(tree.Root);
            Assert.Equal(1, tree.Root.Value);
            Assert.Equal(2, tree.Root.Left.Value);
            Assert.Equal(3, tree.Root.Right.Value);
        }
        
        [Fact]
        public void PreorderTraverse()
        {
            var tree = new BinaryTree<int>
            {
                Root = new BinaryTreeNode<int>(1)
                {
                    Left = new BinaryTreeNode<int>(2),
                    Right = new BinaryTreeNode<int>(3)
                }
            };
            var traverseResult = new List<int>();
            tree.PreorderTraversal(node => traverseResult.Add(node.Value));
            Assert.Equal(3, traverseResult.Count);
            Assert.Equal(1, traverseResult[0]);
            Assert.Equal(2, traverseResult[1]);
            Assert.Equal(3, traverseResult[2]);
        }
        
        [Fact]
        public void InorderTraverse()
        {
            var tree = new BinaryTree<int>
            {
                Root = new BinaryTreeNode<int>(1)
                {
                    Left = new BinaryTreeNode<int>(2),
                    Right = new BinaryTreeNode<int>(3)
                }
            };
            var traverseResult = new List<int>();
            tree.InorderTraversal(node => traverseResult.Add(node.Value));
            Assert.Equal(3, traverseResult.Count);
            Assert.Equal(2, traverseResult[0]);
            Assert.Equal(1, traverseResult[1]);
            Assert.Equal(3, traverseResult[2]);
        }
        
        [Fact]
        public void PostorderTraverse()
        {
            var tree = new BinaryTree<int>
            {
                Root = new BinaryTreeNode<int>(1)
                {
                    Left = new BinaryTreeNode<int>(2),
                    Right = new BinaryTreeNode<int>(3)
                }
            };
            var traverseResult = new List<int>();
            tree.PostorderTraversal(node => traverseResult.Add(node.Value));
            Assert.Equal(3, traverseResult.Count);
            Assert.Equal(2, traverseResult[0]);
            Assert.Equal(3, traverseResult[1]);
            Assert.Equal(1, traverseResult[2]);
        }
    }
}