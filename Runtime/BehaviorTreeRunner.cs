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

        [SerializeField]
        private UpdateType updateType;

        [SerializeField]
        private BehaviorTree tree;

        private Context context;

        private void Start()
        {
            if (tree != null)
            {
                context = Context.CreateFromGameObject(gameObject);
                tree = tree.Clone();
                tree.Bind(context);
            }
        }

        private void Update()
        {
            if (updateType == UpdateType.Auto)
            {
                Tick();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (tree != null)
            {
                tree.OnDrawGizmos();
            }
        }

        public void Tick()
        {
            if (tree != null)
            {
                tree.Update();
            }
        }

        public UpdateType GetUpdateType() => updateType;
        public BehaviorTree GetTree() => tree;
    }
}