namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class TextBlock : Block
    {
        private string text;

        private TextBlock(Mdf mdf) : base(mdf)
        {
        }

        public string Text
        {
            get { return text; }
            set { SetStringValue(ref text, value, ushort.MaxValue - 4); }
        }

        public static implicit operator string(TextBlock textBlock)
        {
            if (textBlock == null)
                return null;

            return textBlock.Text;
        }
        public static implicit operator TextBlock(string text)
        {
            if (text == null)
                return null;

            return text;
        }

        public static TextBlock Create(Mdf mdf)
        {
            return Create(mdf, "");
        }
        public static TextBlock Create(Mdf mdf, string text)
        {
            return new TextBlock(mdf)
            {
                Identifier = "TX",
                Text = text
            };
        }

        public override string ToString()
        {
            return "{TXBLOCK: " + Text + "}";
        }

        internal static TextBlock Read(Mdf mdf, int position)
        {
            if (mdf.Data.Length < position)
                return null;

            mdf.UpdatePosition(position);

            var block = new TextBlock(mdf);
            block.Read();

            if (block.Size > (ulong)mdf.data.Length || block.Size <= 4)
                return null;

            block.Text = mdf.GetString((int)block.Size - (mdf.position - block.BlockAddress));

            return block;
        }

        internal override ushort GetSize()
        {
            return (ushort)(4 + Mdf.IDBlock.Encoding.GetBytes(Text).Length);
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            var bytesText = Mdf.IDBlock.Encoding.GetBytes(Text);

            Array.Copy(bytesText, 0, array, index + 4, bytesText.Length);

            index += GetSize();
        }
        internal override void Write(List<byte> array)
        {
            var newList = new List<byte>(); 

            var bytesText = Mdf.IDBlock.Encoding.GetBytes(Text);
            
            newList.AddRange(bytesText);

            base.Write(newList);

            array.AddRange(newList);
        }
    }
}
