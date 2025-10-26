using Newtonsoft.Json;
using BlockGeometry;
using System.Collections.Generic;
using UnityEngine;

public class BlockGeometryRegistry : MonoBehaviour
{
    public List<BlockGeometryAsset> blockGeometries = new List<BlockGeometryAsset>();
    private static Dictionary<string, BlockGeometryFile> typeIdToGeometry = new Dictionary<string, BlockGeometryFile>();

    public void Init() {
        var settings = new JsonSerializerSettings {
            Converters = new JsonConverter[] {
                new Vector2Converter(),
                new Vector3Converter()
            },
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        foreach (var geometry in blockGeometries) {
            BlockGeometryFile file = JsonConvert.DeserializeObject<BlockGeometryFile>(geometry.model.text, settings);
            foreach(var kvp in file.bones) {
                kvp.Value.name = kvp.Key;
            }
            GeometryBuilder.Build(file);
            typeIdToGeometry[geometry.id] = file;
        }
    }
    public static bool GetBlockGeometry(string typeId, out BlockGeometryFile geometry) {
        if (typeIdToGeometry.TryGetValue(typeId, out geometry)) {
            return true;
        }
        geometry = null;
        return false;
    }
    private void OnDestroy() {
        typeIdToGeometry.Clear();
    }
}