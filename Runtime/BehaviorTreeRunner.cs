using UnityEngine;

namespace Prybh
{
    public class BehaviorTreeRunner : MonoBehaviour
    {
        public enum UpdateType
        {
            Auto,
            Manual
        }

        [SerializeField] private BehaviorTree tree;
        [SerializeField] private UpdateType updateType;
        [SerializeField] private bool resetOnFinished = true;

        private void Awake()
        {
            if (tree != null)
            {
                tree = tree.Clone();
                tree.Bind(Context.CreateFromGameObject(gameObject));
            }
        }

        private void Update()
        {
            if (updateType == UpdateType.Auto)
            {
                UpdateBehaviorTree();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (tree != null)
            {
                tree.OnDrawGizmos();
            }
        }

        public void UpdateBehaviorTree()
        {
            if (tree != null)
            {
                if (resetOnFinished && IsStateFinished())
                {
                    ResetState();
                }

                tree.Update();
            }
        }

        public BehaviorTree GetTree() => tree;
        public UpdateType GetUpdateType() => updateType;
        public bool GetResetOnFinished() => resetOnFinished;

        public bool IsStateFinished() => tree.IsStateFinished();

        public void ResetState()
        {
            if (tree.IsStateFinished())
            {
                tree.ResetState();
            }
            else
            {
                Debug.LogWarning("Can't reset a BehaviorTree when the state isn't finished");
            }
        }
    }
}