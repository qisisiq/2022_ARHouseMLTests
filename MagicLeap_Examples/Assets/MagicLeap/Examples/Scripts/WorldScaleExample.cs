// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="WorldScaleExample.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by your Early Access Terms and Conditions.
// This software is an Early Access Product.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using MagicLeap.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.InteractionSubsystems;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Examples
{
    /// <summary>
    /// This class displays the current world scale information
    /// and allows the user to adjust the position marker between
    /// the different markers in the ruler.
    /// </summary>
    public class WorldScaleExample : MonoBehaviour
    {
        [SerializeField, Tooltip("The world scale scene component attached to the main camera.")]
        private MLWorldScaleBehavior _worldScale = null;

        [SerializeField, Tooltip("The reference to the controller connection handler in the scene.")]
        private GestureSubsystemComponent _gestureSubsystem = null;

        [SerializeField, Tooltip("Text to display the current distance and world scale.")]
        private Text _statusText = null;

        [SerializeField, Tooltip("Ruler object to get marker position data from.")]
        private Ruler _ruler = null;

        [SerializeField,
         Tooltip("The Transform of the position marker that indicates the current position in the ruler.")]
        private Transform _positionMarker = null;

        private float[] _marks = null;
        private int _currentMarkIndex = 0;

        private const float POSITION_MARKER_Y_OFFSET = 0.03f;
        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.ControllerActions controllerActions;

        /// <summary>
        /// Register callbacks, assure all required variables are set and set position marker to start position.
        /// </summary>
        void Start()
        {
            if (_worldScale == null)
            {
                Debug.LogError("Error: WorldScaleExample._worldScale is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_gestureSubsystem == null)
            {
                Debug.LogError("Error: WorldScaleExample._gestureSubsystem is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_statusText == null)
            {
                Debug.LogError("Error: WorldScaleExample._statusText is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_ruler == null)
            {
                Debug.LogError("Error: WorldScaleExample._ruler is not set, disabling script.");
                enabled = false;
                return;
            }

            if (_positionMarker == null)
            {
                Debug.LogError("Error: WorldScaleExample._positionMarker is not set, disabling script.");
                enabled = false;
                return;
            }

            _marks = _ruler.Marks;
            if (_marks.Length > 0)
            {
                _currentMarkIndex = _marks.Length - 1;
                _positionMarker.localPosition = new Vector3(0, POSITION_MARKER_Y_OFFSET, _marks[_currentMarkIndex]);
            }

            _worldScale.OnUpdateEvent += _ruler.OnWorldScaleUpdate;

#if UNITY_MAGICLEAP || UNITY_ANDROID
            mlInputs = new MagicLeapInputs();
            mlInputs.Enable();
            controllerActions = new MagicLeapInputs.ControllerActions(mlInputs);

            // Register listeners.
            controllerActions.Bumper.performed += HandleOnBumperDown;
            _gestureSubsystem.onTouchpadGestureChanged += HandleOnTouchpadGestureStart;
#endif
        }

        /// <summary>
        /// Unregister callbacks.
        /// </summary>
        void OnDestroy()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            // Unregister listeners.
            _gestureSubsystem.onTouchpadGestureChanged -= HandleOnTouchpadGestureStart;
            controllerActions.Bumper.performed -= HandleOnBumperDown;

            mlInputs.Dispose();
#endif

            _worldScale.OnUpdateEvent -= _ruler.OnWorldScaleUpdate;
        }

        /// <summary>
        /// Update status data with new information.
        /// </summary>
        void Update()
        {
            _statusText.text = string.Format("<color=#dbfb76><b>Controller Data</b></color>\nStatus: {0}\n\n",
                ControllerStatus.Text);

            _statusText.text += string.Format(
                "<color=#dbfb76><b>World Scale Data</b></color>\nMeasurement: {0}\nScale:{1}\nDistance:{2} {3}",
                _worldScale.Measurement.ToString(),
                _worldScale.Scale,
                _positionMarker.localPosition.z * _worldScale.Scale,
                _worldScale.Units);
        }

        /// <summary>
        /// Handles the event for button down.
        /// </summary>
        /// <param name="controller_id">The id of the controller.</param>
        /// <param name="button">The button that is being pressed.</param>
        private void HandleOnBumperDown(InputAction.CallbackContext callbackContext)
        {
            switch (_worldScale.Measurement)
            {
                case MLWorldScaleBehavior.ScaleMeasurement.Meters:
                    _worldScale.Measurement = MLWorldScaleBehavior.ScaleMeasurement.Decimeters;
                    break;
                case MLWorldScaleBehavior.ScaleMeasurement.Decimeters:
                    _worldScale.Measurement = MLWorldScaleBehavior.ScaleMeasurement.Centimeters;
                    break;
                case MLWorldScaleBehavior.ScaleMeasurement.Centimeters:
                    _worldScale.Measurement = MLWorldScaleBehavior.ScaleMeasurement.CustomUnits;
                    break;
                case MLWorldScaleBehavior.ScaleMeasurement.CustomUnits:
                    _worldScale.Measurement = MLWorldScaleBehavior.ScaleMeasurement.Meters;
                    break;
                default:
                    Debug.LogError("Error: WorldScaleExample measurement type is an invalid value, disabling script.");
                    enabled = false;
                    return;
            }

            _worldScale.UpdateWorldScale();
        }

        /// <summary>
        /// Handles the event for touchpad gesture start.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="gesture">The type of gesture that started.</param>
        private void HandleOnTouchpadGestureStart(GestureSubsystem.Extensions.TouchpadGestureEvent touchpadGestureEvent)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            if (touchpadGestureEvent.state == GestureState.Started &&
                touchpadGestureEvent.type == InputSubsystem.Extensions.TouchpadGesture.Type.Swipe)
            {
                // Increase / Decrease the marker distance based on the swipe gesture.
                if (touchpadGestureEvent.direction == InputSubsystem.Extensions.TouchpadGesture.Direction.Up ||
                    touchpadGestureEvent.direction == InputSubsystem.Extensions.TouchpadGesture.Direction.Down)
                {
                    if (_marks.Length > 0)
                    {
                        _currentMarkIndex =
                            (touchpadGestureEvent.direction == InputSubsystem.Extensions.TouchpadGesture.Direction.Up)
                                ? Mathf.Min(_currentMarkIndex + 1, _marks.Length - 1)
                                : Mathf.Max(_currentMarkIndex - 1, 0);
                        _positionMarker.localPosition =
                            new Vector3(0, POSITION_MARKER_Y_OFFSET, _marks[_currentMarkIndex]);
                    }
                }
            }
#endif
        }
    }
}
