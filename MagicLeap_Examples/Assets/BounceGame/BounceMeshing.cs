// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MeshingExample.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by your Early Access Terms and Conditions.
// This software is an Early Access Product.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if UNITY_EDITOR || UNITY_MAGICLEAP || UNITY_ANDROID

using System;
using System.Collections;
using System.Collections.Generic;
using CMF;
using Dynamite3D.RealIvy;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.InteractionSubsystems;
using UnityEngine.XR.MagicLeap;
using UnityEngine.XR.Management;
using Random = UnityEngine.Random;

namespace MagicLeap.Examples
{
    /// <summary>
    /// This represents all the runtime control over meshing component in order to best visualize the
    /// affect changing parameters has over the meshing API.
    /// </summary>
    public class BounceMeshing : MonoBehaviour
    {
        private static BounceMeshing _instance;
        public static BounceMeshing Instance
        {
            get { return _instance; }
        }
        
        [SerializeField, Tooltip("The spatial mapper from which to update mesh params.")]
        private MeshingSubsystemComponent _meshingSubsystemComponent = null;

        [SerializeField, Tooltip("Visualizer for the meshing results.")]
        private MeshingVisualizer _meshingVisualizer = null;

        [SerializeField, Space, Tooltip("A visual representation of the meshing bounds.")]
        private GameObject _visualBounds = null;

        [SerializeField, Space, Tooltip("Flag specifying if mesh extents are bounded.")]
        private bool _bounded = false;

        [SerializeField, Space, Tooltip("The text to place mesh data on.")]
        private Text _statusLabel = null;
        
        [SerializeField, Space, Tooltip("Ball character to drop into the scene.")]
        private GameObject _ballPrefab;
        
        [SerializeField, Space, Tooltip("Reference to the controller gameobject.")]
        private GameObject _controllerObject = null;

        [SerializeField, Space, Tooltip("Render mode to render mesh data with.")]
        private MeshingVisualizer.RenderMode _renderMode = MeshingVisualizer.RenderMode.Wireframe;
        private int _renderModeCount;

        [SerializeField, Space, Tooltip("Size of the bounds extents when bounded setting is enabled.")]
        private Vector3 _boundedExtentsSize = new Vector3(2.0f, 2.0f, 2.0f);

        [SerializeField, Space, Tooltip("Size of the bounds extents when bounded setting is disabled.")]
        private Vector3 _boundlessExtentsSize = new Vector3(10.0f, 10.0f, 10.0f);
        

        private const float SHOOTING_FORCE = 300.0f;
        private const float MIN_BALL_SIZE = 0.2f;
        private const float MAX_BALL_SIZE = 0.5f;
        private const int BALL_LIFE_TIME = 10;

        private List<GameObject> ballPool = new List<GameObject>();
        private int ballCounter = 0;
        private int ballPoolSize = 3;
        
        private Camera _camera = null;

        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.ControllerActions controllerActions;
        private XRRayInteractor xRRayInteractor;
        private XRInputSubsystem inputSubsystem;

        private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

        private ActionBasedController _controller; 
        
        //Plane area -----------------------------------------------------------------------------------

        public bool UsePlaneManager = false;
        private ARPlaneManager planeManager;

        [SerializeField, Tooltip("Maximum number of planes to return each query")]
        private uint maxResults = 100;

        [SerializeField, Tooltip("Minimum hole length to treat as a hole in the plane")]
        private float minHoleLength = 0.5f;

        [SerializeField, Tooltip("Minimum plane area to treat as a valid plane")]
        private float minPlaneArea = 0.25f;
        
        // Bouncy ball area ---------------------------------------------------
        
        [SerializeField, Space, Tooltip("Ball character to drop into the scene.")]
        private GameObject _bouncyBallPrefab;
        
        private GameObject bouncyBall;

        [SerializeField]
        private float throwForce = 300f;
        [SerializeField]
        private float ballForce = 300f;


        [SerializeField, Space, Tooltip("Controller rigidbody")]
        private Rigidbody _controllerRigidbody;
        
        [SerializeField, Space, Tooltip("Current bounces")]
        private TMP_Text numBouncesText;

        [SerializeField, Space, Tooltip("Current bounces")]
        private TMP_Text highScoreBouncesText;
        
        [HideInInspector] public int NumBounces;
        [HideInInspector] public int HighScoreBounces;
        
        [SerializeField]
        private TMP_Text highScoreCelebrationText;

        private int _currentFrameIndex = 0;
        
