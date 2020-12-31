﻿namespace LeaderEngine
{
    public class Component
    {
        public GameObject gameObject = null;
        public Transform transform => gameObject.transform;

        public bool Enabled = true;

        public virtual void Start() { return; }
        public virtual void Update() { return; }
        public virtual void LateUpdate() { return; }
        public virtual void OnRender() { return; }
        public virtual void OnRenderGui() { return; }
        public virtual void OnRemove() { return; }
    }
}
