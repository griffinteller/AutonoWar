using System;
using UnityEngine;

namespace Utility
{
    [Serializable]
    public class FloatMatrix
    {
        [SerializeField] private  float[] matrix;
        [SerializeField] private int rows;
        [SerializeField] private int cols;
        
        public FloatMatrix(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            matrix = new float[rows * cols];
        }

        public float this[int i, int j]
        {
            get => matrix[i * cols + j];
            set => matrix[i * cols + j] = value;
        }

        public float[] this[int i]
        {
            set
            {
                if (value.Length != cols)
                    throw new ArgumentException("Array not of correct shape");

                for (var j = 0; j < cols; j++)
                    matrix[i * cols + j] = value[j];
            }
        }

        public void SetColumn(int j, float[] column)
        {
            for (var i = 0; i < rows; i++)
                matrix[i * cols + j] = column[i];
        }
    }
}