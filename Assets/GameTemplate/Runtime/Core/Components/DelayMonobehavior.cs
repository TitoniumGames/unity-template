using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameTemplate.Runtime
{
    public class DelayMonobehavior : MonoBehaviour
    {
        [SerializeField] private float time;
        [SerializeField] private bool delayOnStart;
        public UnityEvent OnDelayCompleted;
        public Coroutine delayCoroutine;
        private void Start()
        {
            if (delayOnStart)
            {
                StarDelay();
            }
        }

        public void StarDelay()
        {
            if (delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
            }
            delayCoroutine = StartCoroutine(DelayCoroutine());
        }

        public void StopDelay()
        {
            if (delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
            }
        }
        
        private IEnumerator DelayCoroutine()
        {
            yield return new WaitForSeconds(time);
            OnDelayCompleted?.Invoke();
        }
    }
}
