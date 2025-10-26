using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] private bool _ignoreLoops = false;
    public bool ignoreLoops { get { return _ignoreLoops; } protected set { _ignoreLoops = value; } }

    private List<EntityComponent> components = new List<EntityComponent>();

    private void Awake() {
        components = GetComponentsInChildren<EntityComponent>().ToList();

        foreach (var component in components) {
            component.Initialize(this);
            component.OnEntityAwake();
        }
    }
    private void Start() {
        foreach (var component in components) {
            if (component.enabled) {
                component.OnEntityStart();
            }
        }
    }
    private void Update() {
        foreach (var component in components) {
            if (component.enabled) {
                component.OnEntityUpdate(ignoreLoops);
            }
        }
    }
    private void FixedUpdate() {
        foreach (var component in components) {
            if (component.enabled) {
                component.OnEntityFixedUpdate(ignoreLoops);
            }
        }
    }
    private void LateUpdate() {
        foreach (var comp in components) {
            if (comp.enabled) {
                comp.OnEntityLateUpdate(ignoreLoops);
            }
        }
    }
    public T GetPlayerComponent<T>() where T : EntityComponent {
        foreach (var component in components) {
            if (component is T typedComponent)
                return typedComponent;
        }

        return null;
    }
    public bool TryGetPlayerComponent<T>(out T component) where T : EntityComponent {
        foreach (var _component in components) {
            if (_component is T typedComponent) {
                component = typedComponent;
                return true;
            }
        }
        component = null;
        return false;
    }
}
