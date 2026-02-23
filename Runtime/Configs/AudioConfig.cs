using System;
using UnityEngine;

namespace PullGame
{
	[Serializable]
	public class AudioConfig
	{
	    public AudioClip shootSound;
	    public AudioClip reloadSound;
	    public AudioClip noAmmoSound;
	    
	    [Range(0, 1)] public float shootVolume = 1f;
	    [Range(0, 1)] public float reloadVolume = 1f;
	    [Range(0, 1)] public float noAmmoVolume = 1f;
	    
	    public float noAmmoSoundCooldown = 0.5f;
	}
}