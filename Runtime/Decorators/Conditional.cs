using UnityEngine;

namespace Prybh
{
    public abstract class Conditional : DecoratorNode
    {
        [SerializeField] private bool dontReEvaluateOnRunning = false;

        private bool isRunning = false;

        protected override void OnStop()
        {
            isRunning = false;
        }

        protected override State OnUpdate()
        {
            if (child == null)
            {
                return CanUpdate() ? State.Success : State.Failure;
            }
            if (CanUpdate())
            {
                var state = child.Update();
                isRunning = state == State.Running;
                return state;
            }
            return State.Failure;
        }

        public bool CanUpdate()
        {
            return (isRunning && !dontReEvaluateOnRunning) || IsUpdatable();
        }

        protected abstract bool IsUpdatable();
    }
}

