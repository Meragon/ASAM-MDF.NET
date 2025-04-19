namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public class ChannelGroupBlock : Block, INext<ChannelGroupBlock>, IPrevious<ChannelGroupBlock>, IParent<DataGroupBlock>
    {
        public delegate void ChannelGroupBlockHandler(ChannelGroupBlock block, List<byte> bytes);

        internal PointerAddress<uint> ptrNextChannelGroup;
        internal PointerAddress<uint> ptrFirstChannelBlock;
        internal PointerAddress<uint> ptrTextName;
        internal PointerAddress<uint> ptrTextBlock;
        internal PointerAddress<uint> ptrSourceInfo;
        internal PointerAddress<uint> ptrFirstSampleReductionBlock;
        
        internal PointerAddress<ulong> ptrNextChannelGroupV4;
        internal PointerAddress<ulong> ptrFirstChannelBlockV4;
        internal PointerAddress<ulong> ptrTextNameV4;
        internal PointerAddress<ulong> ptrTextBlockV4;
        internal PointerAddress<ulong> ptrSourceInfoV4;
        internal PointerAddress<ulong> ptrFirstSampleReductionBlockV4;
        
        private char pathSeparator;

        private ChannelGroupBlock next;

        private ChannelGroupBlock(Mdf mdf) : base(mdf)
        {
            Channels = new ChannelCollection(mdf, this);
        }

        public ChannelGroupBlock Next
        {
            get
            {
                if (Mdf.IDBlock.Version >= 400)
                {
                    if (next == null && ptrNextChannelGroupV4 != null && ptrNextChannelGroupV4.address != 0)
                        next = Read(Mdf, (int)ptrNextChannelGroupV4.address);
                }
                else if (next == null && ptrNextChannelGroup != null && ptrNextChannelGroup.address != 0)
                    next = Read(Mdf, (int)ptrNextChannelGroup.address);

                return next;
            }
        }
        public event ChannelGroupBlockHandler ChannelGroupRemove;

        public ChannelGroupBlock Previous { get; set; }
        public ChannelCollection Channels { get; private set; }
        public TextBlock Comment { get; set; }
        public ulong RecordID { get; private set; }
        public ulong CycleCount { get; private set; }
        public ushort Flags { get; private set; }
        public uint Reserved1 { get; private set; }
        public uint DataBytes { get; private set; }
        public uint InvalidBytes { get; private set; }
        public ushort NumChannels { get; private set; }
        public ushort RecordSize { get; set; }
        public uint NumRecords { get; set; }
        public SampleReductionCollection SampleReductions { get; private set; }
        public TextBlock TextName { get; private set; }
        public DataGroupBlock Parent { get; set; }
        public SourceInformation SourceInformation { get; private set; }

        public static ChannelGroupBlock Create(Mdf mdf)
        {
            return new ChannelGroupBlock(mdf) { Identifier = "CG" };
        }

        internal static ChannelGroupBlock Read(Mdf mdf, int position)
        {
            mdf.UpdatePosition(position);

            var block = new ChannelGroupBlock(mdf);
            block.next = null;
            block.Comment = null;
            block.SampleReductions = null;

            block.Read();

            return block;
        }
        internal override void ReadV23()
        {
            base.ReadV23();

            listAddressesV23 = new List<PointerAddress<uint>>();

            ptrNextChannelGroup = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), 4);
            ptrFirstChannelBlock = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), ptrNextChannelGroup.offset + 4);
            ptrTextBlock = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), ptrFirstChannelBlock.offset + 4);
            RecordID = Mdf.ReadU16();
            NumChannels = Mdf.ReadU16();
            RecordSize = Mdf.ReadU16();
            NumRecords = Mdf.ReadU32();

            listAddressesV23.AddRange(new PointerAddress<uint>[]
            {
                ptrNextChannelGroup,
                ptrFirstChannelBlock,
                ptrTextBlock,
            });

            if (Size >= 26)
            {
                ptrFirstSampleReductionBlock = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), ptrTextBlock.offset + 2 + 2 + 2 + 4);
                listAddressesV23.Add(ptrFirstSampleReductionBlock);
            }

            if (ptrTextBlock.address != 0)
                Comment = TextBlock.Read(Mdf, (int)ptrTextBlock.address);

            if (ptrFirstChannelBlock.address != 0)
            {
                var chBlock = ChannelBlock.Read(Mdf, (int)ptrFirstChannelBlock.address);
                //chBlock.ChanelsRemovedAddress += (ch, bytes) => ChBlock_ChanelsRemovedAddress(ch, bytes);

                Channels.Read(chBlock, ChBlock_ChanelsRemovedAddress);
            }
        }
        internal override void ReadV4()
        {
            base.ReadV4();

            listAddressesV4 = new List<PointerAddress<ulong>>();

            ptrNextChannelGroupV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), 24);
            ptrFirstChannelBlockV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrNextChannelGroupV4.offset + 8);
            ptrTextNameV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrFirstChannelBlockV4.offset + 8);
            ptrSourceInfoV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrTextNameV4.offset + 8);
            ptrFirstSampleReductionBlockV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrSourceInfoV4.offset + 8);
            ptrTextBlockV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrFirstSampleReductionBlockV4.offset + 8);
            RecordID = Mdf.ReadU64();
            CycleCount = Mdf.ReadU64();
            Flags = Mdf.ReadU16();
            pathSeparator = Mdf.ReadChar();
            Reserved1 = Mdf.ReadU32();
            DataBytes = Mdf.ReadU32();
            InvalidBytes = Mdf.ReadU32();

            listAddressesV4.AddRange(new PointerAddress<ulong>[]
            {
                ptrNextChannelGroupV4,
                ptrFirstChannelBlockV4,
                ptrTextNameV4,
                ptrSourceInfoV4,
                ptrFirstSampleReductionBlockV4,
                ptrTextBlockV4,
            });

            if (ptrSourceInfoV4.address != 0)
                SourceInformation = SourceInformation.Read(Mdf, (int)ptrSourceInfoV4.address);

            if (ptrTextBlockV4.address != 0)
                Comment = TextBlock.Read(Mdf, (int)ptrTextBlockV4.address);

            if (ptrTextNameV4.address != 0)
                TextName = TextBlock.Read(Mdf, (int)ptrTextNameV4.address);

            if (ptrFirstChannelBlockV4.address != 0)
                Channels.Read(ChannelBlock.Read(Mdf, (int)ptrFirstChannelBlockV4.address), ChBlock_ChanelsRemovedAddress);
        }
        private void ChBlock_ChanelsRemovedAddress(ChannelBlock block, List<byte> bytes)
        {
            if (Mdf.IDBlock.Version >= 400)
                RemoveChannelsV4(block, bytes);
            else 
                RemoveChannelsV23(block, bytes);
        }

        private void RemoveChannelsV23(ChannelBlock block, List<byte> bytes)
        {
            NumChannels -= 1;

            if (NumChannels > 1)
            {
                var thisPointer = BlockAddress;
                if (Previous != null)
                    thisPointer = (int)Previous.ptrNextChannelGroup.address;
                else
                    thisPointer = (int)Parent.ptrFirstChannelGroupBlock.address;

                var addressNumChannels = thisPointer + ptrTextBlock.offset + 4 + 2/*RecordID*/;
                var newbytes = BitConverter.GetBytes(NumChannels);

                for (int i = addressNumChannels, j = 0; j < newbytes.Length; i++, j++)
                    bytes[i] = newbytes[j];
            }
            else
                ChannelGroupRemove?.Invoke(this, bytes);
        }

        private void RemoveChannelsV4(ChannelBlock block, List<byte> bytes)
        {
            if (Channels[0] == block) //change first channel address on channelGroup
            {
                var nextBlock = block.Next;
                var ptrFirst = 0ul;
                
                if (nextBlock != null)
                    ptrFirst = (ulong)block.Next.BlockAddress;

                var addressOfFirstChannel = BlockAddress + ptrFirstChannelBlockV4.offset;
                var bytesFirstChannelAddress = BitConverter.GetBytes(ptrFirst);

                for (int i = addressOfFirstChannel, j = 0; j < bytesFirstChannelAddress.Length; i++, j++)
                    bytes[i] = bytesFirstChannelAddress[j];

            }

            if (block.Previous == null && block.Next == null)
            {
                var ptrFirst = 0ul;
                var addressOfFirstChannel = BlockAddress + ptrFirstChannelBlockV4.offset;
                var bytesFirstChannelAddress = BitConverter.GetBytes(ptrFirst);

                for (int i = addressOfFirstChannel, j = 0; j < bytesFirstChannelAddress.Length; i++, j++)
                    bytes[i] = bytesFirstChannelAddress[j];
            }
        }

        internal override ushort GetSize()
        {
            return 30;
        }
        internal override int GetSizeTotal()
        {
            var size = base.GetSizeTotal();

            for (int i = 0; i < Channels.Count; i++)
                size += Channels[i].GetSizeTotal();

            size += Comment.GetSizeSafe();

            return size;
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            var bytesRecordId = BitConverter.GetBytes(RecordID);
            var bytesNumChannels = BitConverter.GetBytes(Channels.Count);
            var bytesRecordSize = BitConverter.GetBytes(RecordSize);
            var bytesNumRecords = BitConverter.GetBytes(NumRecords);

            Array.Copy(bytesRecordId, 0, array, index + 16, bytesRecordId.Length);
            Array.Copy(bytesNumChannels, 0, array, index + 18, bytesNumChannels.Length);
            Array.Copy(bytesRecordSize, 0, array, index + 20, bytesRecordSize.Length);
            Array.Copy(bytesNumRecords, 0, array, index + 22, bytesNumRecords.Length);

            index += GetSize();
        }
        internal void WriteChannels(byte[] array, ref int index, int blockIndex)
        {
            if (Channels.Count == 0)
                return;

            var bytesFirstChannelLink = BitConverter.GetBytes(index);

            Array.Copy(bytesFirstChannelLink, 0, array, blockIndex + 8, bytesFirstChannelLink.Length);

            Channels.Write(array, ref index);
        }
        internal void WriteComment(byte[] array, ref int index, int blockIndex)
        {
            if (Comment == null)
                return;

            var bytesCommentLink = BitConverter.GetBytes(index);

            Array.Copy(bytesCommentLink, 0, array, blockIndex + 12, bytesCommentLink.Length);

            Comment.Write(array, ref index);
        }
        internal void WriteNextChannelGroupBlockLink(byte[] array, int index, int blockIndex)
        {
            var bytesNextChannelGroupBlockLink = BitConverter.GetBytes(index);

            Array.Copy(bytesNextChannelGroupBlockLink, 0, array, blockIndex + 4, bytesNextChannelGroupBlockLink.Length);
        }

        internal void ChannelGroupUpdateAddress(int indexDeleted, List<byte> bytes, ulong countDeleted)
        {
            if (Mdf.IDBlock.Version >= 400)
                ChannelGroupUpdateAddressV4(indexDeleted, bytes, countDeleted);
            else
                ChannelGroupUpdateAddressV23(indexDeleted, bytes, (uint)countDeleted);

            SourceInformation?.SourceInformationUpdateAddress(indexDeleted, bytes, countDeleted);
        }

        private void ChannelGroupUpdateAddressV23(int indexDeleted, List<byte> bytes, uint countDeleted)
        {
            foreach (var ptr in listAddressesV23)
            {
                if ((int)ptr.address >= indexDeleted)
                {
                    ptr.address -= countDeleted;

                    this.CopyAddress(ptr, bytes, indexDeleted, countDeleted);
                }
            }
        }

        private void ChannelGroupUpdateAddressV4(int indexDeleted, List<byte> bytes, ulong countDeleted)
        {
            foreach (var ptr in listAddressesV4)
            {
                if ((int)ptr.address >= indexDeleted)
                {
                    ptr.address -= countDeleted;

                    this.CopyAddress(ptr, bytes, indexDeleted, countDeleted);
                }
            }
        }

        internal void ClearDataType(List<byte> bytes)
        {
            RecordSize = 0;
            var addressRecordSize = BlockAddress + ptrTextBlock.offset + 4 + 2/*RecordID*/ + 2/*NumChannels*/;
            var newbytes = BitConverter.GetBytes(RecordSize);

            for (int i = addressRecordSize, j = 0; j < newbytes.Length; i++, j++)
                bytes[i] = newbytes[j];

            NumRecords = 0;
            var addressNumRecords = BlockAddress + ptrTextBlock.offset + 4 + 2/*RecordID*/ + 2/*NumChannels*/ + 2/*RecordSize*/;
            newbytes = BitConverter.GetBytes(NumRecords);

            for (int i = addressNumRecords, j = 0; j < newbytes.Length; i++, j++)
                bytes[i] = newbytes[j];
        }
    }
}
