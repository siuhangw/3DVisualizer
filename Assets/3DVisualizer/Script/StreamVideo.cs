using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace S3
{
	public class StreamVideo : MonoBehaviour {

        private VideoPlayer videoPlayer;
        private AudioSource audioSource;

        private bool isPause = false;

        private void Start()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            audioSource = GetComponent<AudioSource>();
        }

        public void PlayPause()
        {
            if (!isPause)
            {
                videoPlayer.Pause();
                audioSource.Pause();
                isPause = true;
                Debug.Log("Pause the video.");
            }
            else
            {
                videoPlayer.Play();
                audioSource.Play();
                isPause = false;
                Debug.Log("Resume the video.");
            }
        }
	}
}
