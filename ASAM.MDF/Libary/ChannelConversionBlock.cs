namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using ASAM.MDF.Libary.Types;

    public class ChannelConversionBlock : Block
    {
        internal PointerAddress<uint> ptrTextBlockName;
        internal PointerAddress<uint> ptrTextBlockUnit;
        internal PointerAddress<uint> ptrFileComment;
        internal PointerAddress<uint> ptrInverseConversion;
        
        internal PointerAddress<ulong> ptrTextBlockNameV4;
        internal PointerAddress<ulong> ptrTextBlockUnitV4;
        internal PointerAddress<ulong> ptrFileCommentV4;
        internal PointerAddress<ulong> ptrInverseConversionV4;

        private string physicalUnit;

        private ChannelConversionBlock(Mdf mdf) : base(mdf)
        {
            AdditionalConversionData = new ConversionData(this);
        }

        public bool PhysicalValueRangeValid { get; set; }
        public double MinPhysicalValue { get; set; }
        public double MaxPhysicalValue { get; set; }
        public string PhysicalUnit
        {
            get { return physicalUnit; }
            set { SetStringValue(ref physicalUnit, value, 20); }
        }
        public ConversionType ConversionType { get; set; }
        public ConversionType4 ConversionType4 { get; private set; }
        public byte Precision { get; private set; }
        public ushort Flags { get; private set; }
        public ushort SizeInformation { get; private set; }
        public ushort ValParamCount { get; private set; }
        public ConversionData AdditionalConversionData { get; internal set; }
        public TextBlock FileComment { get; private set; }
        public TextBlock ConversionUnit { get; private set; }
        public TextBlock ConversionName { get; private set; }
        public List<TextBlock> ConvTabT { get; internal set; }
        public List<double> ConvTabTValue { get; internal set; }

        public static ChannelConversionBlock Create(Mdf mdf)
        {
            return new ChannelConversionBlock(mdf)
            {
                Identifier = "CC",
                PhysicalUnit = "",
            };
        }
        internal static ChannelConversionBlock Read(Mdf mdf, int position)
        {
            mdf.position = position;

            var block = new ChannelConversionBlock(mdf);

            block.Read();
            return block;
        }

        internal override void ReadV23()
        {
            base.ReadV23();

            PhysicalValueRangeValid = Mdf.Read16() != 0;
            MinPhysicalValue = Mdf.ReadDouble();
            MaxPhysicalValue = Mdf.ReadDouble();
            PhysicalUnit = Mdf.GetString(20);
            ConversionType = (ConversionType)Mdf.ReadU16();
            SizeInformation = Mdf.ReadU16();

            if (SizeInformation > 0)
            {
                AdditionalConversionData.Data = new byte[ConversionData.GetEstimatedParametersSize(ConversionType)];
                Array.Copy(Mdf.Data, Mdf.position, AdditionalConversionData.Data, 0, AdditionalConversionData.Data.Length);
            }
        }

        internal override void ReadV4()
        {
            base.ReadV4();

            listAddressesV4 = new List<PointerAddress<ulong>>();

            ptrTextBlockNameV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), 24);
            ptrTextBlockUnitV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf),ptrTextBlockName.offset + 8);
            ptrFileCommentV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrTextBlockUnit.offset + 8);
            ptrInverseConversionV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrFileComment.offset + 8);
            var lastPosAddress = Mdf.position;

            if (LinksCount > 4)
                Mdf.UpdatePosition(lastPosAddress + ((int)LinksCount - 4) * 8);

            ConversionType4 = (ConversionType4)Mdf.ReadByte();
            Precision = Mdf.ReadByte();
            Flags = Mdf.ReadU16();
            SizeInformation = Mdf.ReadU16();
            ValParamCount = Mdf.ReadU16();
            MinPhysicalValue = Mdf.ReadDouble();
            MaxPhysicalValue = Mdf.ReadDouble();

            listAddressesV4.AddRange(new PointerAddress<ulong>[]
            {
                ptrTextBlockNameV4,
                ptrTextBlockUnitV4,
                ptrFileCommentV4,
                ptrInverseConversionV4,
            });

            var indexPointer = Mdf.position;

            AdditionalConversionData.Data = new byte[ValParamCount * 8];

            Array.Copy(Mdf.Data, indexPointer, AdditionalConversionData.Data, 0, AdditionalConversionData.Data.Length);

            if (ptrFileCommentV4.address != 0)
                FileComment = TextBlock.Read(Mdf, (int)ptrFileCommentV4.address);

            if (ptrTextBlockNameV4.address != 0)
                ConversionName = TextBlock.Read(Mdf, (int)ptrTextBlockNameV4.address);

            if (ptrTextBlockUnitV4.address != 0)
                ConversionUnit = TextBlock.Read(Mdf, (int)ptrTextBlockUnitV4.address);
        }
        internal override ushort GetSize()
        {
            ushort size = 46;

            if (AdditionalConversionData.Data != null)
                size += (ushort)AdditionalConversionData.Data.Length;

            return size;
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            var bytesValueRange = BitConverter.GetBytes(PhysicalValueRangeValid);
            var bytesMinValue = BitConverter.GetBytes(MinPhysicalValue);
            var bytesMaxValue = BitConverter.GetBytes(MaxPhysicalValue);
            var bytesPhyUnit = Mdf.IDBlock.Encoding.GetBytes(PhysicalUnit);
            var bytesConversionType = BitConverter.GetBytes((ushort)ConversionType);

            ValidateEncodingLength(ref bytesPhyUnit, 20);
            
            Array.Copy(bytesValueRange, 0, array, index + 4, bytesValueRange.Length);
            Array.Copy(bytesMinValue, 0, array, index + 6, bytesMinValue.Length);
            Array.Copy(bytesMaxValue, 0, array, index + 14, bytesMaxValue.Length);
            Array.Copy(bytesPhyUnit, 0, array, index + 22, bytesPhyUnit.Length);
            Array.Copy(bytesConversionType, 0, array, index + 42, bytesConversionType.Length);

            if (AdditionalConversionData.Data != null && AdditionalConversionData.Data.Length > 0)
            {
                var bytesSizeInformation = BitConverter.GetBytes(ConversionData.GetEstimatedParametersCount(ConversionType));

                Array.Copy(bytesSizeInformation, 0, array, index + 44, bytesSizeInformation.Length);
                Array.Copy(AdditionalConversionData.Data, 0, array, index + 46, AdditionalConversionData.Data.Length);
            }

            index += GetSize();
        }

        internal void ChannelConversionUpdateAddress(int indexDeleted, List<byte> bytes, ulong countDeleted)
        {
            if (Mdf.IDBlock.Version >= 400)
                ChannelConversionUpdateAddressV4(indexDeleted, bytes, countDeleted);
        }

        private void ChannelConversionUpdateAddressV4(int indexDeleted, List<byte> bytes, ulong countDeleted)
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
