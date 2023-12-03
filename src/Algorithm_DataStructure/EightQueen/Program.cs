using System;
using System.Collections.Generic;
using System.Text;

namespace EightQueen
{

    public class Solution {
        // public IList<IList<string>> SolveNQueens(int n) {
        //     bool[,] board = new bool[n,n];
        //     List<IList<string>> result = new List<IList<string>>();
        //     SearchQueen(result,board,0,0);
        //     return result;
        // }

        public int TotalNQueens(int n) {
            bool[,] board = new bool[n,n];
            int count = 0;
            SearchQueen(board,0,0,ref count);
            return count;
        }

        bool IsContaineQueens(bool[,] board,int xPos,int yPos){
            var n = (int)Math.Sqrt(board.Length);
            
            //<---
            for (int x = xPos; x >=0 ; x--)
            {
                if (board[yPos,x]) return true;
            }

            //|
            for (int y = yPos; y >=0 ; y--)
            {
                if (board[y,xPos]) return true;
            }

            // \
            for (int y = yPos,x=xPos; y >=0 && x>=0 ; y--,x--)
            {
                if (board[y,x]) return true;
            }

            // /
            for (int y = yPos,x=xPos; y >=0 && x<n ; y--,x++)
            {
                if (board[y,x]) return true;
            }

            return false;
            
        }

        void SearchQueen(bool[,] board,int xPos,int yPos,ref int count){
            int n = (int)Math.Sqrt(board.Length);
            if(yPos== n){
                count++;
                return;
            }
            
            for (int x = 0; x < n; x++)
            {
                if(!IsContaineQueens(board,x,yPos)){
                    board[yPos,x] = true;
                    SearchQueen(board,x,yPos+1,ref count);
                    board[yPos,x] = false;
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Solution s = new Solution();
            //var r = s.SolveNQueens(4);
            Console.WriteLine("Hello World!");
        }
    }
}
