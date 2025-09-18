using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculus
{
    public class Matrix
    {
        public int[] shape;
        public Constant[,] Data;

        public Matrix(int n, int m)
        {
            Data = new Constant[n, m];
            shape = new int[2];
            shape[0] = n;
            shape[1] = m;

        }
        public Constant this[int i, int j]
        {
            get => Data[i, j];
            set => Data[i, j] = value;
        }
        public Matrix Transpose()
        {
            Matrix mat = new Matrix(shape[1], shape[0]);
            for (int i = 0; i < shape[0]; i++)
            {
                for (int j = 0; j < shape[1]; j++)
                {
                    mat[j, i] = this[i, j];
                }
            }
            return mat;
        }
        public Matrix MatrixMultiply(Matrix mat)
        {
            Matrix result = new Matrix(shape[0], mat.shape[1]);
            for (int i = 0; i < shape[0]; i++)
            {
                Constant[] row = this.GetRow(i);
                for (int j = 0; j < mat.shape[1]; j++)
                {
                    Constant[] col = mat.GetColumn(j);
                    result[i, j] = DotProduct(row, col);
                }
            }

            return result;
        }

        public Matrix AddMatrix(Matrix mat)
        {
            if (mat.shape != shape)
            {
                throw new ArgumentException("Matrix shapes must be the same");
            }
            else
            {
                Matrix Output = new Matrix(shape[0], shape[1]);
                Constant[,] Result = new Constant[shape[0], shape[1]];
                for (int i = 0; i < shape[0]; i++)
                {
                    for (int ii = 0; ii < shape[1]; ii++)
                    {
                        Result[i, ii] = new Constant(mat[i, ii].Value + this[i, ii].Value);
                    }
                }
                Output.SetValues(Result);
                return Output;
            }

        }
        public void SetValues(Constant[,] vals)
        {
            if (Data.Length != vals.Length)
            {
                throw new ArgumentException("Matrix shape and data shape do not match");
            }
            else
            {
                Data = vals;
            }
        }
        public Constant[] GetRow(int rowIndex)
        {
            int cols = Data.GetLength(1);
            Constant[] row = new Constant[cols];
            for (int j = 0; j < cols; j++)
            {
                row[j] = Data[rowIndex, j];
            }
            return row;
        }
        public Constant[] GetColumn(int colIndex)
        {
            int rows = Data.GetLength(0);
            Constant[] col = new Constant[rows];
            for (int j = 0; j < rows; j++)
            {
                col[j] = Data[j, colIndex];
            }
            return col;
        }
        public Vectors Multiply(Vectors vec)
        {
            Vectors output = new Vectors(vec.shape);
            if (vec.Data is Constant[] vd)
            {
                for (int i = 0; i < vec.shape; i++)
                {
                    Constant[] row = GetRow(i);
                    output[i] = DotProduct(vd, row);
                }
            }
            return output;
        }
        private Constant DotProduct(Constant[] a, Constant[] b)
        {
            if (a.Length != b.Length)
            {
                throw new ArgumentException("Vectors must be the same length for dot product.");
            }

            double result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result += a[i].Value * b[i].Value;
            }

            return new Constant(result);
        }
    }
    public class Vectors
    {
        public expr[] Data;
        public int shape;
        public Vectors(int dimensions)
        {
            Data = new Constant[dimensions];
            shape = dimensions;
        }
        public expr this[int i]
        {
            get => Data[i];
            set => Data[i] = value;
        }

        public Constant DotProduct(Vectors vector)
        {
            if (Data is Constant[] d && vector.Data is Constant[] vd)
            {
                CheckSize(vector);
                double result = 0;
                for (int i = 0; i < Data.Length; i++)
                {
                    result += d[i].Value * vd[i].Value;
                }
                return new Constant(result);
            }
            else
            {
                throw new Exception("Expected a vector of constants");
            }
        }
        public Constant Magnitude() //Returns the magnitude of the vector
        {
            if (Data is Constant[] d)
            {
                double result = 0;
                for (int i = 0; i < Data.Length; i++)
                {
                    result += d[i].Value * d[i].Value;
                }

                return new Constant((double)Math.Sqrt(result));
            }
            else
            {
                throw new Exception("Expected a vector of constants");
            }
        }
        public Constant CosineSimilarity(Vectors vector) //Returns the value of cos(x), where x is the angle between the two vectors
        {
            CheckSize(vector);
            return new Constant((DotProduct(vector).Value / (Magnitude().Value * vector.Magnitude().Value)));
        }
        public void SetValue(int index, Constant value) //Sets the value of the vector at the given index
        {
            if (index < 0 || index >= Data.Length)
            {
                throw new IndexOutOfRangeException();
            }
            Data[index] = value;
        }
        public void MultiplyByScalar(int scalar)
        {
            if(Data is Constant[] d)
            for (int i = 0; i < Data.Length; i++)
            {
                d[i].Value *= scalar;
            }
        }
        public void Add(Vectors vector)
        {
            CheckSize(vector);
            if(Data is Constant[] d && vector.Data is Constant[] vd)
            for (int i = 0; i < Data.Length; i++)
            {
                d[i].Value += vd[i].Value;
            }
        }
        public void DivideByScalar(int scalar)
        {
            if (scalar == 0)
            {
                throw new DivideByZeroException();
            }
            if (Data is Constant[] d)
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    d[i].Value /= scalar;
                }
            }

        }
        public void Subtract(Vectors vector)
        {
            CheckSize(vector);
            if (Data is Constant[] d && vector.Data is Constant[] vd)
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    d[i].Value -= vd[i].Value;
                }
            }
        }
        public void Subtract(int scalar)
        {
            if (Data is Constant[] d)
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    d[i].Value -= scalar;
                }
            }
        }
        private void CheckSize(Vectors vector)
        {
            if (Data.Length != vector.Data.Length)
            {
                throw new ArgumentException("Vectors must be of the same length");
            }
        }
    }
}
