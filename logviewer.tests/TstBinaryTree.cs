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
    }
}