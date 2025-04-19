namespace ASAM.MDF.Libary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using ASAM.MDF.Libary.Types;

    /// <summary>
    /// The IDBLOCK always begins at file position 0 and has a constant length 
    /// of 64 Bytes. It contains information to identify the file. This 
    /// includes information about the source of the file and general 
    /// format specifications.
    /// </summary>
    public class IdentificationBlock
    {
        private ushort codePage;
        private string fileIdentifier;
        private string formatIdentifier;
        private string programIdentifier;
        private string reserved1;
        private string reserved2;

        private IdentificationBlock()
        { }

        public Mdf Mdf { get; private set; }

        public Encoding Encoding { get; private set; }
        public int Size { get; set; }

        /// <summary>
        /// The file identifier always contains "MDF". ("MDF" followed by five spaces)
        /// </summary>
        /// <value>
        /// The file identifier.
        /// </value>
        public string FileIdentifier
        {
            get { return fileIdentifier; }
            set { SetStringValue(ref fileIdentifier, value, 8); }
        }

        /// <summary>
        /// The format identifier is a textual representation of the format 
        /// version for display, e.g. "3.30 " for version 3.3 revision 0
        /// (see section 2 in specification MDF Version).
        /// </summary>
        /// <value>
        /// The format identifier.
        /// </value>
        public string FormatIdentifier
        {
            get { return formatIdentifier; }
            set { SetStringValue(ref formatIdentifier, value, 8); }
        }

        /// <summary>
        /// Program identifier, to identify the program which generated the MDF file
        /// </summary>
        /// <value>
        /// The program identifier.
        /// </value>
        public string ProgramIdentifier
        {
            get { return programIdentifier; }
            set { SetStringValue(ref programIdentifier, value, 8); }
        }

        /// <summary>
        /// Default Byte order used for this file 
        /// (can be overruled for values of a signal in CNBLOCK)
        /// 0 = Little Endian (Intel order)
        /// Any other value = Big Endian (Motorola order)
        /// </summary>
        /// <value>
        /// The byte order.
        /// </value>
        public ByteOrder ByteOrder { get; set; }

        /// <summary>
        /// Gets the floating point format.
        /// (can be overruled for values of a signal in CNBLOCK)
        /// 0 = Floating-point format compliant with IEEE 754 standard
        /// 1 = Floating-point format compliant with G_Float (VAX architecture) (obsolete)
        /// 2 = Floating-point format compliant with D_Float (VAX architecture) (obsolete)
        /// </summary>
        /// <value>
        /// The floating-point format.
        /// </value>
        public FloatingPointFormat FloatingPointFormat { get; set; }

        /// <summary>
        /// Version number of MDF format, i.e. 330 for this version
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public ushort Version { get; set; }

        // TODO: Add supported CodePages
        /// <summary>
        /// Code Page number
        /// The code page used for all strings in the MDF file except of strings in IDBLOCK and string signals (string encoded in a record).
        /// Value = 0: code page is not known.
        /// Value > 0: identification number of extended ASCII code page (includes all ANSI and OEM code pages) as specified by Microsoft, see http://msdn.microsoft.com/en-us/library/dd317756(VS.85).aspx.
        /// The code page number can be used to choose the correct character set for displaying special characters (usually ASCII code ≥ 128) if the writer of the file used a different code page than the reader. Reading tools might not support the display of strings stored with a different code page, or they might only support a selection of (common) code pages.
        /// Valid since version 3.30. Default value: 0 Note: the code page is only for documentation and not required information. It might be ignored by tools reading the file, especially if they support only a MDF version lower than 3.30.
        /// </summary>
        /// <value>
        /// The code page.
        /// </value>
        [MdfVersion(330, 0)]
        public ushort CodePage
        {
            get { return codePage; }
            set
            {
                codePage = value;
                if (value == 1251)
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding = value == 0 ? Encoding.ASCII : Encoding.GetEncoding(value);
            }
        }

        /// <summary>
        /// Gets or sets the reserved1.
        /// </summary>
        /// <value>
        /// The reserved1.
        /// </value>
        public string Reserved1
        {
            get { return reserved1; }
            set { SetStringValue(ref reserved1, value, 2); }
        }

        /// <summary>
        /// Gets or sets the reserved2.
        /// </summary>
        /// <value>
        /// The reserved2.
        /// </value>
        public string Reserved2
        {
            get { return reserved2; }
            set { SetStringValue(ref reserved2, value, 30); }
        }

        public static IdentificationBlock Create(Mdf mdf)
        {
            var block = new IdentificationBlock();

            block.Mdf = mdf;
            block.FileIdentifier = "MDF     ";
            block.FormatIdentifier = "3.30";
            block.ProgramIdentifier = "";
            block.ByteOrder = ByteOrder.LittleEndian;
            block.FloatingPointFormat = FloatingPointFormat.IEEE754;
            block.Version = 330;
            block.CodePage = 0;
            block.Reserved1 = "";
            block.Reserved2 = "";

            return block;
        }
        public static IdentificationBlock Read(Mdf mdf)
        {
            var block = new IdentificationBlock();
            var prevPos = mdf.position;

            block.Mdf = mdf;
            block.fileIdentifier = GetString(mdf, 8).Humanize();
            block.formatIdentifier = GetString(mdf, 8).Humanize();
            block.programIdentifier = GetString(mdf, 8).Humanize();
            block.ByteOrder = (ByteOrder)mdf.ReadU16();
            block.FloatingPointFormat = (FloatingPointFormat)mdf.ReadU16();
            block.Version = mdf.ReadU16();
            block.CodePage = mdf.ReadU16();
            block.reserved1 = GetString(mdf, 2).Humanize();
            block.reserved2 = GetString(mdf, 30).Humanize();
            block.Size  = mdf.position - prevPos;

            return block;
        }

        internal int GetSize()
        {
            return 64;
        }
        internal void Write(byte[] array, ref int index)
        {
            var bytesFileIdentifier = Encoding.UTF8.GetBytes(FileIdentifier);
            var bytesFormatIdentifier = Encoding.UTF8.GetBytes(FormatIdentifier);
            var bytesProgramIdentifier = Encoding.UTF8.GetBytes(ProgramIdentifier);
            var bytesByteOrder = BitConverter.GetBytes((ushort)ByteOrder);
            var bytesFloatingPointFormat = BitConverter.GetBytes((ushort)FloatingPointFormat);
            var bytesVersion = BitConverter.GetBytes(Version);
            var bytesCodePage = BitConverter.GetBytes(CodePage);
            var bytesReserved1 = Encoding.UTF8.GetBytes(Reserved1);
            var bytesReserved2 = Encoding.UTF8.GetBytes(Reserved2);

            ValidateEncodingLength(ref bytesFileIdentifier, 8);
            ValidateEncodingLength(ref bytesFormatIdentifier, 8);
            ValidateEncodingLength(ref bytesProgramIdentifier, 8);
            ValidateEncodingLength(ref bytesReserved1, 2);
            ValidateEncodingLength(ref bytesReserved2, 30);

            Array.Copy(bytesFileIdentifier, 0, array, index, bytesFileIdentifier.Length);
            Array.Copy(bytesFormatIdentifier, 0, array, index + 8, bytesFormatIdentifier.Length);
            Array.Copy(bytesProgramIdentifier, 0, array, index + 16, bytesProgramIdentifier.Length);
            Array.Copy(bytesByteOrder, 0, array, index + 24, bytesByteOrder.Length);
            Array.Copy(bytesFloatingPointFormat, 0, array, index + 26, bytesFloatingPointFormat.Length);
            Array.Copy(bytesVersion, 0, array, index + 28, bytesVersion.Length);
            Array.Copy(bytesCodePage, 0, array, index + 30, bytesCodePage.Length);
            Array.Copy(bytesReserved1, 0, array, index + 32, bytesReserved1.Length);
            Array.Copy(bytesReserved2, 0, array, index + 34, bytesReserved2.Length);

            index += GetSize();
        }

        internal void Write(List<byte> array)
        {
            var bytesFileIdentifier = Encoding.UTF8.GetBytes(FileIdentifier);
            var bytesFormatIdentifier = Encoding.UTF8.GetBytes(FormatIdentifier);
            var bytesProgramIdentifier = Encoding.UTF8.GetBytes(ProgramIdentifier);
            var bytesByteOrder = BitConverter.GetBytes((ushort)ByteOrder);
            var bytesFloatingPointFormat = BitConverter.GetBytes((ushort)FloatingPointFormat);
            var bytesVersion = BitConverter.GetBytes(Version);
            var bytesCodePage = BitConverter.GetBytes(CodePage);
            var bytesReserved1 = Encoding.UTF8.GetBytes(Reserved1);
            var bytesReserved2 = Encoding.UTF8.GetBytes(Reserved2);

            array.AddRange(bytesFileIdentifier);
            array.AddRange(bytesFormatIdentifier);
            array.AddRange(bytesProgramIdentifier);
            array.AddRange(bytesByteOrder);
            array.AddRange(bytesFloatingPointFormat);
            array.AddRange(bytesVersion);
            array.AddRange(bytesCodePage);
            array.AddRange(bytesReserved1);
            array.AddRange(bytesReserved2);
        }

        /// <summary>
        /// Sets the string value.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        /// <param name="maxLength">The maximum length.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">value</exception>
        private void SetStringValue(ref string target, string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                target = value;
            }
            else
            {
                if (value.Length > maxLength)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                target = value;
            }
        }
        private static string GetString(Mdf mdf, int count)
        {
            return Encoding.UTF8.GetString(mdf.Data, mdf.AdvanceIndex(count), count);
        }
        public void ValidateEncodingLength(ref byte[] bytes, int countConstraints)
        {
            if (bytes.Length > countConstraints)
                bytes = bytes.Take(countConstraints).ToArray();
        }
    }
}
