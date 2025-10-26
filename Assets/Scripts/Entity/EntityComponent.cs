using UnityEngine;

public class EntityComponent : MonoBehaviour
{
    protected Entity entity;

    public virtual void Initialize(Entity entity) {
        this.entity = entity;
    }
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public virtual void OnEntityAwake() { }
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public virtual void OnEntityStart() { }
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public virtual void OnEntityUpdate(bool ignoreLoops) { }
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public virtual void OnEntityFixedUpdate(bool ignoreLoops) { }
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public virtual void OnEntityLateUpdate(bool ignoreLoops) { }
}
