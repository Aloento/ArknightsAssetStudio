namespace SoarCraft.QYun.ArknightsAssetStudio.Converters {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AssetReader.Entities.Structs;
    using Core.Unity.CecilTools;
    using Core.Unity.SerializationLogic;
    using Helpers;
    using Mono.Cecil;

    public class TypeDefinitionConverter {
        private readonly TypeDefinition TypeDef;
        private readonly TypeResolver TypeResolver;
        private readonly SerializedTypeHelper Helper;
        private readonly int Indent;

        public TypeDefinitionConverter(TypeDefinition typeDef, SerializedTypeHelper helper, int indent) {
            TypeDef = typeDef;
            TypeResolver = new TypeResolver(null);
            Helper = helper;
            Indent = indent;
        }

        public List<TypeTreeNode> ConvertToTypeTreeNodes() {
            var nodes = new List<TypeTreeNode>();

            var baseTypes = new Stack<TypeReference>();
            var lastBaseType = TypeDef.BaseType;
            while (!UnitySerializationLogic.IsNonSerialized(lastBaseType)) {
                if (lastBaseType is GenericInstanceType genericInstanceType) {
                    TypeResolver.Add(genericInstanceType);
                }
                baseTypes.Push(lastBaseType);
                lastBaseType = lastBaseType.Resolve().BaseType;
            }
            while (baseTypes.Count > 0) {
                var typeReference = baseTypes.Pop();
                var typeDefinition = typeReference.Resolve();
                foreach (var fieldDefinition in typeDefinition.Fields.Where(WillUnitySerialize)) {
                    if (!IsHiddenByParentClass(baseTypes, fieldDefinition, TypeDef)) {
                        nodes.AddRange(ProcessingFieldRef(ResolveGenericFieldReference(fieldDefinition)));
                    }
                }

                if (typeReference is GenericInstanceType genericInstanceType) {
                    TypeResolver.Remove(genericInstanceType);
                }
            }
            foreach (var field in FilteredFields()) {
                nodes.AddRange(ProcessingFieldRef(field));
            }

            return nodes;
        }

        private bool WillUnitySerialize(FieldDefinition fieldDefinition) {
            try {
                var resolvedFieldType = TypeResolver.Resolve(fieldDefinition.FieldType);
                if (UnitySerializationLogic.ShouldNotTryToResolve(resolvedFieldType)) {
                    return false;
                }
                if (!UnityEngineTypePredicates.IsUnityEngineObject(resolvedFieldType)) {
                    if (resolvedFieldType.FullName == fieldDefinition.DeclaringType.FullName) {
                        return false;
                    }
                }
                return UnitySerializationLogic.WillUnitySerialize(fieldDefinition, TypeResolver);
            } catch (Exception ex) {
                throw new Exception(string.Format("Exception while processing {0} {1}, error {2}", fieldDefinition.FieldType.FullName, fieldDefinition.FullName, ex.Message));
            }
        }

        private static bool IsHiddenByParentClass(IEnumerable<TypeReference> parentTypes, FieldDefinition fieldDefinition, TypeDefinition processingType) => processingType.Fields.Any(f => f.Name == fieldDefinition.Name) || parentTypes.Any(t => t.Resolve().Fields.Any(f => f.Name == fieldDefinition.Name));

        private IEnumerable<FieldDefinition> FilteredFields() => TypeDef.Fields.Where(WillUnitySerialize).Where(f =>
                 UnitySerializationLogic.IsSupportedCollection(f.FieldType) ||
                 !f.FieldType.IsGenericInstance ||
                 UnitySerializationLogic.ShouldImplementIDeserializable(f.FieldType.Resolve()));

        private FieldReference ResolveGenericFieldReference(FieldReference fieldRef) {
            var field = new FieldReference(fieldRef.Name, fieldRef.FieldType, ResolveDeclaringType(fieldRef.DeclaringType));
            return TypeDef.Module.ImportReference(field);
        }

        private TypeReference ResolveDeclaringType(TypeReference declaringType) {
            var typeDefinition = declaringType.Resolve();
            if (typeDefinition is not { HasGenericParameters: true }) {
                return typeDefinition;
            }
            var genericInstanceType = new GenericInstanceType(typeDefinition);
            foreach (var genericParameter in typeDefinition.GenericParameters) {
                genericInstanceType.GenericArguments.Add(genericParameter);
            }
            return TypeResolver.Resolve(genericInstanceType);
        }

        private List<TypeTreeNode> ProcessingFieldRef(FieldReference fieldDef) {
            var typeRef = TypeResolver.Resolve(fieldDef.FieldType);
            return TypeRefToTypeTreeNodes(typeRef, fieldDef.Name, Indent, false);
        }

        private static bool IsStruct(TypeReference typeRef) => typeRef.IsValueType && !IsEnum(typeRef) && !typeRef.IsPrimitive;

        private static bool IsEnum(TypeReference typeRef) => !typeRef.IsArray && typeRef.Resolve().IsEnum;

        private static bool RequiresAlignment(TypeReference typeRef) => typeRef.MetadataType switch {
            MetadataType.Boolean or MetadataType.Char or MetadataType.SByte or MetadataType.Byte or MetadataType.Int16 or MetadataType.UInt16 => true,
            _ => UnitySerializationLogic.IsSupportedCollection(typeRef) && RequiresAlignment(CecilUtils.ElementTypeOfCollection(typeRef)),
        };

        private static bool IsSystemString(TypeReference typeRef) => typeRef.FullName == "System.String";

        private List<TypeTreeNode> TypeRefToTypeTreeNodes(TypeReference typeRef, string name, int indent, bool isElement) {
            var align = false;

            if (!IsStruct(TypeDef) || !UnityEngineTypePredicates.IsUnityEngineValueType(TypeDef)) {
                if (IsStruct(typeRef) || RequiresAlignment(typeRef)) {
                    align = true;
                }
            }

            var nodes = new List<TypeTreeNode>();
            if (typeRef.IsPrimitive) {
                var primitiveName = typeRef.Name;
                primitiveName = primitiveName switch {
                    "Boolean" => "bool",
                    "Byte" => "UInt8",
                    "SByte" => "SInt8",
                    "Int16" => "SInt16",
                    "UInt16" => "UInt16",
                    "Int32" => "SInt32",
                    "UInt32" => "UInt32",
                    "Int64" => "SInt64",
                    "UInt64" => "UInt64",
                    "Char" => "char",
                    "Double" => "double",
                    "Single" => "float",
                    _ => throw new NotSupportedException(),
                };
                if (isElement) {
                    align = false;
                }
                nodes.Add(new TypeTreeNode(primitiveName, name, indent, align));
            } else if (IsSystemString(typeRef)) {
                Helper.AddString(nodes, name, indent);
            } else if (IsEnum(typeRef)) {
                nodes.Add(new TypeTreeNode("SInt32", name, indent, align));
            } else if (CecilUtils.IsGenericList(typeRef)) {
                var elementRef = CecilUtils.ElementTypeOfCollection(typeRef);
                nodes.Add(new TypeTreeNode(typeRef.Name, name, indent, align));
                Helper.AddArray(nodes, indent + 1);
                nodes.AddRange(TypeRefToTypeTreeNodes(elementRef, "data", indent + 2, true));
            } else if (typeRef.IsArray) {
                var elementRef = typeRef.GetElementType();
                nodes.Add(new TypeTreeNode(typeRef.Name, name, indent, align));
                Helper.AddArray(nodes, indent + 1);
                nodes.AddRange(TypeRefToTypeTreeNodes(elementRef, "data", indent + 2, true));
            } else if (UnityEngineTypePredicates.IsUnityEngineObject(typeRef)) {
                Helper.AddPPtr(nodes, typeRef.Name, name, indent);
            } else if (UnityEngineTypePredicates.IsSerializableUnityClass(typeRef) || UnityEngineTypePredicates.IsSerializableUnityStruct(typeRef)) {
                switch (typeRef.FullName) {
                    case "UnityEngine.AnimationCurve":
                        Helper.AddAnimationCurve(nodes, name, indent + 1);
                        break;
                    case "UnityEngine.Gradient":
                        Helper.AddGradient(nodes, name, indent + 1);
                        break;
                    case "UnityEngine.GUIStyle":
                        Helper.AddGUIStyle(nodes, name, indent + 1);
                        break;
                    case "UnityEngine.RectOffset":
                        Helper.AddRectOffset(nodes, name, indent + 1);
                        break;
                    case "UnityEngine.Color32":
                        Helper.AddColor32(nodes, name, indent + 1);
                        break;
                    case "UnityEngine.Matrix4x4":
                        Helper.AddMatrix4x4(nodes, name, indent + 1);
                        break;
                    case "UnityEngine.Rendering.SphericalHarmonicsL2":
                        Helper.AddSphericalHarmonicsL2(nodes, name, indent + 1);
                        break;
                    case "UnityEngine.PropertyName":
                        Helper.AddPropertyName(nodes, name, indent + 1);
                        break;
                }
            } else {
                nodes.Add(new TypeTreeNode(typeRef.Name, name, indent, align));
                var typeDef = typeRef.Resolve();
                var typeDefinitionConverter = new TypeDefinitionConverter(typeDef, Helper, indent + 1);
                nodes.AddRange(typeDefinitionConverter.ConvertToTypeTreeNodes());
            }

            return nodes;
        }
    }
}
