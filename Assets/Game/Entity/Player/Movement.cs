using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Minicop.Game.CubeDino
{
    public class Movement : NetworkBehaviour
    {
        private Transform _transform;
        private Rigidbody _rigidbody;

        public LayerMask GroundLayer;
        public UnityEvent OnMove = new UnityEvent();
        public UnityEvent OnJump = new UnityEvent();
        public UnityEvent OnStand = new UnityEvent();



        private void Start()
        {
            _transform = transform;
            _rigidbody = GetComponent<Rigidbody>();
        }

        [SerializeField]
        private float _rotateSpeed = 1f;
        private float _rotationY;
        private float _rotationX;
        public void Rotate(Vector2 direction)
        {
            CmdRotate(direction);
            [Command(requiresAuthority = false)]
            void CmdRotate(Vector2 direction)
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                //_rotationY -= direction.y * _rotateSpeed * Time.deltaTime;
                //_rotationY = Mathf.Clamp(_rotationY, -90, 90);
                _rotationX += direction.x * _rotateSpeed * Time.deltaTime;
                _transform.rotation = Quaternion.Euler(0f, _rotationX, 0f);
#endif
            }
        }


        public void Stand()
        {
            OnStand.Invoke();
            CmdStand();
            [Command(requiresAuthority = false)]
            void CmdStand()
            {
                OnStand.Invoke();
            }
        }
        [SerializeField]
        private float _speedMax = 1f;
        [SyncVar]
        public float Speed = 5f;
        public void Move(Vector3 direction)
        {
            if (direction == Vector3.zero) return;
            CmdMove(direction);
            [Command(requiresAuthority = false)]
            void CmdMove(Vector3 direction)
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                OnMove.Invoke();
                _transform.Translate(direction.x * Speed, 0f, direction.z * Speed);
                return;
                _rigidbody.AddRelativeForce(new Vector3(direction.x, 0f, direction.y) * Speed * Time.fixedDeltaTime);
                float speed = Vector3.Magnitude(_rigidbody.velocity);
                if (speed > _speedMax)
                {
                    float brakeSpeed = speed - _speedMax;
                    Vector3 normalisedVelocity = _rigidbody.velocity.normalized;
                    Vector3 brakeVelocity = normalisedVelocity * brakeSpeed;
                    _rigidbody.AddForce(-brakeVelocity);
                }
#endif
            }
        }



        [SyncVar]
        public float JumpForce = 5f;
        public void Jump()
        {
            CmdJump(ConnectionInfo.LocalName);
            /*
            if (!Physics.SphereCast(transform.position, 0.45f, Vector3.down, out RaycastHit sphereCast, 1f + 0.1f, GroundLayer)) return;
            OnJump.Invoke();
            CmdJump();
            [Command(requiresAuthority = false)]
            void CmdJump()
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                //OnJump.Invoke();
                Vector3 flatVel = new Vector3(0f, _rigidbody.velocity.y, 0f);
                if (flatVel.magnitude > _speedMax) return;
                _rigidbody.AddForce(new Vector3(0f, JumpForce, 0f));
#endif
        }
*/
        }
        [Command(requiresAuthority = false)]
        private void CmdJump(string name) => RpcJump(name);
        [ClientRpc]
        private void RpcJump(string name)
        {
            Debug.Log($"Привет от {name}");
        }
    }
}