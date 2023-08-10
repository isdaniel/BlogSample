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
            
            Move(3,'A','B','C');
        }

        static void Move(int level,char source,char auxiliary,char target){
            if(level == 1){
                MoveAction(level,source,target);
                return;
            }
            Move(level - 1,source,target,auxiliary);
            MoveAction(level,source,target);
            Move(level - 1,auxiliary,source,target);
        }

        static private void MoveAction(int level,char source,char target){
            System.Console.WriteLine($"第 {level} 層, 來自{source} 搬移到 {target}");
        }
    }
}
