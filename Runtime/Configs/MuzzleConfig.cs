using System;
using UnityEngine;

namespace PullGame
{
	[Serializable]
	public class MuzzleConfig
	{
	    public MuzzleMode mode;
	    [Min(1)] public int pointsCount = 1;
	    
	    [Min(0.0001f)] public float distanceBetweenPoints = 0.5f;
	    
	    [Min(0.0001f)] public float radius = 1f;
	    public bool autoCalculateStepAngle = true;
	    [Min(0.0001f)] public float stepAngle = 45f;
	    
	    [Range(0, 1)] public float fireAngleModifier = 0f;
	    public Vector3 localPosition;
	    public Quaternion localRotation = Quaternion.identity;
	}
	
	public enum MuzzleMode
	{
	    Line,
	    Circle
	}
}