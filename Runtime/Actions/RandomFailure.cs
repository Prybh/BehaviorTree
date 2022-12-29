using UnityEngine;

namespace Prybh 
{
    public class RandomFailure : ActionNode 
    {
        [Range(0,1)]
        [SerializeField] private float chanceOfFailure = 0.5f;

        protected override State OnUpdate() 
        {
            float value = Random.value;
            if (value > chanceOfFailure) 
            {
                return State.Failure;
            }
            return State.Success;
        }
    }
}