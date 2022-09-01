// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "EnumDropdown.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by your Early Access Terms and Conditions.
// This software is an Early Access Product.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace UnityEngine.XR.MagicLeap
{
    public class EnumDropdown : Dropdown
    {
        public T GetSelected<T>()
        {
            if (options.Count <= value)
            {
                return default(T);
            }
            return (T)Enum.Parse(typeof(T), options[value].text);
        }

        public void AddOptions<T>(params T[] options)
        {
            foreach (T option in options)
            {
                base.AddOptions(new List<string> { option.ToString() });
            }
        }

        public void SelectOption<T>(T option, bool notify)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (!((T)Enum.Parse(typeof(T), options[i].text)).Equals(option))
                    continue;

                if (notify)
                {
                    value = i;
                    return;
                }

                SetValueWithoutNotify(i);
                return;
            }
        }
    }
}
