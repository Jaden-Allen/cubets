using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CubeCollider {
    public float width = 1f;
    public float height = 1.8f;
    public Vector3 offset;

    [System.Serializable]
    public class Line {
        public Vector3 a;
        public Vector3 b;
        public Line(Vector3 a, Vector3 b) {
            this.a = a;
            this.b = b;
        }
    }

    public List<Line> GetLines() {
        List<Line> lines = new List<Line>();

        float halfWidth = width * 0.5f;

        // 8 corners of the box in local space
        Vector3[] corners =
        {
            // Bottom
            new Vector3(-halfWidth, 0f, -halfWidth),
            new Vector3( halfWidth, 0f, -halfWidth),
            new Vector3( halfWidth, 0f,  halfWidth),
            new Vector3(-halfWidth, 0f,  halfWidth),

            // Top
            new Vector3(-halfWidth, height, -halfWidth),
            new Vector3( halfWidth, height, -halfWidth),
            new Vector3( halfWidth, height,  halfWidth),
            new Vector3(-halfWidth, height,  halfWidth)
        };

        // Apply offset to all corners
        for (int i = 0; i < corners.Length; i++)
            corners[i] += offset;

        // Bottom square
        lines.Add(new Line(corners[0], corners[1]));
        lines.Add(new Line(corners[1], corners[2]));
        lines.Add(new Line(corners[2], corners[3]));
        lines.Add(new Line(corners[3], corners[0]));

        // Top square
        lines.Add(new Line(corners[4], corners[5]));
        lines.Add(new Line(corners[5], corners[6]));
        lines.Add(new Line(corners[6], corners[7]));
        lines.Add(new Line(corners[7], corners[4]));

        // Vertical edges
        lines.Add(new Line(corners[0], corners[4]));
        lines.Add(new Line(corners[1], corners[5]));
        lines.Add(new Line(corners[2], corners[6]));
        lines.Add(new Line(corners[3], corners[7]));

        return lines;
    }
}

