using UnityEngine;

namespace Prybh 
{
    public class Succeed : DecoratorNode 
    {
        protected override State OnUpdate() 
        {
            var state = child.Update();
            if (state == State.Failure) 
            {
                return State.Success;
            }
            return state;
        }
    }
}