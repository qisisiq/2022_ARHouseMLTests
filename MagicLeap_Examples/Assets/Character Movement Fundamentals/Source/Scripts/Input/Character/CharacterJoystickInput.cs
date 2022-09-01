using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.InteractionSubsystems;
using UnityEngine.XR.MagicLeap;

namespace CMF
{
	//This character movement input class is an example of how to get input from a gamepad/joystick to control the character;
	//It comes with a dead zone threshold setting to bypass any unwanted joystick "jitter";
	public class CharacterJoystickInput : CharacterInput {

		public string horizontalInputAxis = "Horizontal";
		public string verticalInputAxis = "Vertical";
		public KeyCode jumpKey = KeyCode.Joystick1Button0;

		//If this is enabled, Unity's internal input smoothing is bypassed;
		public bool useRawInput = true;

		//If any input falls below this value, it is set to '0';
        //Use this to prevent any unwanted small movements of the joysticks ("jitter");
		public float deadZoneThreshold = 0.2f;

		private MagicLeapInputs mlInputs;
		private MagicLeapInputs.ControllerActions controllerActions;

		void Start()
		{
			mlInputs = new MagicLeapInputs();
			mlInputs.Enable();
			controllerActions = new MagicLeapInputs.ControllerActions(mlInputs);

			controllerActions.TouchpadPosition.performed += HandleOnTouchpad;
			// canceled event used to detect when bumper button is released
			controllerActions.Bumper.canceled += HandleOnBumper;
			controllerActions.Bumper.performed += HandleOnBumper;
			controllerActions.Trigger.performed += HandleOnTrigger;
			
			MLDevice.GestureSubsystemComponent.onTouchpadGestureChanged += OnTouchpadGestureStart;


			InputSubsystem.Extensions.Controller.AttachTriggerListener(HandleOnTriggerEvent);
			MLDevice.RegisterGestureSubsystem();

		}
		
		/// <summary>
		/// Stop input api and unregister callbacks.
		/// </summary>
		void OnDestroy()
		{
			controllerActions.TouchpadPosition.performed -= HandleOnTouchpad;
			controllerActions.Bumper.canceled -= HandleOnBumper;
			controllerActions.Bumper.performed -= HandleOnBumper;
			controllerActions.Trigger.performed -= HandleOnTrigger;
			
			if (MLDevice.GestureSubsystemComponent != null)
				MLDevice.GestureSubsystemComponent.onTouchpadGestureChanged -= OnTouchpadGestureStart;


			InputSubsystem.Extensions.Controller.RemoveTriggerListener(HandleOnTriggerEvent);

			mlInputs.Dispose();
		}
		
		/// <summary>
		/// Handles the event for bumper.
		/// </summary>
		/// <param name="obj">Input Callback</param>
		private void HandleOnBumper(InputAction.CallbackContext obj)
		{
			bool bumperDown = obj.ReadValueAsButton();

			Debug.Log("Bumper was released this frame: " + obj.action.WasReleasedThisFrame());
		}

		private void HandleOnTrigger(InputAction.CallbackContext obj)
		{
			float triggerValue = obj.ReadValue<float>();
		}

		private void HandleOnTouchpad(InputAction.CallbackContext obj)
		{
			Vector2 triggerValue = obj.ReadValue<Vector2>();
		}

		private void HandleOnTriggerEvent(ushort controllerId, InputSubsystem.Extensions.Controller.MLInputControllerTriggerEvent triggerEvent, float depth)
		{
			Debug.Log($"Received trigger event: {triggerEvent} with trigger depth: {depth}, on controller id: {controllerId} ");
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
			}
#endif
		}

		
        public override float GetHorizontalMovementInput()
		{
			float _horizontalInput;

			if(useRawInput)
				_horizontalInput = controllerActions.TouchpadPosition.ReadValue<Vector2>().x;
			else
				_horizontalInput = controllerActions.TouchpadPosition.ReadValue<Vector2>().x;

			//Set any input values below threshold to '0';
			if(Mathf.Abs(_horizontalInput) < deadZoneThreshold)
				_horizontalInput = 0f;

			return _horizontalInput;
		}

		public override float GetVerticalMovementInput()
		{
			float _verticalInput;

			if(useRawInput)
				_verticalInput = controllerActions.TouchpadPosition.ReadValue<Vector2>().y;
			else
				_verticalInput = controllerActions.TouchpadPosition.ReadValue<Vector2>().y;

			//Set any input values below threshold to '0';
			if(Mathf.Abs(_verticalInput) < deadZoneThreshold)
				_verticalInput = 0f;

			return _verticalInput;
		}

		public override bool IsJumpKeyPressed()
		{
			return controllerActions.Trigger.ReadValue<float>() > 0;
		}

	}
}
