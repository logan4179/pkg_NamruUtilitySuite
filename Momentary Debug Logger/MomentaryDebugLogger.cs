using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NamruUtilitySuite
{
	public class MomentaryDebugLogger : MonoBehaviour
	{
		[Header("[-----------REFERENCE----------]")]
		public TextMeshProUGUI TMP_MomentaryDebugLogger;
		[SerializeField] private RawImage ri_background;

		[Header("[-----------SETTINGS----------]")]
		[SerializeField] private float duration_momentaryDebugLoggerMessages = 3.5f;
		[SerializeField] private int count_maxDebugLogMessages = 8;
		[SerializeField, Tooltip("If enabled, causes the logger to turn the background on and off depending on whether there are messages showing.")]
		private bool swizzleBackground = false;

		//[Header("[-----------OTHER----------]")]
		private List<string> tempStrings = new List<string>();
		private List<float> tempStringCountdowns = new List<float>();
		private string threadsafeString;
		private Color ri_background_color_cached;

		//[TextArea(1,10)] public string dbgString; //uncomment this variable, and it's code in the update if you want to debug the messages in this logger.

		private void Awake()
		{
			//Setting the following text to empty in Awake() instead of Start() prevents it from
			//potentially getting erased if it gets logged on Start()...

			if (tempStrings == null || tempStrings.Count == 0)
			{
				TMP_MomentaryDebugLogger.text = string.Empty;
			}

			if (swizzleBackground)
			{
				ri_background_color_cached = ri_background.color;
				ri_background.color = Color.clear;
			}
		}

		void Start()
		{
			CheckIfKosher();
		}

		void Update()
		{
			if (!string.IsNullOrEmpty(threadsafeString))
			{
				LogMomentarily(threadsafeString);
				threadsafeString = string.Empty;
			}

			if (tempStrings != null && tempStrings.Count > 0)
			{
				bool updateMade = false;

				for (int i = tempStrings.Count - 1; i > -1; i--)
				{
					if (i > count_maxDebugLogMessages)
					{
						tempStrings.RemoveAt(i);
						tempStringCountdowns.RemoveAt(i);
						updateMade = true;
					}
					else
					{
						tempStringCountdowns[i] -= Time.deltaTime;

						if (tempStringCountdowns[i] <= 0f)
						{
							tempStrings.RemoveAt(i);
							tempStringCountdowns.RemoveAt(i);
							updateMade = true;
						}
					}
				}

				if (updateMade)
				{
					UpdateTemporaryDebugLoggerText();

					if (swizzleBackground && tempStringCountdowns.Count <= 0)
					{
						ri_background.color = Color.clear;
					}
				}
			}

			/*
			dbgString = string.Empty;

			if( tempStrings.Count > 0 )
			{
				for ( int i = tempStrings.Count - 1; i > -1; i-- )
				{
					dbgString += $"{tempStrings[i]}, {tempStringCountdowns[i]}\n";
				}
			}
			*/
		}

		public void LogMomentarily(string msg)
		{
			if (swizzleBackground && tempStrings.Count <= 0)
			{
				ri_background.color = ri_background_color_cached;
			}

			tempStrings.Insert(0, msg);
			tempStringCountdowns.Insert(0, duration_momentaryDebugLoggerMessages);
			UpdateTemporaryDebugLoggerText();
		}

		public void LogMomentarily_threadSafe(string msg)
		{
			threadsafeString = msg;
		}

		private void UpdateTemporaryDebugLoggerText()
		{
			if (tempStrings != null && tempStrings.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				foreach (string s in tempStrings)
				{
					sb.AppendLine(s);
				}

				TMP_MomentaryDebugLogger.text = sb.ToString();
			}
			else
			{
				TMP_MomentaryDebugLogger.text = string.Empty;
			}
		}

		public bool CheckIfKosher()
		{
			bool amKosher = true;

			if (TMP_MomentaryDebugLogger == null)
			{
				amKosher = false;
				Debug.LogError("TMP_MomentaryDebugLogger reference was not set in MomentaryDebugLogger!");
			}

			if (duration_momentaryDebugLoggerMessages <= 0f)
			{
				amKosher = false;
				Debug.LogWarning("<b>'duration_momentaryDebugLoggerMessages'</b> not set inside MomentaryDebugLogger!");
			}

			if (count_maxDebugLogMessages <= 0)
			{
				amKosher = false;
				Debug.LogWarning("<b>'count_maxDebugLogMessages'</b> needs to be greater than 0 inside MomentaryDebugLogger!");
			}

			return amKosher;
		}
	}
}