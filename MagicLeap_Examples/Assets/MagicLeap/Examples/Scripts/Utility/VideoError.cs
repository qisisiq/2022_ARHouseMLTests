// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="VideoError.cs" company="Magic Leap, Inc">
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
    /// This provides textual state feedback for the connected controller.
    /// </summary>
    public class VideoError : MonoBehaviour
    {
        public Texture2D errorImage;

        private void Awake()
        {
            if (errorImage == null)
            {
                Debug.LogError("Error: VideoError no image found, disabling script.");
                enabled = false;
                return;
            }
        }

        public void ShowError()
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer)
            {
                renderer.material.SetTexture("_MainTex", errorImage);
            }
        }
    }
}
