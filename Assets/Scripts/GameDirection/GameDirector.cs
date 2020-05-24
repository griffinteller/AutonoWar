using Networking;
using UnityEngine;

namespace GameDirection
{
    public abstract class GameDirector : MonoBehaviour
    {
        public abstract GameModeEnum GameMode { get; }

        public bool FullyLoaded
        {
            get => _fullyLoaded;
            set
            {
                _fullyLoaded = value;
                if (_fullyLoaded)
                    OnFullyLoaded();
            }
        }

        private bool _fullyLoaded;

        protected virtual void OnFullyLoaded() {}
    }
}