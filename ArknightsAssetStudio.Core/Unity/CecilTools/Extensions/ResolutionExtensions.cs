// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

namespace SoarCraft.QYun.ArknightsAssetStudio.Core.Unity.CecilTools.Extensions {
    using System;
    using Mono.Cecil;

    public static class ResolutionExtensions {
        public static TypeDefinition CheckedResolve(this TypeReference type) => Resolve(type, reference => reference.Resolve());

        public static MethodDefinition CheckedResolve(this MethodReference method) => Resolve(method, reference => reference.Resolve());

        private static TDefinition Resolve<TReference, TDefinition>(TReference reference, Func<TReference, TDefinition> resolve)
            where TReference : MemberReference
            where TDefinition : class, IMemberDefinition {
            if (reference.Module == null)
                throw new ResolutionException(reference);

            var definition = resolve(reference);
            if (definition == null)
                throw new ResolutionException(reference);

            return definition;
        }
    }
}
