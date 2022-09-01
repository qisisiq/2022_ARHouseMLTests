// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="ControllerStatusText.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by your Early Access Terms and Conditions.
// This software is an Early Access Product.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MagicLeap.Examples
{
    /// <summary>
    /// This represents the controller text connectivity status.
    /// Red: MLInput error.
    /// Green: Controller connected.
    /// Yellow: Controller disconnected.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class ControllerStatusText : MonoBehaviour
    {
        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.ControllerActions controllerActions;
        private Text _controllerStatusText = null;

        /// <summary>
        /// Initializes component data and starts MLInput.
        /// </summary>
        void Awake()
        {
            _controllerStatusText = gameObject.GetComponent<Text>();
        }

        void Start()
        {
            mlInputs = new MagicLeapInputs();
            mlInputs.Enable();
            controllerActions = new MagicLeapInputs.ControllerActions(mlInputs);

            controllerActions.IsTracked.performed += HandleOnControllerChanged;
            controllerActions.IsTracked.canceled += HandleOnControllerChanged;
            // Wait until the next cycle to check the status.
            UpdateStatus();
        }

        void OnDestroy()
        {
            controllerActions.IsTracked.performed -= HandleOnControllerChanged;
            controllerActions.IsTracked.canceled -= HandleOnControllerChanged;

            mlInputs.Dispose();
        }

        void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                UpdateStatus();
            }
        }

        /// <summary>
        /// Update the text for the currently connected Control or MCA device.
        /// </summary>
        private void UpdateStatus()
        {
            if (controllerActions.IsTracked.IsPressed())
            {
#if UNITY_MAGICLEAP || UNITY_ANDROID
                _controllerStatusText.text = "Controller Connected";
                _controllerStatusText.color = Color.green;
#else
                    _controllerStatusText.text = "Unknown";
                    _controllerStatusText.color = Color.red;
#endif
            }
            else
            {
                _controllerStatusText.text = "Disconnected";
                _controllerStatusText.color = Color.yellow;
            }
        }

        private void HandleOnControllerChanged(InputAction.CallbackContext callbackContext)
        {
            UpdateStatus();
        }
    }
}
