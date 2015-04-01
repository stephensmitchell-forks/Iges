﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using IxMilia.Iges.Entities;
using Xunit;

namespace IxMilia.Iges.Test
{
    public class IgesWriterTests
    {
        private static void VerifyFileText(IgesFile file, string expected, Action<string, string> verifier)
        {
            var stream = new MemoryStream();
            file.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = stream.ToArray();
            var actual = Encoding.ASCII.GetString(bytes);
            verifier(expected.Trim('\r', '\n'), actual.Trim('\r', '\n'));
        }

        private static void VerifyFileExactly(IgesFile file, string expected)
        {
            VerifyFileText(file, expected, (ex, ac) => Assert.Equal(ex, ac));
        }

        private static void VerifyFileContains(IgesFile file, string expected)
        {
            VerifyFileText(file, expected, (ex, ac) => Assert.True(ac.Contains(ex)));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Writing)]
        public void WriteEmptyFileTest()
        {
            var date = new DateTime(2000, 12, 25, 13, 8, 5);
            var file = new IgesFile()
            {
                ModifiedTime = date,
                TimeStamp = date
            };
            VerifyFileExactly(file, @"
                                                                        S      1
1H,,1H;,,,,,32,8,23,11,52,,1.,1,,0,1.,15H20001225.130805,1E-10,0.,,,11, G      1
0,15H20001225.130805,;                                                  G      2
S      1G      2D      0P      0                                        T      1
");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Writing)]
        public void WriteLineWithSpanningParametersTest()
        {
            var file = new IgesFile();
            file.Entities.Add(new IgesLine()
            {
                P1 = new IgesPoint(1.1234512345, 2.1234512345, 3.1234512345),
                P2 = new IgesPoint(4.1234512345, 5.1234512345, 6.1234512345),
                Color = IgesColorNumber.Green
            });
            VerifyFileContains(file, @"
     110       1       0       0       0                               0D      1
     110       0       3       1       0                                D      2
110,1.1234512345,2.1234512345,3.1234512345,4.1234512345,               1P      1
5.1234512345,6.1234512345;                                             1P      2
");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Writing)]
        public void WriteLineWithTransformationMatrixTest()
        {
            var file = new IgesFile();
            var trans = new IgesTransformationMatrix()
            {
                R11 = 1.0,
                R12 = 2.0,
                R13 = 3.0,
                T1 = 4.0,
                R21 = 5.0,
                R22 = 6.0,
                R23 = 7.0,
                T2 = 8.0,
                R31 = 9.0,
                R32 = 10.0,
                R33 = 11.0,
                T3 = 12.0
            };
            var line = new IgesLine()
            {
                P1 = new IgesPoint(1, 2, 3),
                P2 = new IgesPoint(4, 5, 6),
                TransformationMatrix = trans,
            };
            file.Entities.Add(line);
            VerifyFileContains(file, @"
     124       1       0       0       0                               0D      1
     124       0       0       0       0                                D      2
     110       2       0       0       0               1               0D      3
     110       0       0       1       0                                D      4
124,1.,2.,3.,4.,5.,6.,7.,8.,9.,10.,11.,12.;                            1P      1
110,1.,2.,3.,4.,5.,6.;                                                 3P      2
");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Writing)]
        public void WriteSpecificGlobalValuesTest()
        {
            var file = new IgesFile()
            {
                FieldDelimiter = ',',
                RecordDelimiter = ';',
                Identification = "identifier",
                FullFileName = @"C:\path\to\full\filename.igs",
                SystemIdentifier = "abcd",
                SystemVersion = "1.0",
                IntegerSize = 16,
                SingleSize = 7,
                DecimalDigits = 22,
                DoubleMagnitude = 10,
                DoublePrecision = 51,
                Identifier = "ident2",
                ModelSpaceScale = 0.75,
                ModelUnits = IgesUnits.Centimeters,
                CustomModelUnits = null,
                MaxLineWeightGraduations = 4,
                MaxLineWeight = 0.8,
                TimeStamp = new DateTime(2000, 12, 25, 13, 8, 11),
                MinimumResolution = 0.001,
                MaxCoordinateValue = 500.0,
                Author = "Brett",
                Organization = "IxMilia",
                IgesVersion = IgesVersion.v5_0,
                DraftingStandard = IgesDraftingStandard.BSI,
                ModifiedTime = new DateTime(1987, 5, 8, 12, 34, 56),
                ApplicationProtocol = "protocol"
            };
            VerifyFileExactly(file, @"
                                                                        S      1
1H,,1H;,10Hidentifier,28HC:\path\to\full\filename.igs,4Habcd,3H1.0,16,7,G      1
22,10,51,6Hident2,0.75,10,,4,0.8,15H20001225.130811,0.001,500.,5HBrett, G      2
7HIxMilia,8,4,15H19870508.123456,8Hprotocol;                            G      3
S      1G      3D      0P      0                                        T      1
");
        }
    }
}