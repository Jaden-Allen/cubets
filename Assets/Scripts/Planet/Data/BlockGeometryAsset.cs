using UnityEngine;

[CreateAssetMenu(fileName = "BlockGeometryAsset", menuName = "Data/BlockGeometryAsset", order = 1)]
public class BlockGeometryAsset : ScriptableObject
{
    public string id;
    public TextAsset model;
    public bool isDefaultCube => model == null;
}
