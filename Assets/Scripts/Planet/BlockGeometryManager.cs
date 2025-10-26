using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockGeometryManager : MonoBehaviour
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
[Serializable]
public class BlockGeometryFile {
    public Dictionary<string, GeometryBone> bones;
    
    [JsonIgnore] public List<Vector3> vertices = new List<Vector3>();
    [JsonIgnore] public List<int> indices = new List<int>();
    [JsonIgnore] public List<Vector3> normals = new List<Vector3>();
    [JsonIgnore] public List<(Vector2, string)> uvs = new List<(Vector2, string)>();
}

[Serializable]
public class GeometryBone {
    public string name;
    public Vector3 pivot;
    public Vector3 rotation;
    public Vector3 scale;
    public List<GeometryCube> cubes;
    public string parent;
}

[Serializable]
public class GeometryCube {
    public Vector3 pivot;
    public Vector3 origin;
    public Vector3 size;
    public Vector3 rotation;
    public Dictionary<string, GeometryUV> uvs;
}

[Serializable]
public class GeometryUV {
    public Vector2 min;
    public Vector2 size;
    public string texture;
    public bool enabled;
}
