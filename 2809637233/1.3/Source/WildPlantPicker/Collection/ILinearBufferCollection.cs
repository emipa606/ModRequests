using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildPlantPicker.Collection
{
    public interface ILinearBufferCollection<in TElement, out TResult>
    {
        int Next(Action<TResult> action);
    }
    public abstract class LinearBufferCollection<TCollection, TElement, TResult> : ILinearBufferCollection<TElement, TResult>
         where TCollection : IEnumerable<TResult>, new()
    {
        protected int m_CurrentIndex = 0;
        protected TCollection m_Collection;

        protected abstract void MergeSource(TCollection dst, IEnumerable<TElement> src);
        public abstract TCollection GetCollection();
        public abstract void Add(TElement elm);
        public abstract void Remove(TElement elm);

        public int Next(Action<TResult> action)
        {
            if (!m_Collection.Any())
            {
                return m_CurrentIndex = 0;
            }
            if (m_CurrentIndex >= m_Collection.Count())
            {
                m_CurrentIndex = 0;
            }
            action(m_Collection.ElementAt(m_CurrentIndex));
            return ++m_CurrentIndex;
        }

        public void SetIndex(int index)
        {
            m_CurrentIndex = index;
        }

        public LinearBufferCollection(int index = 0)
        {
            m_CurrentIndex = index;
            m_Collection = new TCollection();
        }

        public LinearBufferCollection(IEnumerable<TElement> src, int index = 0)
        {
            m_CurrentIndex = index;
            m_Collection = new TCollection();
            this.MergeSource(m_Collection, src);
        }
    }
    public class LinearBufferHashSet<TElement> : LinearBufferCollection<HashSet<TElement>, TElement, TElement>
    {
        public override HashSet<TElement> GetCollection()
        {
            return m_Collection;
        }

        public override void Add(TElement elm)
        {
            if (!m_Collection.Contains(elm))
            {
                m_Collection.Add(elm);
            }
        }

        public override void Remove(TElement elm)
        {
            if (m_Collection.Contains(elm))
            {
                m_Collection.Remove(elm);
            }
        }

        protected override void MergeSource(HashSet<TElement> dst, IEnumerable<TElement> src)
        {
            foreach (TElement elm in src)
            {
                if (!dst.Contains(elm))
                {
                    dst.Add(elm);
                }
            }
        }

        public LinearBufferHashSet(int index = 0) : base(index)
        {
        }

        public LinearBufferHashSet(IEnumerable<TElement> src, int index = 0) : base(src, index)
        {
        }

    }
}
