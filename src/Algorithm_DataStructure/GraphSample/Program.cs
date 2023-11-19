using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphSample
{
    public class Node{
        public Node Next { get; set; }

        public string Value { get; set; }
    }
    //show Graph
    public class Graph{
        public Dictionary<string,Node> Vertexs{get; private set;}
        //public Dictionary<string,Node> Edges{get;}
        public int Size { get; }
        public Graph(int n)
        {
            this.Size = n;
            Vertexs = new Dictionary<string,Node>();
        }

        public bool InsertVertex(string vertex){
            return Vertexs.TryAdd(vertex,null);
        }

        public void InsertEdge(string gFrom,string gTo){
            if (Vertexs[gFrom] == null)
            {
                Vertexs[gFrom] = new Node(){
                    Value = gTo
                };
            } else {

                Node cur = Vertexs[gFrom];
                while (cur.Next != null)
                {
                    cur = cur.Next;
                }

                cur.Next = new Node(){
                    Value = gTo
                };
                
            }
        }

        public Node GetVertexNode(string vertex){
            return Vertexs[vertex];
        }

        public void ShowGraph(){
            foreach (var vertex in Vertexs)
            {
                Console.Write(vertex.Key);
                Node cur = vertex.Value;
                while (cur != null)
                {
                    Console.Write($"->{cur.Value}");
                    cur = cur.Next;
                }
                System.Console.WriteLine();
            }
        }
    }

    public class DFS{
        private readonly Graph _graph;
        public DFS(Graph graph)
        {
            this._graph = graph;
            
        }

        public void Scan(string vertex){
            var visited = _graph.Vertexs.ToDictionary(x=>x.Key,v=> false);
            Scan(vertex,visited);
        }

        private void Scan(string vertex,Dictionary<string,bool> visited){
            System.Console.WriteLine(vertex);
            var node = _graph.GetVertexNode(vertex);
            visited[vertex] = true;
            while (node != null)
            {
                if (!visited[node.Value])
                {
                    visited[node.Value] = true;
                    Scan(node.Value,visited);
                }
                node = node.Next;
            }
        }
    }
    public class BFS{
        public Graph Graph { get; }
        public BFS(Graph graph)
        {
            this.Graph = graph;
            
        }
        public void Scan(string vertex){
            var visited = Graph.Vertexs.ToDictionary(x=>x.Key,v=> false);
            Queue<string> _queue = new Queue<string>();

            visited[vertex] = true;
            _queue.Enqueue(vertex);

            while (_queue.Count > 0)
            {
                var vtx = _queue.Dequeue();
                System.Console.WriteLine($"{vtx}");
                var node = Graph.GetVertexNode(vtx);
                while (node != null)
                {
                    if (!visited[node.Value])
                    {
                        _queue.Enqueue(node.Value);
                    }
                    visited[node.Value] = true;
                    node = node.Next;
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Graph graph= new Graph(5);
            graph.InsertVertex("A");
            graph.InsertVertex("B");
            graph.InsertVertex("C");
            graph.InsertVertex("D");
            
            graph.InsertEdge("A","B");
            graph.InsertEdge("A","C");
            graph.InsertEdge("B","C");
            graph.InsertEdge("C","A");
            graph.InsertEdge("C","D");
            graph.InsertEdge("D","D");
            BFS bfs = new BFS(graph);
            //bfs.Scan("C");
            DFS dfs = new DFS(graph);
            dfs.Scan("B");
        }
    }
}
