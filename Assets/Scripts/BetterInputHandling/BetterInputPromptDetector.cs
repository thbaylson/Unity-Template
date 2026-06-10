using System.Collections.Generic;
using UnityEngine;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Detects nearby prompt providers and publishes the highest-priority prompt to BetterInputService.
    /// </summary>
    public class BetterInputPromptDetector : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float radius = 2.25f;
        [SerializeField, Min(0.02f)] private float scanIntervalSeconds = 0.1f;
        [SerializeField] private LayerMask promptLayers = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

        private readonly Collider[] colliderBuffer = new Collider[32];
        private readonly HashSet<Object> visitedProviders = new HashSet<Object>();
        private float nextScanTime;
        private BetterInputPrompt activePrompt;
        private bool hasActivePrompt;

        private void OnEnable()
        {
            if (BetterInputService.Instance != null)
            {
                BetterInputService.Instance.ActionPerformed += OnActionPerformed;
            }
        }

        private void OnDisable()
        {
            if (BetterInputService.Instance != null)
            {
                BetterInputService.Instance.ActionPerformed -= OnActionPerformed;
                BetterInputService.Instance.ClearCurrentPrompt();
            }
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScanTime)
            {
                return;
            }

            nextScanTime = Time.unscaledTime + scanIntervalSeconds;
            ScanForPrompt();
        }

        private void ScanForPrompt()
        {
            visitedProviders.Clear();

            var position = transform.position;
            var count = Physics.OverlapSphereNonAlloc(position, radius, colliderBuffer, promptLayers, triggerInteraction);

            var foundPrompt = default(BetterInputPrompt);
            var foundDistance = float.MaxValue;
            var found = false;

            for (var index = 0; index < count; index++)
            {
                var collider = colliderBuffer[index];
                if (collider == null)
                {
                    continue;
                }

                var providerComponent = FindPromptProvider(collider);
                var provider = providerComponent as IBetterInputPromptProvider;
                if (provider == null || visitedProviders.Contains(providerComponent))
                {
                    continue;
                }

                visitedProviders.Add(providerComponent);

                var distance = Vector3.Distance(position, collider.transform.position);
                var query = new BetterInputPromptQuery(transform, position, distance);
                if (!provider.TryGetPrompt(query, out var prompt))
                {
                    continue;
                }

                if (!found || prompt.Priority > foundPrompt.Priority || prompt.Priority == foundPrompt.Priority && distance < foundDistance)
                {
                    foundPrompt = prompt;
                    foundDistance = distance;
                    found = true;
                }
            }

            hasActivePrompt = found;
            activePrompt = foundPrompt;

            if (BetterInputService.Instance == null)
            {
                return;
            }

            if (found)
            {
                BetterInputService.Instance.SetCurrentPrompt(foundPrompt);
            }
            else
            {
                BetterInputService.Instance.ClearCurrentPrompt();
            }
        }

        private static MonoBehaviour FindPromptProvider(Collider source)
        {
            var components = source.GetComponentsInParent<MonoBehaviour>();
            foreach (var component in components)
            {
                if (component is IBetterInputPromptProvider)
                {
                    return component;
                }
            }

            return null;
        }

        private void OnActionPerformed(BetterInputActionReference actionReference)
        {
            if (!hasActivePrompt || !actionReference.Equals(activePrompt.ActionReference))
            {
                return;
            }

            activePrompt.Provider?.ExecutePrompt(activePrompt);
            ScanForPrompt();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.1f, 0.8f, 1f, 0.25f);
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
