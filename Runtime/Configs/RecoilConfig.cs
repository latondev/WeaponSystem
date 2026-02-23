using System;
using UnityEngine;

namespace PullGame
{
	[Serializable]
	public class RecoilConfig
	{
	    public bool enabled = true;
	    public float force = 5f;
	    [Min(0)] public float recoveryTime = 0.5f;
	    public AnimationCurve recoilCurve = AnimationCurve.Linear(0, 0, 1, 1);
	}
}