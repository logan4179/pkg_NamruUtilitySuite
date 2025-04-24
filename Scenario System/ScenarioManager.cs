using UnityEngine;
using UnityEngine.Events;

namespace NamruUtilitySuite
{
    public class ScenarioManager : MonoBehaviour
    {
        public static ScenarioManager Instance;

        [Header("[--- SESSION ---]")]
        private SessionState _sessionState;
        public SessionState _SessionState => _sessionState;

        /// <summary>
        /// How long the session is supposed to run after put in the Started state.
        /// </summary>
        public float SessionDuration = 60f;

        private float runningSessionDuration = 0f;
        /// <summary>
        /// Total time the scenario has been considered 'started'
        /// </summary>
        public float RunningSessionDuration => runningSessionDuration;


        [Header("[--- STAGES ---]")]
        public ScenarioStage[] Stages;
        private int index_currentStage = -1;
        public int Index_currentStage
        {
            get
            {
                return index_currentStage;
            }
        }

        public ScenarioStage CurrentStage => Stages[index_currentStage];

        /// <summary>
        /// This is a flag I added so that the scenario manager won't get caught in a stack over flow (infinite loop) 
        /// or incorrect behavior by calling a stage advance within a stage advance. It will also warn the dev if 
        /// this should occur.
        /// </summary>
        private bool flag_inStageAdvance = false;


        [Header("[--- WAIT MECHANISM ---]")]
        private Alarm waitAlarm;
        /// <summary>
        /// Stage to go to after waitAlarm counts down.
        /// </summary>
        private int index_StageAfterWait;
        private bool flag_amInWait = false;
        public bool Flag_AmInWait => flag_amInWait;

        /// <summary>
        /// Optional event for when a stage advance is requested within a stage advance. This would 
        /// typically be used to log an error to a custom console in the Unity app to let the user 
        /// know something has gone wrong in a build.
        /// </summary>
        [HideInInspector] public NSSErrorEvent Event_Warning;

        [SerializeField] private string DBG_History;

        public UnityEvent Event_SessionEnded;


        private void Awake()
        {
            Instance = this;

            Event_Warning = new NSSErrorEvent();

            if (Stages != null)
            {
                foreach (ScenarioStage s in Stages)
                {
                    s.InitFromManager(this);
                }
            }
        }

        private void Start()
        {
            DBG_History = "";

            if (Stages != null)
            {
                foreach (ScenarioStage s in Stages)
                {
                    s.CheckIfKosher();
                }
            }

            flag_inStageAdvance = false;

            index_currentStage = -1; //This is so that GoToOrderedStage() will know it's being called from start()
            GoToStage(0);
        }

        public void Update()
        {
            if( Stages == null || Stages.Length <= 0 )
            {
                return;
            }

            if (index_currentStage < Stages.Length)
            {
                Stages[index_currentStage].UpdateMe(Time.deltaTime);
            }

            if (_sessionState == SessionState.Started)
            {
                runningSessionDuration += Time.deltaTime;

                if (runningSessionDuration >= SessionDuration)
                {
                    EndSession();
                }
            }

            flag_amInWait = false;
            if (waitAlarm.CurrentValue > 0)
            {
                flag_amInWait = true;

                if (waitAlarm.MoveTowardGoal(Time.deltaTime))
                {
                    GoToStage(index_StageAfterWait);
                }
            }
        }

        /*
        //Note, I used this when I went from a single alarm to an array of alarms. It helped me transfer the original 
        // alarm data to the first entry in the new list. Even the event stuff was succesfully transfered!
        // Leaving it here in case I want to do something like this again in the future.
        [ContextMenu("z call TransferAlarms()")]
        public void TransferAlarms() 
        {
            print($"{nameof(TransferAlarms)}. There are '{Stages.Length}' stages...");
            for ( int i = 0; Stages.Length > i; i++ )
            {
                print($"i: '{i}'...");
                if ( Stages[i].MyAlarm.Duration > 0 )
                {
                    print($"found alarm...");

                    Stages[i].MyAlarms = new AdvancedAlarm[1];

                    Stages[i].MyAlarms[0].Duration = Stages[i].MyAlarm.Duration;
                    Stages[i].MyAlarms[0].Mode = Stages[i].MyAlarm.Mode;
                    Stages[i].MyAlarms[0].RandomJitter = Stages[i].MyAlarm.RandomJitter;
                    if( Stages[i].MyAlarm.Event_OnAlarmEnd != null )
                    {
                        Stages[i].MyAlarms[0].Event_OnAlarmEnd = Stages[i].MyAlarm.Event_OnAlarmEnd;
                    }
                }
            }
        }
        */

