using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Calculus
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string,expr> Functions = new Dictionary<string,expr>();
            string input = "3*x^2 + sin(x) ";
            var parser = new Parser(input);
            expr ast = parser.ParseExpr();
            Variable V = new Variable("x");
            
            Console.WriteLine(input);
            Console.WriteLine(SingleVarCalc.EvaluateExpression(1.0, SingleVarCalc.Simplify(SingleVarCalc.Differentiate(ast, V))));
        }
        static void Print(expr e, string indent = "")
        {
            switch (e)
            {
                case Constant c: Console.WriteLine($"{indent}Const({c.Value})"); break;
                case Variable v: Console.WriteLine($"{indent}Var({v.Identifier})"); break;
                case Add a:
                    Console.WriteLine($"{indent}Add");
                    Print(a.left, indent + "  ");
                    Print(a.right, indent + "  ");
                    break;
                case Multiply m:
                    Console.WriteLine($"{indent}Mul");
                    Print(m.left, indent + "  ");
                    Print(m.right, indent + "  ");
                    break;
                case Power p:
                    Console.WriteLine($"{indent}Pow");
                    Print(p.Base, indent + "  ");
                    Print(p.Expo, indent + "  ");
                    break;
                case FunctionCall f:
                    Console.WriteLine($"{indent}Func({f.Name})");
                    PrintEq(f.Function); Console.Write( indent + "  ");
                    break;
            }
        }
        static void PrintEq(expr e)
        {
            switch (e)
            {
                case Constant c:
                    Console.Write(c.Value);
                    break;

                case Variable v:
                    Console.Write(v.Identifier);
                    break;
                case Add a:
                    PrintEq(a.left);
                    Console.Write("+");
                    PrintEq(a.right);
                    break;
                case Multiply m:
                    PrintEq(m.left);
                    Console.Write("*");
                    PrintEq(m.right);
                    break;
                case Divide d:
                    PrintEq(d.left);
                    Console.Write("/");
                    PrintEq(d.right);
                    break;
                case FunctionCall f:
                    Console.Write(f.Name + "(");
                    PrintEq(f.Function);
                    Console.Write(")");
                    break;
                case Power p:
                    if(p.Expo is Constant ce && ce.Value == 1)
                    {
                        PrintEq(p.Base);
                    }
                    else
                    {
                        PrintEq(p.Base);
                        Console.Write("^");
                        PrintEq(p.Expo);
                    }
                    break;

            }
        }
    }
    


    
}