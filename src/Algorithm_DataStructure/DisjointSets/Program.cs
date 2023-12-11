using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq.Expressions;

public class DisjointSets{
    public int FindRoot(int x, int[] parent){
        int x_root = x;
        while (parent[x_root] != -1)
        {
            x_root = parent[x_root];
        }

        return x_root;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="parent"></param>
    /// <returns> 1 - union successfully, 0 - failed</returns>

    public int UnionVertices(int x, int y, int[] parent, int[] rank){
        int x_Root = FindRoot(x,parent);
        int y_Root = FindRoot(y,parent);
        if (x_Root == y_Root)
        {
            return 0;
        }

        if(rank[x_Root] > rank[y_Root]){
            parent[y_Root] = x_Root;
        } else if (rank[x_Root] > rank[y_Root]){
            parent[y_Root] = x_Root; 
        } else {
            rank[x_Root]++;
            parent[y_Root] = x_Root;
        }

        return 1;
    }
}

public class DisjointSets_Main
{
    public static void Main(string[] args)
    {
        DisjointSets disjoint = new DisjointSets();
        int[] rank = new int[6];
        int[] parent = new int[6];
        (int x,int y)[] edges = new (int x,int y)[]{
            (0,1),
            (1,2),
            (1,3),
            (2,4),
            (3,4),
            (2,5)
        };
        Array.Fill(parent,-1);
        Array.Fill(rank,0);

        foreach(var edge in edges)
        {
            if (disjoint.UnionVertices(edge.x,edge.y,parent,rank) == 0)
            {
                System.Console.WriteLine("circle detected!");
                return;
            }
        }

        System.Console.WriteLine("NO circle!");
    }
    // two node in same group that represnt it must contain circle.
    // bool IsGrapCircle(){

    // }
}