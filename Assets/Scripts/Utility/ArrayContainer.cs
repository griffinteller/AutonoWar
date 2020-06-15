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
    public class FloatArrayContainer : ArrayContainer<float>
    {
        public FloatArrayContainer(float[] arr) : base(arr)
        {
        }
    }
}