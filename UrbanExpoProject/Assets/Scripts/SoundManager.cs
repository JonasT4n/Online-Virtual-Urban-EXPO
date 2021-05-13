using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

namespace UrbanExpo
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioSource[] musicAudios = null;
        [SerializeField] private AudioSource[] sfxAudios = null;

        [Header("Sound Setting Attributes")]
        [SerializeField, Range(0f, 1f)] private float currentMusicVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float currentSFXVolume = 1f;

        [Header("Additional UI")]
        [SerializeField] private Slider musicVolumeSlider = null;
        [SerializeField] private Slider sfxVolumeSlider = null;

        [BoxGroup("DEBUG"), SerializeField] private bool isMuted = false;

        #region Properties
        public float MusicVolume
        {
            set
            {
                currentMusicVolume = value;
                if (!isMuted) SetMusicVolume(currentMusicVolume);
            }
            get => currentMusicVolume;
        }

        public float SFXVolume
        {
            set
            {
                currentSFXVolume = value;
                if (!isMuted) SetSFXVolume(currentSFXVolume);
            }
            get => currentSFXVolume;
        }
        #endregion

        #region Unity BuiltIn Methods
        private void Start()
        {
            // Initialize value of volume from master value to slider value
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = (musicVolumeSlider.maxValue - musicVolumeSlider.minValue) * currentMusicVolume;
            SetMusicVolume(currentMusicVolume);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = (sfxVolumeSlider.maxValue - sfxVolumeSlider.minValue) * currentSFXVolume;
            SetSFXVolume(currentSFXVolume);
        }
        #endregion

        public void SliderValueChange(Slider slider)
        {
            // Check if slider value change was music volume slider
            if (musicVolumeSlider.Equals(slider))
            {
                float min = musicVolumeSlider.minValue;
                MusicVolume = (musicVolumeSlider.value - min) / (musicVolumeSlider.maxValue - min);
            }

            // Check if slider value change was sfx volume slider
            if (sfxVolumeSlider.Equals(slider))
            {
                float min = sfxVolumeSlider.minValue;
                SFXVolume = (sfxVolumeSlider.value - min) / (sfxVolumeSlider.maxValue - min);
            }
        }

        public void ToggleSoundMaster(bool active)
        {
            isMuted = !active;
            SetMusicVolume(isMuted ? 0 : currentMusicVolume);
            SetSFXVolume(isMuted ? 0 : currentSFXVolume);
        }

        private void SetMusicVolume(float volume)
        {
            foreach (AudioSource audio in musicAudios)
            {
                audio.volume = volume;
            }
        }

        private void SetSFXVolume(float volume)
        {
            foreach (AudioSource audio in sfxAudios)
            {
                audio.volume = volume;
            }
        }
    }

}