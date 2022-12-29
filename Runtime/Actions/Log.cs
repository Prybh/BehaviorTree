using UnityEngine;

namespace Prybh 
{
    public class Log : ActionNode
    {
        [SerializeField] private string message;

        protected override State OnUpdate() 
        {
            Debug.Log($"{message}");
            return State.Success;
        }
    }
}
