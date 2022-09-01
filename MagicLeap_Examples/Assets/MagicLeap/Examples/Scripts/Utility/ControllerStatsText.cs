// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="ControllerStatsText.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by your Early Access Terms and Conditions.
// This software is an Early Access Product.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap.Examples
{
    /// <summary>
    /// This provides textual state feedback for the connected controller.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class ControllerStatsText : MonoBehaviour
    {
        private GestureSubsystemComponent gestureComponent;
        private Text _controllerStatsText = null;
        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.ControllerActions controllerActions;
        
        /// <summary>
        /// Initializes component data and starts MLInput.
        /// </summary>
        void Awake()
        {
            gestureComponent = gameObject.AddComponent<GestureSubsystemComponent>();

            _controllerStatsText = gameObject.GetComponent<Text>();
            _controllerStatsText.color = Color.white;
        }

        private void Start()
        {
            mlInputs = new MagicLeapInputs();
            mlInputs.Enable();
            controllerActions = new MagicLeapInputs.ControllerActions(mlInputs);
        }

        private void OnDestroy()
        {
            mlInputs.Dispose();
        }

        /// <summary>
        /// Updates text with latest controller stats.
        /// </summary>
        void Update()
        {
            if (controllerActions.IsTracked.IsPressed())
            {
                #if UNITY_MAGICLEAP || UNITY_ANDROID
                
                _controllerStatsText.text =
                string.Format("" +
                    "Position:\t<i>{0}</i>\n" +
                    "Rotation:\t<i>{1}</i>\n\n" +
                    "<color=#ffc800>Buttons</color>\n" +
                    "Trigger:\t\t<i>{2}</i>\n" +
                    "Bumper:\t\t<i>{3}</i>\n\n" +
                    "<color=#ffc800>Touchpad</color>\n" +
                    "Location:\t<i>({4},{5})</i>\n" +
                    "Pressure:\t<i>{6}</i>\n\n" +
                    "<color=#ffc800>Gestures</color>\n" +
                    "<i>{7} {8}</i>",

                    controllerActions.Position.ReadValue<Vector2>().ToString("n2"),
                    controllerActions.Rotation.ReadValue<Quaternion>().eulerAngles.ToString("n2"),
                    controllerActions.Trigger.ReadValue<float>().ToString("n2"),
                    controllerActions.Bumper.IsPressed(),
                    controllerActions.TouchpadPosition.ReadValue<Vector2>().x.ToString("n2"),
                    controllerActions.TouchpadPosition.ReadValue<Vector2>().y.ToString("n2"),
                    controllerActions.TouchpadForce.ReadValue<float>().ToString("n2"),
                    gestureComponent.gestureSubsystem.touchpadGestureEvents.FirstOrDefault().type.ToString(),
                    gestureComponent.gestureSubsystem.touchpadGestureEvents.FirstOrDefault().state.ToString());

                #else
                _controllerStatsText.text = "Invalid Controller!";
                #endif
            }
            else
            {
                _controllerStatsText.text = "";
            }
        }
    }
}
