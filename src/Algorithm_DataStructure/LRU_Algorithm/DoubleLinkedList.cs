namespace LRU_Algorithm
{
    public class DoubleLinkedList<Tkey,TValue>{
        private Node<Tkey,TValue> _header;

        private Node<Tkey,TValue> _tail;
        public DoubleLinkedList()
        {
            _header = new Node<Tkey, TValue>();
            _tail = new Node<Tkey, TValue>();

            _header.Next = _tail;
            _tail.Prev = _header;
        }

        public void AddHeader(Node<Tkey,TValue> node){
            node.Next = _header.Next;
            node.Prev = _header;
            _header.Next.Prev = node;
            _header.Next = node;
        }

        public void RemoveNode(Node<Tkey,TValue> node){
            node.Next.Prev = node.Prev;
            node.Prev.Next = node.Next;
            node.Prev = node.Next = null;
        }

        public Node<Tkey,TValue> GetLastNode(){
            return _tail.Prev;
        }
        
        public void PrintAll(){
            var cur = _header.Next;
            while(cur != _tail){
                System.Console.WriteLine($"key: {cur.Key} value:{cur.Value}");
                cur = cur.Next;
            }
        }
    }
}
