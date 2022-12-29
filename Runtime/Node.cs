using System.Collections.Generic;
using UnityEngine;

namespace Prybh 
{
    public abstract class Node : ScriptableObject 
    {
        public enum State 
        {
            Running,
            Failure,
            Success
        }

#if UNITY_EDITOR
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        [TextArea]
        [SerializeField] private string description;
#endif // UNITY_EDITOR

        private BehaviorTree tree = null;
        [HideInInspector] public State state = State.Running;
        [HideInInspector] public bool started = false;

        public State Update() 
        {
            if (!started) 
            {
                OnStart();
                started = true;
            }

            state = OnUpdate();

            if (state != State.Running) 
            {
                OnStop();
                started = false;
            }

            return state;
        }

        public virtual Node Clone()
        {
            return Instantiate(this);
        }

        public virtual void Bind(BehaviorTree tree)
        {
            this.tree = tree;
        }

        public virtual void Abort()
        {
            started = false;
            state = State.Running;
            OnStop();
        }

        public virtual void OnDrawGizmos() 
        {
        }

        protected virtual void OnStart() {}
        protected virtual void OnStop() {}
        protected abstract State OnUpdate();

        public BehaviorTree GetTree() => tree;
        public Context GetContext() => tree.GetContext();
        public Blackboard GetBlackboard() => tree.GetBlackboard();

#if UNITY_EDITOR
        public virtual List<Node> GetChildren() => null;

        public List<Node> GetAllChildren()
        {
            List<Node> children = new List<Node>(); 
            List<Node> openList = new List<Node> { this };
            while (openList.Count > 0)
            {
                int index = openList.Count - 1;
                Node node = openList[index];
                openList.RemoveAt(index);
                if (node != null)
                {
                    children.Add(node);
                    var currentChildren = node.GetChildren();
                    if (currentChildren != null)
                    {
                        openList.AddRange(currentChildren);
                    }
                }
            }
            return children;
        }
#endif // UNITY_EDITOR
    }
}