using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculus
{
    public abstract class expr {
    }
    public class Constant : expr { public double Value; public Constant(double v) { Value = v; } }
    public class Variable : expr { public string Identifier; public Variable(string i) { Identifier = i; } }
    public class Add : expr { public expr left; public expr right; public Add(expr l, expr r) { left = l; right = r; } }
    public class Divide : expr { public expr left; public expr right; public Divide(expr l, expr r) { left = l; right = r; } }
    public class Multiply : expr { public expr left; public expr right; public Multiply(expr l, expr r) { left = l; right = r; } }
    public class Power : expr { public expr Base; public expr Expo; public Power(expr b, expr e) { Base = b; Expo = e; } }
    public class FunctionCall : expr { 
        public string Name;
        public expr Function;
        public List<Variable> variables;
        public FunctionCall(string n, expr f) { Name = n; Function = f; FindVars(f); }
        public FunctionCall(string n, expr f, List<Variable> vars) {
            Name = n; Function = f; variables = vars;
        }
        private void FindVars(expr express)
        {
            switch (express)
            {
                case Add a:
                    FindVars(a.left);
                    FindVars(a.right);
                    break;
                case Multiply m:
                    FindVars(m.left);
                    FindVars(m.right);
                    break;
                case Divide d:
                    FindVars(d.left);
                    FindVars(d.right);
                    break;
                case Power p:
                    FindVars(p.Expo);
                    FindVars(p.Base);
                    break;
                case Variable v:
                    variables.Add(v);
                    break;
                case FunctionCall f: FindVars(f.Function); break;

            }
        }
    
    }
    public class Derivative : expr { public string variable; public string wrt; public Derivative(string vari, string w) {variable =vari; wrt = w; } }
    public class Mtrx : expr { public string Identifier; public Matrix mat; public Mtrx(string id, Matrix M) { Identifier = id; mat = M; } }
    public class Vec : expr { public string Identifier; Vectors vec; public Vec(string id, Vectors vectors) { Identifier = id; vec = vectors; } }
    public class ParametricFunction : expr
    {
        public string Identifier;public expr X;public expr Y; public Variable V; public ParametricFunction(string id, expr fx, expr fy, Variable var)
        {
            Identifier = id;
            X = fx;
            Y = fy;
            V = var;
        }
    }
    public class Parser
    {
        private string _text;
        private int _pos = 0;

        public Parser(string text)
        {
            _text = text;
        }

        private void Advance() => _pos++;
        private char Current => _pos < _text.Length ? _text[_pos] : '\0';
        private void SkipWhitespace()
        {
            while (char.IsWhiteSpace(Current)) Advance();
        }
        public expr ParseExpr() => ParseAddSub();
        private expr ParseAddSub()
        {
            expr node = ParseMulDiv();
            SkipWhitespace();
            while (Current == '-' || Current == '+')
            {
                char oper = Current;
                Advance();
                expr right = ParseMulDiv();
                if (oper == '-')
                {
                    node = new Add(node, new Multiply(new Constant(-1), right));
                }
                else
                {
                    node = new Add(node, right);
                }
            }
            return node;
        }
        private expr ParseMulDiv()
        {
            expr node = ParsePower();
            SkipWhitespace();
            while (Current == '*' || Current == '/')
            {
                char oper = Current;
                Advance();
                expr right = ParsePower();
                if (oper == '*')
                {
                    node = new Multiply(node, right);
                }
                else if (oper == '/')
                {
                    node = new Divide(node, right);
                }
            }
            return node;
        }
        private expr ParsePower()
        {
            expr node = ParsePrimary();
            SkipWhitespace();
            if (Current == '^')
            {
                Advance();
                expr expo = ParsePrimary();
                node = new Power(node, expo);
            }
            return node;
        }
        private expr ParsePrimary()
        {
            SkipWhitespace();
            if (char.IsDigit(Current))
            {
                return ParseNumber();
            }
            else if (char.IsLetter(Current))
            {
                string name = ParseIdentifier();
                if (Current == '(')
                {
                    Advance();
                    expr arg = ParseExpr();
                    Advance();
                    return new FunctionCall(name, arg);
                }
                else
                {
                    return new Variable(name);
                }
            }
            else if (Current == '(')
            {
                Advance();
                expr node = ParseExpr();
                Advance();
                return node;
            }
            else
            {
                throw new Exception("Unexpected character");
            }
        }
        private expr ParseNumber()
        {
            var sb = new StringBuilder();
            while (char.IsDigit(Current) || Current == '.')
            {
                sb.Append(Current);
                Advance();
            }
            return new Constant(double.Parse(sb.ToString()));
        }
        private string ParseIdentifier()
        {
            var sb = new StringBuilder();
            while (char.IsLetterOrDigit(Current))
            {
                sb.Append(Current);
                Advance();
            }
            return sb.ToString();
        }
    }
}
