using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace GameDirection
{
    public class GameInitializer : MonoBehaviour
    {
        public float waitingTime = 10f;
        public DefaultCycleGameDirector gameDirector;

        private float _timeIntroStarted;
        private Text _messageText;

        public void OnEnable()
        {
            _timeIntroStarted = Time.time;
            _messageText = GameObject.FindWithTag("MessageText").GetComponent<Text>();
        }

        public void Update()
        {
            var starting = Time.time - _timeIntroStarted < waitingTime;

            if (starting)
            {
                _messageText.text = "Starting in: " + ((int) (waitingTime - (Time.time - _timeIntroStarted)) + 1) +
                                    "...";
                return;
            }

            enabled = false;
            _messageText.text = "";
            
            if (!PhotonNetwork.IsMasterClient)
                return;
            
            gameDirector.RaiseStartGameEvent();
        }
    }
}