// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Developer Agreement, located
// here: https://auth.magicleap.com/terms/developer
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using MagicLeap.Examples;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap
{
    [CustomEditor(typeof(MarkerTrackingExample))]
    [CanEditMultipleObjects]
    [ExecuteInEditMode]
    public class MarkerTrackingExampleEditor : Editor
    {
        SerializedProperty MarkerTypes;
        SerializedProperty QRCodeSize;
        SerializedProperty ArucoMarkerSize;
        SerializedProperty ArucoDicitonary;
        SerializedProperty FPSHint;
        SerializedProperty EnableMarkerScanning;

        private void OnEnable()
        {
            MarkerTypes = serializedObject.FindProperty("MarkerTypes");
            QRCodeSize = serializedObject.FindProperty("QRCodeSize");
            ArucoMarkerSize = serializedObject.FindProperty("ArucoMarkerSize");
            ArucoDicitonary = serializedObject.FindProperty("ArucoDicitonary");
            FPSHint = serializedObject.FindProperty("FPSHint");
            EnableMarkerScanning = serializedObject.FindProperty("EnableMarkerScanning");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MarkerTrackingExample myTarget = (MarkerTrackingExample)target;
            
            GUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Marker tracker settings:");
            EditorGUILayout.PropertyField(EnableMarkerScanning);
            EditorGUILayout.PropertyField(MarkerTypes);
            EditorGUILayout.PropertyField(FPSHint);

            if (myTarget.MarkerTypes.HasFlag(MLMarkerTracker.MarkerType.All) ||
                myTarget.MarkerTypes.HasFlag(MLMarkerTracker.MarkerType.QR) ||
                myTarget.MarkerTypes.HasFlag(MLMarkerTracker.MarkerType.EAN_13) ||
                myTarget.MarkerTypes.HasFlag(MLMarkerTracker.MarkerType.UPC_A))
            {
                EditorGUILayout.PropertyField(QRCodeSize);
            }

            if (myTarget.MarkerTypes.HasFlag(MLMarkerTracker.MarkerType.All) ||
                myTarget.MarkerTypes.HasFlag(MLMarkerTracker.MarkerType.Aruco_April))
            {
                EditorGUILayout.PropertyField(ArucoMarkerSize);
                EditorGUILayout.PropertyField(ArucoDicitonary);
            }

            GUILayout.EndVertical();


            serializedObject.ApplyModifiedProperties();
        }
    }
}
