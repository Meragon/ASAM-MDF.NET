namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;

    public class ChannelGroupCollection : IList<ChannelGroupBlock>
    {
        private List<ChannelGroupBlock> items = new List<ChannelGroupBlock>();
        
        public ChannelGroupCollection(Mdf mdf, DataGroupBlock parent)
        {
            if (mdf == null)
                throw new ArgumentNullException("mdf");

            Mdf = mdf;
            Parent = parent;
        }

        DataGroupBlock Parent { get; }
        public Mdf Mdf { get; private set; }
        public int Count
        {
            get { return items.Count; }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }

        public ChannelGroupBlock this[int index]
        {
            get { return this.items[index]; }
            set { throw new NotImplementedException(); }
        }

        internal void Read(ChannelGroupBlock channelGroupBlock, ChannelGroupBlock.ChannelGroupBlockHandler action)
        {
            items = Common.BuildBlockList(null, channelGroupBlock, Parent);

            if (action != null)
                foreach (ChannelGroupBlock block in items)
                    block.ChannelGroupRemove += (cg, bytes) => action(cg, bytes);
        }
        internal void Write(byte[] array, ref int index)
        {
            ChannelGroupBlock prev = null;
            int prevIndex = 0;
            for (int i = 0; i < Count; i++)
            {
                var block = items[i];

                if (prev != null)
                    prev.WriteNextChannelGroupBlockLink(array, index, prevIndex);
                
                prev = block;
                prevIndex = index;

                block.Write(array, ref index);
                block.WriteComment(array, ref index, prevIndex);
                block.WriteChannels(array, ref index, prevIndex);
            }
        }

        public int IndexOf(ChannelGroupBlock item)
        {
            return items.IndexOf(item);
        }
        public void Insert(int index, ChannelGroupBlock item)
        {
            items.Insert(index, item);
        }
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }
        public void Add(ChannelGroupBlock item)
        {
            items.Add(item);
        }
        public void Clear()
        {
            items.Clear();
        }
        public bool Contains(ChannelGroupBlock item)
        {
            return items.Contains(item);
        }
        public void CopyTo(ChannelGroupBlock[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }
        public bool Remove(ChannelGroupBlock item)
        {
            return items.Remove(item);
        }

        public IEnumerator<ChannelGroupBlock> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        public ChannelGroupBlock Find(Predicate<ChannelGroupBlock> predicate)
        {
            return items.Find(predicate);
        }
    }
}
