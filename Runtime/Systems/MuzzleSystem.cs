using System.Collections.Generic;
using UnityEngine;

namespace PullGame
{
	public class MuzzleSystem
	{
	    private readonly Transform weaponTransform;
	    private readonly WeaponConfig config;
	    private readonly List<Transform> muzzleTransforms = new();
	    private readonly List<FirePoint> firePoints = new();
	    
	    public MuzzleSystem(Transform weaponTransform, WeaponConfig config)
	    {
	        this.weaponTransform = weaponTransform;
	        this.config = config;
	    }
	    
	    public void Initialize()
	    {
	        CreateMuzzleTransforms();
	        GenerateFirePoints();
	    }
	    
	    private void CreateMuzzleTransforms()
	    {
	        foreach (Transform child in muzzleTransforms)
	        {
	            if (child != null && Application.isPlaying)
	            {
	                Object.Destroy(child.gameObject);
	            }
	        }
	        muzzleTransforms.Clear();
	        
	        for (int i = 0; i < config.muzzleConfigs.Length; i++)
	        {
	            var muzzleConfig = config.muzzleConfigs[i];
	            var muzzleObj = new GameObject($"Muzzle_{i}");
	            muzzleObj.transform.SetParent(weaponTransform);
	            muzzleObj.transform.localPosition = muzzleConfig.localPosition;
	            muzzleObj.transform.localRotation = muzzleConfig.localRotation;
	            
	            muzzleTransforms.Add(muzzleObj.transform);
	        }
	    }
	    
	    private void GenerateFirePoints()
	    {
	        firePoints.Clear();
	        
	        for (int i = 0; i < config.muzzleConfigs.Length; i++)
	        {
	            var muzzleConfig = config.muzzleConfigs[i];
	            var muzzleTransform = muzzleTransforms[i];
	            
	            var points = muzzleConfig.mode switch
	            {
	                MuzzleMode.Line => GenerateLinePoints(muzzleConfig, muzzleTransform),
	                MuzzleMode.Circle => GenerateCirclePoints(muzzleConfig, muzzleTransform),
	                _ => new List<FirePoint>()
	            };
	            
	            firePoints.AddRange(points);
	        }
	    }
	    
	    private List<FirePoint> GenerateLinePoints(MuzzleConfig config, Transform muzzle)
	    {
	        var points = new List<FirePoint>();
	        float halfDistance = (config.pointsCount - 1) * config.distanceBetweenPoints / 2f;
	        Vector2 startPos = Vector2.up * halfDistance;
	        
	        for (int i = 0; i < config.pointsCount; i++)
	        {
	            Vector2 localPos = startPos - Vector2.up * (i * config.distanceBetweenPoints);
	            Vector3 direction = Vector2.Lerp(Vector2.right, localPos, config.fireAngleModifier);
	            
	            points.Add(new FirePoint(localPos, direction, muzzle));
	        }
	        
	        return points;
	    }
	    
	    private List<FirePoint> GenerateCirclePoints(MuzzleConfig config, Transform muzzle)
	    {
	        var points = new List<FirePoint>();
	        float stepAngle = config.autoCalculateStepAngle 
	            ? 360f / config.pointsCount 
	            : config.stepAngle;
	            
	        float minAngle = -((config.pointsCount - 1) * stepAngle) / 2f;
	        
	        for (int i = 0; i < config.pointsCount; i++)
	        {
	            float angleRad = Mathf.Deg2Rad * (minAngle + i * stepAngle);
	            Vector2 localPos = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * config.radius;
	            Vector3 direction = Vector2.Lerp(localPos.normalized, Vector2.right, config.fireAngleModifier);
	            
	            points.Add(new FirePoint(localPos, direction, muzzle));
	        }
	        
	        return points;
	    }
	    
	    public FirePoint[] GetFirePoints() => firePoints.ToArray();
	    
	    public void DrawGizmos()
	    {
	        if (firePoints == null || firePoints.Count == 0) return;
	        
	        Gizmos.color = Color.yellow;
	        foreach (var point in firePoints)
	        {
	            Vector3 pos = point.GetWorldPosition();
	            Vector3 dir = point.GetWorldDirection();
	            
	            Gizmos.DrawRay(pos, dir * 0.5f);
	            DrawArrow(pos, dir * 0.5f);
	        }
	    }
	    
	    private void DrawArrow(Vector3 pos, Vector3 direction, float arrowSize = 0.1f, float angle = 20f)
	    {
	        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(180 + angle, 0, 0) * Vector3.forward;
	        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(180 - angle, 0, 0) * Vector3.forward;
	        
	        Gizmos.DrawRay(pos + direction, right * arrowSize);
	        Gizmos.DrawRay(pos + direction, left * arrowSize);
	    }
	}
	
	public class FirePoint
	{
	    private readonly Vector3 localPosition;
	    private readonly Vector3 localDirection;
	    private readonly Transform muzzle;
	    
	    public FirePoint(Vector3 localPosition, Vector3 localDirection, Transform muzzle)
	    {
	        this.localPosition = localPosition;
	        this.localDirection = localDirection;
	        this.muzzle = muzzle;
	    }
	    
	    public Vector3 GetWorldPosition()
	    {
	        return muzzle.TransformPoint(localPosition);
	    }
	    
	    public Vector3 GetWorldDirection()
	    {
	        return muzzle.TransformDirection(localDirection).normalized;
	    }
	    
	    public Quaternion GetWorldRotation()
	    {
	        Vector3 dir = GetWorldDirection();
	        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
	        return Quaternion.Euler(0, 0, angle);
	    }
	}
}