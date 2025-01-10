using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NamruUtilitySuite
{
    public static class SM_Utils
    {

    }

    public enum SessionState
    {
        WaitingToStart,
        Started,
        Ended
    }

    public enum NSS_PressMode
    {
        GetKeyDown,
        GetKey,
        GetKeyUp
    }
}
