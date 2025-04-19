using System.Collections.Generic;

namespace ASAM.MDF.Libary
{
    public class SourceInformation : Block
    {
        internal PointerAddress<ulong> ptrTextNameV4;
        internal PointerAddress<ulong> ptrTextPathV4;
        internal PointerAddress<ulong> ptrTextCommentV4;

        public byte SourceType { get; private set; }
        public byte BusType { get; private set; }
        public byte SiFlags { get; private set; }
        public byte Reserved1 { get; private set; }
        public TextBlock FileComment { get; private set; }
        public TextBlock TextBlockName { get; private set; }
        public TextBlock TextBlockPath { get; private set; }

        public SourceInformation(Mdf mdf) : base(mdf)
        { }

        public static SourceInformation Read(Mdf mdf, int position)
        {
            mdf.UpdatePosition(position);

            var block = new SourceInformation(mdf);

            block.Read();
            return block;
        }
        internal override void ReadV4()
        {
            base.ReadV4();

            listAddressesV4 = new List<PointerAddress<ulong>>();

            ptrTextNameV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), 24);
            ptrTextPathV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrTextNameV4.offset + 8);
            ptrTextCommentV4 = new PointerAddress<ulong>(Mdf.ReadU64().ValidateAddress(Mdf), ptrTextPathV4.offset + 8);
            SourceType = Mdf.ReadByte();
            BusType = Mdf.ReadByte();
            SiFlags = Mdf.ReadByte();
            Reserved1 = Mdf.ReadByte();

            listAddressesV4.AddRange(new PointerAddress<ulong>[]
            {
                ptrTextNameV4,
                ptrTextPathV4,
                ptrTextCommentV4,
            });

             if (ptrTextCommentV4.address != 0)
                FileComment = TextBlock.Read(Mdf, (int)ptrTextCommentV4.address);

             if (ptrTextNameV4.address != 0)
                TextBlockName = TextBlock.Read(Mdf, (int)ptrTextNameV4.address);

             if (ptrTextPathV4.address != 0)
                TextBlockPath = TextBlock.Read(Mdf, (int)ptrTextPathV4.address);
        }
        internal void SourceInformationUpdateAddress(int indexDeleted, List<byte> bytes, ulong countDeleted)
        {
            if (Mdf.IDBlock.Version >= 400)
                SourceInformationUpdateAddressV4(indexDeleted, bytes, countDeleted);
        }

        private void SourceInformationUpdateAddressV4(int indexDeleted, List<byte> bytes, ulong countDeleted)
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
