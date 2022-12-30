using UnityEngine;

namespace Prybh 
{
    [System.Serializable]
    public class Blackboard {}

    [System.Serializable]
    public class SimpleVectorBlackboard : Blackboard
    {
        public Vector3 moveToPosition;
    }
}