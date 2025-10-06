using UnityEngine;
using System;

namespace NamruUtilitySuite
{
	public enum NAMRU_LogType
	{
		None,
		UserMethod,
		UnityAPI,
	}

	/// <summary>
	/// Destination for where logs should go. Note: Hidden only logs to the session logger. All destinations log to session logger.
	/// </summary>
	public enum LogDestination
	{
		Hidden,//Hidden only logs to the session logger
		Console,
		MomentaryDebugLogger,
		ConsoleAndMomentaryDebugLogger,
		Everywhere,
	}

	/// <summary>
	/// Static class for logging features.
	/// </summary>
	public class NamruLogManager : MonoBehaviour
	{
		public static NamruLogManager Instance;

		[SerializeField] private MomentaryDebugLogger momentaryDebugLogger;
		public MomentaryDebugLogger MomentaryDebugLogger => momentaryDebugLogger;

		private int tabLevel = 0;
		/// <summary>
		/// Name of the application/project. Set this from an outside script.
		/// </summary>
		public static string ApplicationName = string.Empty;

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
		}

		private void Start()
		{
			CheckIfKosher();
		}

		public static void IncrementTabLevel()
		{
			if( Instance == null )
			{
				return;
			}

			Instance.tabLevel++;
		}

		public static void DecrementTabLevel()
		{
			if (Instance == null)
			{
				return;
			}

			Instance.tabLevel--;

			if(Instance.tabLevel < 0 )
			{
				Instance.tabLevel = 0;
			}
		}

		public static void Log( string message, LogDestination destination = LogDestination.Console )
		{
			if ( DestinationIncludesConsole(destination) )
			{
				Debug.Log(message);
			}

			if ( DestinationIncludesMomentaryDebugLogger(destination) )
			{
				if ( !Application.isPlaying )
				{
					Debug.LogWarning($"Application is not playing. can't momentary log message: '{message}'");
				}
				if ( Instance == null)
				{
					Debug.LogWarning($"{nameof(NamruLogManager)}.Instance null. can't momentary log message: '{message}'");
				}
				else
				{
					Instance.momentaryDebugLogger.LogMomentarily( message );
				}
			}

			if ( NamruSessionManager.Instance != null )
			{
				string tabsString = "";
				if( Instance != null && Instance.tabLevel > 0 )
				{
					for ( int i = 0; i < Instance.tabLevel; i++ )
					{
						tabsString += "\t";
					}
				}

				NamruSessionManager.Instance.WriteToLogFile( tabsString + message );
			}
		}

		public static void LogWarning(string msg, bool fromThread = false)
		{
			Debug.LogWarning($"{ApplicationName} WARNING! " + msg);

			if ( !fromThread && Application.isPlaying )
			{
				Instance.momentaryDebugLogger.LogMomentarily($"<color=yellow>{msg}</color>");

			}

			if ( NamruSessionManager.Instance != null )
			{
				NamruSessionManager.Instance.WriteToLogFile(Environment.NewLine + "WARNING!!!!!!!!!!!!!!!!");
				NamruSessionManager.Instance.WriteToLogFile(msg);
				NamruSessionManager.Instance.WriteToLogFile("!!!!!!!!!!!!!!!!!!!" + Environment.NewLine);
			}
		}

		public static void LogError( string msg, bool fromThread = false ) //todo: do I need the fromthread parameter?
		{
			Debug.LogError( $"{ApplicationName} ERROR! " + msg );

			if ( !fromThread && Application.isPlaying )
			{
				if ( Instance == null )
				{
					Debug.LogWarning( $"Singleton instance was null. Can't use Momentary Debug Logger!" );
				}
				else
				{
					Instance.momentaryDebugLogger.LogMomentarily( $"<color=red>{msg}</color>" );
				}
			}

			if ( NamruSessionManager.Instance != null )
			{
				NamruSessionManager.Instance.WriteToLogFile(Environment.NewLine + "ERROR!!!!!!!!!!!!!!!!");
				NamruSessionManager.Instance.WriteToLogFile(msg);
				NamruSessionManager.Instance.WriteToLogFile("!!!!!!!!!!!!!!!!!!!" + Environment.NewLine);

			}
		}

		public static void LogException( Exception e )
		{
			LogError( $"Caught exception of type: {e.GetType()}. Exceptions says: '{e}'" );
        }

        public static void LogException(Exception e, string errorDumpString)
        {
            LogError($"Caught exception of type: {e.GetType()}. Exceptions says: '{e}'");
            NamruSessionManager.Instance.WriteToLogFile($"----Error Dump String----'n" +
                $"{errorDumpString}");
        }

        public static void LogTrialMessage( string msg )
		{
			NamruSessionManager.Instance.WriteToTrialResults(msg);
		}

		private static bool DestinationIncludesConsole(LogDestination destination_passed )
		{
			if ( destination_passed == LogDestination.Everywhere || destination_passed == LogDestination.Console || destination_passed == LogDestination.ConsoleAndMomentaryDebugLogger )
			{
				return true;
			}

			return false;
		}

		private static bool DestinationIncludesMomentaryDebugLogger( LogDestination destination_passed )
		{
			if ( destination_passed == LogDestination.Everywhere || destination_passed == LogDestination.MomentaryDebugLogger || destination_passed == LogDestination.ConsoleAndMomentaryDebugLogger)
			{
				return true;
			}

			return false;
		}

		public bool CheckIfKosher()
		{
			bool amKosher = true;

			if ( momentaryDebugLogger == null )
			{
				amKosher = false;
				LogError($"{nameof(momentaryDebugLogger)} reference was null!");
			}

			return amKosher;
		}
	}
}