        // number of velocity frames to track 
        private const int numFrames = 15;
        private Vector3[] _controllerPositions = new Vector3[numFrames];

        private bool _isHoldingBall;

        [HideInInspector] public Vector3 PreviousBallPos;
        [SerializeField] private float minDistanceBetweenBounce = 0.03f; // how far away for it to count as a legit bounce 
        public float MinDistanceBetweenBounce => minDistanceBetweenBounce;

        public AudioSource shootAudio;
        
        /// <summary>
        /// Initializes component data and starts MLInput.
        /// </summary>
        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            } else {
                _instance = this;
            }

            _meshingVisualizer.SetRenderers(_renderMode);

            permissionCallbacks.OnPermissionDenied += OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain += OnPermissionDenied;

            if (_meshingSubsystemComponent == null)
            {
                Debug.LogError("MeshingExample._meshingSubsystemComponent is not set. Disabling script.");
                enabled = false;
                return;
            }
            if (_meshingVisualizer == null)
            {
                Debug.LogError("MeshingExample._meshingVisualizer is not set. Disabling script.");
                enabled = false;
                return;
            }
            if (_visualBounds == null)
            {
                Debug.LogError("MeshingExample._visualBounds is not set. Disabling script.");
                enabled = false;
                return;
            }

#if UNITY_MAGICLEAP || UNITY_ANDROID
            MLDevice.RegisterGestureSubsystem();
            if (MLDevice.GestureSubsystemComponent == null)
            {
                Debug.LogError("MLDevice.GestureSubsystemComponent is not set. Disabling script.");
                enabled = false;
                return;
            }
#endif
            xRRayInteractor = FindObjectOfType<XRRayInteractor>();

            _renderModeCount = System.Enum.GetNames(typeof(MeshingVisualizer.RenderMode)).Length;

            _camera = Camera.main;

#if UNITY_MAGICLEAP || UNITY_ANDROID
            mlInputs = new MagicLeapInputs();
            mlInputs.Enable();
            controllerActions = new MagicLeapInputs.ControllerActions(mlInputs);

            controllerActions.Trigger.started += OnTriggerStarted;
            controllerActions.Trigger.canceled += OnTriggerCanceled;

