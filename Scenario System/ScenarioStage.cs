using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NamruUtilitySuite
{
    [System.Serializable]
    public class ScenarioStage
    {
        public string _description;
        private ScenarioManager _mgr;

        [Header("[---- INPUT ----]")]
        [SerializeField] private StageInput[] _inputs;
        public UnityEvent Event_InputTriggered;

        [Space(5)]

        [Header("[---- START ----]")]
        [SerializeField] List<GameObject> GOs_ActivateOnStart;
        public UnityEvent Event_StageStart;
        [Space(5)]

        [Header("[---- ALARM ----]")]
        public AdvancedAlarm[] MyAlarms;
        public int index_currentAlarm = 0;
        public int Index_currentAlarm => index_currentAlarm;
        public AdvancedAlarm CurrentAlarm => MyAlarms[index_currentAlarm];

        [Header("[---- END ----]")]
        public UnityEvent Event_StageEnd;


        /// <summary>
        /// Made this because I need a way to assign the manager reference, and I figure maybe one day 
        /// I'll want to do more initialization at this point.
        /// </summary>
        /// <param name="mgr"></param>
        public void InitFromManager(ScenarioManager mgr)
        {
            _mgr = mgr;

        }

        public void CheckIfKosher()
        {
            if (MyAlarms != null && MyAlarms.Length > 0)
            {
                for (int i = 0; i < MyAlarms.Length; i++)
                {
                    if (MyAlarms[i].Event_OnAlarmEnd.GetPersistentEventCount() > 0 && MyAlarms[i].Duration == 0)
                    {
                        string errorString = $"NSS WARNING! Alarm {i} for Stage '{_description}' has a subscriber, but no duration set. " +
                            $"Was this intentional?";

                        Debug.LogWarning(errorString);

                        _mgr.Event_Warning.Invoke(errorString);
                    }
                }
            }

            if (GOs_ActivateOnStart != null && GOs_ActivateOnStart.Count > 0)
            {
                foreach (GameObject go in GOs_ActivateOnStart)
                {
                    if (go == null)
                    {
                        string errorString = $"NSS WARNING! gameObject reference in stage '{_description}' in {nameof(GOs_ActivateOnStart)} was null";

                        Debug.LogWarning(errorString);

                        _mgr.Event_Warning.Invoke(errorString);
                    }
                }
            }
        }

        public void BeginStage()
        {
            if (GOs_ActivateOnStart != null && GOs_ActivateOnStart.Count > 0)
            {
                foreach (GameObject gOb in GOs_ActivateOnStart)
                {
                    gOb.SetActive(true);
                }
            }

            index_currentAlarm = 0;
            if (MyAlarms != null && MyAlarms.Length > 0)
            {
                for (int i = 0; i < MyAlarms.Length; i++)
                {
                    MyAlarms[i].Reset();
                }
            }

            if (Event_StageStart != null)
            {
                Event_StageStart.Invoke();
            }
        }

        public void UpdateMe(float timeDelta)
        {
            if (_inputs != null && _inputs.Length > 0)
            {
                bool allTriggered = true;
                foreach (StageInput pi in _inputs)
                {
                    if (!pi.AmBeingTriggered())
                    {
                        allTriggered = false;
                        break;
                    }
                }

                if (allTriggered)
                {
                    Event_InputTriggered.Invoke();
                }
            }

            if (MyAlarms != null && MyAlarms.Length > 0)
            {
                if (MyAlarms[index_currentAlarm].MoveTowardGoal(timeDelta))
                {
                    MyAlarms[index_currentAlarm].Event_OnAlarmEnd.Invoke();
                    MyAlarms[index_currentAlarm].Reset();
                    index_currentAlarm++;
                    if (index_currentAlarm >= MyAlarms.Length)
                    {
                        index_currentAlarm = 0;
                    }
                }
            }
        }

        public void EndStage()
        {
            if (Event_StageEnd != null)
            {
                Event_StageEnd.Invoke();
            }

            //MyAlarm.Reset(); //to set any alarms back to 0 so their end event doesn't accidentally get called.
        }

        public string GetDiagnosticString()
        {
            string s = $"Current Stage: '{_description}'\n";
            if (MyAlarms != null && MyAlarms.Length > 0)
            {
                s += $"alarm[{Index_currentAlarm}]: " +
                    $"'{CurrentAlarm.CurrentValue.ToString("#.##")} / " +
                    $"{CurrentAlarm.Duration}\n";

            }

            return s;
        }
    }
}