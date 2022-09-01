// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="QRCodeVisual.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2021 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by your Early Access Terms and Conditions.
// This software is an Early Access Product.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Examples
{
    public class QRCodeVisual : MonoBehaviour
    {
        [SerializeField]
        private TextMesh dataText;
#if UNITY_MAGICLEAP || UNITY_ANDROID
        private Timer disableTimer;
#endif

        void Awake()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            disableTimer = new Timer(3f);
#endif
        }

        void Update()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            if (gameObject.activeSelf && disableTimer.LimitPassed)
            {
                gameObject.SetActive(false);
            }
#endif
        }

        public void Set(MLMarkerTracker.MarkerData data)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            disableTimer?.Reset();
#endif
            transform.position = data.Pose.position;
            transform.rotation = data.Pose.rotation;
            dataText.text = data.ToString();
            gameObject.SetActive(true);
        }


    }
}
