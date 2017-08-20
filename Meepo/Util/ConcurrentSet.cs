using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meepo.Util
{
    internal class ConcurrentSet<T> : IEnumerable<T> where T : IHasIndex
    {
        private readonly object thisLock = new object();

        private readonly HashSet<T> set = new HashSet<T>();

        private int Count
        {
            get
            {
                lock (thisLock)
                {
                    return set.Count;
                }
            }
        }

        public T this[Guid id]
        {
            get
            {
                lock (thisLock)
                {
                    return set.FirstOrDefault(x => x.Id == id);
                }
            }
        }

        public void Add(T item)
        {
            lock (thisLock)
            {
                set.Add(item);
            }
        }

        public void Remove(Guid id)
        {
            lock (thisLock)
            {
                set.RemoveWhere(x => x.Id == id);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var clone = new T[Count];

            lock (thisLock)
            {
                set.CopyTo(clone);
            }

            foreach (var value in clone)
            {
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
