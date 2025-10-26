using UnityEngine;
using System.Collections.Generic;

public class Water : BlockType
{
    public override Color VertexColorPaint(Vector3 vertex, Vector3Int globalPosition, Planet planet) {
        // Round to nearest block coordinates
        Vector3Int blockPos = globalPosition;

        // Only care about top surface vertices
        if (vertex.y < 1f) return Color.white;

        // Figure out which corner of the top face this vertex is
        Vector3Int corner = Vector3Int.RoundToInt(vertex);

        // Neighbor offsets that share this vertex
        Vector3Int[] neighborOffsets = new Vector3Int[3];

        if (corner.x == 0 && corner.z == 0) {
            neighborOffsets = new Vector3Int[] { new Vector3Int(-1, 0, -1), new Vector3Int(-1, 0, 0), new Vector3Int(0, 0, -1) };
        }
        else if (corner.x == 0 && corner.z == 1) {
            neighborOffsets = new Vector3Int[] { new Vector3Int(-1, 0, 1), new Vector3Int(-1, 0, 0), new Vector3Int(0, 0, 1) };
        }
        else if (corner.x == 1 && corner.z == 0) {
            neighborOffsets = new Vector3Int[] { new Vector3Int(1, 0, -1), new Vector3Int(1, 0, 0), new Vector3Int(0, 0, -1) };
        }
        else if (corner.x == 1 && corner.z == 1) {
            neighborOffsets = new Vector3Int[] { new Vector3Int(1, 0, 1), new Vector3Int(1, 0, 0), new Vector3Int(0, 0, 1) };
        }

        // Check neighbors for opaque blocks
        foreach (var offset in neighborOffsets) {
            Vector3Int checkPos = blockPos + offset;
            BlockType neighbor = planet.GetBlockType(checkPos); 
            if (neighbor.materialInstances.renderType == BlockRenderType.Opaque) {
                return Color.black; 
            }
        }

        return Color.white;
    }
    public override BlockPermutation ResolvePermutation() {
        return new BlockPermutation() {
            BaseType = this,
            States = new Dictionary<string, object>() {
                {
                    "water_levels", new WaterLevelComponent(){
                        levels = new bool[16]{
                            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true

                        }
                    }
                }
            }
        };
    }
    public class WaterTickingComponent : BlockTickComponent {
        public override int tickInterval => 20;
        public override void OnTick(BlockTickEvent e) {
            Debug.Log("Tick");
        }
    }
    public class WaterLevelComponent {
        public bool[] levels = new bool[16];
    }
}
