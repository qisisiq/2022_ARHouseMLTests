// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="RaycastExample.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by your Early Access Terms and Conditions.
// This software is an Early Access Product.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine.InputSystem;

namespace MagicLeap.Examples
{
    using MagicLeap.Core;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.XR;
    using UnityEngine.XR.MagicLeap;

    /// <summary>
    /// This example demonstrates using the magic leap raycast functionality to calculate intersection with the physical space.
    /// It demonstrates casting rays from the users headpose, controller, and eyes position and orientation.
    ///
    /// This example uses a raycast visualizer which represents these intersections with the physical space.
    /// </summary>
    public class RaycastExample : MonoBehaviour
    {
        public enum RaycastMode
        {
            Controller,
            Head,
            Eyes
        }

        [SerializeField, Tooltip("The overview status text for the UI interface.")]
        private Text _overviewStatusText = null;

        [SerializeField, Tooltip("Raycast Visualizer.")]
        private MLRaycastVisualizer _raycastVisualizer = null;

        [SerializeField, Tooltip("Raycast from controller.")]
        private MLRaycastBehavior _raycastController = null;

        [SerializeField, Tooltip("Raycast from headpose.")]
        private MLRaycastBehavior _raycastHead = null;

        [SerializeField, Tooltip("Raycast from eyegaze.")]
        private MLRaycastBehavior _raycastEyes = null;

        private RaycastMode _raycastMode = RaycastMode.Controller;
        private int _modeCount = System.Enum.GetNames(typeof(RaycastMode)).Length;

        private float _confidence = 0.0f;
        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.ControllerActions controllerActions;

        private InputDevice eyesDevice;

        /// <summary>
        /// Validate all required components and sets event handlers.
        /// </summary>
        void Awake()
        {
            if (_overviewStatusText == null)
            {
                Debug.LogError("Error: RaycastExample._overviewStatusText is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_raycastController == null)
            {
                Debug.LogError("Error: RaycastExample._raycastController is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_raycastHead == null)
            {
                Debug.LogError("Error: RaycastExample._raycastHead is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_raycastEyes == null)
            {
                Debug.LogError("Error: RaycastExample._raycastEyes is not set, disabling script.");
                enabled = false;
                return;
            }

            _raycastController.gameObject.SetActive(false);
            _raycastHead.gameObject.SetActive(false);
            _raycastEyes.gameObject.SetActive(false);
            _raycastMode = RaycastMode.Controller;

            UpdateRaycastMode();

#if UNITY_MAGICLEAP || UNITY_ANDROID
            mlInputs = new MagicLeapInputs();
            mlInputs.Enable();
            controllerActions = new MagicLeapInputs.ControllerActions(mlInputs);
            
            controllerActions.Bumper.performed += OnButtonDown;
#endif
        }

        void Update()
        {
            if (!eyesDevice.isValid)
            {
                eyesDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.TrackedDevice);
                return;
            }

            UpdateStatusText();
        }

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDestroy()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            controllerActions.Bumper.performed -= OnButtonDown;
            mlInputs.Dispose();
#endif
        }

        /// <summary>
        /// Updates type of raycast and enables correct cursor.
        /// </summary>
        private void UpdateRaycastMode()
        {
            DisableRaycast(_raycastVisualizer.raycast);

            switch (_raycastMode)
            {
                case RaycastMode.Controller:
                    {
                        EnableRaycast(_raycastController);
                        break;
                    }
                case RaycastMode.Head:
                    {
                        EnableRaycast(_raycastHead);
                        break;
                    }
                case RaycastMode.Eyes:
                    {
                        EnableRaycast(_raycastEyes);
                        break;
                    }
            }
        }

        /// <summary>
        /// Enables raycast behavior and raycast visualizer
        /// </summary>
        private void EnableRaycast(MLRaycastBehavior raycast)
        {
            raycast.gameObject.SetActive(true);
            _raycastVisualizer.raycast = raycast;

#if UNITY_MAGICLEAP || UNITY_ANDROID
            _raycastVisualizer.raycast.OnRaycastResult += _raycastVisualizer.OnRaycastHit;
            _raycastVisualizer.raycast.OnRaycastResult += OnRaycastHit;
#endif
        }

        /// <summary>
        /// Disables raycast behavior and raycast visualizer
        /// </summary>
        private void DisableRaycast(MLRaycastBehavior raycast)
        {
            if (raycast != null)
            {
                raycast.gameObject.SetActive(false);

#if UNITY_MAGICLEAP || UNITY_ANDROID
                raycast.OnRaycastResult -= _raycastVisualizer.OnRaycastHit;
                raycast.OnRaycastResult -= OnRaycastHit;
#endif
            }
        }

        /// <summary>
        /// Updates Status Label with latest data.
        /// </summary>
        private void UpdateStatusText()
        {
            _overviewStatusText.text = string.Format("<color=#dbfb76><b>Controller Data</b></color>\nStatus: {0}\n\n", ControllerStatus.Text);

            _overviewStatusText.text += string.Format("<color=#dbfb76><b>Raycast Data</b></color>\nMode: {0} \nConfidence: {1}\n\n",
                _raycastMode.ToString(),
                _confidence.ToString());
        }

        /// <summary>
        /// Handles the event for button down and cycles the raycast mode.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void OnButtonDown(InputAction.CallbackContext callbackContext)
        {
            _raycastMode = (RaycastMode)((int)(_raycastMode + 1) % _modeCount);
            UpdateRaycastMode();
        }

        /// <summary>
        /// Callback handler called when raycast has a result.
        /// Updates the confidence value to the new confidence value.
        /// </summary>
        /// <param name="state"> The state of the raycast result.</param>
        /// <param name="mode">The mode that the raycast was in (physical, virtual, or combination).</param>
        /// <param name="ray">A ray that contains the used direction and origin for this raycast.</param>
        /// <param name="result">The hit results (point, normal, distance).</param>
        /// <param name="confidence">Confidence value of hit. 0 no hit, 1 sure hit.</param>
        public void OnRaycastHit(MLRaycast.ResultState state, MLRaycastBehavior.Mode mode, Ray ray, RaycastHit result, float confidence)
        {
            _confidence = confidence;
        }
    }
}
