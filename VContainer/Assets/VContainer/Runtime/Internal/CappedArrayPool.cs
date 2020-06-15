using System;

namespace VContainer.Internal
{
    public sealed class CappedArrayPool<T>
    {
        const int InitialBucketSize = 4;

        public static readonly T[] EmptyArray = new T[0];
        public static readonly CappedArrayPool<T> Shared8Limit = new CappedArrayPool<T>(8);

        readonly T[][][] buckets;
        readonly object syncRoot = new object();
        int[] tails;

        CappedArrayPool(int maxLength)
        {
            buckets = new T[maxLength][][];
            tails = new int[maxLength];
            for (var i = 0; i < maxLength; i++)
            {
                var arrayLength = i + 1;
                buckets[i] = new T[InitialBucketSize][];
                for (var j = 0; j < InitialBucketSize; j++)
                {
                    buckets[i][j] = new T[arrayLength];
                };
                tails[i] = buckets[i].Length - 1;
            }
        }

        public T[] Rent(int length)
        {
            if (length <= 0)
                return EmptyArray;

            if (length > buckets.Length)
                return new T[length]; // Not supported

            var i = length - 1;

            lock (syncRoot)
            {
                var bucket = buckets[i];
                var tail = tails[i];
                if (tail >= bucket.Length)
                {
                    Array.Resize(ref bucket, bucket.Length * 2);
                    for (var j = tail; j < bucket.Length; j++)
                    {
                        bucket[j] = new T[length];
                    }
                }

                var result = bucket[tail];
                tails[i] += 1;
                return result;
            }
        }

        public void Return(T[] array)
        {
            if (array.Length <= 0 || array.Length > buckets.Length)
                return;

            lock (syncRoot)
            {
                var i = array.Length - 1;
                var bucket = buckets[i];
                var tail = (tails[i] -= 1);
                bucket[tail] = array;
            }
        }
    }
}
