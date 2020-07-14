using System;

namespace Utility
{
    [Serializable]
    public class ArrayContainer<T>
    {
        public T[] array;

        public ArrayContainer(T[] arr)
        {
            array = arr;
        }
    }

    [Serializable]
    public class FloatArrayContainer
    {
        public float[] array;
        
        public FloatArrayContainer(float[] arr)
        {
            array = arr;
        }
    }
}