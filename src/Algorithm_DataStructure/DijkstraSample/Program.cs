

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

        Dictionary<string,TreeNode> map = new Dictionary<string, TreeNode>();
        map.Add("A",new TreeNode(){ name = "A" , next = new List<TreeNode>(){
            new TreeNode(){
                val = 5,
                name = "B"
            },new TreeNode(){
                val = 1,
                name = "C"
            }
        }});
        map.Add("B",new TreeNode(){ name = "B" , next = new List<TreeNode>(){
            new TreeNode(){
                val = 5,
                name = "A"
            },new TreeNode(){
                val = 2,
                name = "C"
            },new TreeNode(){
                val = 1,
                name = "D"
            }
        }});
        map.Add("C",new TreeNode(){ name = "C" , next = new List<TreeNode>(){
            new TreeNode(){
                val = 1,
                name = "A"
            },new TreeNode(){
                val = 2,
                name = "B"
            },new TreeNode(){
                val = 4,
                name = "D"
            },new TreeNode(){
                val = 8,
                name = "E"
            }
        }});
        map.Add("D",new TreeNode(){ name = "D" , next = new List<TreeNode>(){
            new TreeNode(){
                val = 1,
                name = "B"
            },new TreeNode(){
                val = 4,
                name = "C"
            },new TreeNode(){
                val = 3,
                name = "E"
            },new TreeNode(){
                val = 6,
                name = "F"
            }
        }});
        map.Add("E",new TreeNode(){ name = "E" , next = new List<TreeNode>(){
            new TreeNode(){
                val = 8,
                name = "C"
            },new TreeNode(){
                val = 3,
                name = "D"
            }
        }});
        map.Add("F",new TreeNode(){ name = "F" , next = new List<TreeNode>(){
            new TreeNode(){
                val = 6,
                name = "D"
            }
        }});
        
        //BFS(map);
        var res = Dijkstra(map,"A");
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