﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IxMilia.Iges.Entities;
using Xunit;

namespace IxMilia.Iges.Test
{
    public class IgesReaderTests
    {
        internal static IgesFile CreateFile(string content)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(content.Trim('\r', '\n'));
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                var file = IgesFile.Load(stream);
                return file;
            }
        }

        internal static List<IgesEntity> ParseEntities(string content)
        {
            var file = CreateFile(content);
            return file.Entities;
        }

        internal static IgesEntity ParseLastEntity(string content)
        {
            return ParseEntities(content).Last();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void GlobalParseTest()
        {
            var file = CreateFile(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4Habcd,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H20001225.130811,1.0E-003,500,5HBrettG      2
,7HIxMilia,8,4,13H870508.123456,8Hprotocol;                             G      3
S      1G      3D      0P      0                                        T      1
");
            Assert.Equal(',', file.FieldDelimiter);
            Assert.Equal(';', file.RecordDelimiter);
            Assert.Equal("identifier", file.Identification);
            Assert.Equal(@"C:\path\to\full\filename.igs", file.FullFileName);
            Assert.Equal(@"abcd", file.SystemIdentifier);
            Assert.Equal(@"1.0", file.SystemVersion);
            Assert.Equal(16, file.IntegerSize);
            Assert.Equal(7, file.SingleSize);
            Assert.Equal(22, file.DecimalDigits);
            Assert.Equal(10, file.DoubleMagnitude);
            Assert.Equal(51, file.DoublePrecision);
            Assert.Equal("ident2", file.Identifier);
            Assert.Equal(0.75, file.ModelSpaceScale);
            Assert.Equal(IgesUnits.Centimeters, file.ModelUnits);
            Assert.Null(file.CustomModelUnits);
            Assert.Equal(4, file.MaxLineWeightGraduations);
            Assert.Equal(0.8, file.MaxLineWeight);
            Assert.Equal(new DateTime(2000, 12, 25, 13, 08, 11), file.TimeStamp);
            Assert.Equal(0.001, file.MinimumResolution);
            Assert.Equal(500.0, file.MaxCoordinateValue);
            Assert.Equal("Brett", file.Author);
            Assert.Equal("IxMilia", file.Organization);
            Assert.Equal(IgesVersion.v5_0, file.IgesVersion);
            Assert.Equal(IgesDraftingStandard.BSI, file.DraftingStandard);
            Assert.Equal(new DateTime(1987, 5, 8, 12, 34, 56), file.ModifiedTime);
            Assert.Equal("protocol", file.ApplicationProtocol);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void GlobalParseWithLeadingWhitespaceTest()
        {
            var file = CreateFile(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4Habcd,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H20001225.130811, 1.0E-003,500,5HBretG      2
t,7HIxMilia, 8,4,13H870508.123456, 8Hprotocol;                          G      3
S      1G      3D      0P      0                                        T      1
");
            Assert.Equal(0.001, file.MinimumResolution); // leading space on double
            Assert.Equal(IgesVersion.v5_0, file.IgesVersion); // leading space on int
            Assert.Equal("protocol", file.ApplicationProtocol); // leading space on string
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void GlobalParseWithMissingStringField()
        {
            var file = CreateFile(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4Habcd,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H20001225.130811,1.0E-003,500,5HBrettG      2
,7HIxMilia,8,4,13H870508.123456,;                                       G      3
S      1G      3D      0P      0                                        T      1
");
            Assert.Equal(null, file.ApplicationProtocol);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void GlobalParseWithMissingIntField()
        {
            var file = CreateFile(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4Habcd,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H20001225.130811,1.0E-003,500,5HBrettG      2
,7HIxMilia,8,,13H870508.123456,8Hprotocol;                              G      3
S      1G      3D      0P      0                                        T      1
");
            Assert.Equal(IgesDraftingStandard.None, file.DraftingStandard);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void GlobalParseWithMissingDoubleField()
        {
            var file = CreateFile(@"
0123456789012345678901234567890123456789012345678901234567890123456789--S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4Habcd,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H20001225.130811,,500,5HBrett,7HIxMilG      2
ia,8,4,13H870508.123456,8Hprotocol;                                     G      3
S      1G      3D      0P      0                                        T      1
");
            Assert.Equal(0.0, file.MinimumResolution);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void FileWithNonStandardDelimitersTest()
        {
            var file = CreateFile(@"
                                                                        S      1
1H//1H#/10Hidentifier/12Hfilename.igs#                                  G      1
");
            Assert.Equal('/', file.FieldDelimiter);
            Assert.Equal('#', file.RecordDelimiter);
            Assert.Equal("identifier", file.Identification);
            Assert.Equal("filename.igs", file.FullFileName);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void RoundTripFileWithNonStandardDelimitersTest()
        {
            var file = new IgesFile()
            {
                FieldDelimiter = ';', // inverted field and record delimiters
                RecordDelimiter = ',',
            };
            file.Entities.Add(new IgesLine(new IgesPoint(1.0, 2.0, 3.0), new IgesPoint(4.0, 5.0, 6.0)));

            // write then re-read the file
            using (var ms = new MemoryStream())
            {
                file.Save(ms);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                var file2 = IgesFile.Load(ms);
                Assert.Equal(';', file.FieldDelimiter);
                Assert.Equal(',', file.RecordDelimiter);
                var line = (IgesLine)file.Entities.Single();
                Assert.Equal(new IgesPoint(1.0, 2.0, 3.0), line.P1);
                Assert.Equal(new IgesPoint(4.0, 5.0, 6.0), line.P2);
            }
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void FileWithEmptyFieldOrRecordSpecifierTest()
        {
            var file = CreateFile(@"
,;                                                                      G      1
");
            Assert.Equal(',', file.FieldDelimiter);
            Assert.Equal(';', file.RecordDelimiter);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void FileWithEmptyDelimitersTest()
        {
            var file = CreateFile(@"
,,;                                                                     G      1
");
            Assert.Equal(',', file.FieldDelimiter);
            Assert.Equal(';', file.RecordDelimiter);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void StringContainingDelimiterValuesTest()
        {
            var file = CreateFile(@"
                                                                        S      1
1H,,1H;,6H,;,;,;;                                                       G      1
");
            Assert.Equal(",;,;,;", file.Identification);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void StringWithLeadingAndTrailingWhitespaceTest()
        {
            var file = CreateFile(@"
                                                                        S      1
1H,,1H;,                                                        7H  foo G      1
 ;                                                                      G      2
");
            Assert.Equal("  foo  ", file.Identification);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void MissingStartSectionTest()
        {
            var file = CreateFile(@"
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4Habcd,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H20001225.130811,,500,5HBrett,7HIxMilG      2
ia,8,4,13H870508.123456,8Hprotocol;                                     G      3
S      0G      3D      0P      0                                        T      1
");
            Assert.Equal(',', file.FieldDelimiter);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void MissingGlobalSectionTest()
        {
            var file = CreateFile(@"
                                                                        S      1
S      1G      0D      0P      0                                        T      1
");
            Assert.Equal(',', file.FieldDelimiter);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void OnlyTerminateLineTest()
        {
            var file = CreateFile(@"
S      0G      0D      0P      0                                        T      1
");
            Assert.Equal(',', file.FieldDelimiter);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void IgnoreTextAfterTerminateLineTest()
        {
            var file = CreateFile(@"
S      0G      0D      0P      0                                        T      1

012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void EmptyFileTest()
        {
            var file = CreateFile(string.Empty);
            Assert.Equal(',', file.FieldDelimiter);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadStructureFromEntityTest()
        {
            var line = (IgesLine)ParseEntities(@"
     110       1      -3       0       0                               0D      1
     110       0       0       1       0                                D      2
     110       2       0       0       0                               0D      3
     110       0       0       1       0                                D      4
110,11,22,33,44,55,66;                                                 1P      1
110,77,88,99,10,20,30;                                                 3P      2
").First();
            Assert.Equal(new IgesPoint(11, 22, 33), line.P1);
            var structure = (IgesLine)line.StructureEntity;
            Assert.Equal(new IgesPoint(77, 88, 99), structure.P1);
            Assert.Null(structure.StructureEntity);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadLineFontPatternFromEntityTest()
        {
            var line = new IgesLine();
            Assert.Equal(IgesLineFontPattern.Default, line.LineFont);

            // read enumerated value
            line = (IgesLine)ParseLastEntity(@"
     110       1       0       3       0                        00000000D      1
     110       0       0       1       0                                D      2
110,0.,0.,0.,0.,0.,0.;                                                 1P      1
");
            Assert.Equal(IgesLineFontPattern.Phantom, line.LineFont);

            // read custom value
            line = (IgesLine)ParseLastEntity(@"
     304       1       0       0       0                        00000200D      1
     304       0       0       1       2                                D      2
     110       2       0      -1       0                        00000000D      3
     110       0       0       1       0                                D      4
304,1,23.,1H0;                                                         1P      1
110,0.,0.,0.,0.,0.,0.;                                                 3P      2
");
            var lineFont = (IgesPatternLineFontDefinition)line.CustomLineFont;
            Assert.Equal(23.0, lineFont.SegmentLengths.Single());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadLineWithLevelsTest()
        {
            // single specified value
            var line = (IgesLine)ParseLastEntity(@"
     110       1       0       0      13                        00000000D      1
     110       0       0       1       0                                D      2
110,0.,0.,0.,0.,0.,0.;                                                 1P      1
");
            Assert.Equal(13, line.Levels.Single());

            // multiple values
            line = (IgesLine)ParseLastEntity(@"
     406       1       0       0       0                        00000000D      1
     406       0       0       1       1                                D      2
     110       2       0       0      -1                        00000000D      3
     110       0       0       1       0                                D      4
406,2,13,23;                                                           1P      1
110,0.,0.,0.,0.,0.,0.;                                                 3P      2
");
            Assert.Equal(2, line.Levels.Count);
            Assert.Equal(13, line.Levels.First());
            Assert.Equal(23, line.Levels.Last());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadLineWithAssociatedLabelDisplayTest()
        {
            // specified pointer
            var line = (IgesLine)ParseLastEntity(@"
     410       1       0       0       0                        00000100D      1
     410       0       0       2       1                                D      2
     214       3       0       0       0                        00000100D      3
     214       0       0       1       1                                D      4
     116       4       0       0       0                        00000000D      5
     116       0       0       1       0                                D      6
     402       5       0       0       0                        00000000D      7
     402       0       0       1       5                                D      8
     110       6       0       0       0                       700000000D      9
     110       0       0       1       0                                D     10
410,0,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0,         1P      1
0.,0.;                                                                 1P      2
214,0,0.,0.,0.,0.,0.;                                                  3P      3
116,0.,0.,0.;                                                          5P      4
402,1,1,1.,2.,3.,3,7,5;                                                7P      5
110,0.,0.,0.,0.,0.,0.;                                                 9P      6
");
            Assert.Equal(1, line.LabelDisplay.LabelPlacements.Count);
            var placement = line.LabelDisplay.LabelPlacements.Single();
            Assert.IsType<IgesPerspectiveView>(placement.View);
            Assert.Equal(new IgesPoint(1, 2, 3), placement.Location);
            Assert.Equal(7, placement.Level);
            Assert.IsType<IgesLocation>(placement.Label);

            // no label display pointer
            line = (IgesLine)ParseLastEntity(@"
     110       1       0       0       0                        00000000D      1
     110       0       0       1       0                                D      2
110,0.,0.,0.,0.,0.,0.;                                                 1P      1
");
            Assert.Null(line.LabelDisplay);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadLineWithEntityLabelAndSubscriptTest()
        {
            var line = (IgesLine)ParseLastEntity(@"
     110       1       0       0       0                        00000000D      1
     110       0       0       1       0                abcdefgh      15D      2
110,0.,0.,0.,0.,0.,0.;                                                 1P      1
");
            Assert.Equal("abcdefgh", line.EntityLabel);
            Assert.Equal(15u, line.EntitySubscript);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadCommentFromEntityTest()
        {
            var line = (IgesLine)ParseLastEntity(@"
     110       1       0       0       0                        00000000D      1
     110       0       0       3       0                                D      2
110,0.,0.,0.,0.,0.,0.;This is a really long comment that should        1P      1
be ignored.\nIt also contains things that look like fields, and        1P      2
also contains things that look like 7Hstrings and records;             1P      3
");
            Assert.Equal("This is a really long comment that should be ignored.\nIt also contains things that look like fields, and also contains things that look like 7Hstrings and records;", line.Comment);

            line = (IgesLine)ParseLastEntity(@"
     110       1       0       0       0                        00000000D      1
     110       0       0       1       0                                D      2
110,0.,0.,0.,0.,0.,0.;                                                  P      1
");
            Assert.Null(line.Comment);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadViewFromEntityTest()
        {
            // read view
            var line = (IgesLine)ParseLastEntity(@"
     410       1       0       0       0                        00000100D      1
     410       0       0       1       0                                D      2
     110       2       0       0       0       1                00000000D      3
     110       0       0       1       0                                D      4
410,0,2.,0,0,0,0,0,0;                                                  1P      1
110,0.,0.,0.,0.,0.,0.;                                                 3P      2
");
            Assert.Equal(2.0, line.View.ScaleFactor);

            // ensure null view if not specified
            line = (IgesLine)ParseLastEntity(@"
     110       1       0       0       0                        00000000D      1
     110       0       0       1       0                                D      2
110,0.,0.,0.,0.,0.,0.;                                                 1P      1
");
            Assert.Null(line.View);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadLineWeightFromEntityTest()
        {
            var line = (IgesLine)ParseLastEntity(@"
     110       1       0       0       0                        00000000D      1
     110      42       0       1       0                                D      2
110,0.,0.,0.,0.,0.,0.;                                                 1P      1
");
            Assert.Equal(42, line.LineWeight);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadTransformationMatrixFromEntityTest()
        {
            var line = (IgesLine)ParseLastEntity(@"
     124       1       0       0       0                               0D      1
     124       0       0       1       0                               0D      2
     110       2       0       0       0               1               0D      3
     110       0       3       1       0                               0D      4
124,1,2,3,4,5,6,7,8,9,10,11,12;                                        1P      1
110,11,22,33,44,55,66;                                                 3P      2
".Trim('\r', '\n'));
            var matrix = line.TransformationMatrix;
            Assert.Equal(1.0, matrix.R11);
            Assert.Equal(2.0, matrix.R12);
            Assert.Equal(3.0, matrix.R13);
            Assert.Equal(4.0, matrix.T1);
            Assert.Equal(5.0, matrix.R21);
            Assert.Equal(6.0, matrix.R22);
            Assert.Equal(7.0, matrix.R23);
            Assert.Equal(8.0, matrix.T2);
            Assert.Equal(9.0, matrix.R31);
            Assert.Equal(10.0, matrix.R32);
            Assert.Equal(11.0, matrix.R33);
            Assert.Equal(12.0, matrix.T3);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadCommonPointersTest()
        {
            var file = CreateFile(@"
     402       1       0       0       0                        00000000D      1
     402       0       0       1       5                                D      2
     212       2       0       0       0                        00000100D      3
     212       0       0       1       0                                D      4
     312       3       0       0       0                        00000200D      5
     312       0       0       1       0                                D      6
     116       4       0       0       0                        00000000D      7
     116       0       0       1       0                                D      8
     110       5       0       0       0                        00000000D      9
     110       0       0       1       0                                D     10
402,0;                                                                 1P      1
212,0;                                                                 3P      2
312,0.,0.,1,0.,0.,0,0,0.,0.,0.;                                        5P      3
116,0.,0.,0.;                                                          7P      4
110,0.,0.,0.,0.,0.,0.,3,1,3,5,1,7;                                     9P      5
");
            var line = file.Entities.OfType<IgesLine>().Single();
            Assert.Equal(3, line.AssociatedEntities.Count);
            Assert.IsType<IgesLabelDisplayAssociativity>(line.AssociatedEntities[0]);
            Assert.IsType<IgesGeneralNote>(line.AssociatedEntities[1]);
            Assert.IsType<IgesTextDisplayTemplate>(line.AssociatedEntities[2]);
            Assert.IsType<IgesLocation>(line.Properties.Single());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadWithDifferningNewlinesTest()
        {
            var file = new IgesFile();
            file.Entities.Add(new IgesLine(new IgesPoint(0.0, 0.0, 0.0), new IgesPoint(1.0, 1.0, 1.0)));

            using (var ms = new MemoryStream())
            {
                file.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(ms))
                {
                    var text = reader.ReadToEnd();

                    // verify file reading with LF
                    text = text.Replace("\r", "");
                    var lengthLF = 0L;
                    using (var ms2 = new MemoryStream())
                    {
                        using (var writer = new StreamWriter(ms2, Encoding.ASCII, bufferSize: 1024, leaveOpen: true))
                        {
                            writer.Write(text);
                        }

                        ms2.Seek(0, SeekOrigin.Begin);
                        lengthLF = ms2.Length;
                        var fileLF = IgesFile.Load(ms2);
                        Assert.Equal(1, fileLF.Entities.Count);
                    }

                    // verify file reading with CRLF
                    text = text.Replace("\n", "\r\n");
                    var lengthCRLF = 0L;
                    using (var ms2 = new MemoryStream())
                    {
                        using (var writer = new StreamWriter(ms2, Encoding.ASCII, bufferSize: 1024, leaveOpen: true))
                        {
                            writer.Write(text);
                        }

                        ms2.Seek(0, SeekOrigin.Begin);
                        lengthCRLF = ms2.Length;
                        var fileLF = IgesFile.Load(ms2);
                        Assert.Equal(1, fileLF.Entities.Count);
                    }

                    // verify that the file lengths were non-zero and not equal
                    Assert.NotEqual(0L, lengthLF);
                    Assert.NotEqual(0L, lengthCRLF);
                    Assert.NotEqual(lengthLF, lengthCRLF);
                }
            }
        }
    }
}
