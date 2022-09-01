// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="ImageTrackerExample.cs" company="Magic Leap, Inc">
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
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Examples
{
    /// <summary>
    /// This provides an example of interacting with the image tracker visualizers using the controller.
    /// </summary>
    public class ImageTrackerExample : MonoBehaviour
    {
        /// <summary>
        /// List of all the MLImageTrackerBehaviors in the scene to use.
        /// </summary>
        [SerializeField, Tooltip("The MLImageTrackerBehaviors that will have their status tracked.")]
        private MLImageTrackerBehavior[] _imageTrackers = null;

        [SerializeField, Tooltip("The status text for the UI.")]
        private Text _statusText = null;

        /// <summary>
        /// User granted HEAD_POSE and CAMERA permissions
        /// </summary>
        private bool permissionsGranted = false;
        private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

        private void Awake()
        {
            permissionCallbacks.OnPermissionGranted += OnPermissionGranted;
            permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;
        }

        private void OnDestroy()
        {
            permissionCallbacks.OnPermissionGranted -= OnPermissionGranted;
            permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;
        }

        private void Start()
        {
            MLPermissions.RequestPermission(MLPermission.Camera, permissionCallbacks);
        }

        /// <summary>
        /// Updates the _statusText used by the UI.
        /// </summary>
        void Update()
        {
            if(!permissionsGranted)
            {
                return;
            }

            if (_statusText != null)
            {
                _statusText.text = string.Format("<color=#dbfb76><b>Controller Data</b></color>\nStatus: {0}\n\n", ControllerStatus.Text);

                foreach (MLImageTrackerBehavior imageTracker in _imageTrackers)
                {
                    _statusText.text += string.Format("<color=#dbfb76><b>{0}</b></color>\nStatus: {1}\n\n",
                    imageTracker.name,
                    imageTracker.TrackingStatus.ToString()
                    );
                }
            }
        }

        private void OnPermissionDenied(string permission)
        {
#if UNITY_ANDROID
            MLPluginLog.Error($"{permission} denied, example won't function.");
#endif
        }

        private void OnPermissionGranted(string permission)
        {
            permissionsGranted = true;
        }
    }
}
