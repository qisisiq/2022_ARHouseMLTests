// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="CombinedTrackingExample.cs" company="Magic Leap, Inc">
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
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using static UnityEngine.XR.MagicLeap.MLMarkerTracker;
using MarkerSettings = UnityEngine.XR.MagicLeap.MLMarkerTracker.Settings;

namespace MagicLeap.Examples
{
    public class MarkerTrackingExample : MonoBehaviour
    {
        [SerializeField, Tooltip("The status text for the UI.")]
        private Text statusText = null;

        [SerializeField]
        private MarkerVisual markerVisualPrefab;

        [SerializeField]
        private GameObject markersRoot;

        [SerializeField, Tooltip("Wait for a given amount of time before removing unobsereved trackers.")]
        private bool removeMarkersUsingTimeStamps = false;

        [SerializeField, Tooltip("The timeout duration before removing unobserved trackers. Only used if removeMarkersUsingTimeStamps is set.")]
        private float markerTrackerTimeout = 0.5f;

        /// <summary>
        ///     If <c> true </c>, Marker Scanner will detect markers and track QR codes.
        ///     Marker Scanner should be disabled when app is paused and enabled when app
        ///     resumes. When enabled, Marker Scanner will gain access to the camera and start
        ///     scanning markers. When disabled Marker Scanner will release the camera and
        ///     stop scanning markers. Internal state of the scanner will be maintained.
        /// </summary>
        [HideInInspector]
        public bool EnableMarkerScanning;

        /// <summary>
        ///     A hint to the back-end the max frames per second hat should be analyzed.
        /// </summary>
        [HideInInspector]
        public FPSHint FPSHint;

        /// <summary>
        ///     Aruco dictionary to use.
        /// </summary>
        [HideInInspector]
        public ArucoDictionaryName ArucoDicitonary;

        /// <summary>
        ///     Aruco marker size to use (in meters).
        /// </summary>
        [HideInInspector]
        public float ArucoMarkerSize = 0.1f;

        /// <summary>
        ///     The physical size of the QR code that shall be tracked (in meters). The physical size is
        ///     important to know, because once a QR code is detected we can only determine its
        ///     3D position when we know its correct size. The size of the QR code is given in
        ///     meters and represents the length of one side of the square code(without the
        ///     outer margin). Min size: As a rule of thumb the size of a QR code should be at
        ///     least a 10th of the distance you intend to scan it with a camera device. Higher
        ///     version markers with higher information density might need to be larger than
        ///     that to be detected reliably. Max size: Our camera needs to see the whole
        ///     marker at once. If it's too large, we won't detect it.
        /// </summary>
        [HideInInspector]
        public float QRCodeSize = 0.1f;

        /// <summary>
        /// The marker types that are enabled for this scanner. Enable markers by
        /// combining any number of <c> MarkerType </c> flags using '|' (bitwise 'or').
        /// </summary>
        [HideInInspector]
        public MarkerType MarkerTypes = MarkerType.All;

        private List<KeyValuePair<string, MarkerVisual>> markers = new();
        private ASCIIEncoding asciiEncoder = new ASCIIEncoding();
        public MarkerSettings markerSettings;
        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.ControllerActions controllerActions;

        void Start()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            mlInputs = new MagicLeapInputs();
            mlInputs.Enable();
            controllerActions = new MagicLeapInputs.ControllerActions(mlInputs);
#endif
            EnableMarkerTrackerExample();
        }

        private void Update()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            // If scanning is enabled from start don't clear flag by input.
            if (!EnableMarkerScanning)
            {
                if (controllerActions.Trigger.ReadValue<float>() > .1f)
                    _ = MLMarkerTracker.StartScanningAsync();
                else
                    _ = MLMarkerTracker.StopScanningAsync();
            }
#endif

            UpdateVisibleTrackers();
            SetStatusText();
        }

        private void OnEnable()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            MLMarkerTracker.OnMLMarkerTrackerResultsFoundArray += OnMLMarkerTrackerResultsFoundArray;
