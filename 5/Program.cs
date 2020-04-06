using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Transactions;

namespace _5
{
    class Program
    {
        static string OutputStringMaker(double[] ResultCoef)
        {
            string s = "";
            int cnt = 0;
            foreach (var elem in ResultCoef)
            {
                s += $"x{++cnt} = {elem}\n";
            }

            return s;
        }

        static string InputStringMaker(double[] input)
        {
            string s = "";
            int cnt = 0;
            foreach (var it in input)
            {
                if (it == input.Last())
                    s += $"{it}*x{++cnt}";
                else
                {
                    s += $"{it}*x{++cnt} + ";
                }
            }

            return s;
        }

        static string InputStringMaker(double[,] a, double[] b)
        {
            var s = "";

            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    if (j == a.GetLength(1) - 1)
                        s += $"{a[i, j]}*x{j + 1} ";
                    else
                    {
                        s += $"{a[i, j]}*x{j + 1} + ";
                    }
                }

                s += $"= {b[i]}\n";
            }

            return s;
        }

        static void Main(string[] args)
        {
            var F = new double[] {11000,20000,13500};
            var a = new double[,]
            {
                {2,2,1},
                {2,8,3},
                {2,2,2},
                {1,1,1 },
                {0,0,9 }
            };
            var b = new double[] {280,200,900,700,200};

            var sm = new Simplex(F, a, b);
            var result = sm.maximize();
            Console.WriteLine("Input:");
            Console.WriteLine("F: " + InputStringMaker(F));
            Console.WriteLine("Restriction:");
            Console.WriteLine(InputStringMaker(a, b));
            Console.WriteLine("========================\n");

            Console.WriteLine($"Maximum = {result.Item1}");
            Console.WriteLine("\n========================\n");
            Console.WriteLine(OutputStringMaker(result.Item2));

            Console.ReadLine();
        }
    }


    class Simplex
    {
        private double[] c;
        private double[,] A;
        private double[] b;
        private HashSet<int> N = new HashSet<int>();
        private HashSet<int> B = new HashSet<int>();
        private double v = 0;

        public Simplex(double[] c, double[,] A, double[] b)
        {
            int vars = c.Length, constraints = b.Length;

            if (vars != A.GetLength(1))
            {
                throw new Exception("Number of variables in c doesn't match number in A.");
            }

            if (constraints != A.GetLength(0))
            {
                throw new Exception("Number of constraints in A doesn't match number in b.");
            }

            this.c = new double[vars + constraints];
            Array.Copy(c, this.c, vars);

            this.A = new double[vars + constraints, vars + constraints];
            for (int i = 0; i < constraints; i++)
            {
                for (int j = 0; j < vars; j++)
                {
                    this.A[i + vars, j] = A[i, j];
                }
            }

            this.b = new double[vars + constraints];
            Array.Copy(b, 0, this.b, vars, constraints);

            for (int i = 0; i < vars; i++)
            {
                N.Add(i);
            }

            for (int i = 0; i < constraints; i++)
            {
                B.Add(vars + i);
            }
        }

        public Tuple<double, double[]> maximize()
        {
            while (true)
            {
                int e = -1;
                double ce = 0;
                foreach (var _e in N)
                {
                    if (c[_e] > ce)
                    {
                        ce = c[_e];
                        e = _e;
                    }
                }

                if (e == -1) break;

                double minRatio = double.PositiveInfinity;
                int l = -1;
                foreach (var i in B)
                {
                    if (A[i, e] > 0)
                    {
                        double r = b[i] / A[i, e];
                        if (r < minRatio)
                        {
                            minRatio = r;
                            l = i;
                        }
                    }
                }

                if (double.IsInfinity(minRatio))
                {
                    return Tuple.Create<double, double[]>(double.PositiveInfinity, null);
                }

                pivot(e, l);
            }

            double[] x = new double[b.Length];
            int n = b.Length;
            for (var i = 0; i < n; i++)
            {
                x[i] = B.Contains(i) ? b[i] : 0;
            }

            return Tuple.Create<double, double[]>(v, x);
        }

        private void pivot(int e, int l)
        {
            N.Remove(e);
            B.Remove(l);

            b[e] = b[l] / A[l, e];

            foreach (var j in N)
            {
                A[e, j] = A[l, j] / A[l, e];
            }

            A[e, l] = 1 / A[l, e];

            foreach (var i in B)
            {
                b[i] = b[i] - A[i, e] * b[e];

                foreach (var j in N)
                {
                    A[i, j] = A[i, j] - A[i, e] * A[e, j];
                }

                A[i, l] = -1 * A[i, e] * A[e, l];
            }

            v = v + c[e] * b[e];

            foreach (var j in N)
            {
                c[j] = c[j] - c[e] * A[e, j];
            }

            c[l] = -1 * c[e] * A[e, l];

            N.Add(l);
            B.Add(e);
        }
    }


}
