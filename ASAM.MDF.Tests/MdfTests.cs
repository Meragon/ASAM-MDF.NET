﻿namespace ASAM.MDF.Tests
{
    using System;
    using System.IO;
    using System.Text;

    using ASAM.MDF.Libary;
    using ASAM.MDF.Libary.Types;

    using NUnit.Framework;

    [TestFixture]
    public class MdfTests
    {
        [Test]
        public void BaseWriteReadTest()
        {
            var idBlockByteOrder = ByteOrder.LittleEndian;
            var idBlockFloatingPointFormat = FloatingPointFormat.IEEE754;
            var idBlockVersion = (ushort)330;
            var idBlockFormatIdentifier = idBlockVersion.ToString();
            var idBlockProgramIdentifier = "MdfTests";
            var idBlockCodePage = (ushort)Encoding.UTF8.CodePage;
            var idBlockReserved1 = "R1";
            var idBlockReserved2 = "reserved2";

            var hdBlockDate = DateTime.Now.ToString("dd:MM:yyyy");
            var hdBlockTime = DateTime.Now.ToString("hh:mm:ss");
            var hdBlockAuthor = "Author";
            var hdBlockOrganization = "Organization";
            var hdBlockProject = "Project";
            var hdBlockSubject = "Subject";
            var hdBlockTimestamp = (ulong)1201278007000000000;
            var hdBlockUtcTimeOffset = (short)3;
            var hdBlockTimeQuality = TimeQuality.External;
            var hdBlockTimerIdentification = "local";
            var hdBlockFileCommentText = "hdBlockFileCommentText";
            var hdBlockProgramBlockData = new byte[] { 1, 2, 3, 4, 5 };

            var dataGroupsCount = 5;
            var dataGroupIndex = 2;
            var dataGroupReserved = 3u;

            var channelGroupsCount = 4;
            var channelGroupIndex = 1;

            var channelsCount = 2;
            var channel1ChannelType = ChannelType.Time;
            var channel1SignalName = "time";
            var channel1SignalDesc = "channel1SignalDesc";
            var channel1BitOffset = (ushort)0;
            var channel1NumOfBits = (ushort)64;
            var channel1SignalType = SignalType.IEEE754Double;
            var channel1ValueRange = true;
            var channel1MinValue = -5.98d;
            var channel1MaxValue = 1.254E29d;
            var channel1SampleRate = 0d;
            var channel1AdditionlOffset = (ushort)0;
            
            var channel2ChannelType = ChannelType.Data;
            var channel2SignalName = "data";
            var channel2SignalDesc = "channel2SignalDesc";
            var channel2BitOffset = (ushort)64;
            var channel2NumOfBits = (ushort)16;
            var channel2SignalType = SignalType.Int;
            var channel2ValueRange = false;
            var channel2MinValue = 0d;
            var channel2MaxValue = 0d;
            var channel2SampleRate = 3d;
            var channel2AdditionlOffset = (ushort)0;

            var conversionValueRange = true;
            var conversionMinValue = -129;
            var conversionMaxValue = 129810;
            var conversionPhyUnit = "unit";
            var conversionType = ConversionType.Linear;
            var conversionSize = (ushort)16;
            var conversionData = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            
            // Write.
            var mdf = new Mdf();

            mdf.IDBlock.ByteOrder = idBlockByteOrder;
            mdf.IDBlock.FloatingPointFormat = idBlockFloatingPointFormat;
            mdf.IDBlock.Version = idBlockVersion;
            mdf.IDBlock.FormatIdentifier = idBlockFormatIdentifier;
            mdf.IDBlock.ProgramIdentifier = idBlockProgramIdentifier;
            mdf.IDBlock.CodePage = idBlockCodePage;
            mdf.IDBlock.Reserved1 = idBlockReserved1;
            mdf.IDBlock.Reserved2 = idBlockReserved2;

            mdf.HDBlock.Date = hdBlockDate;
            mdf.HDBlock.Time = hdBlockTime;
            mdf.HDBlock.Author = hdBlockAuthor;
            mdf.HDBlock.Organization = hdBlockOrganization;
            mdf.HDBlock.Project = hdBlockProject;
            mdf.HDBlock.Subject = hdBlockSubject;
            mdf.HDBlock.TimeStamp = hdBlockTimestamp;
            mdf.HDBlock.UTCTimeOffset = hdBlockUtcTimeOffset;
            mdf.HDBlock.TimeQuality = hdBlockTimeQuality;
            mdf.HDBlock.TimerIdentification = hdBlockTimerIdentification;
            mdf.HDBlock.FileComment = TextBlock.Create(mdf, hdBlockFileCommentText);
            mdf.HDBlock.ProgramBlock = ProgramBlock.Create(mdf, hdBlockProgramBlockData);

            for (int i = 0; i < dataGroupsCount; i++)
                mdf.DataGroups.Add(DataGroupBlock.Create(mdf));

            var dataGroup = mdf.DataGroups[dataGroupIndex];
            dataGroup.Reserved = dataGroupReserved;
            
            for (int i = 0; i < channelGroupsCount; i++)
                dataGroup.ChannelGroups.Add(ChannelGroupBlock.Create(mdf));

            var channelGroup = dataGroup.ChannelGroups[channelGroupIndex];
            
            for (int i = 0; i < channelsCount; i++)
                channelGroup.Channels.Add(ChannelBlock.Create(mdf));

            var channel1 = channelGroup.Channels[0];
            channel1.Type = channel1ChannelType;
            channel1.SignalName = channel1SignalName;
            channel1.SignalDescription = channel1SignalDesc;
            channel1.BitOffset = channel1BitOffset;
            channel1.NumberOfBits = channel1NumOfBits;
            channel1.SignalType = channel1SignalType;
            channel1.ValueRange = channel1ValueRange;
            channel1.MinValue = channel1MinValue;
            channel1.MaxValue = channel1MaxValue;
            channel1.SampleRate = channel1SampleRate;
            channel1.AdditionalByteOffset = channel1AdditionlOffset;

            var channel2 = channelGroup.Channels[1];
            channel2.Type = channel2ChannelType;
            channel2.SignalName = channel2SignalName;
            channel2.SignalDescription = channel2SignalDesc;
            channel2.BitOffset = channel2BitOffset;
            channel2.NumberOfBits = channel2NumOfBits;
            channel2.SignalType = channel2SignalType;
            channel2.ValueRange = channel2ValueRange;
            channel2.MinValue = channel2MinValue;
            channel2.MaxValue = channel2MaxValue;
            channel2.SampleRate = channel2SampleRate;
            channel2.AdditionalByteOffset = channel2AdditionlOffset;

            channel1.ChannelConversion = ChannelConversionBlock.Create(mdf);
            channel1.ChannelConversion.PhysicalValueRangeValid = conversionValueRange;
            channel1.ChannelConversion.MinPhysicalValue = conversionMinValue;
            channel1.ChannelConversion.MaxPhysicalValue = conversionMaxValue;
            channel1.ChannelConversion.PhysicalUnit = "unit";
            channel1.ChannelConversion.ConversionType = conversionType;
            channel1.ChannelConversion.AdditionalConversionData = conversionData;

            var bytes = mdf.GetBytes();

            File.WriteAllBytes("C:\\we\\test.dat", bytes);
            
            // Read.
            using (var stream = new MemoryStream(bytes))
            {
                mdf = new Mdf(stream);

                // IDBLOCK.
                Assert.NotNull(mdf.IDBlock);
                Assert.AreEqual(idBlockByteOrder, mdf.IDBlock.ByteOrder);
                Assert.AreEqual(idBlockFloatingPointFormat, mdf.IDBlock.FloatingPointFormat);
                Assert.AreEqual(idBlockVersion, mdf.IDBlock.Version);
                Assert.AreEqual(idBlockFormatIdentifier, mdf.IDBlock.FormatIdentifier);
                Assert.AreEqual(idBlockProgramIdentifier, mdf.IDBlock.ProgramIdentifier);
                Assert.AreEqual(idBlockCodePage, mdf.IDBlock.CodePage);
                Assert.AreEqual(idBlockReserved1, mdf.IDBlock.Reserved1);
                Assert.AreEqual(idBlockReserved2, mdf.IDBlock.Reserved2);

                // HDBLOCK.
                Assert.NotNull(mdf.HDBlock);
                Assert.AreEqual(hdBlockDate, mdf.HDBlock.Date);
                Assert.AreEqual(hdBlockTime, mdf.HDBlock.Time);
                Assert.AreEqual(hdBlockAuthor, mdf.HDBlock.Author);
                Assert.AreEqual(hdBlockOrganization, mdf.HDBlock.Organization);
                Assert.AreEqual(hdBlockProject, mdf.HDBlock.Project);
                Assert.AreEqual(hdBlockSubject, mdf.HDBlock.Subject);
                Assert.AreEqual(hdBlockTimestamp, mdf.HDBlock.TimeStamp);
                Assert.AreEqual(hdBlockUtcTimeOffset, mdf.HDBlock.UTCTimeOffset);
                Assert.AreEqual(hdBlockTimeQuality, mdf.HDBlock.TimeQuality);
                Assert.AreEqual(hdBlockTimerIdentification, mdf.HDBlock.TimerIdentification);

                // HDBLOCK.FileComment.
                Assert.NotNull(mdf.HDBlock.FileComment);
                Assert.AreEqual(hdBlockFileCommentText, mdf.HDBlock.FileComment.Text);

                // HDBLOCK.ProgramBlock.
                Assert.NotNull(mdf.HDBlock.ProgramBlock);
                Assert.NotNull(mdf.HDBlock.ProgramBlock.Data);
                Assert.That(hdBlockProgramBlockData, Is.EquivalentTo(mdf.HDBlock.ProgramBlock.Data));

                // DGBLOCKs.
                Assert.NotNull(mdf.DataGroups);
                Assert.AreEqual(dataGroupsCount, mdf.HDBlock.DataGroupsCount);
                Assert.AreEqual(dataGroupsCount, mdf.DataGroups.Count);

                dataGroup = mdf.DataGroups[dataGroupIndex];

                Assert.AreEqual(dataGroupReserved, dataGroup.Reserved);

                // CGBLOCK.
                Assert.NotNull(dataGroup.ChannelGroups);
                Assert.AreEqual(channelGroupsCount, dataGroup.NumChannelGroups);
                Assert.AreEqual(channelGroupsCount, dataGroup.ChannelGroups.Count);

                channelGroup = dataGroup.ChannelGroups[channelGroupIndex];

                // CNBLOCK.
                Assert.NotNull(channelGroup);
                Assert.AreEqual(channelsCount, channelGroup.NumChannels);
                Assert.AreEqual(channelsCount, channelGroup.Channels.Count);

                channel1 = channelGroup.Channels[0];
                channel2 = channelGroup.Channels[1];

                Assert.AreEqual(channel1ChannelType, channel1.Type);
                Assert.AreEqual(channel1SignalName, channel1.SignalName);
                Assert.AreEqual(channel1SignalDesc, channel1.SignalDescription);
                Assert.AreEqual(channel1BitOffset, channel1.BitOffset);
                Assert.AreEqual(channel1NumOfBits, channel1.NumberOfBits);
                Assert.AreEqual(channel1SignalType, channel1.SignalType);
                Assert.AreEqual(channel1ValueRange, channel1.ValueRange);
                Assert.AreEqual(channel1MinValue, channel1.MinValue);
                Assert.AreEqual(channel1MaxValue, channel1.MaxValue);
                Assert.AreEqual(channel1SampleRate, channel1.SampleRate);
                Assert.AreEqual(channel1AdditionlOffset, channel1.AdditionalByteOffset);

                Assert.AreEqual(channel2ChannelType, channel2.Type);
                Assert.AreEqual(channel2SignalName, channel2.SignalName);
                Assert.AreEqual(channel2SignalDesc, channel2.SignalDescription);
                Assert.AreEqual(channel2BitOffset, channel2.BitOffset);
                Assert.AreEqual(channel2NumOfBits, channel2.NumberOfBits);
                Assert.AreEqual(channel2SignalType, channel2.SignalType);
                Assert.AreEqual(channel2ValueRange, channel2.ValueRange);
                Assert.AreEqual(channel2MinValue, channel2.MinValue);
                Assert.AreEqual(channel2MaxValue, channel2.MaxValue);
                Assert.AreEqual(channel2SampleRate, channel2.SampleRate);
                Assert.AreEqual(channel2AdditionlOffset, channel2.AdditionalByteOffset);

                var conversion = channel1.ChannelConversion;

                // CCBLOCK.
                Assert.NotNull(conversion);
                Assert.AreEqual(conversionValueRange, conversion.PhysicalValueRangeValid);
                Assert.AreEqual(conversionMinValue, conversion.MinPhysicalValue);
                Assert.AreEqual(conversionMaxValue, conversion.MaxPhysicalValue);
                Assert.AreEqual(conversionPhyUnit, conversion.PhysicalUnit);
                Assert.AreEqual(conversionType, conversion.ConversionType);
                Assert.That(conversionData, Is.EquivalentTo(conversion.AdditionalConversionData));
            }
        }
    }
}