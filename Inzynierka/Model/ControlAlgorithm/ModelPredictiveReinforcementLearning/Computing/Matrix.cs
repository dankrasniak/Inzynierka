using System;

namespace Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.Computing
{
	/// <summary>
	/// Summary description for Vector.
	/// </summary>
	[Serializable]
	public class Matrix
	{
		private double[,] Numbers; 

		#region Constructors
		public Matrix()
		{
		}

		public Matrix(int rows_nr, int columns_nr)
		{
			Numbers = new double[rows_nr, columns_nr]; 
		}

		public Matrix(double[,] numbers)
		{
			Numbers = new double[numbers.GetLength(0), numbers.GetLength(1)];
			for (int i=0; i<Height; i++)
				for (int j=0; j<Width; j++)
					Numbers[i,j] = numbers[i,j];
		}

		public static void AssureDimensions(ref Matrix m, int rows_nr, int columns_nr)
		{
			if (m==null || m.Height!=rows_nr || m.Width!=columns_nr)
				m = new Matrix(rows_nr, columns_nr); 
		}

		public Matrix Clone()
		{
			Matrix ret = new Matrix(Height, Width); 
			Copy(this, ref ret); 
			return (ret); 
		}

		public static void Copy(Matrix pattern, ref Matrix m)
		{
			int rows_nr = pattern.Height;
			int columns_nr = pattern.Width;
			AssureDimensions(ref m, rows_nr, columns_nr); 
			for (int i=0; i<rows_nr; i++)
				for (int j=0; j<columns_nr; j++)
					m.Numbers[i, j] = pattern.Numbers[i, j]; 
		}

		public static void Copy(double[,] pattern, ref Matrix m)
		{
			int rows_nr = pattern.GetLength(0);
			int columns_nr = pattern.GetLength(1);
			AssureDimensions(ref m, rows_nr, columns_nr); 
			for (int i=0; i<rows_nr; i++)
				for (int j=0; j<columns_nr; j++)
					m.Numbers[i, j] = pattern[i, j]; 
		}

		public static void Copy(Matrix pattern, ref double[,] m)
		{
			int rows_nr = pattern.Height;
			int columns_nr = pattern.Width;
			if (m==null || m.GetLength(0)!=rows_nr || m.GetLength(1)!=columns_nr)
				m = new double[rows_nr, columns_nr];
			for (int i=0; i<rows_nr; i++)
				for (int j=0; j<columns_nr; j++)
					m[i, j] = pattern.Numbers[i, j]; 
		}

		public static Matrix Diagonal(int dim, double val)
		{
			Matrix ret = new Matrix(dim, dim); 
			ret.FillWith(0); 
			for (int i=0; i<dim; i++)
				ret[i,i] = val; 
			return (ret); 
		}

		public static Matrix Diagonal(Vector pattern)
		{
			Matrix ret = new Matrix(pattern.Dimension, pattern.Dimension);
			ret.FillWith(0); 
			for (int i=0; i<pattern.Dimension; i++)
				ret[i,i] = pattern[i];
			return (ret); 
		}
		#endregion

		#region Access
		public double this[int i, int j]
		{
			get { return (Numbers[i, j]); }
			set { Numbers[i, j] = value;  } 
		}

		public int Height
		{
			get 
			{
				if (Numbers==null)
					return (0); 
				return (Numbers.GetLength(0)); 
			}
		}

		public int Width
		{
			get
			{
				if (Numbers==null)
					return (0); 
				return (Numbers.GetLength(1)); 
			}
		}

		public void Insert(int first_row, int first_column, Matrix m)
		{
			for (int i=0; i<m.Height; i++)
				for (int j=0; j<m.Width; j++)
					Numbers[first_row + i, first_column + j] = m[i,j]; 
		}

		public Matrix Submatrix(int first_row, int first_column, int rows_nr, int columns_nr)
		{
			Matrix ret = new Matrix(rows_nr, columns_nr);
			for (int i=0; i<rows_nr; i++)
				for (int j=0; j<columns_nr; j++)
					ret[i, j] = Numbers[first_row+i, first_column+j]; 
			return (ret); 
		}

        public Vector GetRow(int i)
        {
            Vector ret = new Vector(Width);
            for (int j = 0; j < Width; j++)
                ret[j] = Numbers[i, j];
            return ret; 
        }

        public Vector GetColumn(int j)
        {
            Vector ret = new Vector(Height);
            for (int i = 0; i < Height; i++)
                ret[i] = Numbers[i, j];
            return ret; 
        }

