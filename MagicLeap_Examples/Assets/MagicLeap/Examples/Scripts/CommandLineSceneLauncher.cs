// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="CommandLineSceneLauncher.cs" company="Magic Leap, Inc">
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
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLeap.Examples
{
    public class CommandLineSceneLauncher : MonoBehaviour
    {
        public static readonly string SceneLaunchArg = @"-scene";

        private string sceneName;

        private void Start()
        {
            if (Application.isEditor)
            {
                return;
            }

            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] == SceneLaunchArg)
                {
                    if (i + 1 < arguments.Length)
                    {
                        sceneName = arguments[i + 1];
                    }
                    else
                    {
                        Debug.LogErrorFormat("Found command line argument {0} but no scene name followed it. Use the syntax \"{0} SceneName\"", SceneLaunchArg);
                    }
                    break;
                }
            }

            StartCoroutine(LoadScene());
        }

        private IEnumerator LoadScene()
        {
            if(string.IsNullOrEmpty(sceneName))
            {
                yield break;
            }

            var loadAsyncOp = SceneManager.LoadSceneAsync(sceneName);

            while (!loadAsyncOp.isDone)
            {
                yield return null;
            }

            Debug.LogFormat("Finished loading Scene from {0} arg: {1}", SceneLaunchArg, sceneName);
        }
    }
}
