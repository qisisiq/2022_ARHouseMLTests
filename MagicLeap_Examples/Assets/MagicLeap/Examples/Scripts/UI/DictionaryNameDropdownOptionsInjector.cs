// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="DictionaryNameDropdownOptionsInjector.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2021 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by your Early Access Terms and Conditions.
// This software is an Early Access Product.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.XR.MagicLeap.MLMarkerTracker;

namespace MagicLeap.Examples
{
    [ExecuteInEditMode]
    public class DictionaryNameDropdownOptionsInjector : MonoBehaviour
    {
        private Dropdown _dropdown;

        // Start is called before the first frame update
        void Start()
        {
            _dropdown = GetComponent<Dropdown>();

            if (_dropdown.options.Count == 0)
            {
                var dictionaryNames = Enum.GetNames(typeof(ArucoDictionaryName)).ToList();
                _dropdown.AddOptions(dictionaryNames);
            }
        }
    }
}
