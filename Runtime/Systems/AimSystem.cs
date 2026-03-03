using UnityEngine;

namespace PullGame
{
	public class AimSystem : IAimSystem
	{
	    private readonly Transform weaponTransform;
	    private readonly AimConfig config;
	    private Transform customTarget;
	    
	    public AimSystem(Transform weaponTransform, AimConfig config)
	    {
	        this.weaponTransform = weaponTransform;
	        this.config = config;
	    }
	    
	    public void Update(float deltaTime)
	    {
	        if (!config.enabled) return;
	        
	        Vector3 targetScreenPos = GetTargetScreenPosition();
	        Vector3 weaponScreenPos = Camera.main.WorldToScreenPoint(weaponTransform.position);
	        Vector3 holderScreenPos = GetHolderScreenPosition();
	        
	        if (weaponTransform.parent != null)
	        {
	            Vector2 directionToTarget = new Vector2(
	                targetScreenPos.x - holderScreenPos.x,
	                targetScreenPos.y - holderScreenPos.y
	            ).normalized;
	            
	            weaponTransform.localPosition = directionToTarget * config.rotationRadius;
	        }
	        
	        Vector2 aimDirection = new Vector2(
	            targetScreenPos.x - weaponScreenPos.x,
	            targetScreenPos.y - weaponScreenPos.y
	        );
	        
	        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
	        Quaternion targetRotation;
	        
	        if (targetScreenPos.x < weaponScreenPos.x)
	        {
	            targetRotation = Quaternion.Euler(180f, 0f, -angle);
	        }
	        else
	        {
	            targetRotation = Quaternion.Euler(0f, 0f, angle);
	        }
	        
	        weaponTransform.rotation = Quaternion.Lerp(
	            weaponTransform.rotation,
	            targetRotation,
	            deltaTime * config.rotateSpeed
	        );
	    }
	    
	    private Vector3 GetTargetScreenPosition()
	    {
	        if (config.aimAtCursor)
	        {
	            return Input.mousePosition;
	        }
	        else if (customTarget != null)
	        {
	            return Camera.main.WorldToScreenPoint(customTarget.position);
	        }
	        
	        return weaponTransform.position + weaponTransform.right;
	    }
	    
	    private Vector3 GetHolderScreenPosition()
	    {
	        if (weaponTransform.parent != null)
	        {
	            return Camera.main.WorldToScreenPoint(weaponTransform.parent.position);
	        }
	        return Camera.main.WorldToScreenPoint(weaponTransform.position);
	    }
	    
	    public void SetCustomTarget(Transform target)
	    {
	        customTarget = target;
	    }
	}
}