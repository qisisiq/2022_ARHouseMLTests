// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="TransformFollower.cs" company="Magic Leap, Inc">
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
    /// This class implements the functionality for the object with this component
    /// to follow an input transform.
    /// </summary>
    public class TransformFollower : MonoBehaviour
    {
        [Tooltip("The object that should be followed.")]
        public Transform ObjectToFollow;

        [Tooltip("Following should respect(local) or ignore(world) hierarchy.")]
        public bool UseLocalTransform = true;

        /// <summary>
        /// Updates the transform of the object.
        /// </summary>
        void Update()
        {
            if (ObjectToFollow != null)
            {
                if (UseLocalTransform)
                {
                    transform.localPosition = ObjectToFollow.localPosition;
                    transform.localRotation = ObjectToFollow.localRotation;
                }
                else
                {
                    transform.position = ObjectToFollow.position;
                    transform.rotation = ObjectToFollow.rotation;
                }
            }
        }
    }
}
