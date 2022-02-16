﻿/*
 * Copyright 2002-2020 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Examproject {
    /// <summary>
    /// player controller.
    /// @author h.adachi
    /// </summary>
    public class PlayerController : GamepadMaper {

        ///////////////////////////////////////////////////////////////////////////////////////////
        // References

        [SerializeField]
        float _jumpPower = 5.0f;

        [SerializeField]
        float _rotationalSpeed = 5.0f;

        [SerializeField]
        float _forwardSpeedLimit = 1.1f;

        [SerializeField]
        float _runSpeedLimit = 3.25f;

        [SerializeField]
        float _backwardSpeedLimit = 0.75f;

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Fields

        DoFixedUpdate _doFixedUpdate;

        float _speed;

        ///////////////////////////////////////////////////////////////////////////////////////////
        // update Methods

        // Awake is called when the script instance is being loaded.
        void Awake() {
            _doFixedUpdate = DoFixedUpdate.GetInstance();
        }

        // Start is called before the first frame update
        new void Start() {
            base.Start();

            var rb = transform.GetComponent<Rigidbody>(); // Rigidbody should be only used in FixedUpdate.

            this.FixedUpdateAsObservable().Subscribe(_ => {
                _speed = rb.velocity.magnitude; // get speed.
                Debug.Log("speed: " + _speed); // FIXME:
            });

            // walk.
            this.UpdateAsObservable().Where(_ => _upButton.isPressed).Subscribe(_ => {
                _doFixedUpdate.ApplyWalk();
            });

            this.FixedUpdateAsObservable().Where(_ => _doFixedUpdate.walk && _speed < _forwardSpeedLimit).Subscribe(_ => {
                rb.AddFor​​ce(transform.forward * 12.0f, ForceMode.Acceleration);
                _doFixedUpdate.CancelWalk();
            });

            // run.
            this.UpdateAsObservable().Where(_ => _upButton.isPressed && _yButton.isPressed).Subscribe(_ => {
                _doFixedUpdate.ApplyRun();
            });

            this.FixedUpdateAsObservable().Where(_ => _doFixedUpdate.run && _speed < _runSpeedLimit).Subscribe(_ => {
                rb.AddFor​​ce(transform.forward * 12.0f, ForceMode.Acceleration);
                _doFixedUpdate.CancelRun();
            });

            // backward.
            this.UpdateAsObservable().Where(_ => _downButton.isPressed).Subscribe(_ => {
                _doFixedUpdate.ApplyBackward();
            });

            this.FixedUpdateAsObservable().Where(_ => _doFixedUpdate.backward && _speed < _backwardSpeedLimit).Subscribe(_ => {
                rb.AddFor​​ce(-transform.forward * 12.0f, ForceMode.Acceleration);
                _doFixedUpdate.CancelBackward();
            });

            // jump.
            this.UpdateAsObservable().Where(_ => _bButton.wasPressedThisFrame).Subscribe(_ => {
                _doFixedUpdate.ApplyJump();
            });

            this.FixedUpdateAsObservable().Where(_ => _doFixedUpdate.jump).Subscribe(_ => {
                rb.useGravity = true;
                rb.AddRelativeFor​​ce(Vector3.up * _jumpPower * 40f, ForceMode.Acceleration);
                _doFixedUpdate.CancelJump();
            });

            // rotate.
            this.UpdateAsObservable().Subscribe(_ => {
                var axis = _rightButton.isPressed ? 1 : _leftButton.isPressed ? -1 : 0;
                transform.Rotate(0, axis * (_rotationalSpeed * Time.deltaTime) * 12.0f, 0);
            });
        }

        #region DoUpdate

        /// <summary>
        /// class for the Update() method.
        /// </summary>
        class DoUpdate {

            ///////////////////////////////////////////////////////////////////////////////////////
            // Fields

            bool _grounded;

            ///////////////////////////////////////////////////////////////////////////////////////
            // Properties

            public bool grounded {
                get => _grounded;
                set => _grounded = value;
            }

            ///////////////////////////////////////////////////////////////////////////////////////
            // Constructor

            /// <summary>
            /// returns an initialized instance.
            /// </summary>
            public static DoUpdate GetInstance() {
                return new DoUpdate();
            }

            ///////////////////////////////////////////////////////////////////////////////////////////
            // public Methods [verb]

            public void ResetState() {
                _grounded = false;
            }
        }

        #endregion

        #region DoFixedUpdate

        /// <summary>
        /// class for the FixedUpdate() method.
        /// </summary>
        class DoFixedUpdate {

            ///////////////////////////////////////////////////////////////////////////////////////
            // Fields

            bool _idol;
            bool _run;
            bool _walk;
            bool _jump;
            bool _backward;

            ///////////////////////////////////////////////////////////////////////////////////////
            // Properties

            public bool idol { get => _idol; }
            public bool run { get => _run; }
            public bool walk { get => _walk; }
            public bool jump { get => _jump; }
            public bool backward { get => _backward; }

            ///////////////////////////////////////////////////////////////////////////////////////
            // Constructor

            /// <summary>
            /// returns an initialized instance.
            /// </summary>
            public static DoFixedUpdate GetInstance() {
                return new DoFixedUpdate();
            }

            ///////////////////////////////////////////////////////////////////////////////////////////
            // public Methods

            public void ApplyRun() {
                _idol = _walk = _backward = false;
                _run = true;
            }

            public void CancelRun() {
                _run = false;
            }

            public void ApplyWalk() {
                _idol = _run = _backward = false;
                _walk = true;
            }

            public void CancelWalk() {
                _walk = false;
            }

            public void ApplyBackward() {
                _idol = _run = _walk = false;
                _backward = true;
            }

            public void CancelBackward() {
                _backward = false;
            }

            public void ApplyJump() {
                _jump = true;
            }

            public void CancelJump() {
                _jump = false;
            }
        }

        #endregion

    }

}
