namespace SoarCraft.QYun.ArknightsAssetStudio.Core.Models {
    using System.Collections.Generic;
    using AssetReader.Entities.Enums;
    using AssetReader.Unity3D;
    using AssetReader.Unity3D.Contracts;
    using AssetReader.Unity3D.Objects;
    using AssetReader.Unity3D.Objects.AnimationClips;
    using AssetReader.Unity3D.Objects.AnimatorControllers;
    using AssetReader.Unity3D.Objects.AnimatorOverrideControllers;
    using AssetReader.Unity3D.Objects.AssetBundles;
    using AssetReader.Unity3D.Objects.Avatars;
    using AssetReader.Unity3D.Objects.Materials;
    using AssetReader.Unity3D.Objects.Meshes;
    using AssetReader.Unity3D.Objects.Shaders;
    using AssetReader.Unity3D.Objects.SpriteAtlases;
    using AssetReader.Unity3D.Objects.Sprites;
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using AssetReader.Unity3D.Objects.VideoClips;
    using Base62;

    public class AssetItem {
        public UObject Obj;
        public ClassIDType Type;
        public GameObjectNode Node;

        public long PathID;
        public long FullSize;

        public string Name;
        public string BaseID;
        public string InfoText;
        public string Container;

        public AssetItem(UObject obj, out ICollection<(PPtr<UObject>, string)> container, out (string, string) names) {
            this.Obj = obj;
            this.Type = obj.type;
            this.PathID = obj.m_PathID;
            this.FullSize = obj.byteSize;

            this.BaseID = obj.m_PathID.ToBase62();
            this.Name = $"{this.Type}_{this.BaseID}";

            names = ("", "");
            container = new List<(PPtr<UObject>, string)>();

            switch (obj) {
                case Font:
                case Mesh:
                case Avatar:
                case Sprite:
                case Material:
                case TextAsset:
                case MonoScript:
                case SpriteAtlas:
                case MovieTexture:
                case AnimationClip:
                case AnimatorController:
                case AnimatorOverrideController:
                    this.Name = ((NamedObject)obj).m_Name;
                    break;

                case Animation:
                case Transform:
                case MeshFilter:
                case MeshRenderer:
                // case RectTransform:
                case BuildSettings:
                case SkinnedMeshRenderer:
                    break;

                case Animator animator:
                    if (animator.m_GameObject.TryGet(out var result))
                        this.Name = result.m_Name;
                    break;

                case AssetBundle assetBundle:
                    foreach (var (key, value) in assetBundle.m_Container) {
                        var preLoadIndex = value.preloadIndex;
                        var preLoadSize = value.preloadSize;
                        var preLoadEnd = preLoadIndex + preLoadSize;
                        for (var k = preLoadIndex; k < preLoadEnd; k++) {
                            container.Add((assetBundle.m_PreloadTable[k], key));
                        }
                    }

                    this.Name = assetBundle.m_Name;
                    break;

                case AudioClip audioClip:
                    if (!string.IsNullOrEmpty(audioClip.m_Source))
                        this.FullSize = obj.byteSize + audioClip.m_Size;

                    this.Name = audioClip.m_Name;
                    break;

                case GameObject gameObject:
                    this.Name = gameObject.m_Name;
                    break;

                case MonoBehaviour monoBehaviour:
                    if (monoBehaviour.m_Name == "" && monoBehaviour.m_Script.TryGet(out var m_Script))
                        this.Name = m_Script.m_ClassName;
                    else
                        this.Name = monoBehaviour.m_Name;
                    break;

                case PlayerSettings playerSettings:
                    names = (playerSettings.companyName, playerSettings.productName);
                    break;

                case Shader shader:
                    this.Name = shader.m_ParsedForm?.m_Name ?? shader.m_Name;
                    break;

                case Texture2D texture2D:
                    if (!string.IsNullOrEmpty(texture2D.m_StreamData?.path))
                        this.FullSize = obj.byteSize + texture2D.m_StreamData.size;

                    this.Name = texture2D.m_Name;
                    break;

                case VideoClip videoClip:
                    if (!string.IsNullOrEmpty(videoClip.m_OriginalPath))
                        this.FullSize = obj.byteSize + videoClip.m_ExternalResources.m_Size;

                    this.Name = videoClip.m_Name;
                    break;

                case NamedObject namedObject:
                    this.Name = namedObject.m_Name;
                    break;
            }
        }
    }
}
