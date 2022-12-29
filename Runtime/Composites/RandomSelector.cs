using UnityEngine;

namespace Prybh 
{
    public class RandomSelector : CompositeNode 
    {
        protected int current;

        protected override void OnStart() 
        {
            current = Random.Range(0, children.Count);
        }

        protected override State OnUpdate() 
        {
            return children[current].Update();
        }
    }
}