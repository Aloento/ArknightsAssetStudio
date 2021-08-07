namespace SoarCraft.QYun.AssetReader.Unity3D.Contracts {
    using Utils;

    public class StaticBatchInfo {
        public ushort firstSubMesh;
        public ushort subMeshCount;

        public StaticBatchInfo(ObjectReader reader) {
            firstSubMesh = reader.ReadUInt16();
            subMeshCount = reader.ReadUInt16();
        }
    }
}
