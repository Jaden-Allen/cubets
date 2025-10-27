using UnityEngine;
using System.Collections.Generic;

public class Grass : BlockType
{
    public override Color VertexColorPaint(Vector3 vertex, Vector3Int globalPosition, Planet planet) {
        return Color.Lerp(Color.black, Color.white, vertex.y);
    }
    private GrassTickComponent grassTickComponent = null;
    public override BlockTickComponent tickComponent { get {
            //if (grassTickComponent == null) {
            //    grassTickComponent = new GrassTickComponent();
            //}
            return grassTickComponent;
        }
    }
    public class GrassTickComponent : BlockTickComponent {
        public override int tickInterval => 100;
        public override void OnTick(BlockTickEvent e) {
            int randX = Random.Range(-1, 2);
            int randY = Random.Range(-1, 2);
            int randZ = Random.Range(-1, 2);

            Vector3Int offset = new Vector3Int(randX, randY, randZ);

            if (offset == Vector3Int.zero) return;

            var belowOffset = e.block.Offset(offset + new Vector3Int(0, -1, 0));
            if (belowOffset != null && belowOffset.typeId == BlockTypes.GrassBlock.typeId) {
                var offsetBlock = e.block.Offset(new Vector3Int(randX, randY, randZ));
                if (offsetBlock != null && offsetBlock.isAir) {
                    offsetBlock.SetType(BlockTypes.Grass.type, 0);
                }
            }
        }
    }
}
