namespace ASAM.MDF.Libary
{
    using System;

    using ASAM.MDF.Libary.Types;

    public struct DataRecord
    {
        public DataRecord(ChannelGroupBlock channelGroup, byte[] data)
        {
            ChannelGroup = channelGroup;
            Data = data;
        }

        public ChannelGroupBlock ChannelGroup { get; private set; }
        public byte[] Data { get; set; }

        public object GetValue(ChannelBlock channel)
        {
            if (ChannelGroup.Channels.Contains(channel) == false)
                throw new ArgumentException("channel");

            var byteOffset = channel.AdditionalByteOffset != 0 ? (int)channel.AdditionalByteOffset + channel.BitOffset / 8 : channel.BitOffset / 8;
            var value = (object)null;

            // TODO: BigEndian byte order not supported yet.
            if (channel.Mdf.IDBlock.Version >= 400)
            {
                switch (channel.SignalTypeV4)
                {
                    case SignalTypeV4.Unsigned_LE:
                        {
                            var result = 0u;
                            for (int i = 0; i < channel.NumberOfBits; i++)
                                result |= (uint)((GetBit(Data, channel.BitOffset + (int)channel.AdditionalByteOffset * 8 + i) ? 1 : 0) << i);

                            value = result;
                            break;
                        }

                    case SignalTypeV4.Signed_LE:
                        {
                            var result = 0;
                            for (int i = 0; i < channel.NumberOfBits; i++)
                                result |= (GetBit(Data, channel.BitOffset + (int)channel.AdditionalByteOffset * 8 + i ) ? 1 : 0) << i;

                            var maxValue = Math.Pow(2, channel.NumberOfBits) / 2;
                            if (result > maxValue)
                                result -= (int)(maxValue * 2);

                            value = result;
                            break;
                        }

                    case SignalTypeV4.Float_LE:
                        if (channel.NumberOfBits == 32)
                            value = BitConverter.ToSingle(Data, byteOffset);
                        if (channel.NumberOfBits == 64)
                            value = BitConverter.ToDouble(Data, byteOffset);
                        break;

                    case SignalTypeV4.String:
                        value = ChannelGroup.Mdf.IDBlock.Encoding.GetString(Data, byteOffset, channel.NumberOfBits / 8);
                        break;

                }
            }
            else
            {
                switch (channel.SignalTypeV3)
                {
                    case SignalTypeV3.UInt:
                        {
                            var result = 0u;
                            for (int i = 0; i < channel.NumberOfBits; i++)
                                result |= (uint)((GetBit(Data, channel.BitOffset + i) ? 1 : 0) << i);

                            value = result;
                            break;
                        }

                    case SignalTypeV3.Int:
                        {
                            var result = 0;
                            for (int i = 0; i < channel.NumberOfBits; i++)
                                result |= (GetBit(Data, channel.BitOffset + i) ? 1 : 0) << i;

                            var maxValue = Math.Pow(2, channel.NumberOfBits) / 2;
                            if (result > maxValue)
                                result -= (int)(maxValue * 2);

                            value = result;
                            break;
                        }

                    case SignalTypeV3.IEEE754Float:
                        if (channel.NumberOfBits == 32)
                            value = BitConverter.ToSingle(Data, byteOffset);
                        if (channel.NumberOfBits == 64)
                            value = (float)BitConverter.ToDouble(Data, byteOffset);
                        break;

                    case SignalTypeV3.IEEE754Double:
                        if (channel.NumberOfBits == 32)
                            value = (double)BitConverter.ToSingle(Data, byteOffset);
                        if (channel.NumberOfBits == 64)
                            value = BitConverter.ToDouble(Data, byteOffset);
                        break;

                    case SignalTypeV3.String:
                        var valueString = ChannelGroup.Mdf.IDBlock.Encoding.GetString(Data, byteOffset, channel.NumberOfBits / 8).Replace("\0", "");
                        value = string.IsNullOrEmpty(valueString) ? null : valueString;
                        break;

                }
            }

            if (channel.ChannelConversion != null && value != null)
                return channel.ChannelConversion.AdditionalConversionData.GetPhyValue(Convert.ToDouble(value));

            return value;
        }

        private static bool GetBit(byte[] array, int bit)
        {
            var bytePosition = bit / 8;
            var bitOffset = bit % 8;

            return (array[bytePosition] & (1 << bitOffset)) != 0;   
        }
    }
}
