// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="HandMeshingExample.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by your Early Access Terms and Conditions.
// This software is an Early Access Product.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Examples
{
    public class HandMeshingExample : MonoBehaviour
    {
        /// <summary>
        /// Different Render Modes for the Hand Meshing Example
        /// </summary>
        public enum RenderMode : uint
        {
            Occlusion,
            Wireframe
        }

        [SerializeField, Tooltip("The Hand Meshing Behavior to control")]
        private MLHandMeshingBehavior _behavior = null;

        [SerializeField, Tooltip("Material used in Occlusion Render Mode")]
        private Material _occlusionMaterial = null;

        [SerializeField, Tooltip("Material used in Wireframe Render Mode")]
        private Material _wireframeMaterial = null;

        [SerializeField, Tooltip("Duration, in seconds, to hold key pose before changing render modes"), Min(1.0f)]
        private float _secondsBetweenModes = 2;

        [SerializeField, Tooltip("Status Text")]
        private Text _statusText = null;

        [SerializeField, Tooltip("Switching tooltip text")]
        private TextMesh _switchTooltip = null;

        private const float _minimumConfidence = 0.8f;
        private float _timer = 0;
        private RenderMode _mode = RenderMode.Occlusion;
        private InputDevice rightHandDevice;
        private List<Bone> thumbBones = new List<Bone>();

        /// <summary>
        /// Validate and initialize properties
        /// </summary>
        void Start()
        {
            if (_behavior == null)
            {
                Debug.LogError("Error: HandMeshingExample._behavior is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_occlusionMaterial == null)
            {
                Debug.LogError("Error: HandMeshingExample._occlusionMaterial is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_wireframeMaterial == null)
            {
                Debug.LogError("Error: HandMeshingExample._wireframeMaterial is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_statusText == null)
            {
                Debug.LogError("Error: HandMeshingExample._status is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_switchTooltip == null)
            {
                Debug.LogError("Error: HandMeshingExample._switchTooltip is not set, disabling script.");
                enabled = false;
                return;
            }
            _switchTooltip.gameObject.SetActive(false);

            _timer = _secondsBetweenModes;
        }

        /// <summary>
        /// Updates timer and render mode
        /// </summary>
        void Update()
        {
            if (!rightHandDevice.isValid)
            {
                rightHandDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Right);
                return;
            }

            UpdateStatusText();

            if (!IsSwitchingModes())
            {
                _timer = _secondsBetweenModes;
                _switchTooltip.gameObject.SetActive(false);
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer > 0)
            {
                _switchTooltip.gameObject.SetActive(true);
                UpdateSwitchTooltip();
                return;
            }

            _timer = _secondsBetweenModes;
            _mode = GetNextRenderMode();

            UpdateHandMeshingBehavior();
        }

        private bool IsSwitchingModes()
        {
            //TODO create new switch mode logic
#if UNITY_MAGICLEAP || UNITY_ANDROID
            return false;
#else
            return false;
#endif
        }

        private void UpdateStatusText()
        {
            _statusText.text = string.Format("<color=#dbfb76><b>Controller Data </b></color>\nStatus: {0}\n",
                ControllerStatus.Text);

            _statusText.text += string.Format("\n<color=#dbfb76><b>Hand Mesh Data </b></color>\n Current Render Mode: <color=green>{0}</color>", _mode.ToString());
        }

        private RenderMode GetNextRenderMode()
        {
            return (_mode == RenderMode.Wireframe) ? RenderMode.Occlusion : (RenderMode)((uint)_mode + 1);
        }

        private void UpdateSwitchTooltip()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            if(rightHandDevice.TryGetFeatureValue(CommonUsages.handData, out Hand hand))
            {
                hand.TryGetFingerBones(HandFinger.Thumb, thumbBones);
                thumbBones[0].TryGetPosition(out Vector3 thumbPosition);
                _switchTooltip.transform.position = thumbPosition;
            }
#endif

            _switchTooltip.text = string.Format("Switching to<color=yellow>{0}</color> In {1} seconds",
                GetNextRenderMode(),
                _timer.ToString("0.0"));
        }

        private void UpdateHandMeshingBehavior()
        {
            switch (_mode)
            {
                case RenderMode.Occlusion:
                    _behavior.MeshMaterial = _occlusionMaterial;
                    break;
                case RenderMode.Wireframe:
                    _behavior.MeshMaterial = _wireframeMaterial;
                    break;
            }
        }
    }
}
