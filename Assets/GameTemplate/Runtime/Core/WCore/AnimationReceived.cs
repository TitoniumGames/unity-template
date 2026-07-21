using UnityEngine;
using UnityEngine.Events;

namespace WCore
{
    public class AnimationReceived: MonoBehaviour
    {
        public UnityEvent OnAnimationStarted;
        public UnityEvent OnAnimationFinished;
        
        
        public void AnimationStarted()
        {
            OnAnimationStarted?.Invoke();
        }

        public void AnimationFinished()
        {
            OnAnimationFinished?.Invoke();
        }
    }
}