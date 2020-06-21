using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Main
{
    public class ScheduledFlip : MonoBehaviourPun
    {
        private const float WaitTime = 10f + StartDelay;
        private const float StartDelay = 2f;
        private const float RaiseDistance = 2f;
        private const float RaiseTime = 0.7f;
        private const float RotateTime = 0.5f;
        private bool _flipping;
        private Quaternion _goalRotation;
        private Text _messageText;
        private Vector3 _raisedPosition;
        private Vector3 _startPosition;
        private Quaternion _startRotation;

        private float _startTime;
        private FlippingState _state;

        public void TryCancelFlip()
        {
            if (_state == FlippingState.Waiting)
            {
                enabled = false;
                _messageText.text = "";
            }
        }

        public void OnEnable()
        {
            _startTime = Time.time;
            _startPosition = transform.position;
            _startRotation = transform.rotation;
            _state = FlippingState.Waiting;

            _raisedPosition = _startPosition + Vector3.up * RaiseDistance;

            var forwardProjOntoXZ =
                Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            _goalRotation = Quaternion.LookRotation(forwardProjOntoXZ, Vector3.up);

            if (!PhotonNetwork.InRoom || photonView.IsMine)
                _messageText = GameObject.FindWithTag("MessageText").GetComponent<Text>();
        }

        private void FixedUpdate()
        {
            float t;
            switch (_state)
            {
                case FlippingState.Waiting:

                    var remainingTime = WaitTime - (Time.time - _startTime);

                    if (_messageText && remainingTime < WaitTime - StartDelay)
                        _messageText.text = "Flipping in: " + ((int) remainingTime + 1) + "...";

                    if (remainingTime < 0)
                    {
                        GetComponent<Rigidbody>().isKinematic = true;

                        if (_messageText)
                            _messageText.text = "";

                        _startTime = Time.time;
                        _state += 1;
                    }

                    break;

                case FlippingState.Raising:

                    t = (Time.time - _startTime) / RaiseTime;
                    if (t > 1)
                    {
                        _state += 1;
                        _startTime = Time.time;
                        break;
                    }

                    transform.position = Vector3.Lerp(_startPosition, _raisedPosition, t);
                    break;

                case FlippingState.Rotating:

                    t = (Time.time - _startTime) / RotateTime;
                    if (t > 1)
                    {
                        GetComponent<Rigidbody>().isKinematic = false;
                        enabled = false;
                        break;
                    }

                    transform.rotation = Quaternion.Slerp(_startRotation, _goalRotation, t);
                    break;
            }
        }

        private enum FlippingState
        {
            Waiting,
            Raising,
            Rotating
        }
    }
}