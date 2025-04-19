using System;
using System.Text;

namespace ASAM.MDF.Libary
{
    using ASAM.MDF.Libary.Types;
    using System.Linq;

    public class ChannelExtensionBlock : Block
    {
        public ExtensionType Type { get; private set; }
        public DimBlockSupplement DimBlockSupplement { get; private set; }
        public VectorCanBlockSupplement VectorCanBlockSupplement { get; private set; }

        public ChannelExtensionBlock(Mdf mdf, ulong position) : base(mdf)
        {
            byte[] data = new byte[Size - 4];
            data = Mdf.Data.Take(data.Length).ToArray();

            Type = (ExtensionType)BitConverter.ToUInt16(data, 0);

            if (Type == ExtensionType.DIM)
            {
                DimBlockSupplement = new DimBlockSupplement(mdf);
            }
            else if (Type == ExtensionType.VectorCAN)
            {
                VectorCanBlockSupplement = new VectorCanBlockSupplement(mdf);
            }
        }
    }
}
