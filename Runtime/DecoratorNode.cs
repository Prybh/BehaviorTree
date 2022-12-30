using System.Collections.Generic;
using UnityEngine;

namespace Prybh 
{
    public abstract class DecoratorNode : Node 
    {
        [HideInInspector, SerializeField] protected Node child;

        public override Node Clone()
        {
            DecoratorNode node = Instantiate(this);
            node.child = child.Clone();
            return node;
        }

        public override void Bind(BehaviorTree tree)
        {
            base.Bind(tree);
            child?.Bind(tree);
        }

        public override void Abort()
        {
            if (IsStarted())
            {
                base.Abort();
                child?.Abort();
            }
        }

        public override void OnDrawGizmos()
        {
            child?.OnDrawGizmos();
        }

#if UNITY_EDITOR
        public void SetChild(Node child)
        {
            this.child = child;
        }

        public override sealed List<Node> GetChildren() => new List<Node> { child };
#endif // UNITY_EDITOR
    }
}
