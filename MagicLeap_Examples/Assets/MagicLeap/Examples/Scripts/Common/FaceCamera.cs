// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="FaceCamera.cs" company="Magic Leap, Inc">
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

namespace MagicLeap.Examples
{
    /// <summary>
    /// This behavior rotates the transform to always look at the Main camera
    /// </summary>
    public class FaceCamera : MonoBehaviour
    {
        [SerializeField, Tooltip("Rotation Offset in Euler Angles")]
        Vector3 _rotationOffset = Vector3.zero;

        /// <summary>
        /// Initialize rotation
        /// </summary>
        void Start()
        {
            transform.LookAt(Camera.main.transform);
        }

        /// <summary>
        /// Update rotation to look at main camera
        /// </summary>
        void Update ()
        {
            transform.LookAt(Camera.main.transform);
            transform.rotation *= Quaternion.Euler(_rotationOffset);
        }
    }
}
