using System.Collections.ObjectModel;
using System.Linq;

namespace logviewer.engine.Tree
{
    public class NodeList<T> : Collection<Node<T>>
    {
        public NodeList()
        {
        }

        public NodeList(int initialSize)
        {
            for (int i = 0; i < initialSize; i++)
            {
                this.Items.Add(default(Node<T>));
            }
        }

        public Node<T> FindByValue(T value)
        {
            return this.Items.FirstOrDefault(node => node.Value.Equals(value));
        }
    }
}