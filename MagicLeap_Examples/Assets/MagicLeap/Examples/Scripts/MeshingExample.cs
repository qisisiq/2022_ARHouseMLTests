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

using System.Collections;
using System.Collections.Generic;
using CMF;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public class MeshingExample : MonoBehaviour
    {
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

        [SerializeField, Space, Tooltip("Bean character to drop into the scene.")]
        private GameObject _beanPrefab = null;

        private GameObject _beanObject;

        [SerializeField, Space, Tooltip("Ball character to drop into the scene.")]
        private GameObject _ballPrefab;

        [SerializeField, Space, Tooltip("Reference to the controller gameobject.")]
        private GameObject _controllerObject = null;

        [SerializeField, Space, Tooltip("Render mode to render mesh data with.")]
        private MeshingVisualizer.RenderMode _renderMode = MeshingVisualizer.RenderMode.None;
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
        private int ballPoolSize = 30;
        
        private Camera _camera = null;

        private MagicLeapInputs mlInputs;
        private MagicLeapInputs.ControllerActions controllerActions;
        private XRRayInteractor xRRayInteractor;
        private XRInputSubsystem inputSubsystem;

        private readonly MLPermissions.Callbacks permissionCallbacks = new MLPermissions.Callbacks();

        private ActionBasedController _controller; 

        private bool isBeanInHand;
        
        // Hackathon area --------------------------------------------------------------------
        [SerializeField] private GameObject keerPrefab;
        
        private float horizontalRandom = 1f;
        private float verticalRandom = 0.25f;

        private GameObject model1;
        private GameObject model2;
        private GameObject model3;

        private Vector3 model1OrigPos;
        private Vector3 model2OrigPos;
        private Vector3 model3OrigPos;



        
        //Plane area -----------------------------------------------------------------------------------
        
        private ARPlaneManager planeManager;

        [SerializeField, Tooltip("Maximum number of planes to return each query")]
        private uint maxResults = 100;

        [SerializeField, Tooltip("Minimum hole length to treat as a hole in the plane")]
        private float minHoleLength = 0.5f;

        [SerializeField, Tooltip("Minimum plane area to treat as a valid plane")]
        private float minPlaneArea = 0.25f;

        /// <summary>
        /// Initializes component data and starts MLInput.
        /// </summary>
        void Awake()
        {
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
            if (_statusLabel == null)
            {
                Debug.LogError("MeshingExample._statusLabel is not set. Disabling script.");
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

            controllerActions.Trigger.performed += OnTriggerDown;
            controllerActions.Bumper.performed += OnBumperDown;
            controllerActions.Menu.performed += OnMenuDown;

            //MLDevice.GestureSubsystemComponent.onTouchpadGestureChanged += OnTouchpadGestureStart;
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

            //_meshingVisualizer.SetRenderers(_renderMode);

            _meshingSubsystemComponent.gameObject.transform.position = _camera.gameObject.transform.position;
            _meshingSubsystemComponent.density = 0.5f;
            
            _controller = FindObjectOfType<ActionBasedController>();
            
            UpdateBounds();
            
            // Plane ------
            planeManager = FindObjectOfType<ARPlaneManager>();
            if (planeManager == null)
            {
                Debug.LogError("Failed to find ARPlaneManager in scene. Disabling Script");
                //enabled = false;
            }
            
            // Keer prefab ---

            model1 = Instantiate(keerPrefab, new Vector3(-0.5f, 0.5f, 3f), Quaternion.identity);
            model1OrigPos = MoveModelOutOfMesh(model1);
            model2 = Instantiate(keerPrefab, new Vector3(-1.5f, 0.25f, 2f), Quaternion.identity);
            model2OrigPos = MoveModelOutOfMesh(model2);
            model3 = Instantiate(keerPrefab, new Vector3(-2f, 0.75f, 0f), Quaternion.identity);
            model3OrigPos = MoveModelOutOfMesh(model3);


            
            
        }

        Vector3 MoveModelOutOfMesh(GameObject model)
        {
            var origPosition = model.transform.position;
            // approximating a sphere of where my current object is 
            Collider[] hitColliders = Physics.OverlapSphere(model.transform.position - Vector3.down * -4.52f, 0.3f);

            for(int i = 0; i < 10; i++)
            {
                model.transform.position = origPosition + new Vector3(
                    Random.Range(-horizontalRandom, horizontalRandom),
                    Random.Range(-verticalRandom, verticalRandom),
                    Random.Range(-horizontalRandom, horizontalRandom));
            
                hitColliders = Physics.OverlapSphere(this.transform.position - Vector3.down * -4.52f, 0.3f);
                if (hitColliders.Length == 0)
                {
                    return model.transform.position;
                }
            }
            return model.transform.position;
        }


        /// <summary>
        /// Update mesh polling center position to camera.
        /// </summary>
        void Update()
        {
            //if (_meshingVisualizer.renderMode != _renderMode)
            //{
                //_meshingVisualizer.SetRenderers(_renderMode);
            //}
            
            //Meshing -----------------------------------
            _meshingSubsystemComponent.gameObject.transform.position = _camera.gameObject.transform.position;
            if ((_bounded && _meshingSubsystemComponent.gameObject.transform.localScale != _boundedExtentsSize) ||
                (!_bounded && _meshingSubsystemComponent.gameObject.transform.localScale != _boundlessExtentsSize))
            {
                UpdateBounds();
            }
            
            
            // Planes -----------------
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

        /// <summary>
        /// Cleans up the component.
        /// </summary>
        void OnDestroy()
        {
            permissionCallbacks.OnPermissionDenied -= OnPermissionDenied;
            permissionCallbacks.OnPermissionDeniedAndDontAskAgain -= OnPermissionDenied;

#if UNITY_MAGICLEAP || UNITY_ANDROID
            controllerActions.Trigger.performed -= OnTriggerDown;
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


        private void ResetModel(GameObject model, Vector3 origPos)
        {
            model.transform.position = origPos;
            model.transform.rotation = Quaternion.identity;

            var rb = model.gameObject.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        /// <summary>
        /// Handles the event for bumper down. Changes render mode.
        /// </summary>
        /// <param name="callbackContext"></param>
        private void OnBumperDown(InputAction.CallbackContext callbackContext)
        {
            ResetModel(model1, model1OrigPos);
            ResetModel(model2, model2OrigPos);
            ResetModel(model3, model3OrigPos);






            /*
            
            if (xRRayInteractor.TryGetCurrentUIRaycastResult(out UnityEngine.EventSystems.RaycastResult result))
            {
                return;
            }

            // TODO: Use pool object instead of instantiating new object on each trigger down.
            // Create the ball and necessary components and shoot it along raycast.
            GameObject ball = _beanObject;

            ball.SetActive(true);
            float ballsize = Random.Range(MIN_BALL_SIZE, MAX_BALL_SIZE);
            ball.transform.localScale = new Vector3(ballsize, ballsize, ballsize);
            ball.transform.position = _camera.gameObject.transform.position;

            Rigidbody rigidBody = ball.GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                rigidBody = ball.AddComponent<Rigidbody>();
            }
            rigidBody.AddForce(_camera.gameObject.transform.forward * 300f);

            Destroy(ball, BALL_LIFE_TIME);*/


/*
            if (_beanObject != null)
            {
                Destroy(_beanObject);
            } 
            _beanObject = Instantiate(_beanPrefab, _controllerObject.transform.position, quaternion.identity);
            */

        }

        /// <summary>
        ///  Handles the event for Home down. 
        /// changes from bounded to boundless and viceversa.
        /// </summary>
        /// <param name="callbackContext"></param>
        private void OnMenuDown(InputAction.CallbackContext callbackContext)
        {
            _bounded = !_bounded;
            UpdateBounds();
        }

        /// <summary>
        /// Handles the event for trigger down. Throws a ball in the direction of
        /// the camera's forward vector.
        /// </summary>
        /// <param name="controllerId">The id of the controller.</param>
        /// <param name="button">The button that is being released.</param>
        private void OnTriggerDown(InputAction.CallbackContext callbackContext)
        {
            if (xRRayInteractor.TryGetCurrentUIRaycastResult(out UnityEngine.EventSystems.RaycastResult result))
            {
                return;
            }

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
            
            float ballsize = Random.Range(MIN_BALL_SIZE, MAX_BALL_SIZE);
            ball.transform.localScale = new Vector3(ballsize, ballsize, ballsize);
            ball.transform.position = _controller.gameObject.transform.position;


            Rigidbody rigidBody = ball.GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                rigidBody = ball.AddComponent<Rigidbody>();
            }

            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.AddForce(_controller.gameObject.transform.forward * SHOOTING_FORCE);
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
        }
    }
}

#endif
