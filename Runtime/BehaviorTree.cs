using UnityEngine;

namespace Prybh 
{
    [CreateAssetMenu()]
    public class BehaviorTree : ScriptableObject
    {
        [SerializeField] private Node rootNode = null;
        [SerializeField] private Blackboard blackboard = new Blackboard();

        private Context context = null;

        public BehaviorTree Clone() 
        {
            BehaviorTree tree = Instantiate(this);
            tree.rootNode = tree.rootNode.Clone();
            return tree;
        }

        public void Bind(Context context) 
        {
            this.context = context;
            rootNode.Bind(this);
        }

        public void Abort()
        {
            rootNode.Abort();
        }

        public void OnDrawGizmos()
        {
            rootNode.OnDrawGizmos();
        }

        public Node.State Update()
        {
            return rootNode.Update();
        }

        public Context GetContext() => context;
        public Blackboard GetBlackboard() => blackboard;

#if UNITY_EDITOR
        public Node GetRootNode() => rootNode;
        public void SetRootNode(RootNode node)
        {
            rootNode = node;
        }
#endif // UNITY_EDITOR
    }
}