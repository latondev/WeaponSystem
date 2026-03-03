using UnityEngine;

namespace PullGame
{
	public class AudioSystem : IAudioSystem
	{
	    private readonly GameObject owner;
	    private readonly AudioConfig config;
	    
	    private AudioSource shootSource;
	    private AudioSource reloadSource;
	    private AudioSource noAmmoSource;
	    
	    private float noAmmoSoundTimer;
	    
	    public AudioSystem(GameObject owner, AudioConfig config)
	    {
	        this.owner = owner;
	        this.config = config;
	        
	        InitializeAudioSources();
	    }
	    
	    private void InitializeAudioSources()
	    {
	        if (config.shootSound != null)
	        {
	            shootSource = CreateAudioSource("ShootAudio", config.shootSound, config.shootVolume);
	        }
	        
	        if (config.reloadSound != null)
	        {
	            reloadSource = CreateAudioSource("ReloadAudio", config.reloadSound, config.reloadVolume);
	        }
	        
	        if (config.noAmmoSound != null)
	        {
	            noAmmoSource = CreateAudioSource("NoAmmoAudio", config.noAmmoSound, config.noAmmoVolume);
	        }
	    }
	    
	    private AudioSource CreateAudioSource(string name, AudioClip clip, float volume)
	    {
	        var sourceObj = new GameObject(name);
	        sourceObj.transform.SetParent(owner.transform);
	        sourceObj.transform.localPosition = Vector3.zero;
	        
	        var source = sourceObj.AddComponent<AudioSource>();
	        source.clip = clip;
	        source.volume = volume;
	        source.playOnAwake = false;
	        source.loop = false;
	        
	        return source;
	    }
	    
	    public void Update(float deltaTime)
	    {
	        noAmmoSoundTimer -= deltaTime;
	    }
	    
	    public void PlayShootSound()
	    {
	        shootSource?.Play();
	    }
	    
	    public void PlayReloadSound()
	    {
	        reloadSource?.Play();
	    }
	    
	    public void PlayNoAmmoSound()
	    {
	        if (noAmmoSoundTimer <= 0f && noAmmoSource != null)
	        {
	            noAmmoSource.Play();
	            noAmmoSoundTimer = config.noAmmoSoundCooldown;
	        }
	    }
	}
}