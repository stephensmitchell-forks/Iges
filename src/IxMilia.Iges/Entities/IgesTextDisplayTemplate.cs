﻿// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace IxMilia.Iges.Entities
{
    public class IgesTextDisplayTemplate : IgesEntity
    {
        public override IgesEntityType EntityType { get { return IgesEntityType.TextDisplayTemplate; } }

        public double CharacterBoxWidth { get; set; }
        public double CharacterBoxHeight { get; set; }
        public int FontCode { get; set; }
        public IgesTextFontDefinition TextFontDefinition { get; set; }
        public double SlantAngle { get; set; }
        public double RotationAngle { get; set; }
        public IgesTextMirroringAxis MirroringAxis { get; set; }
        public IgesTextRotationType RotationType { get; set; }
        public IgesVector LocationOrOffset { get; set; }

        public bool IsAbsoluteDisplayTemplate
        {
            get { return FormNumber == 0; }
            set { FormNumber = value ? 0 : 1; }
        }

        public bool IsIncrementalDisplayTemplate
        {
            get { return !IsAbsoluteDisplayTemplate; }
            set { IsAbsoluteDisplayTemplate = !value; }
        }

        public IgesTextDisplayTemplate()
            : base()
        {
            SubordinateEntitySwitchType = IgesSubordinateEntitySwitchType.Independent;
            EntityUseFlag = IgesEntityUseFlag.Definition;
            Hierarchy = IgesHierarchy.GlobalTopDown;
            FontCode = 1;
            LocationOrOffset = IgesVector.Zero;
        }

        protected override int ReadParameters(List<string> parameters)
        {
            this.CharacterBoxWidth = Double(parameters, 0);
            this.CharacterBoxHeight = Double(parameters, 1);
            var fontCode = Integer(parameters, 2);
            if (fontCode < 0)
            {
                SubEntityIndices.Add(-fontCode);
                this.FontCode = 0;
            }
            else
            {
                SubEntityIndices.Add(0);
                this.FontCode = fontCode;
            }

            this.SlantAngle = Double(parameters, 3);
            this.RotationAngle = Double(parameters, 4);
            this.MirroringAxis = (IgesTextMirroringAxis)Integer(parameters, 5);
            this.RotationType = (IgesTextRotationType)Integer(parameters, 6);
            this.LocationOrOffset.X = Double(parameters, 7);
            this.LocationOrOffset.Y = Double(parameters, 8);
            this.LocationOrOffset.Z = Double(parameters, 9);
            return 10;
        }

        internal override void OnAfterRead(IgesDirectoryData directoryData)
        {
            base.OnAfterRead(directoryData);
            Debug.Assert(SubordinateEntitySwitchType == IgesSubordinateEntitySwitchType.Independent);
            Debug.Assert(EntityUseFlag == IgesEntityUseFlag.Definition);
            Debug.Assert(Hierarchy == IgesHierarchy.GlobalTopDown);
            TextFontDefinition = SubEntities[0] as IgesTextFontDefinition;
        }

        internal override void OnBeforeWrite()
        {
            SubEntities.Add(TextFontDefinition);
        }

        protected override void WriteParameters(List<object> parameters)
        {
            parameters.Add(CharacterBoxWidth);
            parameters.Add(CharacterBoxHeight);
            if (TextFontDefinition == null)
            {
                parameters.Add(FontCode);
            }
            else
            {
                parameters.Add(-SubEntityIndices[0]);
            }

            parameters.Add(SlantAngle);
            parameters.Add(RotationAngle);
            parameters.Add((int)MirroringAxis);
            parameters.Add((int)RotationType);
            parameters.Add(LocationOrOffset.X);
            parameters.Add(LocationOrOffset.Y);
            parameters.Add(LocationOrOffset.Z);
        }
    }
}
