// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="LightTrackingExample.cs" company="Magic Leap, Inc">
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
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Examples
{
    /// <summary>
    /// The UI displays the detected light intensity level indicated.
    /// </summary>
    public class LightTrackingExample : MonoBehaviour
    {
        [SerializeField, Tooltip("The primary light that is used in the scene.")]
        private Light _light = null;

        [SerializeField, Tooltip("The text used to display status information for the example..")]
        private Text _statusText = null;

        [SerializeField, Tooltip("The canvas used to diplay the light intensity meter.")]
        private GameObject _lightTrackingCanvas = null;

        private Camera _camera = null;
        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.ControllerActions controllerActions;

        /// <summary>
        /// Gets the Temperature Color.
        /// </summary>
        private Color TemperatureColor
        {
            get
            {
#if UNITY_MAGICLEAP || UNITY_ANDROID
                return MLLightingTracking.GlobalTemperatureColor;
#else
                return Color.black;
#endif
            }
        }

        private void Start()
        {
            if (_light == null)
            {
                Debug.LogError("Error: LightTrackingExample._light is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_statusText == null)
            {
                Debug.LogError("Error: LightTrackingExample._statusText is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_lightTrackingCanvas == null)
            {
                Debug.LogError("Error: LightTrackingExample._lightTrackingCanvas is not set, disabling script.");
                enabled = false;
                return;
            }

            _camera = Camera.main;
            UpdateStatus();

            #if UNITY_MAGICLEAP || UNITY_ANDROID
            mlInputs = new MagicLeapInputs();
            mlInputs.Enable();
            controllerActions = new MagicLeapInputs.ControllerActions(mlInputs);
            
            controllerActions.Bumper.performed += OnButtonDown;
            #endif

            StartCoroutine(UpdateLightCanvas());
        }

        private void OnDestroy()
        {
            controllerActions.Bumper.performed -= OnButtonDown;
            mlInputs.Dispose();
        }

        void Update()
        {
            UpdateStatus();

            // Sets the color and intensity in the scene.
            RenderSettings.ambientLight = TemperatureColor;
        }

        /// <summary>
        /// Updates the status text.
        /// </summary>
        private void UpdateStatus()
        {
            _statusText.text = $"<color=#dbfb76><b>Controller Data</b></color>\nStatus: {ControllerStatus.Text}\n";
        }

        private void OnButtonDown(InputAction.CallbackContext inputCallback)
        {
            TranslateLightingCanvas();
        }

        /// <summary>
        /// Translate light tracking canvas in camera view.
        /// </summary>
        private void TranslateLightingCanvas()
        {
            _lightTrackingCanvas.transform.position = _camera.transform.position + (_camera.transform.forward * 1.25f);
            _lightTrackingCanvas.transform.rotation = Quaternion.LookRotation(_lightTrackingCanvas.transform.position - _camera.transform.position);
        }

        /// <summary>
        /// Waits to the first frame to be rendered to get a valid camera transform.
        /// </summary>
        private IEnumerator UpdateLightCanvas()
        {
            yield return new WaitForEndOfFrame();
            TranslateLightingCanvas();
        }
    }
}
