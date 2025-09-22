using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculus
{
    public static class SingleVarCalc
    {
        static public expr SecondDerivative(expr express, Variable wrt)
        {
            return Differentiate(Differentiate(express, wrt), wrt);  
        }
        static public Vectors Differentiate(Vectors vec, Variable wrt)
        {
            Vectors newvec = new Vectors(vec.shape);
            for (int i = 0; i < vec.Data.Count(); i++)
            {
                newvec.Data[i] = Differentiate(vec.Data[i], wrt);
            }
            return newvec;
        }
        public static expr Differentiate(expr expression, Variable wrt)
        {
            switch (expression)
            {
                case Constant c:
                    return new Constant(0);
                case Variable v:
                    if (v.Identifier == wrt.Identifier)
                        return new Constant(1);
                    else
                        return new Derivative(v.Identifier, wrt.Identifier);
                case Add a:
                    return new Add(
                        Differentiate(a.left, wrt),
                        Differentiate(a.right, wrt)
                    );
                case Multiply m: //Uses product rule
                    return new Add(
                        new Multiply(Differentiate(m.left, wrt), m.right),
                        new Multiply(Differentiate(m.right, wrt), m.left)
                        );
                case Power p:
                    if (p.Base is Variable V && p.Expo is Constant exp)
                    {
                        return new Multiply(
                            new Constant(exp.Value),
                            new Power(V, new Constant(exp.Value - 1))
                            );

                    }
                    else
                    {
                        throw new Exception("Power cannot be differentiated");
                    }
                case FunctionCall f:
                    return DifferentiateFunction(f, wrt);

                default:
                    throw new Exception("Unknown expression type");
            }

        }
        public static expr DifferentiateFunction(FunctionCall f, Variable wrt)
        {
            expr inner = f.Function;
            expr innerDeriv = Differentiate(inner, wrt);

            switch (f.Name.ToLower())
            {
                case "sin":
                    return new Multiply(new FunctionCall("Cos", inner), innerDeriv);
                case "cos":
                    return new Multiply(new Constant(-1), new Multiply(new FunctionCall("Sin", inner), innerDeriv));
                case "exp":
                    return new Multiply(f, innerDeriv);
                case "ln":
                    return new Multiply(new Divide(new Constant(1), inner), innerDeriv);
                default:
                    throw new Exception("Function not defined");
            }
        }
        public static expr Simplify(expr expres)
        {
            switch (expres)
            {
                case Constant c:
                    return c;

                case Variable v:
                    return v;

                case Derivative d:
                    return d;

                case Add a:
                    expr aleft = Simplify(a.left);
                    expr aright = Simplify(a.right);

                    if (aleft is Constant l && l.Value == 0) return aright;
                    if (aright is Constant r && r.Value == 0) return aleft;
                    if (aright is Constant rc && aleft is Constant lc) return new Constant(rc.Value + lc.Value);

                     
                    return new Add(aleft, aright);

                case Multiply m:
                    expr mleft = Simplify(m.left);
                    expr mright = Simplify(m.right);

                    if ((mleft is Constant ml && ml.Value == 0) || mright is Constant mr && mr.Value == 0) return new Constant(0);
                    if (mleft is Constant mll && mright is Constant mrr) return new Constant(mrr.Value * mll.Value);
                    if (mleft is Constant mlll && mlll.Value == 1) return mright;
                    if (mright is Constant mrrr && mrrr.Value == 1) return mleft;

                    if (mleft is Constant c1 && mright is Multiply m2 && m2.left is Constant c2)
                    {
                        return new Multiply(new Constant(c1.Value * c2.Value), m2.right);
                    }
                    if (mright is Constant r3 && mleft is Multiply m1 && m1.left is Constant l3)
                    {
                        return new Multiply(new Constant(r3.Value * l3.Value), m1.right);
                    }
                    return new Multiply(m.left, m.right);

                case Power p:
                    expr baseExpr = Simplify(p.Base);
                    expr expo = Simplify(p.Expo);

                    if (expo is Constant e)
                    {
                        
                        if (e.Value == 0) return new Constant(1);
                        if (e.Value == 1) return Simplify(baseExpr);

                        
                        if (baseExpr is Constant bc)
                            return new Constant(Math.Pow(bc.Value, e.Value));
                    }

                    return new Power(baseExpr, expo);

                case FunctionCall f:
                    return new FunctionCall(f.Name, Simplify(f.Function));

                default:
                    return expres;
                 
                 
            }

        }
        public static double EvaluateExpression(double x, expr expression)
        {
            switch (expression)
            {
                case Constant c:
                    return c.Value;
                case Variable v:
                    return x;
                case Power p:
                    return Math.Pow(EvaluateExpression(x,p.Base), EvaluateExpression(x,p.Expo));
                case Add a:
                    return EvaluateExpression(x,a.left) + EvaluateExpression(x,a.right);
                case Divide d:
                    return EvaluateExpression(x,d.left) / EvaluateExpression(x,d.right);
                case Multiply m:
                    return EvaluateExpression(x,m.left) * EvaluateExpression(x,m.right);
                case FunctionCall f:
                    if(f.Name.ToLower() == "sin")
                    {
                        return Math.Sin(EvaluateExpression(x,f.Function));
                    }
                    else if (f.Name.ToLower() == "cos")
                    {
                        return Math.Cos(EvaluateExpression(x, f.Function));
                    }
                    else if(f.Name.ToLower() == "ln")
                    {
                        return Math.Log(EvaluateExpression(x,f.Function));
                    }
                    else if (f.Name.ToLower() == "tan")
                    {
                        return Math.Tan(EvaluateExpression(x,f.Function));
                    }
                    else if (f.Name.ToLower() == "exp")
                    {
                        return Math.Exp(EvaluateExpression(x,f.Function));
                    }
                        break;
                default:
                    throw new ArgumentException("Function not recognised");
            }
            throw new Exception("Something went wrong");
        }
        public static double FindTurningPoint(expr expression, Variable v, double x, int ttl=0)
        {
            const int maxloop = 50;
            if (ttl >= maxloop)
            {
                return x;
            }
            else
            {
                expr d1 = Differentiate(expression, v);
                expr d2 = SecondDerivative(expression, v);

                double newx = x - EvaluateExpression(x,d1)/EvaluateExpression(x,d2);
                return FindTurningPoint(expression,v, newx, ttl+1);
            }

        }
        public static expr IndefiniteIntegral(expr express, Variable wrt)
        {
            switch (express)
            {
                case Constant c:
                    return Simplify(new Multiply(c, wrt));

                case Variable v:
                    if (v.Identifier == wrt.Identifier)
                    {
                        return new Divide(new Power(v, new Constant(2)), new Constant(2));
                    }
                    else
                    {
                        return new Multiply(v, wrt);
                    }
                case Add a:
                    return new Add(
                        IndefiniteIntegral(a.left, wrt), IndefiniteIntegral(a.right, wrt)
                        );
                case Multiply m:
                    if (m.left == Differentiate(m.right, wrt))
                    {
                        return m.right;
                    }
                    else if (m.right == Differentiate(m.left, wrt))
                    {
                        return m.left;
                    }
                    else if (m.left is Constant cl)
                    {
                        return new Multiply(cl, IndefiniteIntegral(m.right, wrt));
                    }
                    if (m.right is Constant cr)
                    {
                        return new Multiply(cr, IndefiniteIntegral(m.left, wrt));
                    }
                    throw new Exception("General product integration not implemented");
                case Divide d:
                    if(d.left == Differentiate(d.right, wrt))
                    {

                        return new FunctionCall("ln", d.right);
                    }
                    else
                    {
                        throw new Exception("Expression cannot be integrated algebraically");
                    }
                case Power p:
                    if (p.Base is Variable v2 && v2.Identifier == wrt.Identifier && p.Expo is Constant exp)
                    {
                        
                        if (exp.Value == -1)
                        {
                            return new FunctionCall("ln", v2);
                        }
                        else
                        {
                            return new Divide(
                                new Power(v2, new Constant(exp.Value + 1)),
                                new Constant(exp.Value + 1)
                            );
                        }
                    }
                    throw new Exception("Expression cannot be integrated algebraically");
                default:
                    throw new Exception("Expression cannot be integrated algebraicallly");



            }
        }
        public static double DefiniteIntegral(expr express, Variable wrt, double lower, double upper)
        {
            try
            {
                return EvaluateExpression(upper, IndefiniteIntegral(express, wrt)) - EvaluateExpression(lower, IndefiniteIntegral(express, wrt));
            }
            catch
            {
                return TrapRule(express, wrt, lower, upper);
            }
        }
        private static double TrapRule(expr express, Variable wrt, double lower, double upper, int n = 10)
        {
            double h = (upper - lower) / n;
            double sum = EvaluateExpression(lower, express) + EvaluateExpression(upper, express);
            for (int i = 1; i < n; i++)
            {
                sum += 2 * (EvaluateExpression(i * h, express));
            }
            sum *= (0.5 * h);
            return sum;
                
        }
    }
    public static class MultiVarCalc
    {
        private static expr PartialDeriv(expr express, Variable wrt)
        {
            switch(express) {
                case Constant c:
                    return new Constant(0);
                case Variable v:
                    if (v.Identifier == wrt.Identifier)
                        return new Constant(1.0);
                    else
                        return new Constant(0.0);
                case Add a:
                    return new Add(
                        PartialDeriv(a.left, wrt),
                        PartialDeriv(a.right, wrt)
                    );
                case Multiply m: //Uses product rule
                    return new Add(
                        new Multiply(PartialDeriv(m.left, wrt), m.right),
                        new Multiply(PartialDeriv(m.right, wrt), m.left)
                        );
                case Power p:
                    if (p.Base is Variable V && p.Expo is Constant exp)
                    {
                        return new Multiply(
                            new Constant(exp.Value),
                            new Power(V, new Constant(exp.Value - 1))
                            );

                    }
                    else
                    {
                        return new Constant(0.0);
                    }
                case FunctionCall f:
                    expr inner = f.Function;
                    expr innerDeriv = PartialDeriv(inner, wrt);

                    switch (f.Name.ToLower())
                    {
                        case "sin":
                            return new Multiply(new FunctionCall("Cos", inner), innerDeriv);
                        case "cos":
                            return new Multiply(new Constant(-1), new Multiply(new FunctionCall("Sin", inner), innerDeriv));
                        case "exp":
                            return new Multiply(f, innerDeriv);
                        case "ln":
                            return new Multiply(new Divide(new Constant(1), inner), innerDeriv);
                        default:
                            throw new Exception("Function not defined");
                    }


                default:
                    throw new Exception("Unknown expression type");
            }

        }
        public static Vectors Gradient(List<Variable> vars, expr expression)
        {
            Vectors grad = new Vectors(vars.Count());
            for (int i = 0; i < vars.Count(); i++)
            {
                grad[i] = PartialDeriv(expression, vars[i]);
            }
            return grad;
        }
        public static Constant Curvature(ParametricFunction f, double point)
        {
            double k = SingleVarCalc.EvaluateExpression(point, SingleVarCalc.Differentiate(f.X, f.V)) * SingleVarCalc.EvaluateExpression(point, SingleVarCalc.SecondDerivative(f.Y, f.V));
            k -= SingleVarCalc.EvaluateExpression(point, SingleVarCalc.Differentiate(f.Y, f.V)) - SingleVarCalc.EvaluateExpression(point,SingleVarCalc.SecondDerivative(f.X, f.V));
            double denominator = Math.Pow(SingleVarCalc.EvaluateExpression(point, SingleVarCalc.Differentiate(f.X, f.V)), 2) + Math.Pow(SingleVarCalc.EvaluateExpression(point, SingleVarCalc.Differentiate(f.Y, f.V)), 2);
            denominator = Math.Pow(denominator, 1.5);
            k = k/denominator;
            return new Constant(k);
        }
        public static Constant DirectionalDerivative(FunctionCall funct, Vectors v) { 
            return Gradient(funct.variables, funct.Function).DotProduct(v);
        }
    }

}
