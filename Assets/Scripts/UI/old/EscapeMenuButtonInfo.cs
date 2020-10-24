using UnityEngine.Events;

namespace UI
{
    public readonly struct EscapeMenuButtonInfo
    {
        public readonly string name;
        public readonly UnityEvent clickEvent;

        public EscapeMenuButtonInfo(string name, UnityEvent clickEvent)
        {
            this.name = name;
            this.clickEvent = clickEvent;
        }
    }
}