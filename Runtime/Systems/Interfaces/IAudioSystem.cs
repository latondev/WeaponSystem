namespace PullGame
{
    public interface IAudioSystem
    {
        void Update(float deltaTime);
        void PlayShootSound();
        void PlayReloadSound();
        void PlayNoAmmoSound();
    }
}
