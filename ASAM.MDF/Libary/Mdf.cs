namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Mdf
    {
        internal int position; //like stream position
        internal byte[] data;// data bytes

        /// <summary>
        /// Read MDF from stream.
        /// </summary>
        /// <param name="stream"></param>
        public Mdf(byte[] bytes)
        {
            data = bytes;

            DataGroups = new DataGroupCollection(this);
            IDBlock = IdentificationBlock.Read(this);
            HDBlock = HeaderBlock.Read(this);
        }
        public Mdf()
        {
            DataGroups = new DataGroupCollection(this);
            IDBlock = IdentificationBlock.Create(this);
            HDBlock = HeaderBlock.Create(this);
        }


        public IdentificationBlock IDBlock { get; private set; }
        public HeaderBlock HDBlock { get; private set; }
        public DataGroupCollection DataGroups { get; private set; }

        internal byte[] Data => data;

        /// <summary>
        /// Removing channels from mdf
        /// </summary>
        /// <param name="channelBlocks">An array of channels to delete</param>
        /// <returns>New file byte array</returns>
        public byte[] RemoveChannel(ChannelBlock[] channelBlocks)
        {
            var array = CheckChannelTimes(channelBlocks);
            var bytes = new List<byte>(Data);
            var copiedMDF = new Mdf(bytes.ToArray());
            var BlockAddresses = array.Select(x => x.BlockAddress);

            for (int i = 0; i < copiedMDF.DataGroups.Count; i++)
            {
                var dataGroup = copiedMDF.DataGroups[i];

                for (int j = 0; j < dataGroup.ChannelGroups.Count; j++)
                {
                    var channelGroup = dataGroup.ChannelGroups[j];

                    for (int k = 0; k < channelGroup.Channels.Count; k++)
                    {
                        var channel = channelGroup.Channels[k];

                        if (BlockAddresses.Contains(channel.BlockAddress))
                            bytes = channel.Remove(bytes);
                    }
                }
            }

            return bytes.ToArray();
        }

        private ChannelBlock[] CheckChannelTimes(ChannelBlock[] channelBlocks)
        {
            return channelBlocks.Where(x => x.TypeV3 != Types.ChannelTypeV3.Time).ToArray();
        }
        /// <summary>
        /// Converting an array from mdf to a bytes file. Not supported version 4
        /// </summary>
        /// <returns>byte array</returns>
        public byte[] GetBytes()
        {
            var array = new byte[GetSize()];

            int index = 0;

            // IDBLOCK.
            IDBlock.Write(array, ref index);

            int hdBlockIndex = index;

            // HDBLOCK.
            HDBlock.Write(array, ref index);
            HDBlock.WriteFileComment(array, ref index, hdBlockIndex);
            HDBlock.WriteProgramBlock(array, ref index, hdBlockIndex);
            HDBlock.WriteFirstDataGroupLink(array, index, hdBlockIndex);

            // DGBLOCKs.
            DataGroups.Write(array, ref index);

            return array;
        }


        /// <summary>
        /// Updating the data addresses of all blocks after deletion
        /// </summary>
        /// <param name="data">new data bytes</param>
        /// <param name="countDeleted">count deleted bytes</param>
        /// <param name="indexDeleted">Index start deleted on prevDataBytes</param>
        internal void UpdateAddresses(List<byte> data, ulong countDeleted, int indexDeleted)
        {
            var bytes = data;
            if (countDeleted == 0)
                return;

            HDBlock.HeaderUpdateAddress(indexDeleted, bytes, countDeleted);

            for (int i = 0; i < DataGroups.Count; i++)
            {
                var dataGroup = DataGroups[i];

                if (dataGroup.BlockAddress > indexDeleted)
                    dataGroup.BlockAddress -= (int)countDeleted;

                dataGroup.DataGroupUpdateAddress(indexDeleted, bytes, countDeleted);
                for (int j = 0; j < dataGroup.ChannelGroups.Count; j++)
                {
                    var channelGroup = dataGroup.ChannelGroups[j];

                    if (channelGroup.BlockAddress > indexDeleted)
                        channelGroup.BlockAddress -= (int)countDeleted;

                    channelGroup.ChannelGroupUpdateAddress(indexDeleted, bytes, countDeleted);
                    for (int k = 0; k < channelGroup.Channels.Count; k++)
                    {
                        var channel = channelGroup.Channels[k];

                        if (channel.BlockAddress > indexDeleted)
                            channel.BlockAddress -= (int)countDeleted;

                        channel.ChannelUpdateAddress(indexDeleted, bytes, countDeleted);
                    }
                }
                for (int j = 0; j < dataGroup.DataListColl.Count; j++)
                {
                    var dataList = dataGroup.DataListColl[j];

                    if (dataList.BlockAddress > indexDeleted)
                        dataList.BlockAddress -= (int)countDeleted;

                    dataList.DataListUpdateAddress(indexDeleted, bytes, countDeleted);
                }
            }
        }
        /// <summary>
        /// We get the maximum possible file size
        /// </summary>
        internal int GetSize()
        {
            var size = 0;

            size += IDBlock.GetSize();
            size += HDBlock.GetSizeTotal();

            for (int i = 0; i < DataGroups.Count; i++)
                size += DataGroups[i].GetSizeTotal();

            return size;
        }

        internal byte[] ReadBytes(int recordSize)
        {
            var value = new byte[recordSize];

            Array.Copy(data, position, value, 0, value.Length);

            position += value.Length;

            return value;
        }
        internal byte[] ReadBytes(byte[] data, int recordSize, ref int position)
        {
            var value = new byte[recordSize];

            Array.Copy(data, position, value, 0, value.Length);

            position += value.Length;

            return value;
        }
        internal string GetNameBlock(int position)
        {
            var index = position + 2;
            var name = IDBlock.Encoding.GetString(Data, index, 2);
            return name;
        }
    }
}
