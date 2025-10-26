using System.Collections.Generic;
using UnityEngine;

public class MainRegistry : MonoBehaviour
{
    public List<Block> blocks = new List<Block>();
    public List<BlockGeometryAsset> blockGeometries = new List<BlockGeometryAsset>();
    public List<BlockMaterialPropertyAsset> blockMaterialProperties = new List<BlockMaterialPropertyAsset>();

    private void Awake() {
        
    }
}
