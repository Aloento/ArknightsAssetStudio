namespace SoarCraft.QYun.ArknightsAssetStudio.Converters.ShaderConverters {
    using System;
    using System.IO;
    using System.Text;
    using Core.Unity.Smolv;
    using Core.Unity.SpirV;

    public static class SpirVShaderConverter {
        public static string Convert(byte[] m_ProgramCode) {
            var sb = new StringBuilder();
            using var ms = new MemoryStream(m_ProgramCode);
            using var reader = new BinaryReader(ms);
            var requirements = reader.ReadInt32();
            var minOffset = m_ProgramCode.Length;
            var snippetCount = 5;
            /*if (version[0] > 2019 || (version[0] == 2019 && version[1] >= 3)) //2019.3 and up
                    {
                        snippetCount = 6;
                    }*/
            for (var i = 0; i < snippetCount; i++) {
                if (reader.BaseStream.Position >= minOffset) {
                    break;
                }

                var offset = reader.ReadInt32();
                var size = reader.ReadInt32();
                if (size > 0) {
                    if (offset < minOffset) {
                        minOffset = offset;
                    }
                    var pos = ms.Position;
                    _ = sb.Append(ExportSnippet(ms, offset, size));
                    ms.Position = pos;
                }
            }

            return sb.ToString();
        }

        private static string ExportSnippet(Stream stream, int offset, int size) {
            stream.Position = offset;
            var decodedSize = SmolvDecoder.GetDecodedBufferSize(stream);
            if (decodedSize == 0) {
                throw new Exception("Invalid SMOL-V shader header");
            }

            using var decodedStream = new MemoryStream(new byte[decodedSize]);
            if (SmolvDecoder.Decode(stream, size, decodedStream)) {
                decodedStream.Position = 0;
                var module = Module.ReadFrom(decodedStream);
                var disassembler = new Disassembler();
                return disassembler.Disassemble(module, DisassemblyOptions.Default).Replace("\r\n", "\n");
            }

            throw new Exception("Unable to decode SMOL-V shader");
        }
    }
}
