using System.Collections.Generic;
using UnityEngine;

public class TextureAtlasRegistry : MonoBehaviour
{
    public List<BlockMaterialPropertyAsset> blockMaterialProperties = new List<BlockMaterialPropertyAsset>();
    public static Dictionary<string, BlockMaterialPropertyAsset> typeIdToMaterialProperty = new Dictionary<string, BlockMaterialPropertyAsset>();
    public static Dictionary<string, Rect> typeIdToRect = new Dictionary<string, Rect>();

    private static Texture2D blockAtlas;
    public static Texture2D BlockAtlas => blockAtlas;

    public void Init() {
        foreach (var bmp in blockMaterialProperties) {
            typeIdToMaterialProperty[bmp.id] = bmp;
        }

        // Create block texture atlas
        List<Texture2D> textures = blockMaterialProperties.ConvertAll(bmp => bmp.texture);
        Texture2D tempAtlas = new Texture2D(1, 1, TextureFormat.RGBA32, true);
        Rect[] rects = tempAtlas.PackTextures(textures.ToArray(), 8, 4096, false);
        tempAtlas.filterMode = FilterMode.Point;
        tempAtlas.Apply(updateMipmaps: true, makeNoLongerReadable: false);

        for (int i = 0; i < blockMaterialProperties.Count; i++) {
            typeIdToRect[blockMaterialProperties[i].id] = rects[i];
        }

        int mipCount = 4;
        blockAtlas = new Texture2D(tempAtlas.width, tempAtlas.height, TextureFormat.RGBA32, mipCount, false);
        for (int mip = 0; mip < mipCount; mip++) {
            // Check if this mip actually exists (Unity won't generate beyond 1x1)
            int mipWidth = Mathf.Max(1, tempAtlas.width >> mip);
            int mipHeight = Mathf.Max(1, tempAtlas.height >> mip);

            // Ensure we don’t copy from non-existent mip levels
            if (mipWidth == 1 && mipHeight == 1 && mip < mipCount - 1)
                break;

            Graphics.CopyTexture(tempAtlas, 0, mip, blockAtlas, 0, mip);
        }
        blockAtlas.filterMode = FilterMode.Point;
        blockAtlas.Apply(updateMipmaps: false, makeNoLongerReadable: true);

        Destroy(tempAtlas);
    }
    private void OnDestroy() {
        typeIdToMaterialProperty.Clear();
        typeIdToRect.Clear();
        if (blockAtlas != null) {
            DestroyImmediate(blockAtlas);
            blockAtlas = null;
        }
    }
}
