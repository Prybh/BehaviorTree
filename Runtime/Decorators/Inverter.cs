using UnityEngine;

namespace Prybh
{
    public class Inverter : DecoratorNode 
    {
        protected override State OnUpdate() 
        {
            switch (child.Update()) 
            {
                case State.Running:
                    return State.Running;
                case State.Failure:
                    return State.Success;
                case State.Success:
                    return State.Failure;
            }
            return State.Failure;
        }
    }
}