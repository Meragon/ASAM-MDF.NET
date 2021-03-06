﻿namespace ASAM.MDF.Libary
{
    using System;
    using System.IO;
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

        internal static TextBlock Read(Mdf mdf, Stream stream)
        {
            var block = new TextBlock(mdf);
            block.Read(stream);

            var data = new byte[block.Size - 4];
            var read = stream.Read(data, 0, data.Length);

            if (read != data.Length)
                throw new FormatException();

            block.text = mdf.IDBlock.Encoding.GetString(data, 0, data.Length);

            return block;
        }

        internal override ushort GetSize()
        {
            return (ushort)(4 + Text.Length);
        }
        internal override void Write(byte[] array, ref int index)
        {
            base.Write(array, ref index);

            var bytesText = Mdf.IDBlock.Encoding.GetBytes(Text);

            Array.Copy(bytesText, 0, array, index + 4, bytesText.Length);

            index += GetSize();
        }
    }
}
