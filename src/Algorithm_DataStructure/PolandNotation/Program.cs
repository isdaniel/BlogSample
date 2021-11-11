﻿using System;
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

        private IEnumerable<string> ToSuffixExpression(IEnumerable<string> tokens){
            List<string> result = new List<string>();
            Stack<string> operStack = new Stack<string>();
            foreach (var item in tokens)
            {
                if (Regex.IsMatch(item,"\\d"))
                {
                    result.Add(item);
                }else
                {
                    if(operStack.Count == 0 || item == "(")
                    {
                        operStack.Push(item);
                    }else if(item == ")"){
                        while (operStack.Peek() != "(")
                        {
                            result.Add(operStack.Pop());
                        }
                        operStack.Pop();
                    }
                    else 
                    {
                        while (operStack.Count != 0 && OperPriority(operStack.Peek()) >= OperPriority(item))
                        {
                            result.Add(operStack.Pop());
                        }
                        
                        operStack.Push(item);
                    }
                }
            }

            while (operStack.Count > 0)
            {
                result.Add(operStack.Pop());
            }

            return result;
        }
        public int EvalRPN(IEnumerable<string> tokens) {
            var suffixExpression = ToSuffixExpression(tokens);
            Stack<int> numStack = new Stack<int>();
            foreach (var item in suffixExpression)
            {
                if (Regex.IsMatch(item,"\\d")){
                    numStack.Push(int.Parse(item));
                }else
                {
                    numStack.Push(CalcByOper(item,numStack.Pop(),numStack.Pop()));
                }
            }

            return numStack.Pop();
        }

        private int CalcByOper(string oper,int n1,int n2){
            if (oper == "+")
            {
                return n2 + n1;
            } else if (oper == "-")
            {
                return n2 - n1;
            }else if (oper == "*")
            {
                return n2 * n1;
            }else if (oper == "/")
            {
                return n2 / n1;
            }

            throw new Exception();
        }

        private int OperPriority(string oper){
            if (oper == "*" || oper == "/" )
            {
                return 2;
            }else if (oper == "+" || oper == "-")
            {
                return 1;
            }

            return -1;
        }
    }

    
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

        public IEnumerable<string> ToExpressionList(string formula){
            formula = formula.Replace(" ","");
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
            Solution solution = new Solution();
            Calculator c = new  Calculator();
            System.Console.WriteLine(c.Calc("(5+6) * 5 + 1"));
            System.Console.WriteLine(solution.EvalRPN(c.ToExpressionList("(5+6) * 5+1")));
            Console.WriteLine("Hello World!");
        }
    }
}
