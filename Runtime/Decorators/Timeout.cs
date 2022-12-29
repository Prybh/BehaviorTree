using UnityEngine;

namespace Prybh
{
    public class Timeout : DecoratorNode 
    {
        [SerializeField] private float duration = 1.0f;
        
        private float startTime;

        protected override void OnStart() 
        {
            startTime = Time.time;
        }

        protected override State OnUpdate() 
        {
            if (Time.time - startTime > duration)
            {
                return State.Failure;
            }
            return child.Update();
        }
    }
}