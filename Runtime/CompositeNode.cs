using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prybh 
{
    public abstract class CompositeNode : Node 
    {
        [HideInInspector, SerializeField] protected List<Node> children = new List<Node>();

        public override Node Clone() 
        {
            CompositeNode node = Instantiate(this);
            node.children = children.ConvertAll(c => c.Clone());
            return node;
        }

        public override void Bind(BehaviorTree tree)
        {
            base.Bind(tree);
            foreach (Node child in children)
                child.Bind(tree);
        }

        public override void Abort()
        {
            if (IsStarted())
            {
                base.Abort();
                foreach (Node child in children)
                    child.Abort();
            }
        }

        public override void OnDrawGizmos()
        {
            foreach (Node child in children)
                child.OnDrawGizmos();
        }

#if UNITY_EDITOR
        public void AddChild(Node child)
        {
            children.Add(child);
        }

        public void RemoveChild(Node child)
        {
            children.Remove(child);
        }

        public void SortChildren()
        {
            children.Sort(SortByHorizontalPosition);
        }

        private int SortByHorizontalPosition(Node left, Node right)
        {
            return left.position.x < right.position.x ? -1 : 1;
        }

        public override sealed List<Node> GetChildren() => children;
#endif // UNITY_EDITOR
    }
}