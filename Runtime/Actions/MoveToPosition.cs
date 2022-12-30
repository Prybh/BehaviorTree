using UnityEngine;
using UnityEngine.AI;

namespace Prybh
{
    public class MoveToPosition : ActionNode
    {
        [SerializeField] private float speed = 5.0f;
        [SerializeField] private float stoppingDistance = 0.1f;
        [SerializeField] private float acceleration = 40.0f;
        [SerializeField] private float tolerance = 1.0f;

        private NavMeshAgent agent;

        public override void Bind(BehaviorTree tree)
        {
            base.Bind(tree);
            agent = GetComponent<NavMeshAgent>();
        }

        protected override void OnStart()
        {
            agent.stoppingDistance = stoppingDistance;
            agent.speed = speed;
            agent.acceleration = acceleration;
            agent.destination = GetBlackboard<SimpleVectorBlackboard>().moveToPosition;
        }

        protected override State OnUpdate()
        {
            if (agent == null || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                return State.Failure;
            }
            else if (agent.remainingDistance < tolerance)
            {
                return State.Success;
            }
            else
            {
                return State.Running;
            }
        }
    }
}
