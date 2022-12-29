using UnityEngine;

namespace Prybh
{
    public class RandomPosition : ActionNode
    {
        [SerializeField] private Vector2 min = Vector2.one * -10.0f;
        [SerializeField] private Vector2 max = Vector2.one * 10.0f;

        protected override State OnUpdate()
        {
            GetBlackboard().moveToPosition = new Vector3(Random.Range(min.x, max.x), 0.0f, Random.Range(min.y, max.y));
            return State.Success;
        }

        public override void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            Vector3 center = new Vector3((max.x + min.x) * 0.5f, 0.0f, (max.y + min.y) * 0.5f);
            Vector3 size = new Vector3((max.x - min.x) * 0.5f, 1.0f, (max.y - min.y) * 0.5f);

            Gizmos.DrawWireCube(center, size);
        }
    }
}
