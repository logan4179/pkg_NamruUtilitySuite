using System;
using System.IO;
using TMPro;
using UnityEngine;

namespace NamruUtilitySuite
{
	public class LogansVersionDisplayer : MonoBehaviour
	{
		[Header("REFERENCE")]
		[SerializeField] private GameObject group_version_body;
		[SerializeField] private TextMeshProUGUI txt_Version;
		[SerializeField] private TextMeshProUGUI txt_date;
		[SerializeField] private TextMeshProUGUI txt_body;

		[Header("SETTINGS")]
		[Tooltip("The complete filepath for the changelog text file in the project.")] public string FilePath;
		[Tooltip("String that this versioner looks for in order to separate version texts. This should be how version texts start.")] public string VersionSeparatorString = "Version";

		[Header("OTHER")]
		/// <summary>
		/// Each one of these contains the complete version text information. These are split by the 
		/// </summary>
		private string[] allLogs;
		public string[] AllLogs => allLogs;

		private string[] currentLog_split;
		public string[] CurrentLog_split => currentLog_split;


		private string currentLogVersion;
		public string CurrentLogVersion => currentLogVersion;

		private string currentLogDate;
		public string CurrentLogDate => currentLogDate;

		private string currentLogBody;
		public string CurrentLogBody => currentLogBody;

		/*void Start()
		{
			group_version_body.SetActive( false );

			FilePath_changelog = Path.Combine( NamruSessionManager.Instance.DirPath_NamruDirectory, "changelog.txt" );
			TryGetAndParseChangeLogFile();
			CheckIfKosher();
		}*/


		public bool TryGetAndParseChangeLogFile()
		{
			bool result = false;
			try
			{
				if ( File.Exists(FilePath) )
				{
					//Debug.Log( "found changelog file" );
					string allTxt = File.ReadAllText( FilePath );
					//Debug.Log( allTxt );

					allLogs = allTxt.Split( VersionSeparatorString );


					currentLog_split = allLogs[allLogs.Length - 1].Split(Environment.NewLine);

					currentLogVersion = currentLog_split[1];
					txt_Version.text = "V " + currentLogVersion;

					currentLogDate = currentLog_split[2];
					txt_date.text = currentLogDate;

					currentLogBody = string.Empty;
					for (int i = 3; i < currentLog_split.Length; i++)
					{
						currentLogBody += currentLog_split[i] + Environment.NewLine;
					}
					txt_body.text = currentLogBody;

					return true;
				}
			}
			catch ( Exception e )
			{
				Debug.LogError(e);
			}

			return result;
		}

		/// <summary>
		/// Flips the body between active and inactive.
		/// </summary>
		public void FlipBodyActive()
		{
			group_version_body.SetActive(!group_version_body.activeSelf);
		}

		public void SetBodyActiveState( bool b )
		{
			group_version_body.SetActive( b );
		}

		[ContextMenu("z call CheckIfKosher()")]
		public bool CheckIfKosher()
		{
			bool amKosher = true;

			if( group_version_body == null )
			{
				Debug.LogError( $"{nameof(LogansVersionDisplayer)}.{nameof(group_version_body)} reference was null" );
				amKosher = false;
			}

			if ( txt_Version == null )
			{
				Debug.LogError( $"{nameof(LogansVersionDisplayer)}.{nameof(txt_Version)} reference was null" );
				amKosher = false;
			}

			if ( txt_date == null )
			{
				Debug.LogError( $"{nameof(LogansVersionDisplayer)}.{nameof(txt_date)} reference was null" );
				amKosher = false;
			}

			if ( txt_body == null )
			{
				Debug.LogError( $"{nameof(LogansVersionDisplayer)}.{nameof(txt_body)} reference was null" );
				amKosher = false;
			}

			if( amKosher )
			{
				Debug.Log( "<color=green>all was kosher</color>");
			}
			return amKosher;
		}
	}

}
