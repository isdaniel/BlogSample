

using System.Data;

internal class Program
{
    //假如 Path 有權重,需要找最小路徑就需要使用 Dijkstra
    private static void Main(string[] args){
        // TreeNode a = new TreeNode(){ name = "A"};
        // TreeNode b = new TreeNode(){ name = "B"};
        // TreeNode c = new TreeNode(){ name = "C"};
        // TreeNode d = new TreeNode(){ name = "D"};
        // TreeNode e = new TreeNode(){ name = "E"};
        // TreeNode f = new TreeNode(){ name = "F"};
        #region map version
        // Dictionary<string,TreeNode> map = new Dictionary<string, TreeNode>();
        // map.Add("A",new TreeNode(){ name = "A" , next = new List<TreeNode>(){
        //     new TreeNode(){
        //         val = 5,
        //         name = "B"
        //     },new TreeNode(){
        //         val = 1,
        //         name = "C"
        //     }
        // }});
        // map.Add("B",new TreeNode(){ name = "B" , next = new List<TreeNode>(){
        //     new TreeNode(){
        //         val = 5,
        //         name = "A"
        //     },new TreeNode(){
        //         val = 2,
        //         name = "C"
        //     },new TreeNode(){
        //         val = 1,
        //         name = "D"
        //     }
        // }});
        // map.Add("C",new TreeNode(){ name = "C" , next = new List<TreeNode>(){
        //     new TreeNode(){
        //         val = 1,
        //         name = "A"
        //     },new TreeNode(){
        //         val = 2,
        //         name = "B"
        //     },new TreeNode(){
        //         val = 4,
        //         name = "D"
        //     },new TreeNode(){
        //         val = 8,
        //         name = "E"
        //     }
        // }});
        // map.Add("D",new TreeNode(){ name = "D" , next = new List<TreeNode>(){
        //     new TreeNode(){
        //         val = 1,
        //         name = "B"
        //     },new TreeNode(){
        //         val = 4,
        //         name = "C"
        //     },new TreeNode(){
        //         val = 3,
        //         name = "E"
        //     },new TreeNode(){
        //         val = 6,
        //         name = "F"
        //     }
        // }});
        // map.Add("E",new TreeNode(){ name = "E" , next = new List<TreeNode>(){
        //     new TreeNode(){
        //         val = 8,
        //         name = "C"
        //     },new TreeNode(){
        //         val = 3,
        //         name = "D"
        //     }
        // }});
        // map.Add("F",new TreeNode(){ name = "F" , next = new List<TreeNode>(){
        //     new TreeNode(){
        //         val = 6,
        //         name = "D"
        //     }
        // }});
        
        // //BFS(map);
        // var res = Dijkstra(map,"A");
        #endregion

        List<List<int>> edges = new List<List<int>>(){
            new List<int>(){0,1,10},
            new List<int>(){0,2,3},
            new List<int>(){1,3,2},
            new List<int>(){2,1,4},
            new List<int>(){2,3,8},
            new List<int>(){2,4,2},
            new List<int>(){3,4,5}
        };

        var res = Dijkstra(5,edges,0);
        foreach (var item in res)
        {
            System.Console.WriteLine($"{item.Key} {item.Value}");
        }

        //[[0,1,10],[0,2,3],[1,3,2],[2,1,4],[2,3,8],[2,4,2],[3,4,5]]
    }

    private static Dictionary<int,int> Dijkstra(int n,List<List<int>> edges,int src){
        
        Dictionary<int,List<(int next,int val)>> nextMap = new Dictionary<int, List<(int next, int val)>>();
        PriorityQueue<int,int> q = new PriorityQueue<int, int>();
        Dictionary<int,int> distance = new Dictionary<int,int>();
        HashSet<int> visited = new HashSet<int>();
        Dictionary<int,int> res = new Dictionary<int,int>();

        for (int i = 0; i < n; i++)
        {
            distance.Add(i,int.MaxValue);
            res.Add(i,-1);
        }

        distance[src] = 0;
        res[src] = 0;

        foreach (var d in edges)
        {
            nextMap.TryAdd(d[0],new List<(int next, int val)>());
            nextMap[d[0]].Add((d[1],d[2]));
        }

        q.Enqueue(src,0);

        while(q.Count > 0){
            int cnt = q.Count;
            for(int i = 0; i < cnt ; i++){
                q.TryDequeue(out int cur,out int val);
                visited.Add(cur);
                if(!nextMap.ContainsKey(cur)){
                    continue;
                }

                foreach (var node in nextMap[cur])
                {
                    if(!visited.Contains(node.next)){
                        if (val + node.val < distance[node.next])
                        {
                            distance[node.next] = val + node.val;
                            q.Enqueue(node.next,val + node.val);
                            res[node.next] = val + node.val;
                        }
                    }
                }
            }
        }

        return res;
    }

    private static Dictionary<string,string> Dijkstra(Dictionary<string,TreeNode> map,string s){
        HashSet<string> isVisited = new HashSet<string>();
        PriorityQueue<TreeNode,int> q = new PriorityQueue<TreeNode,int>();
        q.Enqueue(map[s],0);
        Dictionary<string,string> res = new Dictionary<string,string>();
        Dictionary<string,int> dinstnace = new Dictionary<string,int>();
        foreach(var m in map){
            if(m.Key == s){
                dinstnace.Add(s,0);
            } else {
                dinstnace.Add(m.Key,int.MaxValue);
            }

            res.Add(m.Key,string.Empty);
        }

        while(q.Count > 0){
            int cnt = q.Count;
            for(int i = 0; i < cnt ; i++){
                q.TryDequeue(out var cur, out int dist);
                isVisited.Add(cur.name);
                foreach(var n in cur.next){
                    if(!isVisited.Contains(n.name)){
                        if(dist + n.val < dinstnace[n.name]){
                            q.Enqueue(map[n.name],dist + n.val);
                            dinstnace[n.name] = dist + n.val;
                            res[n.name] = cur.name;
                        }
                    }
                }   
            }
        }

        return res;
    }

    private static void BFS(Dictionary<string,TreeNode> map){
        HashSet<string> isVisited = new HashSet<string>();
        Queue<TreeNode> q = new Queue<TreeNode>();
        q.Enqueue(map["A"]);
        isVisited.Add("A");

        while(q.Count > 0){
            int cnt = q.Count;
            for(int i = 0; i < cnt ; i++){
                var cur = q.Dequeue();
                foreach(var n in cur.next){
                    if(!isVisited.Contains(n.name)){
                        isVisited.Add(n.name);
                        q.Enqueue(map[n.name]);
                        Console.WriteLine(n.name);
                    }
                }   
            }
        }
    }
}

public class TreeNode {
    public string name;

    public int val;
    public List<TreeNode> next;
}