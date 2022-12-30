using UnityEngine;

namespace Prybh 
{
    [CreateAssetMenu()]
    public class BehaviorTree : ScriptableObject
    {
        [HideInInspector, SerializeField] private Node rootNode = null;
        [HideInInspector, SerializeField] private SerializableSystemType blackboardType = null;

        private Node.State treeState = Node.State.None;
        private GameObject m_gameObject = null;
        private Blackboard blackboard = null;

        public GameObject gameObject => m_gameObject;
        public Transform transform => m_gameObject.transform;
        public T GetComponent<T>() => m_gameObject.GetComponent<T>();
        public T GetBlackboard<T>() where T : Blackboard => blackboard as T;

        public BehaviorTree Clone() 
        {
            BehaviorTree tree = Instantiate(this);
            tree.rootNode = tree.rootNode.Clone();
            return tree;
        }

        public void Bind(GameObject gameObject) 
        {
            m_gameObject = gameObject;
            blackboard = (Blackboard)System.Activator.CreateInstance(blackboardType.Type);
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

#if UNITY_EDITOR
        // TODO : Expose in window custom inspector
        //[TextArea, SerializeField] private string description;

        public Node GetRootNode() => rootNode;
        public void SetRootNode(RootNode node)
        {
            rootNode = node;
        }

        public SerializableSystemType GetBlackboardType() => blackboardType;
        public void SetBlackboardType(SerializableSystemType type)
        {
            blackboardType = type;
        }
#endif // UNITY_EDITOR
    }
}