            controllerActions.Bumper.performed += OnBumperDown;
            controllerActions.Menu.performed += OnMenuDown;
#endif
        }

        /// <summary>
        /// Set correct render mode for meshing and update meshing settings.
        /// </summary>
        private void Start()
        {
            MLPermissions.RequestPermission(MLPermission.SpatialMapping, permissionCallbacks);

            inputSubsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRInputSubsystem>();
            inputSubsystem.trackingOriginUpdated += OnTrackingOriginChanged;
            
            _meshingSubsystemComponent.gameObject.transform.position = _camera.gameObject.transform.position;
            _meshingSubsystemComponent.density = 0.5f;
            
            _controller = FindObjectOfType<ActionBasedController>();
            
            UpdateBounds();
            
            // Plane ------
            if (UsePlaneManager)
            {
                planeManager = FindObjectOfType<ARPlaneManager>();
                if (planeManager == null)
                {
                    Debug.LogError("Failed to find ARPlaneManager in scene.");
                }
            }
        }

        /// <summary>
        /// Update mesh polling center position to camera.
        /// </summary>
        void Update()
        {
            //Meshing -----------------------------------
            _meshingSubsystemComponent.gameObject.transform.position = _camera.gameObject.transform.position;
            if ((_bounded && _meshingSubsystemComponent.gameObject.transform.localScale != _boundedExtentsSize) ||
                (!_bounded && _meshingSubsystemComponent.gameObject.transform.localScale != _boundlessExtentsSize))
            {
                UpdateBounds();
            }

            if (UsePlaneManager)
            {
                if (planeManager != null)
                {
                    PlanesSubsystem.Extensions.Query = new PlanesSubsystem.Extensions.PlanesQuery
                    {
                        Flags = planeManager.currentDetectionMode.ToMLQueryFlags() | PlanesSubsystem.Extensions.MLPlanesQueryFlags.Polygons | PlanesSubsystem.Extensions.MLPlanesQueryFlags.Semantic_All,
                        BoundsCenter = Camera.main.transform.position,
                        BoundsRotation = Camera.main.transform.rotation,
                        BoundsExtents = Vector3.one * 20f,
                        MaxResults = maxResults,
                        MinHoleLength = minHoleLength,
                        MinPlaneArea = minPlaneArea
                    };

                }
            }
        }

        private void FixedUpdate()
        {
            _currentFrameIndex++;
            if (_currentFrameIndex >= numFrames) _currentFrameIndex = 0;

            _controllerPositions[_currentFrameIndex] = _controllerObject.transform.position;
        }

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDestroy()
        {
            permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;

#if UNITY_MAGICLEAP || UNITY_ANDROID
            controllerActions.Trigger.started -= OnTriggerStarted;
            controllerActions.Trigger.canceled -= OnTriggerCanceled;

            controllerActions.Bumper.performed -= OnBumperDown;
            controllerActions.Menu.performed -= OnMenuDown;
            inputSubsystem.trackingOriginUpdated -= OnTrackingOriginChanged;

            if (MLDevice.GestureSubsystemComponent != null)
                //MLDevice.GestureSubsystemComponent.onTouchpadGestureChanged -= OnTouchpadGestureStart;

            mlInputs.Dispose();
#endif
        }

        private void OnPermissionDenied(string permission)
        {
            Debug.LogError($"Failed to create Meshing Subsystem due to missing or denied {MLPermission.SpatialMapping} permission. Please add to manifest. Disabling script.");
            enabled = false;
            _meshingSubsystemComponent.enabled = false;
        }

        /// <summary>
        /// Updates examples status text.
        /// </summary>
        private void UpdateStatusText()
        {
            _statusLabel.text = string.Format("<color=#dbfb76><b>Controller Data</b></color>\nStatus: {0}\n", ControllerStatus.Text);

            _statusLabel.text += string.Format(
                "\n<color=#dbfb76><b>{0} {1}</b></color>\n{2} {3}: {4}\n{5} {6}: {7}\n{8}: {9}",
                "Meshing",
                "Data",
                "Render",
                "Mode",
                _renderMode.ToString(),
                "Bounded",
                "Extents",
                _bounded.ToString(),
                "LOD",
#if UNITY_2019_3_OR_NEWER
                MeshingSubsystemComponent.DensityToLevelOfDetail(_meshingSubsystemComponent.density).ToString()
#else
                _meshingSubsystemComponent.levelOfDetail.ToString()
#endif
                );
        }

        
        /// <summary>
        /// Handles the event for bumper down. Changes render mode.
        /// </summary>
        /// <param name="callbackContext"></param>
        private void OnBumperDown(InputAction.CallbackContext callbackContext)
        {
            ShootCube();
        }

        /// <summary>
        ///  Handles the event for Home down. 
        /// changes from bounded to boundless and viceversa.
        /// </summary>
        /// <param name="callbackContext"></param>
        private void OnMenuDown(InputAction.CallbackContext callbackContext)
        {
        }

        private void OnTriggerCanceled(InputAction.CallbackContext callbackContext)
        {
            if (_isHoldingBall)
            {
                ThrowBall();
            }
            else
            {
                ResetBall();
            }
        }
        
        /// <summary>
        /// Handles the event for trigger down. Throws a ball in the direction of
        /// the camera's forward vector.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void OnTriggerStarted(InputAction.CallbackContext callbackContext)
        {
            if (xRRayInteractor.TryGetCurrentUIRaycastResult(out UnityEngine.EventSystems.RaycastResult result))
            {
                return;
            }

            if (!_isHoldingBall)
            {
                GenerateBall();
            }
        }

        void GenerateBall()
        {
            if (bouncyBall == null)
            {
                bouncyBall = Instantiate(_bouncyBallPrefab, _controller.gameObject.transform);
                bouncyBall.transform.localPosition = Vector3.zero;
                _isHoldingBall = true;
            }
            else
            {
                ResetBall();
            }
        }

        void ThrowBall()
        {
            if (bouncyBall == null)
            {
                GenerateBall();
            }
            
            bouncyBall.transform.SetParent(null);
            var rb = bouncyBall.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;

            PreviousBallPos = bouncyBall.transform.position;
            
            var direction = (_controllerPositions[_currentFrameIndex] -
                             _controllerPositions[(_currentFrameIndex + 1) % numFrames]);
            
            rb.AddForce(direction * throwForce);
            
            _currentFrameIndex = 0;
            _controllerPositions = new Vector3[numFrames];

            _isHoldingBall = false;
        }

        void ResetBall()
        {
            if (bouncyBall != null)
            {
                bouncyBall.transform.parent = _controllerObject.transform;
                bouncyBall.transform.localPosition = Vector3.zero;
                bouncyBall.GetComponent<Rigidbody>().isKinematic = true;
                _isHoldingBall = true;

                if (NumBounces > HighScoreBounces)
                {
                    HighScoreBounces = NumBounces;
                    StartCoroutine(ScaleUpAndDown(highScoreCelebrationText, new Vector3(2f, 2f, 2f), 1f));
                }

                NumBounces = 0;

                UpdateText();
            }
        }

        public void UpdateText()
        {
            numBouncesText.text = "ur score: " + NumBounces;
            highScoreBouncesText.text = "high score: " + HighScoreBounces;

        }
        
        IEnumerator ScaleUpAndDown(TMP_Text tmpText, Vector3 upScale, float duration)
        {
            tmpText.gameObject.SetActive(true);
            var origText = tmpText.text;
            tmpText.text += HighScoreBounces.ToString();
            
            var objectTransform = tmpText.transform;
            Vector3 initialScale = objectTransform.localScale;
 
            for(float time = 0 ; time < duration ; time += Time.deltaTime)
            {
                float progress = Mathf.PingPong(time, duration) / duration;
                objectTransform.localScale = Vector3.Lerp(initialScale, upScale, progress);
                yield return null;
            }
            objectTransform.localScale = initialScale;
            
            tmpText.text = origText;
            tmpText.gameObject.SetActive(false);


        }
        
        void ShootCube()
        {
            // TODO: Use pool object instead of instantiating new object on each trigger down.
            // Create the ball and necessary components and shoot it along raycast.
            GameObject ball;
            if (ballPool.Count < ballPoolSize)
            {
                ball = Instantiate(_ballPrefab);
                ball.SetActive(true);
                ballPool.Add(ball);
            }
            else
            {
                ball = ballPool[(ballCounter - 1) % ballPoolSize];
            }

            ballCounter++;
            ball.transform.position = _controller.gameObject.transform.position;
            shootAudio.PlayOneShot(shootAudio.clip);

            Rigidbody rigidBody = ball.GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                rigidBody = ball.AddComponent<Rigidbody>();
            }

            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.AddForce(_controller.gameObject.transform.forward * ballForce);
        }

        /// <summary>
        /// Handles the event for touchpad gesture start. Changes level of detail
        /// if gesture is swipe up.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="gesture">The gesture getting started.</param>
        private void OnTouchpadGestureStart(GestureSubsystem.Extensions.TouchpadGestureEvent touchpadGestureEvent)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            if (touchpadGestureEvent.state == GestureState.Started &&
                touchpadGestureEvent.type == InputSubsystem.Extensions.TouchpadGesture.Type.Swipe &&
                touchpadGestureEvent.direction == InputSubsystem.Extensions.TouchpadGesture.Direction.Up)
            {
#if UNITY_2019_3_OR_NEWER
                _meshingSubsystemComponent.density = MLSpatialMapper.LevelOfDetailToDensity((MLSpatialMapper.DensityToLevelOfDetail(_meshingSubsystemComponent.density) == MLSpatialMapper.LevelOfDetail.Maximum) ? MLSpatialMapper.LevelOfDetail.Minimum : (MLSpatialMapper.DensityToLevelOfDetail(_meshingSubsystemComponent.density) + 1));
#else
                _mlSpatialMapper.levelOfDetail = ((_mlSpatialMapper.levelOfDetail == MLSpatialMapper.LevelOfDetail.Maximum) ? MLSpatialMapper.LevelOfDetail.Minimum : (_mlSpatialMapper.levelOfDetail + 1));
#endif
            }
#endif
        }

        /// <summary>
        /// Handle in charge of refreshing all meshes if a new session occurs
        /// </summary>
        /// <param name="inputSubsystem"> The inputSubsystem that invoked this event. </param>
        private void OnTrackingOriginChanged(XRInputSubsystem inputSubsystem)
        {
#if UNITY_MAGICLEAP || UNITY_ANDROID
            _meshingSubsystemComponent.DestroyAllMeshes();
            _meshingSubsystemComponent.RefreshAllMeshes();
#endif
        }

        private void UpdateBounds()
        {
            _visualBounds.SetActive(_bounded);
            _meshingSubsystemComponent.gameObject.transform.localScale = _bounded ? _boundedExtentsSize : _boundlessExtentsSize;
            
            var extent = _bounded ? _boundedExtentsSize : _boundlessExtentsSize;
            if (bouncyBall != null)
            {
                if (bouncyBall.transform.position.x > extent.x || bouncyBall.transform.position.y > extent.y ||
                    bouncyBall.transform.position.z > extent.z)
                {
                    ResetBall();
                }
            }
        }
    }
}

#endif
