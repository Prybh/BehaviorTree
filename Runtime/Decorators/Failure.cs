using UnityEngine;

namespace Prybh 
{
    public class Failure : DecoratorNode 
    {
        protected override State OnUpdate() 
        {
            var state = child.Update();
            if (state == State.Success) 
            {
                return State.Failure;
            }
            return state;
        }
    }
}