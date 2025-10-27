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
    public bool Overlaps(Vector3 point, Vector3 colliderPosition, Vector3 direction, out Vector3Int normal, out Vector3 hitPoint) {
        Vector3 _min = min + colliderPosition;
        Vector3 _max = max + colliderPosition;

        bool overlapX = point.x <= _max.x && point.x >= _min.x;
        bool overlapY = point.y <= _max.y && point.y >= _min.y;
        bool overlapZ = point.z <= _max.z && point.z >= _min.z;

        normal = Vector3Int.zero;
        hitPoint = Vector3.zero;

        if (!(overlapX && overlapY && overlapZ))
            return false;

        Vector3 center = (_min + _max) * 0.5f;
        Vector3 halfSize = (_max - _min) * 0.5f;
        Vector3 localPoint = point - center;

        float dx = halfSize.x - Mathf.Abs(localPoint.x);
        float dy = halfSize.y - Mathf.Abs(localPoint.y);
        float dz = halfSize.z - Mathf.Abs(localPoint.z);

        float minPen = Mathf.Min(dx, Mathf.Min(dy, dz));

        // Determine collision normal and contact point
        if (Mathf.Approximately(minPen, dx)) {
            int sign = -Mathf.RoundToInt(Mathf.Sign(direction.x));
            normal = new Vector3Int(sign, 0, 0);
            hitPoint = new Vector3(
                sign > 0 ? _max.x : _min.x,
                Mathf.Clamp(point.y, _min.y, _max.y),
                Mathf.Clamp(point.z, _min.z, _max.z)
            );
        }
        else if (Mathf.Approximately(minPen, dy)) {
            int sign = -Mathf.RoundToInt(Mathf.Sign(direction.y));
            normal = new Vector3Int(0, sign, 0);
            hitPoint = new Vector3(
                Mathf.Clamp(point.x, _min.x, _max.x),
                sign > 0 ? _max.y : _min.y,
                Mathf.Clamp(point.z, _min.z, _max.z)
            );
        }
        else {
            int sign = -Mathf.RoundToInt(Mathf.Sign(direction.z));
            normal = new Vector3Int(0, 0, sign);
            hitPoint = new Vector3(
                Mathf.Clamp(point.x, _min.x, _max.x),
                Mathf.Clamp(point.y, _min.y, _max.y),
                sign > 0 ? _max.z : _min.z
            );
        }

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