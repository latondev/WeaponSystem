using UnityEngine;

namespace PullGame
{
    public class WeaponTargeting : MonoBehaviour
    {
        [Header("Targeting Settings")]
        [SerializeField] private TargetingMode mode = TargetingMode.NearestEnemy;
        [SerializeField] private float searchRadius = 50f;
        [SerializeField] private string targetTag = "Enemy";
        [SerializeField] private LayerMask targetLayers = -1;
        
        [Header("Manual Target")]
        [SerializeField] private Transform manualTarget;
        
        [Header("Auto Targeting")]
        [SerializeField] private float updateInterval = 0.5f;
        
        private Weapon weapon;
        [SerializeField] Transform currentTarget;
        private float updateTimer;
        
        public enum TargetingMode
        {
            Manual,              // User sets target manually
            NearestEnemy,        // Auto-find nearest enemy
            NearestInCrosshair,  // Nearest to aim direction
            MousePosition        // Target mouse position (for 2D)
        }
        
        private void Awake()
        {
            weapon = GetComponent<Weapon>();
        }
        
        private void Update()
        {
            updateTimer += Time.deltaTime;
            
            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateTarget();
            }
        }
        
        private void UpdateTarget()
        {
            Transform target = mode switch
            {
                TargetingMode.Manual => manualTarget,
                TargetingMode.NearestEnemy => FindNearestTarget(),
                TargetingMode.NearestInCrosshair => FindTargetInCrosshair(),
                TargetingMode.MousePosition => GetMouseWorldPosition(),
                _ => null
            };
            
            if (target != currentTarget)
            {
                currentTarget = target;
                weapon?.SetProjectileTarget(currentTarget);
            }
        }
        
        private Transform FindNearestTarget()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, targetLayers);
            
            Transform nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;
                
                // Check tag if specified
                if (!string.IsNullOrEmpty(targetTag) && !hit.CompareTag(targetTag))
                    continue;
                
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = hit.transform;
                }
            }
            
            return nearest;
        }
        
        private Transform FindTargetInCrosshair()
        {
            Ray ray = new Ray(transform.position, transform.forward);
            
            if (Physics.SphereCast(ray, 1f, out RaycastHit hit, searchRadius, targetLayers))
            {
                if (string.IsNullOrEmpty(targetTag) || hit.collider.CompareTag(targetTag))
                {
                    return hit.collider.transform;
                }
            }
            
            return null;
        }
        
        private Transform GetMouseWorldPosition()
        {
            // For 2D games or top-down
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                // Create a temporary transform at mouse position
                GameObject temp = new GameObject("MouseTarget");
                temp.transform.position = hit.point;
                temp.hideFlags = HideFlags.HideAndDontSave;
                Destroy(temp, 1f); // Auto-destroy after 1 second
                return temp.transform;
            }
            
            return null;
        }
        
        public void SetManualTarget(Transform target)
        {
            manualTarget = target;
            mode = TargetingMode.Manual;
            UpdateTarget();
        }
        
        public Transform GetCurrentTarget()
        {
            return currentTarget;
        }
        
        private void OnDrawGizmos()
        {
            // Draw search radius
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, searchRadius);
            
            // Draw line to target
            if (currentTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentTarget.position);
                Gizmos.DrawWireSphere(currentTarget.position, 0.5f);
            }
        }
    }
}
