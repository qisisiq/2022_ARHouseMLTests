// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MobileAppControllerVisualizer.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by your Early Access Terms and Conditions.
// This software is an Early Access Product.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MagicLeap.Examples
{
    /// <summary>
    /// This represents a virtual controller visualization that mimics the current state of the
    /// Mobile Device running the Magic Leap Mobile Application. Button presses, touch pad are all represented along with
    /// the orientation of the mobile device. There is no position information available.
    /// </summary>
    public class MobileAppControllerVisualizer : MonoBehaviour
    {
        [SerializeField, Tooltip("The highlight for the left button.")]
        private GameObject _leftButtonHighlight = null;

        [SerializeField, Tooltip("The highlight for the right button.")]
        private GameObject _rightButtonHighlight = null;

        [SerializeField, Tooltip("The indicator for the home tap.")]
        private GameObject _homeTapIndicator = null;

        [SerializeField, Tooltip("Number of seconds to show home tap.")]
        private float _homeActiveDuration = 0.5f;

        private float _timeToDeactivateHome = 0;

        [SerializeField, Tooltip("The indicator for the first touch.")]
        private GameObject _touch1Indicator = null;

        [SerializeField, Tooltip("The indicator for the second touch.")]
        private GameObject _touch2Indicator = null;

        [SerializeField, Tooltip("Renderer of the Mesh.")]
        private MeshRenderer _modelRenderer = null;

        private Color _origColor = Color.clear;
        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.ControllerActions controllerActions;

        public string KeyboardText { get; private set; }

        /// <summary>
        /// Initializes component data, starts MLInput, validates parameters, initializes indicator states.
        /// </summary>
        void Awake()
        {
            if (_leftButtonHighlight == null)
            {
                Debug.LogError(
                    "Error: MobileAppControllerVisualizer._leftButtonHighlight is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_rightButtonHighlight == null)
            {
                Debug.LogError(
                    "Error: MobileAppControllerVisualizer._rightButtonHighlight is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_homeTapIndicator == null)
            {
                Debug.LogError("Error: MobileAppControllerVisualizer._homeTapIndicator is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_touch1Indicator == null)
            {
                Debug.LogError("Error: MobileAppControllerVisualizer._touch1Indicator is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_touch2Indicator == null)
            {
                Debug.LogError("Error: MobileAppControllerVisualizer._touch2Indicator is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_modelRenderer == null)
            {
                Debug.LogError("Error: MobileAppControllerVisualizer._modelRenderer is not set, disabling script.");
                enabled = false;
                return;
            }

            _leftButtonHighlight.SetActive(false);
            _rightButtonHighlight.SetActive(false);
            _homeTapIndicator.SetActive(false);
            _touch1Indicator.SetActive(false);
            _touch2Indicator.SetActive(false);

            _origColor = _modelRenderer.material.color;

#if UNITY_MAGICLEAP || UNITY_ANDROID
            mlInputs = new MagicLeapInputs();
            mlInputs.Enable();
            controllerActions = new MagicLeapInputs.ControllerActions(mlInputs);

            controllerActions.Bumper.performed += HandleOnBumperDown;
            controllerActions.Bumper.canceled += HandleOnBumperUp;
            controllerActions.Trigger.performed += HandleOnTriggerDown;
            controllerActions.Trigger.canceled += HandleOnTriggerUp;
            controllerActions.Menu.performed += HandleOnHomeUp;
#endif
        }

        /// <summary>
        /// Updates effects on different input responses via input polling mechanism.
        /// </summary>
        void Update()
        {
            if (controllerActions.IsTracked.IsPressed())
            {
                _modelRenderer.material.color = _origColor;

#if UNITY_MAGICLEAP || UNITY_ANDROID
                UpdateTouchIndicator(_touch1Indicator,controllerActions.IsTracked.ReadValue<bool>(), controllerActions.TouchpadPosition.ReadValue<Vector2>());
                //UpdateTouchIndicator(_touch2Indicator, controller.Touch2Active, controller.Touch2PosAndForce);
#endif

                UpdateHighlights();
            }
            else
            {
                _modelRenderer.material.color = Color.red;
            }
        }

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDestroy()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            controllerActions.Bumper.performed -= HandleOnBumperDown;
            controllerActions.Bumper.canceled -= HandleOnBumperUp;
            controllerActions.Trigger.performed -= HandleOnTriggerDown;
            controllerActions.Trigger.canceled -= HandleOnTriggerUp;
            controllerActions.Menu.performed -= HandleOnHomeUp;

            mlInputs.Dispose();
#endif
        }

        /// <summary>
        /// Turn off HomeTap visualizer after certain time.
        /// </summary>
        private void UpdateHighlights()
        {
            if (_timeToDeactivateHome < Time.time)
            {
                _homeTapIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// Update visualizers for touchpad.
        /// </summary>
        /// <param name="indicator"> Visual object to place on touch position. </param>
        /// <param name="active"> State of the touch.</param>
        /// <param name="pos"> Raw data for touchpad touch position.</param>
        private void UpdateTouchIndicator(GameObject indicator, bool active, Vector2 pos)
        {
            indicator.SetActive(active);
            indicator.transform.localPosition = new Vector3(pos.x * 0.042f,
                pos.y * 0.042f + 0.01f, indicator.transform.localPosition.z);
        }

        /// <summary>
        /// Handles the event for button down.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void HandleOnBumperDown(InputAction.CallbackContext callbackContext)
        {
            _leftButtonHighlight.SetActive(true);
        }

        /// <summary>
        /// Handles the event for button up.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void HandleOnBumperUp(InputAction.CallbackContext callbackContext)
        {
            _leftButtonHighlight.SetActive(false);
        }

        private void HandleOnHomeUp(InputAction.CallbackContext callbackContext)
        {
            _homeTapIndicator.SetActive(true);
            _timeToDeactivateHome = Time.time + _homeActiveDuration;
        }

        /// <summary>
        /// Handles the event for trigger down.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="value">The trigger value.</param>
        private void HandleOnTriggerDown(InputAction.CallbackContext callbackContext)
        {
            _rightButtonHighlight.SetActive(true);
        }

        /// <summary>
        /// Handles the event for trigger up.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="value">The trigger value.</param>
        private void HandleOnTriggerUp(InputAction.CallbackContext callbackContext)
        {
            _rightButtonHighlight.SetActive(false);
        }

        /// <summary>
        /// Keyboard events are propagated via Unity's event system. OnGUI is the preferred way
        /// to catch these events.
        /// </summary>
        private void OnGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Backspace)
                {
                    if (KeyboardText.Length > 0)
                    {
                        KeyboardText = KeyboardText.Substring(0, KeyboardText.Length - 1);
                    }
                }
                else if (e.keyCode == KeyCode.Return)
                {
                    KeyboardText += "\n";
                }
                else if (!Char.IsControl(e.character))
                {
                    KeyboardText += e.character;
                }
            }
        }
    }
}
