using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiClock : MonoBehaviour
    {
        private bool _active;

        private float _seconds;
        private Text _text;

        public bool stopwatch;

        public float Seconds
        {
            get => _seconds;
            set => SetSeconds(value);
        }

        public string StringValue => ((int) (Seconds / 60)).ToString("D2") 
                                     + ":" 
                                     + (Seconds % 60).ToString("00.00");

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