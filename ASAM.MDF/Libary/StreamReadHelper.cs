using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASAM.MDF.Libary
{
    public static class StreamReadHelper
    {
        public static byte ReadByte(this Mdf mdf)
        {
            return mdf.data[mdf.position++];
        }
        
        public static ushort ReadU16(this Mdf mdf)
        {
            var value = BitConverter.ToUInt16(mdf.data, mdf.position);

            mdf.position += 2;

            return value;
        }
        public static uint ReadU32(this Mdf mdf)
        {
            var value = BitConverter.ToUInt32(mdf.data, mdf.position);

            mdf.position += 4;

            return value;
        }
        public static ulong ReadU64(this Mdf mdf)
        {
            var value = BitConverter.ToUInt64(mdf.data, mdf.position);

            mdf.position += 8;

            return value;
        }
        public static short Read16(this Mdf mdf)
        {
            var value = BitConverter.ToInt16(mdf.data, mdf.position);

            mdf.position += 2;

            return value;
        }
        public static int Read32(this Mdf mdf)
        {
            var value = BitConverter.ToInt32(mdf.data, mdf.position);

            mdf.position += 4;

            return value;
        }
        public static long Read64(this Mdf mdf)
        {
            var value = BitConverter.ToInt64(mdf.data, mdf.position);

            mdf.position += 8;

            return value;
        }
        public static char ReadChar(this Mdf mdf)
        {
            var value = mdf.IDBlock.Encoding.GetString(mdf.data, mdf.position, 2);

            mdf.position += 2;

            return value[0];
        }


        public static bool ReadBoolean(this Mdf mdf)
        {
            var value = BitConverter.ToBoolean(mdf.Data, mdf.position);

            mdf.position += 2;

            return value;
        }
        public static double ReadDouble(this Mdf mdf)
        {
            var value = BitConverter.ToDouble(mdf.Data, mdf.position);

            mdf.position += 8;

            return value;
        }


        public static void UpdatePosition(this Mdf mdf, int address)
        {
            mdf.position = address;
        }
        public static int AdvanceIndex(this Mdf mdf, int count)
        {
            var index = mdf.position;

            mdf.position += count;

            return index;
        }

        public static string GetString(this Mdf mdf, int count)
        {
            var value = mdf.IDBlock.Encoding.GetString(mdf.Data, mdf.AdvanceIndex(count), count);
            var indexLastSymbol = value.IndexOf('\0');
            if (indexLastSymbol != -1)
                value = value.Remove(indexLastSymbol);
            
            return value;
        }

        public static ushort ValidateAddress(this ushort value, Mdf mdf)
        {
            if (value >= mdf.Data.Length)
                return 0;
            return value;
        }
        public static uint ValidateAddress(this uint value, Mdf mdf)
        {
            if (value >= mdf.Data.Length)
                return 0;
            return value;
        }
        public static ulong ValidateAddress(this ulong value, Mdf mdf)
        {
            if (value >= (ulong)mdf.Data.Length)
                return 0;
            return value;
        }
        public static short ValidateAddress(this short value, Mdf mdf)
        {
            if (value >= mdf.Data.Length)
                return 0;
            return value;
        }
        public static int ValidateAddress(this int value, Mdf mdf)
        {
            if (value >= mdf.Data.Length)
                return 0;
            return value;
        }
        public static long ValidateAddress(this long value, Mdf mdf)
        {
            if (value >= mdf.Data.Length)
                return 0;
            return value;
        }
    }
}
