namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;
    using System.Text;

    using ASAM.MDF.Libary.Types;

    public class ChannelBlock : Block, INext<ChannelBlock>
    {
        private const int MIN_VERSION_LONG_SIGNAL_NAME = 212;
        private const int MIN_VERSION_DISPLAY_NAME = 300;
        private const int MIN_VERSION_ADDITIONAL_BYTE_OFFSET = 300;

        private ulong ptrNextChannelBlock;

        public ulong ConponentAddress { get; private set; }
        public ulong TextBlockChanelName { get; private set; }

        private ulong ptrChannelConversionBlock;
        private ulong ptrDataBlockSignal;
        private ulong ptrUnit;
        private ulong ptrTextBlockComment;
        private ulong ptrAttachment;
        private ulong ptrDefaultDGBlock;
        private ulong ptrDefaultCGBlock;
        private ulong ptrDefaultCurrentChanelBlock;
        private ulong ptrChannelExtensionBlock;
        private ulong ptrChannelDependencyBlock;
        private ulong ptrChannelComment;
        private ulong ptrLongSignalName;
        private ulong ptrDisplayName;
        private string signalName;
        private string signalDescription;

        private ChannelConversionBlock channelConversion;
        private ChannelBlock next;

        private ChannelBlock(Mdf mdf) : base(mdf)
        {
        }

        public ChannelBlock Next
        {
            get
            {
                if (next == null && ptrNextChannelBlock != 0)
                    next = Read(Mdf, ptrNextChannelBlock);

                return next;
            }
        }
        public ChannelConversionBlock ChannelConversion
        {
            get
            {
                return channelConversion;
            }
            set { channelConversion = value; }
        }
        public ChannelExtensionBlock SourceDepending { get; private set; }
        public ChannelDependencyBlock Dependency { get; private set; }
        public TextBlock Comment { get; private set; }
        public ChannelTypeV3 TypeV3 { get; set; }
        public ChannelTypeV4 TypeV4 { get; set; }

        private byte ptrSyncType;
        private byte ptrDataType;

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

        public static ChannelBlock Create(Mdf mdf)
        {
            return new ChannelBlock(mdf)
            {
                Identifier = "CN",
                SignalName = "",
                SignalDescription = "",
            };
        }
        public static ChannelBlock Read(Mdf mdf, ulong position)
        {
            mdf.UpdatePosition(position);

            var block = new ChannelBlock(mdf);
            block.Read();

            block.next = null;
            block.SourceDepending = null;
            block.Dependency = null;
            block.Comment = null;

            if (mdf.IDBlock.Version >= 400)
            {
                ReadV4(mdf, block);
                return block;
            }
            block.ptrNextChannelBlock = mdf.ReadU32();
            block.ptrChannelConversionBlock = mdf.ReadU32();
            block.ptrChannelExtensionBlock = mdf.ReadU32();
            block.ptrChannelDependencyBlock = mdf.ReadU32();
            block.ptrChannelComment = mdf.ReadU32();
            block.TypeV3 = (ChannelTypeV3)mdf.ReadU16();
            block.SignalName = mdf.GetString(32);
            block.SignalDescription = mdf.GetString(128);
            block.BitOffset = mdf.ReadU16();
            block.NumberOfBits = mdf.ReadU16();
            block.SignalTypeV3 = (SignalTypeV3)mdf.ReadU16();
            block.ValueRange = mdf.ReadBoolean();

            if (block.ValueRange)
            {
                block.MinValue = mdf.ReadDouble();
                block.MaxValue = mdf.ReadDouble();
            }
            else
            {
                mdf.UpdatePosition(mdf.position + 16);
            }

            block.SampleRate = mdf.ReadDouble();

            if (mdf.IDBlock.Version >= MIN_VERSION_LONG_SIGNAL_NAME)
                block.ptrLongSignalName = mdf.ReadU32();

            if (mdf.IDBlock.Version >= MIN_VERSION_DISPLAY_NAME)
                block.ptrDisplayName = mdf.ReadU32();

            if (mdf.IDBlock.Version >= MIN_VERSION_ADDITIONAL_BYTE_OFFSET)
                block.AdditionalByteOffset = mdf.ReadU16();

            if (block.TextBlockChanelName != 0)
                block.LongSignalName = TextBlock.Read(mdf, block.TextBlockChanelName);

            if (block.ptrUnit != 0)
                block.Unit = TextBlock.Read(mdf, block.ptrUnit);

            if (block.ptrTextBlockComment != 0)
                block.Comment = TextBlock.Read(mdf, block.ptrTextBlockComment);

            if (block.ptrLongSignalName != 0)
                block.LongSignalName = TextBlock.Read(mdf, block.ptrLongSignalName);

            if (block.channelConversion == null && block.ptrChannelConversionBlock != 0)
                block.ChannelConversion = ChannelConversionBlock.Read(block.Mdf, block.ptrChannelConversionBlock);
            //if (block.ptrChannelExtensionBlock != 0)
            //{
            //    if (mdf.IDBlock.Version == 400)

            //    block.SourceDepending = new ChannelExtensionBlock(mdf, block.ptrChannelExtensionBlock);
            //}

            return block;
        }

        private static void ReadV4(Mdf mdf, ChannelBlock block)
        {
            block.ptrNextChannelBlock = mdf.ReadU64();
            block.ConponentAddress = mdf.ReadU64();
            block.TextBlockChanelName = mdf.ReadU64();
            block.ptrChannelExtensionBlock = mdf.ReadU64();
            block.ptrChannelConversionBlock = mdf.ReadU64();
            block.ptrDataBlockSignal = mdf.ReadU64();
            block.ptrUnit = mdf.ReadU64();
            block.ptrTextBlockComment = mdf.ReadU64();
            //block.ptrAttachment = mdf.ReadU64();
            //block.ptrDefaultDGBlock = mdf.ReadU64();
            //block.ptrDefaultCGBlock = mdf.ReadU64();
            //block.ptrDefaultCurrentChanelBlock = mdf.ReadU64();
            block.TypeV4 = (ChannelTypeV4)mdf.ReadByte();
            block.ptrSyncType = mdf.ReadByte();
            block.SignalTypeV4 = (SignalTypeV4)mdf.ReadByte();
            block.BitOffset = mdf.ReadByte();
            block.AdditionalByteOffset = mdf.ReadU32();
            block.NumberOfBits = (ushort)mdf.ReadU32();
            block.ChannelFlags = mdf.ReadU32();
            block.InvalidBitPos = mdf.ReadU32();
            block.Precision = mdf.ReadByte();
            block.Reserved1 = mdf.ReadByte();
            block.AttachmentCount = mdf.ReadU16();
            block.ValRangeMin = mdf.ReadDouble();
            block.ValRangeMax = mdf.ReadDouble();
            block.LimitMin = mdf.ReadDouble();
            block.LimitMax = mdf.ReadDouble();
            block.LimitMinExt = mdf.ReadDouble();
            block.LimitMaxExt = mdf.ReadDouble();

            if (block.TextBlockChanelName != 0)
                block.LongSignalName = TextBlock.Read(mdf, block.TextBlockChanelName);

            if (block.ptrUnit != 0)
                block.Unit = TextBlock.Read(mdf, block.ptrUnit);

            if (block.ptrTextBlockComment != 0)
                block.Comment = TextBlock.Read(mdf, block.ptrTextBlockComment);

            if (block.ptrLongSignalName != 0)
                block.LongSignalName = TextBlock.Read(mdf, block.ptrLongSignalName);

            if (block.channelConversion == null && block.ptrChannelConversionBlock != 0)
                block.ChannelConversion = ChannelConversionBlock.Read(block.Mdf, block.ptrChannelConversionBlock);
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

            if (Mdf.IDBlock.Version >= 212)
            {
                // TODO: LongSignalName.
            }

            if (Mdf.IDBlock.Version >= 300)
            {
                // TODO: DisplayName.
                var bytesAdditionalOffset = BitConverter.GetBytes(AdditionalByteOffset);

                Array.Copy(bytesAdditionalOffset, 0, array, index + 226, bytesAdditionalOffset.Length);
            }

            index += GetSize();
        }
        internal void WriteChannelConversion(byte[] array, ref int index, int blockIndex)
        {
            if (channelConversion == null)
                return;

            var bytesConversionIndex = BitConverter.GetBytes(index);

            Array.Copy(bytesConversionIndex, 0, array, blockIndex + 8, bytesConversionIndex.Length);

            ChannelConversion.Write(array, ref index);
        }
        internal void WriteNextChannelLink(byte[] array, int index, int blockIndex)
        {
            var bytesNextChannelLink = BitConverter.GetBytes(index);

            Array.Copy(bytesNextChannelLink, 0, array, blockIndex + 4, bytesNextChannelLink.Length);
        }
    }
}
