namespace logviewer.engine.Tree
{
    public class BinaryTreeNode<T> : Node<T>
    {
        public BinaryTreeNode()
        {
        }

        public BinaryTreeNode(T data) : base(data, null)
        {
        }

        public BinaryTreeNode(T data, Node<T> left, Node<T> right)
        {
            this.Value = data;
            var children = new NodeList<T>(2);
            children[0] = left;
            children[1] = right;

            this.Neighbors = children;
        }

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