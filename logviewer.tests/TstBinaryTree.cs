using System.Collections.Generic;
using logviewer.engine.Tree;
using Xunit;

namespace logviewer.tests
{
    public class TstBinaryTree
    {
        private BinaryTree<int> tree = new BinaryTree<int>
        {
            Root = new BinaryTreeNode<int>(1)
            {
                Left = new BinaryTreeNode<int>(2),
                Right = new BinaryTreeNode<int>(3)
            }
        };

        [Fact]
        public void EmptyTree()
        {
            this.tree = new BinaryTree<int>();
            Assert.Null(this.tree.Root);
        }

        [Fact]
        public void InitChilds()
        {
            Assert.NotNull(this.tree.Root);
            Assert.Equal(1, this.tree.Root.Value);
            Assert.Equal(2, this.tree.Root.Left.Value);
            Assert.Equal(3, this.tree.Root.Right.Value);
        }

        [Fact]
        public void PreorderTraverse()
        {
            var traverseResult = new List<int>();
            this.tree.PreorderTraversal(node => traverseResult.Add(node.Value));
            Assert.Equal(3, traverseResult.Count);
            Assert.Equal(1, traverseResult[0]);
            Assert.Equal(2, traverseResult[1]);
            Assert.Equal(3, traverseResult[2]);
        }

        [Fact]
        public void InorderTraverse()
        {
            var traverseResult = new List<int>();
            this.tree.InorderTraversal(node => traverseResult.Add(node.Value));
            Assert.Equal(3, traverseResult.Count);
            Assert.Equal(2, traverseResult[0]);
            Assert.Equal(1, traverseResult[1]);
            Assert.Equal(3, traverseResult[2]);
        }

        [Fact]
        public void PostorderTraverse()
        {
            var traverseResult = new List<int>();
            this.tree.PostorderTraversal(node => traverseResult.Add(node.Value));
            Assert.Equal(3, traverseResult.Count);
            Assert.Equal(2, traverseResult[0]);
            Assert.Equal(3, traverseResult[1]);
            Assert.Equal(1, traverseResult[2]);
        }

        [Fact]
        public void Contains()
        {
            Assert.True(this.tree.Contains(3));
        }

        [Fact]
        public void NotContains()
        {
            Assert.False(this.tree.Contains(4));
        }
        
        [Fact]
        public void Add()
        {
            this.tree.Add(6);
            this.tree.Add(5);
            Assert.True(this.tree.Contains(5));
            Assert.True(this.tree.Contains(6));
        }
        
        [Fact]
        public void Remove()
        {
            this.tree.Add(6);
            this.tree.Add(5);
            this.tree.Remove(5);
            Assert.False(this.tree.Contains(5));
            Assert.True(this.tree.Contains(6));
        }
    }
}