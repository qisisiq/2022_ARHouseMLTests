// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="UIButtonGroup.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by your Early Access Terms and Conditions.
// This software is an Early Access Product.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using UnityEngine;

namespace MagicLeap.Examples
{
    /// <summary>
    /// This class should be applied to the parent of several UIButtons.
    /// This will enforce a single button active policy, that allow switching between multiple sections.
    /// </summary>
    public class UIButtonGroup : MonoBehaviour
    {
        private HashSet<UIButton> _buttons = null;

        private void Awake()
        {
            _buttons = new HashSet<UIButton>(GetComponentsInChildren<UIButton>(true));
        }

        public void AddUIButton(UIButton button)
        {
            _buttons.Add(button);
        }

        public void Clear()
        {
            if(_buttons == null)
            {
                return;
            }

            foreach (var button in _buttons)
            {
                button.Default(true);
            }
        }
    }
}
