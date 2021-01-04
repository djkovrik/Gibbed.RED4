﻿/* Copyright (c) 2020 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;

namespace Gibbed.RED4.ScriptFormats.Definitions
{
    public class ClassDefinition : Definition
    {
        public override DefinitionType DefinitionType => DefinitionType.Class;

        public ClassDefinition()
        {
            this.Functions = new List<FunctionDefinition>();
            this.Properties = new List<PropertyDefinition>();
            this.OverriddenProperties = new List<PropertyDefinition>();
        }

        public Visibility Visibility { get; set; }
        public ClassFlags Flags { get; set; }
        public ClassDefinition BaseClass { get; set; }
        public List<FunctionDefinition> Functions { get; }
        public List<PropertyDefinition> Properties { get; }
        public List<PropertyDefinition> OverriddenProperties { get; }

        private static readonly ClassFlags KnownFlags =
            ClassFlags.Unknown0 | ClassFlags.IsAbstract |
            ClassFlags.Unknown2 | ClassFlags.IsStruct |
            ClassFlags.HasFunctions | ClassFlags.HasProperties |
            ClassFlags.IsImportOnly | ClassFlags.HasOverriddenProperties;

        public bool IsA(ClassDefinition type)
        {
            var current = this;
            do
            {
                if (current == type)
                {
                    return true;
                }
                current = current.BaseClass;
            }
            while (current != null);
            return false;
        }

        internal override void Serialize(IDefinitionWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteValueU8((byte)this.Visibility);
            writer.WriteValueU16((ushort)this.Flags);
            writer.WriteReference(this.BaseClass);
            if ((this.Flags & ClassFlags.HasFunctions) != 0)
            {
                writer.WriteReferences(this.Functions);
            }
            if ((this.Flags & ClassFlags.HasProperties) != 0)
            {
                writer.WriteReferences(this.Properties);
            }
            if ((this.Flags & ClassFlags.HasOverriddenProperties) != 0)
            {
                writer.WriteReferences(this.OverriddenProperties);
            }
        }

        internal override void Deserialize(IDefinitionReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var visibility = (Visibility)reader.ReadValueU8();

            var flags = (ClassFlags)reader.ReadValueU16();
            var unknownFlags = flags & ~KnownFlags;
            if (unknownFlags != ClassFlags.None)
            {
                throw new FormatException();
            }

            var baseClass = reader.ReadReference<ClassDefinition>();
            var functions = (flags & ClassFlags.HasFunctions) != 0
                ? reader.ReadReferences<FunctionDefinition>()
                : Array.Empty<FunctionDefinition>();
            var properties = (flags & ClassFlags.HasProperties) != 0
                ? reader.ReadReferences<PropertyDefinition>()
                : Array.Empty<PropertyDefinition>();
            var overriddenProperties = (flags & ClassFlags.HasOverriddenProperties) != 0
                ? reader.ReadReferences<PropertyDefinition>()
                : Array.Empty<PropertyDefinition>();

            this.Functions.Clear();
            this.Properties.Clear();
            this.OverriddenProperties.Clear();
            this.Visibility = visibility;
            this.Flags = flags;
            this.BaseClass = baseClass;
            this.Functions.AddRange(functions);
            this.Properties.AddRange(properties);
            this.OverriddenProperties.AddRange(overriddenProperties);
        }
    }
}
