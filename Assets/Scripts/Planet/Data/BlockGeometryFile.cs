using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockGeometry {
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
}
