using UnityEngine;
using UnityEngine.Events;

namespace NamruUtilitySuite
{
    /// <summary>
    /// Version of alarm that has an event that gets evoked automatically at the end of the alarm. 
    /// This is more expensive than a simple alarm, so only use this instead if you're planning on 
    /// using a UnityEvent tied to this alarm.
    /// </summary>
    [System.Serializable]
    public struct AdvancedAlarm
    {
        public AlarmMode Mode;

        private float currentValue;
        /// <summary>
        /// Current value, or count, of this alarm
        /// </summary>
        [HideInInspector] public float CurrentValue => currentValue;

        public float Duration; //Note: I'm NOT making this constant because I've had cases where I needed the duration to change dynamically...
        [Tooltip("Optional amount to randomly 'jitter' the alarm duration")]
        public float RandomJitter;

        public UnityEvent Event_OnAlarmEnd;

        public AdvancedAlarm(AlarmMode mode, float myDur)
        {
            Mode = mode;
            currentValue = 0;
            Duration = myDur;
            RandomJitter = 0f;

            Event_OnAlarmEnd = new UnityEvent();

            Reset();
        }

        /// <summary>
        /// Sets the value of the alarm to the proper start value depending on whether it's supposed to be counting up or down.
        /// </summary>
        public void Reset()
        {
            if (Mode == AlarmMode.CountingDown)
            {
                currentValue = Duration;
            }
            else if (Mode == AlarmMode.CountingUp)
            {
                currentValue = 0f;
            }

            if (RandomJitter > 0f)
            {
                currentValue += Random.Range(-RandomJitter, RandomJitter);
            }
        }

        /// <summary>
        /// Moves the alarm by the supplied value.
        /// </summary>
        /// <param name="timeMult"></param>
        /// <returns>true if the alarm hits its goal on this call. False if not.</returns>
        public bool MoveTowardGoal(float timeMult)
        {
            if (Mode == AlarmMode.CountingDown)
            {
                if (currentValue > 0f)
                {
                    currentValue -= timeMult;

                    if (currentValue <= 0f)
                    {
                        currentValue = 0f;
                        return true;
                    }
                }
            }
            else if (Mode == AlarmMode.CountingUp)
            {
                if (currentValue < Duration)
                {
                    currentValue += timeMult;

                    if (currentValue >= Duration)
                    {
                        currentValue = Duration;
                        return true;
                    }
                }
            }

            return false;
        }

        private static readonly AdvancedAlarm downAlarm = new AdvancedAlarm(AlarmMode.CountingDown, 0f);
        private static readonly AdvancedAlarm upAlarm = new AdvancedAlarm(AlarmMode.CountingUp, 0f);

        public static AdvancedAlarm down
        {
            get
            {
                return downAlarm;
            }
        }

        public static AdvancedAlarm up
        {
            get
            {
                return upAlarm;
            }
        }
    }
}