        public void CopyRowFrom(int i, Vector v)
        {
            if (v.Dimension!=Width)
                throw new Exception("Matrix.CopyRowFrom()");
            for (int j = 0; j < Width; j++)
                Numbers[i, j] = v[j]; 
        }

        public void CopyColumnFrom(int j, Vector v)
        {
            if (v.Dimension != Height)
                throw new Exception("Matrix.CopyColumnFrom()");
            for (int i = 0; i < Height; i++)
                Numbers[i, j] = v[i]; 
        }

        public void FillWith(double val)
		{
			for (int i=Height-1; i>=0; i--)
				for (int j=Width-1; j>=0; j--)
					Numbers[i,j] = val; 
		}

		public double SumOfSquares
		{
			get 
			{
				double ret=0;
				for (int i=Height-1; i>=0; i--)
					for (int j=Width-1; j>=0; j--)
						ret += Sq(Numbers[i,j]); 
				return (ret); 
			}
		}

		public double NormEuclidean
		{
			get { return Math.Sqrt(SumOfSquares); } 
		}

		public double NormSupremum
		{
			get 
			{
				double ret=0;
				for (int i=Height-1; i>=0; i--)
					for (int j=Width-1; j>=0; j--)
						ret = Math.Max(ret, Math.Abs(Numbers[i,j])); 
				return (ret); 
			}
		}
		#endregion

		#region Arythmetic operators
		public static Matrix operator-(Matrix m)
		{
			int rows_nr = m.Height;
			int columns_nr = m.Width;
			Matrix ret = new Matrix(rows_nr, columns_nr); 
			for (int i=0; i<rows_nr; i++)
				for (int j=0; j<columns_nr; j++)
					ret[i,j] = -m[i,j];
			return (ret); 
		}

		public static Matrix operator+(Matrix m1, Matrix m2)
		{
			int rows_nr = m1.Height;
			int columns_nr = m1.Width;
			if (rows_nr!=m2.Height || columns_nr!=m2.Width)
				throw (new Exception("Matrix.operator+() wrong matrices dimensions")); 
			Matrix ret = new Matrix(rows_nr, columns_nr); 
			for (int i=0; i<rows_nr; i++)
				for (int j=0; j<columns_nr; j++)
					ret[i, j] = m1[i, j] + m2[i, j]; 
			return (ret); 
		}

		public static Matrix operator-(Matrix m1, Matrix m2)
		{
			int rows_nr = m1.Height;
			int columns_nr = m1.Width;
			if (rows_nr!=m2.Height || columns_nr!=m2.Width)
				throw (new Exception("Matrix.operator-() wrong matrices dimensions")); 
			Matrix ret = new Matrix(rows_nr, columns_nr); 
			for (int i=0; i<rows_nr; i++)
				for (int j=0; j<columns_nr; j++)
					ret[i, j] = m1[i, j] - m2[i, j]; 
			return (ret); 
		}

		public static Matrix operator*(Matrix m1, double factor)
		{
			int rows_nr = m1.Height;
			int columns_nr = m1.Width;
			Matrix ret = new Matrix(rows_nr, columns_nr); 
			for (int i=0; i<rows_nr; i++)
				for (int j=0; j<columns_nr; j++)
					ret[i, j] = m1[i, j] * factor; 
			return (ret); 
		}

		public static Matrix operator/(Matrix m1, double divisor)
		{
			int rows_nr = m1.Height;
			int columns_nr = m1.Width;
			Matrix ret = new Matrix(rows_nr, columns_nr); 
			for (int i=0; i<rows_nr; i++)
				for (int j=0; j<columns_nr; j++)
					ret[i, j] = m1[i, j] / divisor; 
			return (ret); 
		}

		public static Matrix operator*(Matrix m1, Matrix m2)
		{
			int dim_i = m1.Height;
			int dim_j = m2.Width;
			int dim_k = m2.Height; 
			if (dim_k != m1.Width)
				throw (new System.Exception("Multiplication of matrices imposible due to wrong dimensions")); 
			
			Matrix ret = new Matrix(dim_i, dim_j); 
			ret.FillWith(0); 
			for (int i=0; i<dim_i; i++)
				for (int j=0; j<dim_j; j++)
					for (int k=0; k<dim_k; k++)
						ret[i, j] += m1[i, k] * m2[k, j]; 
			return (ret); 
		}

