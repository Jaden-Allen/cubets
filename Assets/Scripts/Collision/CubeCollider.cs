using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class CubeCollider {
    public float width = 1f;
    public float height = 1f;
    public float length = 1f;
    public Vector3 offset;

    public Vector3 size => new Vector3(width, height, length);
    public Vector3 center => size / 2f + offset;
    public Vector3 min => new Vector3(-(width / 2f), 0f, -(length / 2f)) + offset;
    public Vector3 max => min + size;

    public CubeCollider(float width = 1f, float height = 1f, float length = 1f) {
        this.width = width;
        this.height = height;
        this.length = length;
    }
    public bool Overlaps(Vector3 point, Vector3 colliderPosition) {
        Vector3 _min = min + colliderPosition;
        Vector3 _max = max + colliderPosition;

        bool overlapX = point.x <= _max.x && point.x >= _min.x;
        bool overlapY = point.y <= _max.y && point.y >= _min.y;
        bool overlapZ = point.z <= _max.z && point.z >= _min.z;

        return overlapX && overlapY && overlapZ;
    }
    public bool Overlaps(Vector3 point, Vector3 colliderPosition, Vector3 direction, out Vector3Int normal) {
        Vector3 _min = min + colliderPosition;
        Vector3 _max = max + colliderPosition;

        bool overlapX = point.x <= _max.x && point.x >= _min.x;
        bool overlapY = point.y <= _max.y && point.y >= _min.y;
        bool overlapZ = point.z <= _max.z && point.z >= _min.z;

        normal = Vector3Int.zero;

        if (!(overlapX && overlapY && overlapZ))
            return false;

        Vector3 center = (_min + _max) * 0.5f;
        Vector3 halfSize = (_max - _min) * 0.5f;
        Vector3 localPoint = point - center;

        float dx = (halfSize.x - Mathf.Abs(localPoint.x));
        float dy = (halfSize.y - Mathf.Abs(localPoint.y));
        float dz = (halfSize.z - Mathf.Abs(localPoint.z));

        float minPen = Mathf.Min(dx, Mathf.Min(dy, dz));

        if (Mathf.Approximately(minPen, dx))
            normal = new Vector3Int(-Mathf.RoundToInt(Mathf.Sign(direction.x)), 0, 0);
        else if (Mathf.Approximately(minPen, dy))
            normal = new Vector3Int(0, -Mathf.RoundToInt(Mathf.Sign(direction.y)), 0);
        else
            normal = new Vector3Int(0, 0, -Mathf.RoundToInt(Mathf.Sign(direction.z)));

        return true;
    }

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
        float halfLength = length * 0.5f;

        // 8 corners of the box in local space
        Vector3[] corners =
        {
            // Bottom
            new Vector3(-halfWidth, 0f, -halfLength),
            new Vector3( halfWidth, 0f, -halfLength),
            new Vector3( halfWidth, 0f,  halfLength),
            new Vector3(-halfWidth, 0f,  halfLength),

            // Top
            new Vector3(-halfWidth, height, -halfLength),
            new Vector3( halfWidth, height, -halfLength),
            new Vector3( halfWidth, height,  halfLength),
            new Vector3(-halfWidth, height,  halfLength)
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