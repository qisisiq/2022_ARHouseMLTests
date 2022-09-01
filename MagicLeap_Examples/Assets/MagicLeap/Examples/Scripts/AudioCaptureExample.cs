// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="AudioCaptureExample.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by your Early Access Terms and Conditions.
// This software is an Early Access Product.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Examples
{
    /// <summary>
    /// This class uses a controller to start/stop audio capture
    /// using the Unity Microphone class. The audio is then played
    /// through an audio source attached to the parrot in the scene.
    /// </summary>
    public class AudioCaptureExample : MonoBehaviour
    {
        public enum CaptureMode
        {
            Inactive = 0,
            Realtime,
            Delayed
        }

        [SerializeField, Tooltip("The reference to the place from camera script for the parrot.")]
        private PlaceFromCamera _placeFromCamera = null;

        [SerializeField, Tooltip("The audio source that should capture the microphone input.")]
        private AudioSource _inputAudioSource = null;

        [SerializeField, Tooltip("The audio source that should replay the captured audio.")]
        private AudioSource _playbackAudioSource = null;

        [SerializeField, Tooltip("The text to display the recording status.")]
        private Text _statusLabel = null;

        [Space]
        [Header("Delayed Playback")]
        [SerializeField, Range(1, 2), Tooltip("The pitch used for delayed audio playback.")]
        private float _pitch = 1.5f;

        [SerializeField, Tooltip("Game object to use for visualizing the root mean square of the microphone audio")]
        private GameObject _rmsVisualizer = null;

        [SerializeField, Min(0), Tooltip("Scale value to set for AmplitudeVisualizer when rms is 0")]
        private float _minScale = 0.1f;

        [SerializeField, Min(0), Tooltip("Scale value to set for AmplitudeVisualizer when rms is 1")]
        private float _maxScale = 1.0f;

        private bool hasPermission = false;
        private bool isCapturing = false;
        private CaptureMode captureMode = CaptureMode.Inactive;
        private string deviceMicrophone = string.Empty;

        private float audioMaxSample = 0;
        private readonly float[] audioSamples = new float[128];

        private int numSyncIterations = 30;
        private int numSamplesLatency = 1024;
        private bool playbackStarted = false;

        private bool isAudioDetected = false;
        private float audioLastDetectionTime = 0;
        private float audioDetectionStart = 0;
        private float audioDetectionEnd = 0;

        private float[] playbackSamples = null;

        private const int AUDIO_CLIP_LENGTH_SECONDS = 60;
        private const int AUDIO_CLIP_FREQUENCY_HERTZ = 48000;
        private const float AUDIO_SENSITVITY_DECIBEL = 0.00035f;
        private const float AUDIO_CLIP_TIMEOUT_SECONDS = 2;
        private const float AUDIO_CLIP_FALLOFF_SECONDS = 0.5f;

        private const int NUM_SYNC_ITERATIONS = 30;
        private const int NUM_SAMPLES_LATENCY = 1024;

        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.ControllerActions controllerActions;

        private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();
        private MLAudioInput.Clip mlAudioClip;

        void Awake()
        {
            if (_inputAudioSource == null)
            {
                Debug.LogError("AudioCaptureExample._inputAudioSource is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_playbackAudioSource == null)
            {
                Debug.LogError("AudioCaptureExample._playbackAudioSource is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_statusLabel == null)
            {
                Debug.LogError("AudioCaptureExample._statusLabel is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_placeFromCamera == null)
            {
                Debug.LogError("AudioCaptureExample._placeFromCamera is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_rmsVisualizer == null)
            {
                Debug.LogError("AudioCaptureExample._rmsVisualizer is not set, disabling script.");
                enabled = false;
                return;
            }

            UpdateStatus();

            permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
            permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;

            MLPermissions.RequestPermission(MLPermission.RecordAudio, permissionCallbacks);

#if UNITY_MAGICLEAP || UNITY_ANDROID
            mlInputs = new MagicLeapInputs();
            mlInputs.Enable();
            controllerActions = new MagicLeapInputs.ControllerActions(mlInputs);

            controllerActions.Bumper.performed += HandleOnBumperDown;
            controllerActions.Trigger.performed += HandleOnTriggerDown;
#endif

            // Frequency = number of samples per second
            // 1000ms => AUDIO_CLIP_FREQUENCY_HERTZ
            // 1ms => AUDIO_CLIP_FREQUENCY_HERTZ / 1000
            // 16ms => AUDIO_CLIP_FREQUENCY_HERTZ * 16 / 1000
            playbackSamples = new float[AUDIO_CLIP_FREQUENCY_HERTZ * 16 / 1000];
        }

        void OnDestroy()
        {

            permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
            permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;

#if UNITY_MAGICLEAP || UNITY_ANDROID
            controllerActions.Bumper.performed -= HandleOnBumperDown;
            controllerActions.Trigger.performed -= HandleOnTriggerDown;

            mlInputs.Dispose();
#endif

            StopCapture();
        }

        private void Update()
        {
            if (isCapturing)
            {
                ProcessAudioPlayback();
            }

            if (playbackStarted)
            {
                if (captureMode == CaptureMode.Delayed)
                {
                    DetectAudio();
                }
            }

            UpdateStatus();
        }

        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                captureMode = CaptureMode.Inactive;

                if (playbackStarted)
                {
                    StopCapture();
                }
            }
        }

        private void StartMicrophone()
        {
            if (!MLPermissions.CheckPermission(MLPermission.RecordAudio).IsOk)
            {
                Debug.LogError($"AudioCaptureExample.StartMicrophone() cannot start, {MLPermission.RecordAudio} not granted.");
                return;
            }

            if (captureMode == CaptureMode.Inactive)
            {
                Debug.LogError("AudioCaptureExample.StartMicrophone() cannot start with CaptureMode.Inactive.");
                return;
            }


            _playbackAudioSource.Stop();
            var captureType = MLAudioInput.MicCaptureType.VoiceCapture;
            this.mlAudioClip = MLAudioInput.CreateClip(MLAudioInput.MicCaptureType.VoiceCapture, true, 3, MLAudioInput.GetSampleRate(captureType));
            _inputAudioSource.clip = this.mlAudioClip.UnityAudioClip;
            _inputAudioSource.loop = true;

            isCapturing = true;
            numSamplesLatency = NUM_SAMPLES_LATENCY;
            numSyncIterations = NUM_SYNC_ITERATIONS;
        }

        private void ProcessAudioPlayback()
        {
            if (numSyncIterations > 0)
            {
                --numSyncIterations;
            }

            if (!playbackStarted && numSyncIterations == 0)
            {
                _inputAudioSource.Play();
                if (mlAudioClip.SamplesPosition >= numSamplesLatency)
                    _inputAudioSource.timeSamples = mlAudioClip.SamplesPosition - numSamplesLatency;

                playbackStarted = true;

                switch (captureMode)
                {
                    case CaptureMode.Realtime:
                        {
                            _playbackAudioSource.pitch = 1;
                            _playbackAudioSource.clip = _inputAudioSource.clip;
                            _playbackAudioSource.loop = true;
                            _playbackAudioSource.Play();
                            break;
                        }

                    case CaptureMode.Delayed:
                        {
                            _playbackAudioSource.pitch = _pitch;
                            _playbackAudioSource.loop = false;
                            break;
                        }
                }
            }

            if (this.mlAudioClip == null)
                return;

            // Increasing latency
            if ((_inputAudioSource.timeSamples > mlAudioClip.SamplesPosition) && (mlAudioClip.SamplesPosition > numSamplesLatency * 4))
            {
                numSamplesLatency = numSamplesLatency * 2;
                _inputAudioSource.timeSamples = mlAudioClip.SamplesPosition - numSamplesLatency;
            }

            if (_playbackAudioSource.isPlaying)
            {
                mlAudioClip.GetData(playbackSamples, _playbackAudioSource.timeSamples);

                float squaredSum = 0;
                for (int i = 0; i < playbackSamples.Length; ++i)
                {
                    squaredSum += playbackSamples[i] * playbackSamples[i];
                }

                float rootMeanSq = Mathf.Sqrt(squaredSum / playbackSamples.Length);
                float scaleFactor = rootMeanSq * (_maxScale - _minScale) + _minScale;
                _rmsVisualizer.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            }
        }

        private void StopCapture()
        {
            isCapturing = false;
            playbackStarted = false;

            // Stops the ml audio clip and the input audio source.
            _inputAudioSource.Stop();

            if (!string.IsNullOrEmpty(deviceMicrophone))
            {
                mlAudioClip.Stop();
            }

            // Stop audio playback source and reset settings.
            _playbackAudioSource.Stop();
            _playbackAudioSource.loop = false;
            _playbackAudioSource.clip = null;
        }

        /// <summary>
        /// Update the example status label.
        /// </summary>
        private void UpdateStatus()
        {
            _statusLabel.text = string.Format("<color=#dbfb76><b>Controller Data</b></color>\nStatus: {0}\n", ControllerStatus.Text);
            _statusLabel.text += "\n<color=#dbfb76><b>AudioCapture Data</b></color>\n";
            _statusLabel.text += string.Format("Status: {0}\n", captureMode.ToString());
        }

        private void DetectAudio()
        {
            // Analyze the input spectrum data, to determine when someone is speaking.
            _inputAudioSource.GetSpectrumData(audioSamples, 0, FFTWindow.Rectangular);
            audioMaxSample = audioSamples.Max();

            if (audioMaxSample > AUDIO_SENSITVITY_DECIBEL)
            {
                // Note the first moment speech was detected.
                audioLastDetectionTime = Time.time;

                if (isAudioDetected == false)
                {
                    isAudioDetected = true;
                    audioDetectionStart = _inputAudioSource.time;
                }
            }
            else if (isAudioDetected && (Time.time > audioLastDetectionTime + AUDIO_CLIP_TIMEOUT_SECONDS))
            {
                // Note the last moment speach was detected.
                audioDetectionEnd = _inputAudioSource.time - (AUDIO_CLIP_TIMEOUT_SECONDS - AUDIO_CLIP_FALLOFF_SECONDS);

                // Create the playback clip.
                _playbackAudioSource.clip = CreateAudioClip(mlAudioClip, audioDetectionStart, audioDetectionEnd);
                if (_playbackAudioSource.clip != null)
                {
                    _playbackAudioSource.Play();
                }

                // Reset and allow for new captured speech.
                isAudioDetected = false;
                audioDetectionStart = 0;
                audioDetectionEnd = 0;
            }
        }

        /// <summary>
        /// Creates a new audio clip within the start and stop range.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        private AudioClip CreateAudioClip(MLAudioInput.Clip mlAudioClip, float start, float stop)
        {
            var clip = mlAudioClip.UnityAudioClip;

            int length = (int)(clip.frequency * (stop - start));
            if (length <= 0)
            {
                return null;
            }

            AudioClip newAudioClip = AudioClip.Create("Parrot_Voice", length, 1, clip.frequency, false);

            float[] data = new float[length];
            mlAudioClip.GetData(data, (int)(clip.frequency * start));
            newAudioClip.SetData(data, 0);

            return newAudioClip;
        }

        /// <summary>
        /// Responds to permission requester result.
        /// </summary>
        /// <param name="result"/>
        private void OnPermissionDenied(string permission)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            Debug.LogError($"AudioCaptureExample failed to get requested permission {permission}, disabling script.");
            UpdateStatus();
            enabled = false;
            return;
#endif
        }

        private void OnPermissionGranted(string permission)
        {
            hasPermission = true;
            Debug.Log($"Succeeded in requesting {permission}.");
        }

        private void HandleOnTriggerDown(InputAction.CallbackContext inputCallback)
        {
            if (!hasPermission)
            {
                return;
            }

            if (!controllerActions.Trigger.WasPressedThisFrame())
            {
                return;
            }

            captureMode = (captureMode == CaptureMode.Delayed) ? CaptureMode.Inactive : captureMode + 1;

            // Stop & Start to clear the previous mode.
            if (isCapturing)
            {
                StopCapture();
            }

            if (captureMode != CaptureMode.Inactive)
            {
                StartMicrophone();
            }
        }

        private void HandleOnBumperDown(InputAction.CallbackContext inputCallback)
        {
            StartCoroutine(nameof(SingleFrameUpdate));
        }

        private IEnumerator SingleFrameUpdate()
        {
            _placeFromCamera.PlaceOnUpdate = true;
            yield return new WaitForEndOfFrame();
            _placeFromCamera.PlaceOnUpdate = false;
        }
    }
}
