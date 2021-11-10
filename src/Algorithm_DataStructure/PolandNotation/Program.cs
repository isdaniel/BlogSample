using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PolandNotation
{


    /*
    
    Input: tokens = ["10","6","9","3","+","-11","*","/","*","17","+","5","+"]
    Output: 22
    Explanation: ((10 * (6 / ((9 + 3) * -11))) + 17) + 5
    = ((10 * (6 / (12 * -11))) + 17) + 5
    = ((10 * (6 / -132)) + 17) + 5
    = ((10 * 0) + 17) + 5
    = (0 + 17) + 5
    = 17 + 5
    = 22
    */
    public class Solution {
        string[] _opres = new string[]{"+","-","*","/"};
        public int EvalRPN(string[] tokens) {
            Stack<int> numStack = new Stack<int>();
            Stack<string> operStack = new Stack<string>();

            foreach (var item in tokens)
            {
                if (_opres.Any(x=> x == item))
                {
                    var num1 = numStack.Pop();
                    var num2 = numStack.Pop();
                    var res = CalcByOper(item,num2,num1);
                    numStack.Push(res);
                }else
                {
                    numStack.Push(int.Parse(item));
                }
            }

            return numStack.Pop();
        }

        private int CalcByOper(string oper,int n1,int n2){
            if (oper == "+")
            {
                return n1 + n2;
            } else if (oper == "-")
            {
                return n1 - n2;
            }else if (oper == "*")
            {
                return n1 * n2;
            }else if (oper == "/")
            {
                return n1 / n2;
            }

            throw new Exception();
        }
    }
    // public class Calculator{
    //     public int Calc(IEnumerable<string> suffixList){
    //         Stack<string> numStack = new Stack<string>();

    //         foreach (var item in suffixList)
    //         {
    //             if (Regex.IsMatch(item,"\\d"))
    //             {
    //                 numStack.Push(item);
    //             }else{
                    
    //                 int num1 = int.Parse(numStack.Pop());
    //                 int num2 = int.Parse(numStack.Pop());
    //                 numStack.Push(CalcByOper(item,num2,num1).ToString());
    //             }
    //         }

    //         return int.Parse(numStack.Pop());
    //     }

    //     private int CalcByOper(string oper,int n1,int n2){
    //         if (oper == "+")
    //         {
    //             return n1 + n2;
    //         } else if (oper == "-")
    //         {
    //             return n1 - n2;
    //         }else if (oper == "x" || oper == "*")
    //         {
    //             return n1 * n2;
    //         }else if (oper == "/")
    //         {
    //             return n1 / n2;
    //         }

    //         throw new Exception("不支援此操做符號");
    //     }
    // }
    
    public class Calculator{
        string[] _opres = new string[]{"+","-","x","*","/","(",")"};
        Stack<string> numStack = new Stack<string>();
        Stack<string> operStack = new Stack<string>();
        public int Calc(string formula){
            var formulaArray = ToExpressionList(formula);
            foreach (var item in formulaArray)
            {
                if (IsOper(item))
                {
                    if (IsCalcOper(item))
                    {
                        CalcNumberStack();
                    }

                    if (item == ")")
                    {
                        while (operStack.Peek() != "(")
                        {
                            CalcNumberStack();
                        }
                        operStack.Pop();
                    }
                    else
                    {
                        operStack.Push(item);
                    }

                }
                else
                {
                    numStack.Push(item);
                }
            }

            while (operStack.Count > 0)
            {
                CalcNumberStack();
            }

            return int.Parse(numStack.Pop());
        }

        private bool IsCalcOper(string item)
        {
            return operStack.Count != 0 &&
                    OperPriority(item) <= OperPriority(operStack.Peek()) &&
                    item != "(" &&
                    item != ")";
        }

        private void CalcNumberStack()
        {
            var oper = operStack.Pop();
            var num1 = int.Parse(numStack.Pop());
            var num2 = int.Parse(numStack.Pop());
            var res = CalcByOper(oper, num2, num1);
            numStack.Push(res.ToString());
        }

        private int CalcByOper(string oper,int n1,int n2){
            if (oper == "+")
            {
                return n1 + n2;
            } else if (oper == "-")
            {
                return n1 - n2;
            }else if (oper == "x" || oper == "*")
            {
                return n1 * n2;
            }else if (oper == "/")
            {
                return n1 / n2;
            }

            throw new Exception("不支援此操做符號");
        }
        bool IsOper(string item)
        {
            return _opres.Any(x => item == x);
        }
        
        int OperPriority(string oper){
            if (oper == "*" || oper == "/" ||oper == "x")
            {
                return 2;
            }else if (oper == "+" || oper == "-")
            {
                return 1;
            }

            return -1;
        }

        IEnumerable<string> ToExpressionList(string formula){
            int index = 0;
            List<string> result = new List<string>();
            StringBuilder num = new StringBuilder();
            do
            {
                if (!IsNumber(formula[index]))
                {
                    result.Add(formula[index].ToString());
                    index++;
                }else
                {
                    while (index < formula.Length && IsNumber(formula[index]))
                    {
                        num.Append(formula[index]);
                        index++;
                    }
                    result.Add(num.ToString());
                    num.Clear();
                }
                
            } while (index < formula.Length);

            return result;
        }
        static bool IsNumber(char c){
            return c >= '0' && c <= '9';
        }

    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Calculator c = new  Calculator();
            System.Console.WriteLine(c.Calc("(5+6)*5+1"));
            Console.WriteLine("Hello World!");
        }
    }
}
