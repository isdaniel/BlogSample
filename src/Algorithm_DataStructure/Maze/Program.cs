using System;

namespace Maze
{
    class Program
    {
        //策略 下 => 右 => 左 => 上
        static bool SearchMap(int[,] map,int x,int y){
            if(map[6,6] == 2)
                return true;

            if (map[y,x] == 0)
            {
                map[y,x] = 2;
                if (SearchMap(map,x,y + 1))
                    return true;
                else if (SearchMap(map,x + 1,y))
                    return true;
                else if (SearchMap(map,x - 1,y))
                    return true;
                else if (SearchMap(map,x ,y - 1))
                    return true;
                else{
                    map[y,x] = 3;
                }
            }

            return false;
        }
        
        static void Main(string[] args)
        {
            /*
            1 代表 wall
            2 代表 走過的路
            3 代表死路
            */
            int[,] map = new int[8, 8];
            CreateWall(map);
            
            
            System.Console.WriteLine(SearchMap(map, 1, 1)); ;
            System.Console.WriteLine("===============================");
            PrintMap(map);
            //start at 1,1

        }

        private static void CreateWall(int[,] map)
        {
            for (int i = 0; i < 8; i++)
            {
                map[0, i] = 1;
                map[7, i] = 1;
                map[i, 0] = 1;
                map[i, 7] = 1;
            }
            
            map[2, 1] = 1;
            map[2, 2] = 1;
            map[2, 3] = 1;
            map[2, 4] = 1;
            map[2, 5] = 1;
            //map[2, 6] = 1;
        }

        private static void PrintMap(int[,] map)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    System.Console.Write(map[i, j]);
                }
                System.Console.WriteLine("");
            }
        }
    }
}
