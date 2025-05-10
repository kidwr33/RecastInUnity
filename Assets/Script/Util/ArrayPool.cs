

using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;


namespace Util
{
    public static class UtilFunc
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }
    public static class ObjectPool<T> where T : class, new()
    {
        private static Stack<T> pool = new Stack<T>();
        public static T Get()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }
            else
            {
                return new T();
            }
        }
        public static void Release(T obj)
        {
            pool.Push(obj);
        }
    }
    public static class ArrayPool<T> 
    {
        static int MAXSIZELOG2 = 10;

        static int MAXSIZE = 1 << 10;

        static Stack<T[]>[] pool = new Stack<T[]>[MAXSIZELOG2];
        public static T[] Get(int len)
        {
            if (len >= MAXSIZE) UnityEngine.Debug.LogError("The Length Of Array Try To Get Is Too Big!!");
            int index = GetIndexByLen(len);
            if (pool[index] == null)
            {
                pool[index] = new Stack<T[]>();
            }
            if (pool[index].Count > 0)
            {
                return pool[index].Pop();
            }
            else
            {
                return new T[1 << index];
            }
        }

        public static void Release(T[] values)
        {
            if((values.Length & (values.Length - 1)) != 0)
            {
                UnityEngine.Debug.LogError("The Length Of Array Try To Release Is Not Power Of 2!!");
                return;
            }
            int index = GetIndexByLen(values.Length);
            pool[index].Push(values);
        }
        private static int GetIndexByLen(int len)
        {
            int res = 0;
            for (int i = 0; i < MAXSIZELOG2; i++)
            {
                if (len <= (1 << i))
                {
                    res = i;
                    break;
                }
            }
            return res;
        }
    }
}


