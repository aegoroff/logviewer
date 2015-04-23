namespace logviewer.engine.Tree
{
    public class BinaryTree<T>
    {
        public BinaryTree()
        {
            this.Root = null;
        }

        public BinaryTreeNode<T> Root { get; set; }

        public virtual void Clear()
        {
            this.Root = null;
        }
    }
}