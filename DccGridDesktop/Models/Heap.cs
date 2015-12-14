using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DccGridDesktop.Models
{
    [Serializable]
    class Heap<T>
    {
        T[][] heap;
        public Heap(int minWidth)
        {
            if (minWidth < 1)
            {
                return;
            }
            int power = (int)Math.Ceiling(Math.Log(minWidth) / Math.Log(2));
            int N = (int)Math.Pow(2, power);

            heap = new T[power][];

            int n = N;
            for (int i = 0; i < power; i++)
            {
                heap[i] = new T[n];
                n /= 2;
            }
        }

        public Heap<T> Copy()
        {
            var result = new Heap<T>(heap.Length > 0 ? heap[0].Length : 0);

            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < this[i].Length; j++)
                {
                    result[i, j] = heap[i][j];
                }
            }

            return result;
        }

        public int Length
        {
            get
            {
                return heap.Length;
            }
        }

        public bool inHeap(T x)
        {
            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < this[i].Length; j++)
                {
                    if (heap[i][j] != null && heap[i][j].Equals(x))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public T[] this[int col]
        {
            get
            {
                return heap[col];
            }
        }

        public Point GetCoords(int col, int row, double x, double y, double width, double height, double rowHeight, double rowWidth)
        {
            var result = new Point();

            int N = heap.Length;
            int M = heap[col].Length;

            double colStep = rowWidth + (width - rowWidth * N) / (N - 1);
            double spaceBetweenRows = 2 * (height - rowHeight * M) / M;
            double rowStep = 2 * rowHeight + spaceBetweenRows;

            result.X = colStep * col;

            var rowPairId = row / 2;

            result.Y = spaceBetweenRows / 2 + rowStep * rowPairId + (row % 2 == 0 ? 0 : rowHeight);

            result.X += x;
            result.Y += y;

            return result;
        }

        public T this[int col, int row]
        {
            get
            {
                return heap[col][row];
            }
            set
            {
                heap[col][row] = value;
            }
        }
    }
}
