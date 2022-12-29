using UnityEngine;

namespace Prybh
{
    public class Breakpoint : ActionNode
    {
        protected override void OnStart()
        {
            Debug.Log("Trigging Breakpoint");
            Debug.Break();
        }

        protected override State OnUpdate()
        {
            return State.Success;
        }
    }
}
