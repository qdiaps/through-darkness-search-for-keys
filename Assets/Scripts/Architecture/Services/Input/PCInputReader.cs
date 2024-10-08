﻿using System;
using UnityEngine;
using VContainer.Unity;

namespace Architecture.Services.Input
{
    public class PCInputReader : IJumpInputReader, IMoveInputReader, IScanInputReader, IPauseReader, IInputControlChanger,
        IFixedTickable, IDisposable
    {
        public event Action<Vector3> OnMove;
        public event Action<Vector3> OnSprintMove;
        public event Action OnJump;
        public event Action OnStartScan;
        public event Action OnStopScan;
        public event Action OnPauseEnter;
        public event Action OnPauseExit;
        
        private readonly InputControls _inputControls;
        
        private bool _isSprint;

        public PCInputReader()
        {
            _inputControls = new InputControls();
            RegisterInputAction();
        }

        public void Dispose() => 
            _inputControls.Disable();

        public void FixedTick()
        {
            ReadMove();
            ReadJump();
        }

        public void ChangeInputControl(InputControlType type)
        {
            switch (type)
            {
                case InputControlType.None:
                    _inputControls.UI.Disable();
                    _inputControls.Gameplay.Disable();
                    break;
                case InputControlType.Gameplay:
                    _inputControls.UI.Disable();
                    _inputControls.Gameplay.Enable();
                    break;
                case InputControlType.UI:
                    _inputControls.Gameplay.Disable();
                    _inputControls.UI.Enable();
                    break;
            }
        }

        private void ReadMove()
        {
            var input = _inputControls.Gameplay.Move.ReadValue<Vector2>();
            var velocity = new Vector3(input.x, 0, input.y);
            if (velocity.x != 0 || velocity.z != 0)
            {
                if (_isSprint)
                {
                    OnSprintMove?.Invoke(velocity);
                    return;
                }

                OnMove?.Invoke(velocity);
            }
        }

        private void ReadJump()
        {
            if (_inputControls.Gameplay.Jump.IsPressed())
                OnJump?.Invoke();
        }

        private void RegisterInputAction()
        {
            RegisterGameplay();
            RegisterUI();
        }

        private void RegisterGameplay()
        {
            _inputControls.Gameplay.SprintMove.started += _ => SetSprintValue(true);
            _inputControls.Gameplay.SprintMove.canceled += _ => SetSprintValue(false);
            _inputControls.Gameplay.Scan.started += _ => StartScan();
            _inputControls.Gameplay.Scan.canceled += _ => StopScan();
            _inputControls.Gameplay.Pause.performed += _ => PauseEnter();
        }

        private void RegisterUI()
        {
            _inputControls.UI.Play.performed += _ => PauseExit();
        }

        private void SetSprintValue(bool value) =>
            _isSprint = value;
        
        private void StartScan() =>
            OnStartScan?.Invoke();
        
        private void StopScan() =>
            OnStopScan?.Invoke();

        private void PauseEnter()
        {
            ChangeInputControl(InputControlType.UI);
            OnPauseEnter?.Invoke();
        }

        private void PauseExit()
        {
            ChangeInputControl(InputControlType.Gameplay);
            OnPauseExit?.Invoke();
        }
    }
}