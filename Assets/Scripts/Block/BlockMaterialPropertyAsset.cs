using UnityEngine;

[CreateAssetMenu(fileName = "BlockMaterialProperty", menuName = "Data/BlockMaterialProperty", order = 2)]
public class BlockMaterialPropertyAsset : ScriptableObject
{
    public string id;
    public Texture2D texture;
}