#endif
        }

        private void OnDisable()
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            MLMarkerTracker.OnMLMarkerTrackerResultsFoundArray -= OnMLMarkerTrackerResultsFoundArray;
#endif
        }

        private void OnDestroy()
        {
            mlInputs.Dispose();
        }

        private void OnMLMarkerTrackerResultsFoundArray(MarkerData[] dataArray)
        {
            if (!removeMarkersUsingTimeStamps)
            {
                RemoveNotVisibleTrackers(dataArray);
            }
            foreach (MarkerData data in dataArray)
            {
                ProcessSingleMarker(data);
            }
        }

        private void ProcessSingleMarker(MarkerData data)
        {
            switch (data.Type)
            {
                case MarkerType.Aruco_April:
                    {
                        string id = data.ArucoData.Id.ToString();
                        var existingMarker = markers.Find(x => x.Key == id);
                        if (!string.IsNullOrEmpty(existingMarker.Key))
                        {
                            MarkerVisual marker = existingMarker.Value;
                            marker.Set(data);
                        }
                        else
                        {
                            MarkerVisual marker = Instantiate(markerVisualPrefab, markersRoot.transform);

                            markers.Add(new KeyValuePair<string, MarkerVisual>(id, marker));
                            marker.Set(data);
                        }

                        break;
                    }

                case MarkerType.EAN_13:
                case MarkerType.UPC_A:
                case MarkerType.QR:
                    {
                        string id = asciiEncoder.GetString(data.BinaryData.Data, 0, data.BinaryData.Data.Length);
                        string markerText =
                            $"\nType: {Enum.GetName(typeof(MarkerType), data.Type)}\nReprojection Error: {data.ReprojectionError}\n Data:{id}";
                        var existingMarker = markers.Find(x => x.Key == id);
                        if (!string.IsNullOrEmpty(existingMarker.Key))
                        {
                            MarkerVisual marker = existingMarker.Value;
                            marker.Set(data, markerText);
                        }
                        else
                        {
                            MarkerVisual marker = Instantiate(markerVisualPrefab, markersRoot.transform);

                            markers.Add(new KeyValuePair<string, MarkerVisual>(id, marker));
                            marker.Set(data, markerText);
                        }

                        break;
                    }
            }
        }

        private void EnableMarkerTrackerExample()
        {
            try
            {
#if UNITY_MAGICLEAP || UNITY_ANDROID
                markerSettings = new Settings(EnableMarkerScanning, FPSHint, MarkerTypes, QRCodeSize, ArucoDicitonary, ArucoMarkerSize);
                SetSettingsAsync(markerSettings).GetAwaiter().GetResult();
#endif
            }
            catch (Exception e)
            {
                Debug.Log($"CombinedTrackingExample.EnableMarkerTrackerExample() => error: {e.Message}");
            }
        }

        private void UpdateVisibleTrackers()
        {
            if (removeMarkersUsingTimeStamps)
            {
                UpdateVisibleTrackersByTimeStamp();
            }
        }

        private void UpdateVisibleTrackersByTimeStamp()
        {
            for (int i = markers.Count - 1; i >= 0; i--)
            {
                MarkerVisual marker = markers[i].Value;
                if (!(marker.Timestamp - Time.time > markerTrackerTimeout))
                    continue;

                Destroy(marker.gameObject);
                markers.RemoveAt(i);
            }
        }

        private void RemoveNotVisibleTrackers(MarkerData[] dataArray)
        {
            for (int i = markers.Count - 1; i >= 0; i--)
            {
                MarkerVisual marker = markers[i].Value;

                if (!dataArray.Any(x =>
                {
                    if (x.Type != marker.Type)
                        return false;

                    string id = default;
                    switch (marker.Type)
                    {
                        case MarkerType.Aruco_April:
                            id = x.ArucoData.Id.ToString();
                            break;
                        case MarkerType.EAN_13:
                        case MarkerType.UPC_A:
                        case MarkerType.QR:
                            id = asciiEncoder.GetString(x.BinaryData.Data, 0, x.BinaryData.Data.Length);
                            break;
                    }
                    return id == markers[i].Key;
                }))
                {
                    Destroy(marker.gameObject);
                    markers.RemoveAt(i);
                }
            }
        }

        private void SetStatusText()
        {
            StringBuilder builder = new StringBuilder();

#if UNITY_MAGICLEAP || UNITY_ANDROID
            builder.Append($"<color=#dbfb76><b>ControllerData</b></color>\nStatus: {ControllerStatus.Text}\n\n");
            builder.Append($"<color=#dbfb76><b>Controller Input</b></color>\nTrigger status: {controllerActions.Trigger.ReadValue<float>()}\n");

            builder.Append($"Marker Tracker running: {MLMarkerTracker.IsStarted} \n\n");
            builder.Append($"Scanning status: {EnableMarkerScanning || controllerActions.Trigger.ReadValue<float>() > .1f} \n\n");
            builder.Append($"<color=#dbfb76><b>Marker Settings</b></color>\nScan Types: {MarkerTypes}\n");
            builder.Append($"Enable Marker Scanning: {EnableMarkerScanning}\n");
            builder.Append($"QR Code Size: {QRCodeSize}\n\n");

            foreach (var marker in markers)
            {
                builder.Append(
                        $"<color=#dbfb76><b>{marker.Key}</b></color>" +
                        $"\nData: {marker.Value.DataString}\n\n");
            }
#else
            builder.Append($"Marker Tracker running: false \n\n");
#endif
            statusText.text = builder.ToString();
        }
    }
}
