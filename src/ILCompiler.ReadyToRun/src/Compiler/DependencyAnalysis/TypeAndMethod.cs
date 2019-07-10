﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

using Internal.JitInterface;
using Internal.TypeSystem;

using ILCompiler.DependencyAnalysis.ReadyToRun;

namespace ILCompiler.DependencyAnalysis
{
    internal struct TypeAndMethod : IEquatable<TypeAndMethod>
    {
        public readonly TypeDesc Type;
        public readonly MethodWithToken Method;
        public readonly bool IsUnboxingStub;
        public readonly bool IsInstantiatingStub;
        // TODO: Refactor this out of this struct
        public readonly ReadyToRunConverterKind FixupConventionConverterKind;

        public TypeAndMethod(TypeDesc type, MethodWithToken method, bool isUnboxingStub, bool isInstantiatingStub, ReadyToRunConverterKind fixupConventionConverterKind = ReadyToRunConverterKind.READYTORUN_CONVERTERKIND_Invalid)
        {
            Type = type;
            Method = method;
            IsUnboxingStub = isUnboxingStub;
            IsInstantiatingStub = isInstantiatingStub;
            FixupConventionConverterKind = fixupConventionConverterKind;
        }

        public bool Equals(TypeAndMethod other)
        {
            return Type == other.Type &&
                   Method.Equals(other.Method) &&
                   IsUnboxingStub == other.IsUnboxingStub &&
                   IsInstantiatingStub == other.IsInstantiatingStub &&
                   FixupConventionConverterKind == other.FixupConventionConverterKind;
        }

        public override bool Equals(object obj)
        {
            return obj is TypeAndMethod other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Type?.GetHashCode() ?? 0) ^
                unchecked(Method.GetHashCode() * 31) ^
                unchecked(FixupConventionConverterKind.GetHashCode() * 55733) ^
                (IsUnboxingStub ? -0x80000000 : 0) ^ 
                (IsInstantiatingStub ? 0x40000000 : 0);
        }
    }
}
