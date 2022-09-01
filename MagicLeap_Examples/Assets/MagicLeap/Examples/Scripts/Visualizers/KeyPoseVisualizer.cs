// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="KeyPoseVisualizer.cs" company="Magic Leap, Inc">
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
using UnityEngine.XR;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Examples
{
    /// <summary>
    /// Class for tracking a specific Keypose and handling confidence value
    /// based sprite renderer color changes.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class KeyPoseVisualizer : MonoBehaviour
    {
        private const float ROTATION_SPEED = 100.0f;
        private const float CONFIDENCE_THRESHOLD = 0.95f;

        [Space, SerializeField, Tooltip("Flag to specify if left hand should be tracked.")]
        private bool _trackLeftHand = true;

        [SerializeField, Tooltip("Flag to specify id right hand should be tracked.")]
        private bool _trackRightHand = true;

        private SpriteRenderer _spriteRenderer = null;

        private static InputDevice leftHandDevice;
        private static InputDevice rightHandDevice;

        /// <summary>
        /// Initializes variables.
        /// </summary>
        void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Updates color of sprite renderer material based on confidence of the KeyPose.
        /// </summary>
        void Update()
        {
            if (!leftHandDevice.isValid || !rightHandDevice.isValid)
            {
                leftHandDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Left);
                rightHandDevice = InputSubsystem.Utils.FindMagicLeapDevice(InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.Right);
                return;
            }

            float confidenceLeft =  0.0f;
            float confidenceRight = 0.0f;

            float confidenceValue = Mathf.Max(confidenceLeft, confidenceRight);

            Color currentColor = Color.white;

            if (confidenceValue > 0.0f)
            {
                currentColor.r = 1.0f - confidenceValue;
                currentColor.g = 1.0f;
                currentColor.b = 1.0f - confidenceValue;
            }

            // When the keypose is detected for both hands, spin the image continuously.
            if (confidenceValue > 0.0f && confidenceLeft >= CONFIDENCE_THRESHOLD && confidenceRight >= CONFIDENCE_THRESHOLD)
            {
                transform.Rotate(Vector3.up, ROTATION_SPEED * Time.deltaTime, Space.Self);
            }
            else if(confidenceValue > 0.0f && confidenceRight > confidenceLeft)
            {
                // Shows Right-Hand Orientation.
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 180, 0), ROTATION_SPEED * Time.deltaTime);
            }
            else
            {
                // Shows Left-Hand Orientation (Default).
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.Euler(0, 0, 0), ROTATION_SPEED * Time.deltaTime);
            }

            _spriteRenderer.material.color = currentColor;
        }
    }
}
