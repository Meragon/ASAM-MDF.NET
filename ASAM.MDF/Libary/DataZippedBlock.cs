using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASAM.MDF.Libary
{
    internal class DataZippedBlock : Block
    {
        private int indexStart; // start compressed data index
        public DataZippedBlock(Mdf mdf) : base(mdf)
        { }

        public string OriginalType { get; private set; }
        public ZipTypes ZipType { get; private set; }
        public byte Reserved1 { get; private set; }
        public uint ZipParameter { get; private set; }
        public ulong UncompressDataLength { get; private set; }
        public ulong CompressDataLength { get; private set; }
        public byte ZHeader { get; private set; }
        public ZCompressionInfo CompressionInfo { get; private set; }

        public static DataZippedBlock Read(Mdf mdf, int position)
        {
            mdf.UpdatePosition(position);

            var block = new DataZippedBlock(mdf);
            block.Read();

            block.OriginalType = mdf.GetString(2);
            block.ZipType = (ZipTypes)mdf.ReadByte();
            block.Reserved1 = mdf.ReadByte();
            block.ZipParameter = mdf.ReadU32();
            block.UncompressDataLength = mdf.ReadU64();
            block.CompressDataLength = mdf.ReadU64();
            block.ZHeader = mdf.ReadByte();
            block.CompressionInfo = (ZCompressionInfo)mdf.ReadByte();
            block.indexStart = mdf.position;

            block.DecompressData();

            return block;
        }

        private void DecompressData()
        {
            var data = Mdf.Data;
            var diffDataLength = UncompressDataLength - CompressDataLength;
            var transposedData = new byte[UncompressDataLength];

            Array.Resize(ref Mdf.data, Mdf.data.Length + (int)diffDataLength);

            unsafe
            {
                fixed (byte* dataT = Mdf.data)
                {
                    LibDeflateDLL.Decompress((IntPtr)(dataT + indexStart), CompressDataLength, transposedData, UncompressDataLength);
                }
            }

            for (int i = 0, k = (int)indexStart; i < transposedData.Length; i++, k++)
            {
                var trData = transposedData[i];
                Mdf.data[k] = trData;
            }
        }
    }
}
