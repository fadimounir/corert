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
        protected readonly ReadyToRunFixupKind _fixupKind;

        protected readonly MethodWithToken _method;

        protected readonly SignatureContext _signatureContext;

        protected readonly bool _isUnboxingStub;

        protected readonly bool _isInstantiatingStub;

        public MethodFixupSignature(
            ReadyToRunFixupKind fixupKind, 
            MethodWithToken method, 
            SignatureContext signatureContext,
            bool isUnboxingStub,
            bool isInstantiatingStub)
        {
            _fixupKind = fixupKind;
            _method = method;
            _signatureContext = signatureContext;
            _isUnboxingStub = isUnboxingStub;
            _isInstantiatingStub = isInstantiatingStub;
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

            // Emit typicaly definitions instead of universal canonical ones to avoid having to have type __UniversalCanon in the runtime
            MethodWithToken methodSignatureToEmit = _method;
            if (_method.Method.IsCanonicalMethod(CanonicalFormKind.Universal))
                methodSignatureToEmit = new MethodWithToken(_method.Method.GetTypicalMethodDefinition(), _method.Token, _method.ConstrainedType);

            SignatureContext innerContext = dataBuilder.EmitFixup(r2rFactory, _fixupKind, _method.Token.Module, _signatureContext);
            dataBuilder.EmitMethodSignature(methodSignatureToEmit, enforceDefEncoding: false, innerContext, _isUnboxingStub, _isInstantiatingStub);

            return dataBuilder.ToObjectData();
        }

        protected void AppendMethodSignatureMangledName(NameMangler nameMangler, Utf8StringBuilder sb)
        {
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
            sb.Append(": ");
            _method.AppendMangledName(nameMangler, sb);
        }

        public override void AppendMangledName(NameMangler nameMangler, Utf8StringBuilder sb)
        {
            sb.Append(nameMangler.CompilationUnitPrefix);
            AppendMethodSignatureMangledName(nameMangler, sb);
        }

        public override int CompareToImpl(ISortableNode other, CompilerComparer comparer)
        {
            throw new NotImplementedException();
        }
    }
}
