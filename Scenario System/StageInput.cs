using UnityEngine;

namespace NamruUtilitySuite
{
    [System.Serializable]
    public struct StageInput
    {
        public KeyCode _KeyCode;

        public NSS_PressMode PressMode;

        public bool AmBeingTriggered()
        {
            if (PressMode == NSS_PressMode.GetKeyDown && Input.GetKeyDown(_KeyCode))
            {
                return true;
            }
            else if (PressMode == NSS_PressMode.GetKey && Input.GetKey(_KeyCode))
            {
                return true;
            }
            else if (PressMode == NSS_PressMode.GetKeyUp && Input.GetKeyUp(_KeyCode))
            {
                return true;
            }

            return false;
        }
    }
}