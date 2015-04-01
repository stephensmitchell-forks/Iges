﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using IxMilia.Iges.Entities;
using Xunit;

namespace IxMilia.Iges.Test
{
    public class IgesEntityReaderTests
    {

        #region Private methods

        private static IgesEntity ParseSingleEntity(string content)
        {
            var file = IgesReaderTests.CreateFile(content.Trim('\r', '\n'));
            Assert.Equal(1, file.Entities.Count);
            return file.Entities[0];
        }

        #endregion

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadUnsupportedEntityTest()
        {
            // entity id 888 is invalid
            var file = IgesReaderTests.CreateFile(@"
     888       1       0       0       0                               0D      1
     888       0       0       0       0                               0D      2
888,11,22,33,44,55,66;                                                 1P      1
".Trim('\r', '\n'));
            Assert.Equal(0, file.Entities.Count);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadNullEntityTest()
        {
            var file = IgesReaderTests.CreateFile(@"
       0       1       0       0       0                               0D      1
       0       0       0       0       0                               0D      2
0,11,22,33,44,55,66;                                                   1P      1");
            Assert.Equal(IgesEntityType.Null, file.Entities.Single().EntityType);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadCircularArcTest()
        {
            var circle = (IgesCircularArc)ParseSingleEntity(@"
     100       1       0       0       0                               0D      1
     100       0       3       1       0                               0D      2
100,11,22,33,44,55,66,77;                                              1P      1
");
            Assert.Equal(11.0, circle.PlaneDisplacement);
            Assert.Equal(22.0, circle.Center.X);
            Assert.Equal(33.0, circle.Center.Y);
            Assert.Equal(0.0, circle.Center.Z);
            Assert.Equal(44.0, circle.StartPoint.X);
            Assert.Equal(55.0, circle.StartPoint.Y);
            Assert.Equal(0.0, circle.StartPoint.Z);
            Assert.Equal(66.0, circle.EndPoint.X);
            Assert.Equal(77.0, circle.EndPoint.Y);
            Assert.Equal(0.0, circle.EndPoint.Z);
            Assert.Equal(new IgesPoint(22.0, 33.0, 11.0), circle.ProperCenter);
            Assert.Equal(new IgesPoint(44.0, 55.0, 11.0), circle.ProperStartPoint);
            Assert.Equal(new IgesPoint(66.0, 77.0, 11.0), circle.ProperEndPoint);
            Assert.Equal(IgesColorNumber.Green, circle.Color);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadLineTest()
        {
            var line = (IgesLine)ParseSingleEntity(@"
     110       1       0       0       0                               0D      1
     110       0       3       1       0                               0D      2
110,11,22,33,44,55,66;                                                 1P      1
");
            Assert.Equal(11.0, line.P1.X);
            Assert.Equal(22.0, line.P1.Y);
            Assert.Equal(33.0, line.P1.Z);
            Assert.Equal(44.0, line.P2.X);
            Assert.Equal(55.0, line.P2.Y);
            Assert.Equal(66.0, line.P2.Z);
            Assert.Equal(IgesColorNumber.Green, line.Color);

            // verify transformation matrix is identity
            Assert.Equal(1.0, line.TransformationMatrix.R11);
            Assert.Equal(0.0, line.TransformationMatrix.R12);
            Assert.Equal(0.0, line.TransformationMatrix.R13);
            Assert.Equal(0.0, line.TransformationMatrix.R21);
            Assert.Equal(1.0, line.TransformationMatrix.R22);
            Assert.Equal(0.0, line.TransformationMatrix.R23);
            Assert.Equal(0.0, line.TransformationMatrix.R31);
            Assert.Equal(0.0, line.TransformationMatrix.R32);
            Assert.Equal(1.0, line.TransformationMatrix.R33);
            Assert.Equal(0.0, line.TransformationMatrix.T1);
            Assert.Equal(0.0, line.TransformationMatrix.T2);
            Assert.Equal(0.0, line.TransformationMatrix.T3);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadLineWithNonStandardDelimitersTest()
        {
            var line = (IgesLine)ParseSingleEntity(@"
1H//1H##                                                                G      1
     110       1       0       0       0                               0D      1
     110       0       3       1       0                               0D      2
110/11/22/33/44/55/66#                                                 1P      1
");
            Assert.Equal(11.0, line.P1.X);
            Assert.Equal(22.0, line.P1.Y);
            Assert.Equal(33.0, line.P1.Z);
            Assert.Equal(44.0, line.P2.X);
            Assert.Equal(55.0, line.P2.Y);
            Assert.Equal(66.0, line.P2.Z);
            Assert.Equal(IgesColorNumber.Green, line.Color);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadLocationTest()
        {
            var location = (IgesLocation)ParseSingleEntity(@"
     116       1       0       0       0                               0D      1
     116       0       0       1       0                                D      2
116,11.,22.,33.;                                                       1P      1
");
            Assert.Equal(11.0, location.X);
            Assert.Equal(22.0, location.Y);
            Assert.Equal(33.0, location.Z);
            Assert.Equal(IgesColorNumber.Default, location.Color);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadDirectionTest()
        {
            var direction = (IgesDirection)ParseSingleEntity(@"
     123       1       0       0       0                        00010200D      1
     123       0       0       1       0                                D      2
123,11.,22.,33.;                                                       1P      1
");
            Assert.Equal(11.0, direction.X);
            Assert.Equal(22.0, direction.Y);
            Assert.Equal(33.0, direction.Z);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadTransformationMatrixTest()
        {
            var matrix = (IgesTransformationMatrix)ParseSingleEntity(@"
     124       1       0       0       0                               0D      1
     124       0       0       4       0                               0D      2
124,1,2,3,4,5,6,7,8,9,10,11,12;                                        1P      1
");
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
        public void ReadTransformationMatrixFromEntityTest()
        {
            var file = IgesReaderTests.CreateFile(@"
     124       1       0       0       0                               0D      1
     124       0       0       4       0                               0D      2
     110       2       0       0       0               1               0D      3
     110       0       3       1       0                               0D      4
124,1,2,3,4,5,6,7,8,9,10,11,12;                                        1P      1
110,11,22,33,44,55,66;                                                 3P      2
".Trim('\r', '\n'));
            var matrix = file.Entities.Single(e => e.EntityType == IgesEntityType.Line).TransformationMatrix;
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
        public void ReadSphereTest()
        {
            // fully-specified values
            var sphere = (IgesSphere)ParseSingleEntity(@"
     158       1       0       0       0                            0000D      1
     158       0       0       1       0                                D      2
158,11.,22.,33.,44.;                                                   1P      1
");
            Assert.Equal(11.0, sphere.Radius);
            Assert.Equal(new IgesPoint(22, 33, 44), sphere.Center);

            // default values
            sphere = (IgesSphere)ParseSingleEntity(@"
     158       1       0       0       0                            0000D      1
     158       0       0       1       0                                D      2
158,1.;                                                                1P      1
");
            Assert.Equal(1.0, sphere.Radius);
            Assert.Equal(IgesPoint.Origin, sphere.Center);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadTorusTest()
        {
            // fully-specified values
            var torus = (IgesTorus)ParseSingleEntity(@"
     160       1       0       0       0                        00000000D      1
     160       0       0       1       0                                D      2
160,11.,22.,33.,44.,55.,66.,77.,88.;                                   1P      1
");
            Assert.Equal(11.0, torus.RingRadius);
            Assert.Equal(22.0, torus.DiscRadius);
            Assert.Equal(new IgesPoint(33, 44, 55), torus.Center);
            Assert.Equal(new IgesVector(66, 77, 88), torus.Normal);

            // default values
            torus = (IgesTorus)ParseSingleEntity(@"
     160       1       0       0       0                        00000000D      1
     160       0       0       1       0                                D      2
160,1.,2.;                                                             1P      1
");
            Assert.Equal(1.0, torus.RingRadius);
            Assert.Equal(2.0, torus.DiscRadius);
            Assert.Equal(IgesPoint.Origin, torus.Center);
            Assert.Equal(IgesVector.ZAxis, torus.Normal);

            // default normal
            torus = (IgesTorus)ParseSingleEntity(@"
     160       1       0       0       0                        00000000D      1
     160       0       0       1       0                                D      2
160,9.,8.,1.,2.,3.;                                                    1P      1
");
            Assert.Equal(9.0, torus.RingRadius);
            Assert.Equal(8.0, torus.DiscRadius);
            Assert.Equal(new IgesPoint(1, 2, 3), torus.Center);
            Assert.Equal(IgesVector.ZAxis, torus.Normal);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Reading)]
        public void ReadSubfigureTest()
        {
            var entity = ParseSingleEntity(@"
// The subfigure has two lines; one defined before and one after.       S      1
     110       1       0       0       0                               0D      1
     110       0       0       1       0                               0D      2
     308       2       0       0       0                               0D      3
     308       0       0       1       0                               0D      4
     110       4       0       0       0                               0D      5
     110       0       0       1       0                               0D      6
110,1.0,2.0,3.0,4.0,5.0,6.0;                                            P      1
308,0,                                           22Hthis,is;the         P      2
subfigureH,2,1,5;                                                       P      3
110,7.0,8.0,9.0,10.0,11.0,12.0;                                         P      4
");
            Assert.Equal(IgesEntityType.SubfigureDefinition, entity.EntityType);
            var subfigure = (IgesSubfigureDefinition)entity;
            Assert.Equal(0, subfigure.Depth);
            Assert.Equal("this,is;the subfigureH", subfigure.Name);
            Assert.Equal(2, subfigure.Entities.Count);
            Assert.Equal(IgesEntityType.Line, subfigure.Entities[0].EntityType);
            Assert.Equal(IgesEntityType.Line, subfigure.Entities[1].EntityType);
            var line1 = (IgesLine)subfigure.Entities[0];
            Assert.Equal(1.0, line1.P1.X);
            Assert.Equal(2.0, line1.P1.Y);
            Assert.Equal(3.0, line1.P1.Z);
            Assert.Equal(4.0, line1.P2.X);
            Assert.Equal(5.0, line1.P2.Y);
            Assert.Equal(6.0, line1.P2.Z);
            var line2 = (IgesLine)subfigure.Entities[1];
            Assert.Equal(7.0, line2.P1.X);
            Assert.Equal(8.0, line2.P1.Y);
            Assert.Equal(9.0, line2.P1.Z);
            Assert.Equal(10.0, line2.P2.X);
            Assert.Equal(11.0, line2.P2.Y);
            Assert.Equal(12.0, line2.P2.Z);
        }
    }
}