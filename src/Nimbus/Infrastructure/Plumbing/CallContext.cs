using System.Collections.Concurrent;
using System.Threading;
using Nimbus.ConcurrentCollections;

namespace Nimbus.Infrastructure
{
    /// <summary>
    /// Re-implementation of CallContext for use with .NET Core / .NET Standard
    /// </summary>
    internal static class CallContext
    {
        private static ThreadSafeDictionary<string, AsyncLocal<object>> _data = new ThreadSafeDictionary<string, AsyncLocal<object>>();

        internal static void LogicalSetData(string key, object obj)
        {
            _data.GetOrAdd(key, (s) => new AsyncLocal<object>()).Value = obj;
        }

        internal static object LogicalGetData(string key)
        {
            AsyncLocal<object> val;

            if (_data.TryGetValue(key, out val))
            {
                return val;
            }

            return null;
        }
    }
}