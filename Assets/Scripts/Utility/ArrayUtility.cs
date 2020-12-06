namespace Utility
{
    public static class ArrayUtility
    {
        public static T[][] GenerateJaggedMatrix<T>(int rows, int cols)
        {
            T[][] result = new T[rows][];
            for (int row = 0; row < rows; row++)
                result[row] = new T[cols];

            return result;
        }
    }
}