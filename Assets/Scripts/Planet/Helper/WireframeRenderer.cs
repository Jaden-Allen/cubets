using System.Collections.Generic;
using UnityEngine;

public class WireframeRenderer : MonoBehaviour {
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Material collisionDebugMat;

    public bool debugCollisions = false;

    private readonly List<SelectionBox> selectionBoxes = new();
    private readonly List<SelectionBox> debugSelectionBoxes = new();

    public static WireframeRenderer Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void AddSelection(SelectionBox selectionBox) {
        if (selectionBox != null)
            selectionBoxes.Add(selectionBox);
    }
    public void AddDebugSelection(SelectionBox selectionBox) {
        if (selectionBox != null)
            debugSelectionBoxes.Add(selectionBox);
    }

    private void OnRenderObject() {
        if (!lineMaterial || selectionBoxes.Count == 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        var boxesToDraw = new List<SelectionBox>(selectionBoxes);
        selectionBoxes.Clear();

        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);
        foreach (var selectionBox in boxesToDraw) {
            foreach (var collider in selectionBox.colliders) {
                foreach (var line in collider.GetLines()) {
                    GL.Vertex(line.a + selectionBox.position);
                    GL.Vertex(line.b + selectionBox.position);
                }
            }
        }
        GL.End();

        boxesToDraw = new List<SelectionBox>(debugSelectionBoxes);
        debugSelectionBoxes.Clear();

        collisionDebugMat.SetPass(0);

        GL.Begin(GL.LINES);
        foreach (var selectionBox in boxesToDraw) {
            foreach (var collider in selectionBox.colliders) {
                foreach (var line in collider.GetLines()) {
                    GL.Vertex(line.a + selectionBox.position);
                    GL.Vertex(line.b + selectionBox.position);
                }
            }
        }
        GL.End();
    }
}

[System.Serializable]
public class SelectionBox {
    public List<CubeCollider> colliders { get; }
    public Vector3 position { get; }

    public SelectionBox(List<CubeCollider> colliders, Vector3 position) {
        this.colliders = colliders ?? new List<CubeCollider>();
        this.position = position;
    }
}
