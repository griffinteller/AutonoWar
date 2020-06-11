using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiClock : MonoBehaviour
    {
        public float Seconds
        {
            get => _seconds;
            set => SetSeconds(value);
        }

        public bool stopwatch;

        private float _seconds;
        private Text _text;
        private bool _active;

        public void Stop()
        {
            _active = false;
        }

        private void SetSeconds(float seconds)
        {
            _seconds = seconds;
            _text.text = (int) (seconds / 60) + ":" + ((int) (seconds % 60)).ToString("D2");
        }

        public void Awake()
        {
            _text = GetComponentInChildren<Text>();
            _active = false;
            Seconds = 0;
        }

        public void StartClock()
        {
            _active = true;
        }

        public void Update()
        {
            if (!_active)
                return;
            
            if (stopwatch)
                Seconds += Time.deltaTime;
            else
                Seconds -= Time.deltaTime;
        }
    }
}