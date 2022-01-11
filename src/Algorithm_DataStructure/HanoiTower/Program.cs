using System;
using System.Collections.Generic;
using System.Linq;

namespace HanoiTower
{
    public class ObjectModel{
        public string Name { get; set; }
        public int Weight { get; set; }
        public int Value { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {  
            

            List<ObjectModel> objects = new List<ObjectModel>(){
                new ObjectModel(){
                    Weight = 1,
                    Value = 2925
                },
                new ObjectModel(){
                    Weight = 4,
                    Value = 2040
                },
                new ObjectModel(){
                    Weight = 3,
                    Value = 3000
                },
                new ObjectModel(){
                    Weight = 2,
                    Value = 2040
                },
                new ObjectModel(){
                    Weight = 5,
                    Value = 2700
                },
                new ObjectModel(){
                    Weight = 5,
                    Value = 1500
                },
                new ObjectModel(){
                    Weight = 5,
                    Value = 1400
                },
                new ObjectModel(){
                    Weight = 5,
                    Value = 2000
                }
            };
            var rrr = objects.OrderBy(x=>x.Value).ToList();
            int backageWeight = 8200; 
            int[,] vTable = new int[objects.Count + 1,backageWeight + 1]; 
            
            //var res = knapsack(objects,backageWeight,objects.Count,vTable);

            for (int i = 1; i < vTable.GetLength(0); i++)
            {
                for (int j = 1; j < vTable.GetLength(1); j++){
                    if (j < objects[i - 1].Weight)
                    {
                        vTable[i,j] = vTable[i- 1,j];
                    }else {
                        vTable[i,j] = Math.Max(vTable[i-1,j],vTable[i-1,j-objects[i - 1].Weight] + objects[i - 1].Value);
                    }
                }
            }

            //Move(4,'A','B','C');
        }
        //?
        static int knapsack(List<ObjectModel> objects,int backageWeight,int n,int[,] vTable){
            if (backageWeight == 0 || n == 0)
                return 0;

            if (backageWeight < objects[n - 1].Weight)
                return vTable[n,backageWeight];


            vTable[n,backageWeight] = Math.Max(
                knapsack(objects,backageWeight,n - 1,vTable),
                knapsack(objects,backageWeight - objects[n - 1].Weight,n - 1,vTable) + objects[n - 1].Value
                );
            return vTable[n,backageWeight];
        }

        static void Move(int num,char a,char b,char c){
            if (num == 1)
            {
                System.Console.WriteLine($"第1個盤 從{a}->{c}");
            } else {
                Move(num - 1,a,c,b);
                System.Console.WriteLine($"第{num}個盤 從{a}->{c}");
                Move(num - 1,b,a,c);
            }
            
        }
    }
}
