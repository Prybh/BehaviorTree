using System.Collections.Generic;
using UnityEngine;

namespace Prybh 
{
    public class RootNode : DecoratorNode
    {
        protected override State OnUpdate()
        {
            return child.Update();
        }
    }
}