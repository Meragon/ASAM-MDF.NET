namespace ASAM.MDF.Libary
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;

    public class ProgramBlock : Block
    {
        private byte[] data;

        private ProgramBlock(Mdf mdf) : base(mdf)
        { }

        public byte[] Data
        {
            get { return data; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                data = value;
            }
        }

        public static ProgramBlock Create(Mdf mdf)
        {
            return Create(mdf, new byte[0]);
        }
        public static ProgramBlock Create(Mdf mdf, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            return new ProgramBlock(mdf)
            {
                Identifier = "PR",
                Data = data
            };
        }

        public override string ToString()
        {
            return "{PRBLOCK: Data[" + Data.Length + "]}";
        }

        internal static ProgramBlock Read(Mdf mdf, int position)
        {
            mdf.UpdatePosition(position);

            var block = new ProgramBlock(mdf);

            block.Read();
            return block;
        }
        internal override void ReadV23()
        {
            base.ReadV23();

            Array.Copy(Mdf.Data, Mdf.position, Data, 0, (int)Size);
        }

        internal override ushort GetSize()
        {
            return (ushort)((int)Size + Data.Length);
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            Array.Copy(Data, 0, array, index + 4, Data.Length);

            index += GetSize();
        }
    }
}
