using UnityEngine;

namespace DungeonArchitect.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip explorationMusic;
        [SerializeField] private AudioClip combatMusic;
        [SerializeField] private AudioClip bossMusic;

        [Header("SFX")]
        [SerializeField] private AudioClip roomPlaceSound;
        [SerializeField] private AudioClip cardSelectSound;
        [SerializeField] private AudioClip combatHitSound;
        [SerializeField] private AudioClip goldPickupSound;
        [SerializeField] private AudioClip stairFoundSound;
        [SerializeField] private AudioClip deathSound;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void PlayExplorationMusic() => PlayMusic(explorationMusic);
        public void PlayCombatMusic() => PlayMusic(combatMusic);
        public void PlayBossMusic() => PlayMusic(bossMusic);
        public void PlayMenuMusic() => PlayMusic(menuMusic);

        public void PlaySFX(AudioClip clip)
        {
            if (clip != null)
                sfxSource.PlayOneShot(clip);
        }

        public void PlayRoomPlace() => PlaySFX(roomPlaceSound);
        public void PlayCardSelect() => PlaySFX(cardSelectSound);
        public void PlayCombatHit() => PlaySFX(combatHitSound);
        public void PlayGoldPickup() => PlaySFX(goldPickupSound);
        public void PlayStairFound() => PlaySFX(stairFoundSound);
        public void PlayDeath() => PlaySFX(deathSound);

        public void SetMusicVolume(float volume)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }

        public void SetSFXVolume(float volume)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }
}
