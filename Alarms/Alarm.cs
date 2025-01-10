using UnityEngine;

namespace NamruUtilitySuite
{
    [System.Serializable]
    public struct Alarm
    {
        public AlarmMode Mode;

        private float currentValue;
        /// <summary>
        /// Current value, or count, of this alarm
        /// </summary>
        [HideInInspector] public float CurrentValue => currentValue;

        public float Duration; //Note: I'm NOT making this constant because I've had cases where I needed the duration to change dynamically...

        public Alarm(AlarmMode mode, float myDur)
        {
            Mode = mode;
            currentValue = 0;
            Duration = myDur;

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

        private static readonly Alarm downAlarm = new Alarm(AlarmMode.CountingDown, 0f);
        private static readonly Alarm upAlarm = new Alarm(AlarmMode.CountingUp, 0f);

        public static Alarm down
        {
            get
            {
                return downAlarm;
            }
        }

        public static Alarm up
        {
            get
            {
                return upAlarm;
            }
        }
    }
}

