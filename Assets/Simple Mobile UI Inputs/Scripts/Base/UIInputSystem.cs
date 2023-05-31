using System;
using System.Collections.Generic;
using UI_Inputs;
using UnityEngine;

namespace UI_InputSystem.Base
{
    public class UIInputSystem : MonoBehaviour
    {
        public static UIInputSystem ME { get; private set; }

        private Dictionary<ButtonAction, UIInputButton> uiButtonInputs;
        
        #region Initialize Area

        private void Awake()
        {
            PrepareValues();
        }

        private void PrepareValues()
        {
            ME = this;
            
            CreateButtonDictionary();
        }

        private void CreateButtonDictionary()
        {
            if (uiButtonInputs != null)
                return;

            uiButtonInputs = UIInputsFinder.GetAvailableInputs<ButtonAction, UIInputButton, bool>();
        }
        #endregion

        #region Inputs Area
        private bool ButtonPressProcessor(ButtonAction buttonToCheckPress)
        {
            return uiButtonInputs.ContainsKey(buttonToCheckPress) && uiButtonInputs[buttonToCheckPress].InputValue;
        }
        public bool GetButton(ButtonAction buttonToCheck) => ButtonPressProcessor(buttonToCheck);
        #endregion
        
        #region Events Area
        public void AddOnClickEvent(ButtonAction action, Action @event) => uiButtonInputs[action].OnClick += @event;
        public void AddOnTouchEvent(ButtonAction action, Action @event) => uiButtonInputs[action].OnTouch += @event;
        public void RemoveOnClickEvent(ButtonAction action, Action @event) => uiButtonInputs[action].OnClick -= @event;
        public void RemoveOnTouchEvent(ButtonAction action, Action @event) => uiButtonInputs[action].OnTouch -= @event;
        #endregion
    }
}