		public static void Multiple(Matrix m1, bool T1, Matrix m2, bool T2, ref Matrix mm)
		{
			if (!T1 && !T2)
			{
				int rows_nr = m1.Height; 
				int cols_nr = m2.Width; 
				int inside_dim = m1.Width;
				if (inside_dim!=m2.Height)
					throw new Exception("Wrong matrices dimensions"); 
				Matrix.AssureDimensions(ref mm, rows_nr, cols_nr); 
				for (int i=0; i<rows_nr; i++)
					for (int j=0; j<cols_nr; j++)
					{
						double x=0; 
						for (int k=0; k<inside_dim; k++)
							x += m1[i,k] * m2[k,j]; 
						mm[i,j] = x; 
					}
			}
			else 
				if (!T1 && T2)
			{
				int rows_nr = m1.Height; 
				int cols_nr = m2.Height; 
				int inside_dim = m1.Width;
				if (inside_dim!=m2.Width)
					throw new Exception("Wrong matrices dimensions"); 
				Matrix.AssureDimensions(ref mm, rows_nr, cols_nr); 
				for (int i=0; i<rows_nr; i++)
					for (int j=0; j<cols_nr; j++)
					{
						double x=0; 
						for (int k=0; k<inside_dim; k++)
							x += m1[i,k] * m2[j,k]; 
						mm[i,j] = x; 
					}
			}
			else
				if (T1 && !T2)
			{
				int rows_nr = m1.Width; 
				int cols_nr = m2.Width; 
				int inside_dim = m1.Height;
				if (inside_dim!=m2.Height)
					throw new Exception("Wrong matrices dimensions"); 
				Matrix.AssureDimensions(ref mm, rows_nr, cols_nr); 
				for (int i=0; i<rows_nr; i++)
					for (int j=0; j<cols_nr; j++)
					{
						double x=0; 
						for (int k=0; k<inside_dim; k++)
							x += m1[k,i] * m2[k,j]; 
						mm[i,j] = x; 
					}
			}
			else // (T1 && T2) 
			{
				int rows_nr = m1.Width; 
				int cols_nr = m2.Height; 
				int inside_dim = m1.Height;
				if (inside_dim!=m2.Width)
					throw new Exception("Wrong matrices dimensions"); 
				Matrix.AssureDimensions(ref mm, rows_nr, cols_nr); 
				for (int i=0; i<rows_nr; i++)
					for (int j=0; j<cols_nr; j++)
					{
						double x=0; 
						for (int k=0; k<inside_dim; k++)
							x += m1[k,i] * m2[j,k]; 
						mm[i,j] = x; 
					}
			}
		}

		public Matrix T()
		{
			int dim_i = Height; 
			int dim_j = Width; 
			Matrix ret = new Matrix(dim_j, dim_i);
			for (int i=0; i<dim_i; i++)
				for (int j=0; j<dim_j; j++)
					ret[j,i] = this[i,j]; 
			return (ret); 
		}
		#endregion

		#region Usefull static methods
		public static double Sq(double a)
		{
			return (a*a); 
		}
		#endregion

		#region Operations with vector
		public static Vector operator*(Matrix m, Vector v)
		{
			int ret_dim=m.Height;
			int v_dim=v.Dimension;
			if (v_dim!=m.Width)
				throw (new System.Exception("Multiplication of matrix and vector impossible due to dimensions")); 

			Vector ret = new Vector(ret_dim); 
			ret.FillWith(0); 
			for (int i=0; i<ret_dim; i++)
				for (int j=0; j<v_dim; j++)
					ret[i] += m[i, j] * v[j]; 

			return (ret); 
		}

		public static void Multiple(Matrix m1, bool T, Vector v2, ref Vector vv)
		{
			if (!T)
			{
				int dim = m1.Height;
				int inside_dim = m1.Width;
				if (inside_dim != v2.Dimension)
					throw new Exception("Matrix dimension does not match with vector dimension"); 
				if (vv==null)
					vv = new Vector(dim); 
				else
					vv.Dimension = dim; 
				for (int i=0; i<dim; i++)
				{
					double x = 0; 
					for (int k=0; k<inside_dim; k++)
						x += m1[i,k] * v2[k]; 
					vv[i] = x; 
				}
			}
			else // T==1
			{
				int dim = m1.Width;
				int inside_dim = m1.Height;
				if (inside_dim != v2.Dimension)
					throw new Exception("Matrix dimension does not match with vector dimension"); 
				if (vv==null)
					vv = new Vector(dim); 
				else
					vv.Dimension = dim; 
				for (int i=0; i<dim; i++)
				{
					double x = 0; 
					for (int k=0; k<inside_dim; k++)
						x += m1[k,i] * v2[k]; 
					vv[i] = x; 
				}
			}
		}
		#endregion
	}
}
