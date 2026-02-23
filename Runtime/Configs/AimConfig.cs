using System;
using UnityEngine;

namespace PullGame
{
	[Serializable]
	public class AimConfig
	{
	    public bool enabled = false;
	    public bool aimAtCursor = true;
	    public float rotateSpeed = 10f;
	    public float rotationRadius = 1f;
	}
}