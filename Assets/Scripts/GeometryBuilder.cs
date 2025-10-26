using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GeometryBuilder {
    public static void Build(BlockGeometryFile file) {
        foreach (var bone in file.bones.Values) {
            if (string.IsNullOrEmpty(bone.parent)) {
                Matrix4x4 parentMatrix = Matrix4x4.identity;
                BuildBone(file, bone, parentMatrix);
            }
        }
    }

    private static void BuildBone(BlockGeometryFile file, GeometryBone bone, Matrix4x4 parentMatrix, HashSet<string> visited = null) {
        if (visited == null) visited = new HashSet<string>();
        if (!visited.Add(bone.name)) {
            Debug.LogError($"Cycle detected in bone hierarchy at {bone.name}");
            return;
        }

        Matrix4x4 boneMatrix =
            Matrix4x4.Translate(bone.pivot) *
            Matrix4x4.Rotate(Quaternion.Euler(bone.rotation)) *
            Matrix4x4.Translate(-bone.pivot);

        Matrix4x4 finalMatrix = parentMatrix * boneMatrix;

        if (bone.cubes != null) {
            foreach (var cube in bone.cubes) {
                BuildCube(file, cube, finalMatrix);
            }
        }

        foreach (var child in file.bones.Values) {
            if (child.parent == bone.name) {
                BuildBone(file, child, finalMatrix, new HashSet<string>(visited));
            }
        }
    }



    private static void BuildCube(BlockGeometryFile file, GeometryCube cube, Matrix4x4 parentMatrix) {
        // Build a unit cube mesh from origin/size
        Vector3 min = cube.origin;
        Vector3 max = cube.origin + cube.size;

        // Cube local transform (pivot + rotation only, scale is already baked in size)
        // Build rotation around a world-space pivot
        Matrix4x4 cubeMatrix =
            Matrix4x4.Translate(cube.pivot) *
            Matrix4x4.Rotate(Quaternion.Euler(cube.rotation)) *
            Matrix4x4.Translate(-cube.pivot);

        // Combine with parent (but since pivot is world already, parentMatrix may not apply here)
        Matrix4x4 finalMatrix = parentMatrix * cubeMatrix;


        // Apply transform to vertices
        int startIndex = file.vertices.Count;

        for (int i = 0; i < 6; i++) {
            if (!cube.uvs[GetKey(i)].enabled) continue;

            for (int p = 0; p < 4; p++) {
                Vector3 v = CubeMesh.Vertices[CubeMesh.Indices[i, p]];

                // Scale first
                v = Vector3.Scale(v, cube.size);

                // Offset by origin
                v += cube.origin;

                // Apply final transform
                file.vertices.Add(finalMatrix.MultiplyPoint3x4(v));

                file.normals.Add(finalMatrix.MultiplyVector(CubeMesh.Normals[i]).normalized);
                file.uvs.Add((CubeMesh.Uvs[p] * cube.uvs[GetKey(i)].size + cube.uvs[GetKey(i)].min, cube.uvs[GetKey(i)].texture));
            }
            file.indices.Add(startIndex);
            file.indices.Add(startIndex + 1);
            file.indices.Add(startIndex + 2);
            file.indices.Add(startIndex + 2);
            file.indices.Add(startIndex + 1);
            file.indices.Add(startIndex + 3);

            startIndex += 4;
        }
    }
    public static int GetIndex(string key) {
        return key switch {
            "south" => 0,
            "north" => 1,
            "east" => 2,
            "west" => 3,
            "top" => 4,
            "bottom" => 5,
            _ => 0
        };
    }
    public static string GetKey(int faceIndex) {
        return faceIndex switch {
            0 => "south",
            1 => "north",
            2 => "east",
            3 => "west",
            4 => "top",
            5 => "bottom",
            _ => "south"
        };
    }
}

