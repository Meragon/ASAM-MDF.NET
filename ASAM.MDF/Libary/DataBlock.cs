using System.Security.Cryptography;

namespace ASAM.MDF.Libary
{
    public class DataBlock : Block
    {
        public DataBlock(Mdf mdf) : base(mdf)
        { }

        public byte[] DataOfBlock { get; private set; }

        public static DataBlock Read(Mdf mdf, int position)
        {
            mdf.UpdatePosition(position);

            var block = new DataBlock(mdf);

            block.Read();
            return block;
        }
        internal override void ReadV4()
        {
            base.ReadV4();

            DataOfBlock = Mdf.ReadBytes((int)Size - 24);
        }
    }
}