// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="DeepSpaceExplorerController.cs" company="Magic Leap, Inc">
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
    /// This class makes it easier to set the radius of the orbit of the Deep Space Explorer.
    /// </summary>
    public class DeepSpaceExplorerController : MonoBehaviour
    {
        [SerializeField, Tooltip("Radius of the orbit of the rockets")]
        private Transform _xOffset = null;

        public float OrbitRadius
        {
            set
            {
                _xOffset.localPosition = new Vector3(value, 0, 0);
            }
        }

        /// <summary>
        /// Validate input variables.
        /// </summary>
        void Start ()
        {
            if (null == _xOffset)
            {
                Debug.LogError("Error: DeepSpaceExplorerController._xOffset is not set, disabling script");
                enabled = false;
                return;
            }
        }
    }
}
