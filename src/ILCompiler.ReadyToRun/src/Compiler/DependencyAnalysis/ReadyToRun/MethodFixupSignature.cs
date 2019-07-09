// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

using Internal.JitInterface;
using Internal.Text;
using Internal.TypeSystem;

namespace ILCompiler.DependencyAnalysis.ReadyToRun
{
    public class MethodFixupSignature : Signature
    {
        private readonly ReadyToRunFixupKind _fixupKind;

        private readonly MethodWithToken _method;

        private readonly SignatureContext _signatureContext;

        private readonly bool _isUnboxingStub;

        private readonly bool _isInstantiatingStub;

        private readonly ReadyToRunConverterKind _fixupConventionConverterKind;

        public MethodFixupSignature(
            ReadyToRunFixupKind fixupKind, 
            MethodWithToken method, 
            SignatureContext signatureContext,
            bool isUnboxingStub,
            bool isInstantiatingStub,
            ReadyToRunConverterKind fixupConventionConverterKind)
        {
            _fixupKind = fixupKind;
            _method = method;
            _signatureContext = signatureContext;
            _isUnboxingStub = isUnboxingStub;
            _isInstantiatingStub = isInstantiatingStub;
            _fixupConventionConverterKind = fixupConventionConverterKind;
        }

        public MethodDesc Method => _method.Method;

        public override int ClassCode => 150063499;

        public override ObjectData GetData(NodeFactory factory, bool relocsOnly = false)
        {
            if (relocsOnly)
            {
                // Method fixup signature doesn't contain any direct relocs
                return new ObjectData(data: Array.Empty<byte>(), relocs: null, alignment: 0, definedSymbols: null);
            }

            ReadyToRunCodegenNodeFactory r2rFactory = (ReadyToRunCodegenNodeFactory)factory;
            ObjectDataSignatureBuilder dataBuilder = new ObjectDataSignatureBuilder();
            dataBuilder.AddSymbol(this);
            if (_fixupConventionConverterKind != ReadyToRunConverterKind.READYTORUN_CONVERTERKIND_Invalid)
            {
                SignatureContext innerContext = dataBuilder.EmitFixup(r2rFactory, ReadyToRunFixupKind.READYTORUN_FIXUP_LoadConverterThunk, _method.Token.Module, _signatureContext);
                dataBuilder.EmitByte((byte)_fixupConventionConverterKind);
                dataBuilder.EmitByte((byte)_fixupKind);
                dataBuilder.EmitMethodSignature(_method, enforceDefEncoding: false, innerContext, _isUnboxingStub, _isInstantiatingStub);
            }
            else
            {
                SignatureContext innerContext = dataBuilder.EmitFixup(r2rFactory, _fixupKind, _method.Token.Module, _signatureContext);
                dataBuilder.EmitMethodSignature(_method, enforceDefEncoding: false, innerContext, _isUnboxingStub, _isInstantiatingStub);
            }

            return dataBuilder.ToObjectData();
        }

        public override void AppendMangledName(NameMangler nameMangler, Utf8StringBuilder sb)
        {
            sb.Append(nameMangler.CompilationUnitPrefix);
            sb.Append($@"MethodFixupSignature(");
            sb.Append(_fixupKind.ToString());
            if (_isUnboxingStub)
            {
                sb.Append(" [UNBOX]");
            }
            if (_isInstantiatingStub)
            {
                sb.Append(" [INST]");
            }
            if (_fixupConventionConverterKind != ReadyToRunConverterKind.READYTORUN_CONVERTERKIND_Invalid)
            {
                sb.Append(String.Format(" [{0}]", Enum.GetName(typeof(ReadyToRunConverterKind), _fixupConventionConverterKind)));
            }
            sb.Append(": ");
            _method.AppendMangledName(nameMangler, sb);
        }

        public override int CompareToImpl(ISortableNode other, CompilerComparer comparer)
        {
            throw new NotImplementedException();
        }
    }
}
