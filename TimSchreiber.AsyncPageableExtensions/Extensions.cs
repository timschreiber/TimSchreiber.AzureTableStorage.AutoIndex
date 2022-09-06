using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimSchreiber.AsyncPageableExtensions
{
    public static class Extensions
    {
        public static async Task<IEnumerable<T>> AsEnumerableAsync<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var enumerator = asyncPageable.GetAsyncEnumerator();
            var result = new List<T>();

            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    result.Add(enumerator.Current);
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            return result;
        }

        public static async Task<T> FirstAsync<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var enumerator = asyncPageable.GetAsyncEnumerator();

            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    return enumerator.Current;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            throw new InvalidOperationException("The input sequence contains no elements.");
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var enumerator = asyncPageable.GetAsyncEnumerator();

            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    return enumerator.Current;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            return default;
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var enumerator = asyncPageable.GetAsyncEnumerator();
            T result = default;
            var count = 0;

            try
            {
                while(await enumerator.MoveNextAsync())
                {
                    if (++count > 1)
                        throw new InvalidOperationException("The input sequence contains more than one element.");
                    result = enumerator.Current;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            return result;
        }

        public static async Task<T> SingleAsync<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var enumerator = asyncPageable.GetAsyncEnumerator();
            T result = default;
            var count = 0;

            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    if (++count > 1)
                        throw new InvalidOperationException("The input sequence contains more than one element.");
                    result = enumerator.Current;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            if (result == default)
                throw new InvalidOperationException("The input sequence contains no elements.");

            return result;
        }

        public static async Task<int> Count<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var enumerator = asyncPageable.GetAsyncEnumerator();
            var count = 0;

            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    count++;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            return count;
        }

        public static async Task<long> LongCount<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var enumerator = asyncPageable.GetAsyncEnumerator();
            var count = 0L;

            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    count++;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            return count;
        }

        public static async Task<bool> Any<T>(this Azure.AsyncPageable<T> asyncPageable) where T : class, ITableEntity, new()
        {
            var enumerator = asyncPageable.GetAsyncEnumerator();

            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    return true;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            return false;
        }
    }
}
