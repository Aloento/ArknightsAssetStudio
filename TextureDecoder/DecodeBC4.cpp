#include "pch.h"
#include "TextureDecoder.h"

namespace SoarCraft::QYun::TextureDecoder {
    bool TextureDecoderService::DecodeBC4(FastArgs) {
        BCInsert;

        for (uint32_t i = 0; i < 16; i++)
            buffer[i] = 0xff000000;
        for (uint32_t by = 0; by < m_blocks_y; by++) {
            for (uint32_t bx = 0; bx < m_blocks_x; bx++, d += 8) {
                DecodeDXT5Alpha(d, buffer, 2);
                CopyBlockBuffer(bx, by, width, height, m_block_width, m_block_height, buffer, i);
            }
        }
        return 1;
    }
}
