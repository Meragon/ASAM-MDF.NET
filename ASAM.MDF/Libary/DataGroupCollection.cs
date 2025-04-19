namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;

    public class DataGroupCollection : IList<DataGroupBlock>
    {
        private List<DataGroupBlock> items = new List<DataGroupBlock>();

        public DataGroupCollection(Mdf mdf)
        {
            if (mdf == null)
                throw new ArgumentNullException("mdf");

            Mdf = mdf;
        }

        public Mdf Mdf { get; private set; }
        public int Count
        {
            get { return items.Count; }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }

        internal void Read(DataGroupBlock block)
        {
            items = Common.BuildBlockList(null, block, Mdf);
        }
        internal void Write(byte[] array, ref int index)
        {
            var blockIndexes = new int[Count];

            DataGroupBlock prev = null;
            int prevIndex = 0;

            // Write block info.
            for (int i = 0; i < Count; i++)
            {
                var block = items[i];
                blockIndexes[i] = index;

                if (prev != null)
                    prev.WriteNextBlockLink(array, index, prevIndex);
                
                prev = block;
                prevIndex = index;

                block.Write(array, ref index);
            }

            // Write channel groups.
            var firstChannelGroupIndex = index;
            for (int i = 0; i < Count; i++)
            {
                var block = items[i];

                block.WriteChannelGroups(array, ref index);
                block.WriteFirstChannelGroupBlockLink(array, firstChannelGroupIndex, blockIndexes[i]);
            }

            // Write records.
            for (int i = 0; i < Count; i++)
            {
                var block = items[i];

                block.WriteRecords(array, ref index, blockIndexes[i]);
            }
        }
        internal void Write(List<byte> array)
        {
        }

        // IList.
        public int IndexOf(DataGroupBlock item)
        {
            return items.IndexOf(item);
        }
        public void Insert(int index, DataGroupBlock item)
        {
            items.Insert(index, item);
        }
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }
        public DataGroupBlock this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }
        public void Add(DataGroupBlock item)
        {
            items.Add(item);
        }
        public void Clear()
        {
            items.Clear();
        }
        public bool Contains(DataGroupBlock item)
        {
            return items.Contains(item);
        }
        public void CopyTo(DataGroupBlock[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }
        public bool Remove(DataGroupBlock item)
        {
            return items.Remove(item);
        }
        public IEnumerator<DataGroupBlock> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        public DataGroupBlock Find(Predicate<DataGroupBlock> predicate)
        {
            return items.Find(predicate);
        }
    }
}
