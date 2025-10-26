using UnityEngine;

public class WireframeRenderer : MonoBehaviour
{
    public CubeCollider selectionBox;
    public Material lineMaterial;
    public float thickness = 0.05f;

    private void OnRenderObject() {
        if (!lineMaterial) return;
        lineMaterial.SetPass(0);

        Camera cam = Camera.current;
        if (cam == null) return;

        var lines = selectionBox.GetLines();
        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        foreach (var l in lines) {
            DrawThickLine(l.a, l.b, thickness, cam);
        }

        GL.PopMatrix();
    }

    void DrawThickLine(Vector3 a, Vector3 b, float thickness, Camera cam) {
        Vector3 dir = (b - a).normalized;
        Vector3 side = Vector3.Cross(dir, cam.transform.forward).normalized * thickness * 0.5f;

        GL.Begin(GL.QUADS);
        GL.Color(Color.green);
        GL.Vertex(a - side);
        GL.Vertex(a + side);
        GL.Vertex(b + side);
        GL.Vertex(b - side);
        GL.End();
    }
}
