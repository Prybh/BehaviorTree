using UnityEngine;

namespace Prybh 
{
    [CreateAssetMenu()]
    public class BehaviorTree : ScriptableObject
    {
        [SerializeField] private Node rootNode = null;
        [SerializeField] private Blackboard blackboard = new Blackboard();

        private Node.State treeState = Node.State.None;
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
            if (treeState == Node.State.None || treeState == Node.State.Running)
            {
                treeState = rootNode.Update();
            }
            return treeState;
        }

        public Node.State GetState() => treeState;

        public bool IsStateFinished() => treeState == Node.State.Success || treeState == Node.State.Failure;

        public void ResetState()
        {
            if (IsStateFinished())
            {
                treeState = Node.State.None;
            }
            else
            {
                Debug.LogWarning("Can't reset a BehaviorTree when the state isn't finished");
            }
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