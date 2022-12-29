using UnityEngine;

namespace Prybh 
{
    public class Repeat : DecoratorNode 
    {
        [SerializeField] private bool restartOnSuccess = true;
        [SerializeField] private bool restartOnFailure = false;

        protected override State OnUpdate() 
        {
            switch (child.Update()) 
            {
                case State.Running:
                    break;
                case State.Failure:
                    if (restartOnFailure) 
                    {
                        return State.Running;
                    } 
                    else 
                    {
                        return State.Failure;
                    }
                case State.Success:
                    if (restartOnSuccess) 
                    {
                        return State.Running;
                    } 
                    else 
                    {
                        return State.Success;
                    }
            }
            return State.Running;
        }
    }
}
