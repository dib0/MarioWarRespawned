using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace MarioWarRespawned.Management
{
    public class AudioManager
    {
        private ContentManager _contentManager;
        private float _soundVolume = 1.0f;
        private float _musicVolume = 0.7f;
        private readonly Dictionary<string, SoundEffectInstance> _loopingSounds = new();

        public float SoundVolume
        {
            get => _soundVolume;
            set
            {
                _soundVolume = MathHelper.Clamp(value, 0f, 1f);
                SoundEffect.MasterVolume = _soundVolume;
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = MathHelper.Clamp(value, 0f, 1f);
                MediaPlayer.Volume = _musicVolume;
            }
        }

        public void PlaySound(string name)
        {
            var sound = _contentManager?.GetSound(name);
            sound?.Play();
        }

        public void PlayMusic(string name, bool isRepeating = true)
        {
            var music = _contentManager?.GetMusic(name);
            if (music != null)
            {
                MediaPlayer.Play(music);
                MediaPlayer.IsRepeating = isRepeating;
            }
        }

        public void StopMusic()
        {
            MediaPlayer.Stop();
        }

        public void PauseMusic()
        {
            MediaPlayer.Pause();
        }

        public void ResumeMusic()
        {
            MediaPlayer.Resume();
        }

        public void SetContentManager(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }
    }
}
