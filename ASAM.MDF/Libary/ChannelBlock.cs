namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ASAM.MDF.Libary.Types;

    public class ChannelBlock : Block, INext<ChannelBlock>, IPrevious<ChannelBlock>, IParent<ChannelGroupBlock>
    {
        public delegate void ChanelHandlerRemovedAddress(ChannelBlock block, List<byte> bytes);

        private const int MIN_VERSION_LONG_SIGNAL_NAME = 212;
        private const int MIN_VERSION_DISPLAY_NAME = 300;
        private const int MIN_VERSION_ADDITIONAL_BYTE_OFFSET = 300;

        internal PointerAddress<uint> ptrNextChannelBlock;
        internal PointerAddress<uint> ptrChannelConversionBlock;
        internal PointerAddress<uint> ptrDataBlockSignal;
        internal PointerAddress<uint> ptrUnit;
        internal PointerAddress<uint> ptrTextBlockComment;
        internal PointerAddress<uint> ptrAttachment;
        internal PointerAddress<uint> ptrChannelExtensionBlock;
        internal PointerAddress<uint> ptrChannelDependencyBlock;
        internal PointerAddress<uint> ptrChannelComment;
        internal PointerAddress<uint> ptrLongSignalName;
        internal PointerAddress<uint> ptrDisplayName;
        internal PointerAddress<uint> ptrComponentAddress;
        internal PointerAddress<uint> ptrTextBlockChanelName;

        internal PointerAddress<ulong> ptrNextChannelBlockV4;
        internal PointerAddress<ulong> ptrChannelConversionBlockV4;
        internal PointerAddress<ulong> ptrDataBlockSignalV4;
        internal PointerAddress<ulong> ptrUnitV4;
        internal PointerAddress<ulong> ptrTextBlockCommentV4;
        internal PointerAddress<ulong> ptrAttachmentV4;
        internal PointerAddress<ulong> ptrChannelExtensionBlockV4;
        internal PointerAddress<ulong> ptrChannelDependencyBlockV4;
        internal PointerAddress<ulong> ptrChannelCommentV4;
        internal PointerAddress<ulong> ptrDisplayNameV4;
        internal PointerAddress<ulong> ptrComponentAddressV4;
        internal PointerAddress<ulong> ptrTextBlockChanelNameV4;

        private string signalName;
        private string signalDescription;
        private byte SyncType;
        private byte DataType;
        private ChannelConversionBlock channelConversion;
        private ChannelBlock next;

        private ChannelBlock(Mdf mdf) : base(mdf)
        { }

        public event ChanelHandlerRemovedAddress ChanelsRemovedAddress;

        public ChannelBlock Next
        {
            get
            {
                if (Mdf.IDBlock.Version >= 400)
                {
                    if (next == null && ptrNextChannelBlockV4 != null && ptrNextChannelBlockV4.address != 0 && ptrNextChannelBlockV4.address < (ulong)Mdf.Data.Length)
                        next = Read(Mdf, (int)ptrNextChannelBlockV4.address);
                }
                else if (next == null && ptrNextChannelBlock != null && ptrNextChannelBlock.address != 0 && ptrNextChannelBlock.address < (uint)Mdf.Data.Length)
                    next = Read(Mdf, (int)ptrNextChannelBlock.address);

                return next;
            }
        }
        public ChannelBlock Previous { get; set; }
        public ChannelConversionBlock ChannelConversion { get => channelConversion; set => channelConversion = value; }
        public ChannelExtensionBlock SourceDepending { get; private set; }
        public ChannelDependencyBlock Dependency { get; private set; }
        public TextBlock Comment { get; set; }
        public ChannelTypeV3 TypeV3 { get; set; }
        public ChannelTypeV4 TypeV4 { get; set; }

        public string SignalName
        {
            get { return signalName; }
            set { SetStringValue(ref signalName, value, 32); }
        }
        public string SignalDescription
        {
            get { return signalDescription; }
            set { SetStringValue(ref signalDescription, value, 128); }
        }
        public ushort BitOffset { get; set; }
        public uint BitLength { get; private set; }
        public uint ChannelFlags { get; private set; }
        public uint InvalidBitPos { get; private set; }
        public byte Precision { get; private set; }
        public byte Reserved1 { get; private set; }
        public ushort AttachmentCount { get; private set; }
        public double ValRangeMin { get; private set; }
        public double ValRangeMax { get; private set; }
        public double LimitMin { get; private set; }
        public double LimitMax { get; private set; }
        public double LimitMinExt { get; private set; }
        public double LimitMaxExt { get; private set; }
        public ushort NumberOfBits { get; set; }
        public SignalTypeV3 SignalTypeV3 { get; set; }
        public SignalTypeV4 SignalTypeV4 { get; set; }
        public bool ValueRange { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double SampleRate { get; set; }
        public TextBlock LongSignalName { get; private set; }
        public TextBlock DisplayName { get; private set; }
        public uint AdditionalByteOffset { get; set; }
        public TextBlock Unit { get; private set; }
        public ChannelGroupBlock Parent { get; set; }

        public static ChannelBlock Create(Mdf mdf)
        {
            return new ChannelBlock(mdf)
            {
                Identifier = "CN",
                SignalName = "",
                SignalDescription = "",
            };
        }
        public static ChannelBlock Read(Mdf mdf, int position)
        {
            mdf.UpdatePosition(position);

            var block = new ChannelBlock(mdf);
            block.next = null;
            block.SourceDepending = null;
            block.Dependency = null;
            block.Comment = null;

            block.Read();
            return block;
        }
        internal override void ReadV23()
        {
            base.ReadV23();

            listAddressesV23 = new List<PointerAddress<uint>>();

            ptrNextChannelBlock = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), 4);
            ptrChannelConversionBlock = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), ptrNextChannelBlock.offset + 4);
            ptrChannelExtensionBlock = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), ptrChannelConversionBlock.offset + 4);
            ptrChannelDependencyBlock = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), ptrChannelExtensionBlock.offset + 4);
            ptrChannelComment = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), ptrChannelDependencyBlock.offset + 4);
            TypeV3 = (ChannelTypeV3)Mdf.ReadU16();
            SignalName = Mdf.GetString(32);
            SignalDescription = Mdf.GetString(128);
            BitOffset = Mdf.ReadU16();
            NumberOfBits = Mdf.ReadU16();
            SignalTypeV3 = (SignalTypeV3)Mdf.ReadU16();
            ValueRange = Mdf.ReadBoolean();

            listAddressesV23.AddRange(new PointerAddress<uint>[]
            {
                ptrNextChannelBlock,
                ptrChannelConversionBlock,
                ptrChannelExtensionBlock,
                ptrChannelDependencyBlock,
                ptrChannelComment
            });

            if (ValueRange)
            {
                MinValue = Mdf.ReadDouble();
                MaxValue = Mdf.ReadDouble();
            }
            else
            {
                Mdf.UpdatePosition(Mdf.position + 16);
            }
            SampleRate = Mdf.ReadDouble();

            var offset = ptrChannelComment.offset + 2 + 32 + 128 + 2 + 2 + 2 + 2 + 16 + 8;
            if (Mdf.IDBlock.Version >= MIN_VERSION_LONG_SIGNAL_NAME)
            {
                ptrLongSignalName = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), offset);
                offset += 4;
                listAddressesV23.Add(ptrLongSignalName);

            }
            if (Mdf.IDBlock.Version >= MIN_VERSION_DISPLAY_NAME)
            {
                ptrDisplayName = new PointerAddress<uint>(Mdf.ReadU32().ValidateAddress(Mdf), ptrChannelComment.offset + offset);
                listAddressesV23.Add(ptrDisplayName);
            }
            if (Mdf.IDBlock.Version >= MIN_VERSION_ADDITIONAL_BYTE_OFFSET)
                AdditionalByteOffset = Mdf.ReadU16();

            if (ptrLongSignalName != null && ptrLongSignalName.address != 0)
                LongSignalName = TextBlock.Read(Mdf, (int)ptrLongSignalName.address);

            if (ptrChannelComment.address != 0)
                Comment = TextBlock.Read(Mdf, (int)ptrChannelComment.address);

            if (channelConversion == null && ptrChannelConversionBlock.address != 0)
                ChannelConversion = ChannelConversionBlock.Read(Mdf, (int)ptrChannelConversionBlock.address);
            //if (ptrChannelExtensionBlock != 0)
            //{
            //    if (Mdf.IDBlock.Version == 400)

            //    SourceDepending = new ChannelExtensionBlock(Mdf, ptrChannelExtensionBlock);
            //}
        }

        internal override void ReadV4()
        {
            base.ReadV4();

            listAddressesV4 = new List<PointerAddress<ulong>>();

            ptrNextChannelBlockV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), 24);
            ptrComponentAddressV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrNextChannelBlockV4.offset + 8);
            ptrTextBlockChanelNameV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrComponentAddressV4.offset + 8);
            ptrChannelExtensionBlockV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrTextBlockChanelNameV4.offset + 8);
            ptrChannelConversionBlockV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrChannelExtensionBlockV4.offset + 8);
            ptrDataBlockSignalV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrChannelConversionBlockV4.offset + 8);
            ptrUnitV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrDataBlockSignalV4.offset + 8);
            ptrTextBlockCommentV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrUnitV4.offset + 8);
            //ptrAttachment = Mdf.ReadU64();
            //ptrDefaultDGBlock = Mdf.ReadU64();
            //ptrDefaultCGBlock = Mdf.ReadU64();
            //ptrDefaultCurrentChanelBlock = Mdf.ReadU64();
            TypeV4 = (ChannelTypeV4)Mdf.ReadByte();
            SyncType = Mdf.ReadByte();
            SignalTypeV4 = (SignalTypeV4)Mdf.ReadByte();
            BitOffset = Mdf.ReadByte();
            AdditionalByteOffset = Mdf.ReadU32();
            NumberOfBits = (ushort)Mdf.ReadU32();
            ChannelFlags = Mdf.ReadU32();
            InvalidBitPos = Mdf.ReadU32();
            Precision = Mdf.ReadByte();
            Reserved1 = Mdf.ReadByte();
            AttachmentCount = Mdf.ReadU16();
            ValRangeMin = Mdf.ReadDouble();
            ValRangeMax = Mdf.ReadDouble();
            LimitMin = Mdf.ReadDouble();
            LimitMax = Mdf.ReadDouble();
            LimitMinExt = Mdf.ReadDouble();
            LimitMaxExt = Mdf.ReadDouble();

            listAddressesV4.AddRange(new PointerAddress<ulong>[]
            {
                ptrNextChannelBlockV4,
                ptrComponentAddressV4,
                ptrTextBlockChanelNameV4,
                ptrChannelExtensionBlockV4,
                ptrChannelConversionBlockV4,
                ptrDataBlockSignalV4,
                ptrUnitV4,
                ptrTextBlockCommentV4
            });

            if (ptrTextBlockChanelNameV4.address != 0)
                LongSignalName = TextBlock.Read(Mdf, (int)ptrTextBlockChanelNameV4.address);

            if (ptrUnitV4.address != 0)
                Unit = TextBlock.Read(Mdf, (int)ptrUnitV4.address);

            if (ptrTextBlockCommentV4.address != 0)
                Comment = TextBlock.Read(Mdf, (int)ptrTextBlockCommentV4.address);

            if (channelConversion == null && ptrChannelConversionBlockV4.address != 0)
                ChannelConversion = ChannelConversionBlock.Read(Mdf, (int)ptrChannelConversionBlockV4.address);
        }
        /// <summary>
        /// Set this address 0 for previous channel. Lost address
        /// </summary>
        /// <returns>Copied modified the entire array of mdf bytes</returns>
        public List<byte> Remove(List<byte> bytes)
        {
            if (TypeV3 == ChannelTypeV3.Time)
                return bytes;

            var previous = Previous;
            if (previous == null && next != null)// first of list node channels: [X channel]->[1 channel]->[2 channel]->...->[n channel]
            {
                ChanelsRemovedAddress?.Invoke(this, bytes);

                next.Previous = null;

                return bytes;
            }
            else if (previous == null && next == null) //       [null] -> [X channel] -> [null]
            {
                ChanelsRemovedAddress?.Invoke(this, bytes);

                return bytes;
            }

            if (Mdf.IDBlock.Version >= 400)
                bytes = RemoveV4(bytes, previous);
            else
                bytes = RemoveV23(bytes, previous);

            previous.next = next;

            if (next != null)
                next.Previous = previous;

            ChanelsRemovedAddress?.Invoke(this, bytes);

            return bytes;
        }

        private List<byte> RemoveV23(List<byte> bytes, ChannelBlock previous)
        {
            var thisPointer = previous.BlockAddress + previous.ptrNextChannelBlock.offset;

            var newbytes = BitConverter.GetBytes(ptrNextChannelBlock.address);
            for (int i = thisPointer, j = 0; j < newbytes.Length; i++, j++)
                bytes[i] = newbytes[j];

            previous.ptrNextChannelBlock.address = ptrNextChannelBlock.address;
            ptrNextChannelBlock.address = 0;

            return bytes;
        }

        private List<byte> RemoveV4(List<byte> bytes, ChannelBlock previous)
        {
            var thisPointer = previous.BlockAddress + previous.ptrNextChannelBlockV4.offset; // this pointer on prev channel next

            var newbytes = BitConverter.GetBytes(ptrNextChannelBlockV4.address);
            for (int i = thisPointer, j = 0; j < newbytes.Length; i++, j++)
                bytes[i] = newbytes[j];

            previous.ptrNextChannelBlockV4.address = ptrNextChannelBlockV4.address;

            return bytes;
        }

        public override string ToString()
        {
            return SignalName;
        }
        internal override ushort GetSize()
        {
            // Base size.
            if (Mdf.IDBlock.Version < 212)
                return 218;

            // 2.12
            if (Mdf.IDBlock.Version < 300)
                return 222;

            // 3.00
            return 228;
        }

        internal override int GetSizeTotal()
        {
            var size = base.GetSizeTotal();

            if (channelConversion != null)
                size += channelConversion.GetSizeTotal();

            if (Comment != null)
                size += Comment.GetSizeTotal();

            if (LongSignalName != null)
                size += LongSignalName.GetSizeTotal();

            if (DisplayName != null)
                size += DisplayName.GetSizeTotal();

            return size;
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);
            
            var bytesChannelType = BitConverter.GetBytes((ushort)TypeV3);
            var bytesSignalName = Mdf.IDBlock.Encoding.GetBytes(SignalName);
            var bytesSignalDesc = Mdf.IDBlock.Encoding.GetBytes(SignalDescription);
            var bytesBitOffset = BitConverter.GetBytes(BitOffset);
            var bytesNumOfBits = BitConverter.GetBytes(NumberOfBits);
            var bytesSignalDataType = BitConverter.GetBytes((ushort)SignalTypeV3);
            var bytesValueRangeValid = BitConverter.GetBytes(ValueRange);
            var bytesMinValue = BitConverter.GetBytes(MinValue);
            var bytesMaxValue = BitConverter.GetBytes(MaxValue);
            var bytesSampleRate = BitConverter.GetBytes(SampleRate);

            ValidateEncodingLength(ref bytesSignalName, 32);
            ValidateEncodingLength(ref bytesSignalDesc, 128);

            Array.Copy(bytesChannelType, 0, array, index + 24, bytesChannelType.Length);
            Array.Copy(bytesSignalName, 0, array, index + 26, bytesSignalName.Length);
            Array.Copy(bytesSignalDesc, 0, array, index + 58, bytesSignalDesc.Length);
            Array.Copy(bytesBitOffset, 0, array, index + 186, bytesBitOffset.Length);
            Array.Copy(bytesNumOfBits, 0, array, index + 188, bytesNumOfBits.Length);
            Array.Copy(bytesSignalDataType, 0, array, index + 190, bytesSignalDataType.Length);
            Array.Copy(bytesValueRangeValid, 0, array, index + 192, bytesValueRangeValid.Length);
            Array.Copy(bytesMinValue, 0, array, index + 194, bytesMinValue.Length);
            Array.Copy(bytesMaxValue, 0, array, index + 202, bytesMaxValue.Length);
            Array.Copy(bytesSampleRate, 0, array, index + 210, bytesSampleRate.Length);

            if (Mdf.IDBlock.Version >= 300)
            {
                var bytesAdditionalOffset = BitConverter.GetBytes(AdditionalByteOffset);

                Array.Copy(bytesAdditionalOffset, 0, array, index + 226, bytesAdditionalOffset.Length);
            }

            index += GetSize();
        }
        internal void WriteComment(byte[] array, ref int index, int blockIndex)
        {
            if (Comment == null)
                return;

            var bytesComment = BitConverter.GetBytes(index);

            Array.Copy(bytesComment, 0, array, blockIndex + 20, bytesComment.Length);

            Comment.Write(array, ref index);
        }
        internal void WriteChannelConversion(byte[] array, ref int index, int blockIndex)
        {
            if (channelConversion == null)
                return;

            var bytesConversionIndex = BitConverter.GetBytes(index);

            Array.Copy(bytesConversionIndex, 0, array, blockIndex + 8, bytesConversionIndex.Length);

            ChannelConversion.Write(array, ref index);
        }
        internal void WriteLongSignalName(byte[] array, ref int index, int blockIndex)
        {
            if (LongSignalName == null || Mdf.IDBlock.Version < 212)
                return;

            var bytesptrLongSignalName = BitConverter.GetBytes(index);

            var offset = 20 + 2 + 32 + 128 + 2 + 2 + 2 + 2 + 16 + 8;

            Array.Copy(bytesptrLongSignalName, 0, array, blockIndex + offset, bytesptrLongSignalName.Length);

            LongSignalName.Write(array, ref index);
        }
        internal void WriteDisplayName(byte[] array, ref int index, int blockIndex)
        {
            if (DisplayName == null || Mdf.IDBlock.Version < 300)
                return;

            var bytesptrDisplayName = BitConverter.GetBytes(index);
            var offset = 20 + 2 + 32 + 128 + 2 + 2 + 2 + 2 + 16 + 8;

            if (LongSignalName != null)
                offset += 4;

            Array.Copy(bytesptrDisplayName, 0, array, blockIndex + offset, bytesptrDisplayName.Length);

            DisplayName.Write(array, ref index);
        }
        internal void WriteNextChannelLink(byte[] array, int index, int blockIndex)
        {
            var bytesNextChannelLink = BitConverter.GetBytes(index);

            Array.Copy(bytesNextChannelLink, 0, array, blockIndex + 4, bytesNextChannelLink.Length);
        }

        internal void ChannelUpdateAddress(int indexDeleted, List<byte> bytes, ulong countDeleted)
        {
            if (Mdf.IDBlock.Version >= 400)
                ChannelUpdateAddressV4(indexDeleted,bytes, countDeleted);
            else
                ChannelUpdateAddressV23(indexDeleted, bytes, (uint)countDeleted);

            ChannelConversion?.ChannelConversionUpdateAddress(indexDeleted, bytes, countDeleted);
        }

        private void ChannelUpdateAddressV23(int indexDeleted, List<byte> bytes, uint countDeleted)
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

        private void ChannelUpdateAddressV4(int indexDeleted, List<byte> bytes, ulong countDeleted)
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
    }
}
