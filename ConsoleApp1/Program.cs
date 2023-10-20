using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    public static void ShiftLeft(double[] row)
    {
        double temp = row[0];
        Array.Copy(row, 1, row, 0, row.Length - 1);
        row[row.Length - 1] = temp;
    }

    public static void ShiftUp(double[][] matrix, int j)
    {
        double temp = matrix[0][j];
        for (int i = 0; i < matrix.Length - 1; i++)
        {
            matrix[i][j] = matrix[i + 1][j];
        }
        matrix[matrix.Length - 1][j] = temp;
    }

    public static double[][] Cannon(double[][] A, double[][] B)
    {
        int n = A.Length;
        double[][] C = new double[n][];
        for (int i = 0; i < n; i++)
        {
            C[i] = new double[n];
        }

        for (int k = 0; k < n; k++)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    for (int m = 0; m < n; m++)
                    {
                        C[i][j] += A[i][m] * B[m][(j + (i + k) % n) % n];
                    }
                }
                ShiftLeft(A[i]);
            }
            ShiftUp(B, k);
        }

        return C;
    }

    public static double[][] MultiThreadedCannon(double[][] A, double[][] B, int threadCount)
    {
        int n = A.Length;
        double[][] C = new double[n][];
        for (int i = 0; i < n; i++)
        {
            C[i] = new double[n];
        }

        ManualResetEvent[] doneEvents = new ManualResetEvent[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            doneEvents[i] = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object state)
            {
                int index = (int)state;
                for (int k = index; k < n; k += threadCount)
                {
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            for (int m = 0; m < n; m++)
                            {
                                C[i][j] += A[i][m] * B[m][(j + (i + k) % n) % n];
                            }
                        }
                        ShiftLeft(A[i]);
                    }
                    ShiftUp(B, k);
                }
                doneEvents[index].Set();
            }), i);
        }

        WaitHandle.WaitAll(doneEvents);

        return C;
    }


    static void Main()
    {
        // Input the number of rows and columns
        int rows = 100;
        int columns = 100;

         int threadCount = 20;

        // Create the random matrix A
        double[][] matrixA = new double[rows][];
        Random random = new Random();
        for (int i = 0; i < rows; i++)
        {
            matrixA[i] = new double[columns];
            for (int j = 0; j < columns; j++)
            {
                matrixA[i][j] = random.NextDouble();
            }
        }

        // Create the random matrix B
        double[][] matrixB = new double[rows][];
        for (int i = 0; i < rows; i++)
        {
            matrixB[i] = new double[columns];
            for (int j = 0; j < columns; j++)
            {
                matrixB[i][j] = random.NextDouble();
            }
        }

        // Perform Cannon's algorithm for matrix multiplication
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Cannon(matrixA, matrixB);
        stopwatch.Stop();

        Stopwatch stopwatch2 = new Stopwatch();
        stopwatch2.Start();
        var result2 = MultiThreadedCannon(matrixA, matrixB, threadCount);
        stopwatch2.Stop();

        /*Display the result*/
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Console.Write(result2[i][j] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("Parallel Cannon: " + stopwatch2.Elapsed);
        Console.WriteLine("Cannon: " + stopwatch.Elapsed);
    }
}
