namespace LRU_Algorithm
{
    public class Node<Tkey,TValue>{
        public TValue Value { get; set; }
        public Tkey Key { get; set; }
        public Node<Tkey,TValue> Prev { get; set; }
        public Node<Tkey,TValue> Next { get; set; }
    }
}
