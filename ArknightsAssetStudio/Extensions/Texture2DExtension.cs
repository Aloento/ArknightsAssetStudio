namespace SoarCraft.QYun.ArknightsAssetStudio.Extensions {
    using System.IO;
    using AssetReader.Unity3D.Objects.Texture2Ds;
    using Core.Entities;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using SoarCraft.QYun.TextureDecoderNET;

    public static class Texture2DExtension {
        public static Image ConvertToImage(this Texture2D m_Texture2D, bool flip) {
            _ = new ETC1Decoder(m_Texture2D.image_data.GetData(), (ulong)m_Texture2D.m_Width, (ulong)m_Texture2D.m_Height, out var bytes);
            if (bytes is { Length: > 0 }) {
                var image = Image.LoadPixelData<Bgra32>(bytes, m_Texture2D.m_Width, m_Texture2D.m_Height);
                if (flip) {
                    image.Mutate(x => x.Flip(FlipMode.Vertical));
                }
                return image;
            }
            return null;
        }

        public static MemoryStream ConvertToStream(this Texture2D m_Texture2D, ImageFormat imageFormat, bool flip) {
            var image = ConvertToImage(m_Texture2D, flip);
            if (image != null) {
                using (image) {
                    return image.ConvertToStream(imageFormat);
                }
            }
            return null;
        }
    }
}
