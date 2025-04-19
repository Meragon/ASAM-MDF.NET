namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ChannelCollection : IList<ChannelBlock>
    {
        private List<ChannelBlock> items = new List<ChannelBlock>();

        public ChannelCollection(Mdf mdf, ChannelGroupBlock parent)
        {
            if (mdf == null)
                throw new ArgumentNullException("mdf");

            Parent = parent;
        }

        public Mdf Mdf { get; }
        public int Count => items.Count;
        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        ChannelGroupBlock Parent { get; }

        public ChannelBlock this[int index]
        {
            get
            {
                return this.items[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        internal void Read(ChannelBlock cnBlock, ChannelBlock.ChanelHandlerRemovedAddress action)
        {
            items = Common.BuildBlockList(null, cnBlock, Parent);

            if (action != null)
                foreach (var item in items)
                    item.ChanelsRemovedAddress += (ch, bytes) => action(ch, bytes);
        }
        internal void Write(byte[] array, ref int index)
        {
            ChannelBlock prev = null;
            int prevIndex = 0;

            for (int i = 0; i < items.Count; i++)
            {
                var block = items[i];
                
                if (prev != null)
                    prev.WriteNextChannelLink(array, index, prevIndex);

                prev = block;
                prevIndex = index;

                block.Write(array, ref index);
                block.WriteChannelConversion(array, ref index, prevIndex);
                block.WriteComment(array, ref index, prevIndex);
                block.WriteLongSignalName(array, ref index, prevIndex);
                block.WriteDisplayName(array, ref index, prevIndex);
            }
        }

        public int IndexOf(ChannelBlock item)
        {
            return items.IndexOf(item);
        }
        public void Insert(int index, ChannelBlock item)
        {
            items.Insert(index, item);
        }
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }
        public void Add(ChannelBlock item)
        {
            items.Add(item);
        }
        public void Clear()
        {
            items.Clear();
        }
        public bool Contains(ChannelBlock item)
        {
            return items.Contains(item);
        }
        public void CopyTo(ChannelBlock[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }
        public bool Remove(ChannelBlock item)
        {
            return items.Remove(item);
        }

        public IEnumerator<ChannelBlock> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