        /// <summary>
        /// Call this when you want the ScenarioDuration float to start counting.
        /// </summary>
        public void StartSession()
        {
            _sessionState = SessionState.Started;

            Event_SessionEnded.Invoke();
        }

        /// <summary>
        /// Call this when you want the ScenarioDuration float to stop counting.
        /// </summary>
        public void EndSession()
        {
            _sessionState = SessionState.Ended;
        }

        /// <summary>
        /// Ends the current phase, and passes to the next stage.
        /// </summary>
        public void AdvanceStage()
        {
            if (flag_inStageAdvance)
            {
                string errorString = $"NSS WARNING! {nameof(AdvanceStage)}() was called while the stage advancing flag " +
                    $"was true. This means that there is a problem in your logic causing a call to advance the " +
                    $"stage within another call to advance the stage. This could cause a stack-overflow/endless-loop. " +
                    $"Returning early...";

                Debug.LogWarning(errorString);

                Event_Warning.Invoke(errorString);
                return;
            }

            GoToStage(index_currentStage + 1);
        }

        /// <summary>
        /// Ends current stage and begins stage at supplied index.
        /// </summary>
        /// <param name="indx"></param>
        public void GoToStage(int indx)
        {
            if (flag_inStageAdvance)
            {
                string errorString = $"NSS WARNING! {nameof(GoToStage)}() was called while the stage advancing flag " +
                    $"was true. This means that there is a problem in your logic causing a call to advance the " +
                    $"stage within another call to advance the stage. This could cause a stack-overflow/endless-loop. " +
                    $"Current stage: '{CurrentStage._description}' at index: '{index_currentStage}'. Requested index: '{indx}'. Returning early...";

                Debug.LogWarning(errorString);

                Event_Warning.Invoke(errorString);

                return;
            }

            flag_inStageAdvance = true;

            if (index_currentStage < Stages.Length && index_currentStage >= 0)
            {
                Stages[index_currentStage].EndStage();
            }

            if (indx < Stages.Length && indx >= 0)
            {
                index_currentStage = indx;
                Stages[index_currentStage].BeginStage();
            }

            flag_inStageAdvance = false;
        }

        /// <summary>
        /// Ends current stage and begins stage at supplied index.
        /// </summary>
        /// <param name="indx"></param>
        public void GoToStageAfterWait(int indx, float waitDuration)
        {
            Debug.Log("GoToStageAfterWait()");
            waitAlarm.Duration = waitDuration;
            waitAlarm.Reset();

            index_StageAfterWait = indx;
        }

        public string GetDiagnosticString()
        {
            string s = "";

            if (index_currentStage < Stages.Length && index_currentStage >= 0)
            {
                s = $"{nameof(runningSessionDuration)}: '{runningSessionDuration}' / {SessionDuration}\n";

                s += CurrentStage.GetDiagnosticString();
            }
            else
            {
                s = $"<color=red>{nameof(index_currentStage)}: '{index_currentStage}'</color>\n";
            }

            s += $"{nameof(_sessionState)}: '{_sessionState}'\n" +
                $"{nameof(runningSessionDuration)}: '{runningSessionDuration}' / {SessionDuration}\n" +
                $"{nameof(flag_amInWait)}: '{flag_amInWait}'\n" +
                $"wait duration: '{waitAlarm.CurrentValue}' / {waitAlarm.Duration}";

            return s;
        }
    }

    public class NSSErrorEvent : UnityEvent<string>
    {

    }
}
