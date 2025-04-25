using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine.Events;

namespace NamruUtilitySuite
{
	public class NamruSessionManager : MonoBehaviour
	{
		public static NamruSessionManager Instance;
		/// <summary>
		/// This is a useful truth variable for only doing certain operations if we're before or after the first singleton load of this class.
		/// </summary>
		private bool amPastFirstLoad = false;
		/// <summary>
		/// This is a useful truth variable for only doing certain operations if we're before or after the first singleton load of this class.
		/// </summary>
		public bool AmPastFirstLoad => amPastFirstLoad;

		[Header("[----------- FILE ----------]")]
		private string participantID = "Unnamed"; //"Unnamed" is the default that will persist if not explicitely set to something different.
		public string ParticipantID => participantID;

		/// <summary>
		/// The session part of the full filename string 
		/// </summary>
		private string sessionString;

		[SerializeField, Tooltip("This will be the name of the folder created to store all output data")] 
		private string namruDirectoryName = "NAMRU Data";
		/// <summary>
		/// This will be the path to the folder will all namru-related files will be.  This will include the changelog file, any ini files, and any session output I want to write.
		/// Will be "[Root data folder]/Namru Data/"
		/// </summary>
		private string dirPath_NamruDirectory;
		public string DirPath_NamruDirectory => dirPath_NamruDirectory;

		/// <summary>
		/// Points to a directory that houses all trial data for all participants. Will be "[Root data folder]/Namru Data/TrialResults/"
		/// </summary>
		private string dirPath_TrialResultsDirectory;

		/// <summary>
		/// Points to a directory designated for the current session. Will be "[Root data folder]/Namru Data/TrialResults/[Participant ID]"
		/// </summary>
		private string dirPath_CurrentSessionDirectory;

		[Header("[----------- INI ----------]")]
		private string filePath_ini;
		[SerializeField, Tooltip("These will ideally be read in from a succesful ini read, but also should be populated with default values from the inspector in case the read isn't succesful")]
		public string[] IniValues;

		/// <summary>
		/// Data from the trial results of the current participant for the current session. This is the "final-most" filepath that exists. Ultimately where the session data will log to. 
		/// "will be {ParticipantID}_{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}_session"
		/// </summary>
		private string filePath_sessionTrialResults;
		/// <summary>
		/// Data for the logs from the current session.
		/// </summary>
		private string filePath_log;

		[Header("[----------- WRITERS ----------]")]
		private StreamWriter streamWriter_log;
		private StreamWriter streamWriter_trialResults;

		[Header("[----------- SESSION DATA ----------]")]
		[Tooltip("Makes the format for the csv file headers for the output for this session")]
		public List<string> DataHeaders;
		//[SerializeField] TrialResultsMode trialResultsMode;
		[SerializeField] TrialResultsFileFormat trialResultsFormat;

		[Header("[----------- TRUTH ----------]")]
		[Tooltip("Allows independent control over whether this manager will output trial results for the current session")]
		public bool AmOutputtingTrialResults = true;
		[Tooltip("Allows independent control over whether this manager will output logs for the current session")]
		public bool AmDoingLog = true;
		[Tooltip("Allows independent control over whether this manager will bother with ini checking/setup/reading for the current session")]
		public bool AmUsingIni = true;
		[Tooltip("Allows independent control over whether this manager will start a UDP listener for the current session")]
		public bool AmListeningViaUDP = true;
		[Tooltip("Allows independent control over whether this manager will start a UDPFennec instance for the current session")]
		public bool AmFennecSending = true;

		#region UDP --------------------------------------------------------------
		[Header("[----------- UDP LISTENING ----------]")]
		public int port = 11000;
		UdpClient udpClient;
		IPEndPoint ipEndPt;
		private string receiveString;
		public string ReceiveString => receiveString;
		private byte[] receive_byte_array;
		private Thread listeningThread;
		private bool continueListeningThread = false;
		#endregion

		[Header("[----------- FENNEC SENDING ----------]")]
		private UDPFennec fennecSender;
		public string FennecIPAddress = "127.0.0.1";

		//[Header("[----------- FLAGS ----------]")]
		/// <summary>
		/// This is a flag that gets set when you decide to close the app. This is a fail-safe so it doesn't try to write to the changelog after a certain point in the process of closing.
		/// </summary>
		bool flag_writersShouldBeDisposed = false;

		private bool flag_haveSuccesfullyFoundOrCreatedNamruDirectory = false;

		/// <summary>
		/// This is a flag that gets set when attempt is made to load the ini file with TryToLoadIni(). Note: It only get set to true if the ini file is present in the correct location, and is 
		/// of the correct length. It doesn't validate the individual values. You should do this with an external script you make.
		/// </summary>
		private bool flag_haveSuccesfullyReadAndParsedValidIniFile = false;
		/// <summary>
		/// This is a flag that gets set when attempt is made to load the ini file with TryToLoadIni(). Note: It only get set to true if the ini file is present in the correct location, and is 
		/// of the correct length. It doesn't validate the individual values. You should do this with an external script you make.
		/// </summary>
		public bool Flag_HaveSuccesfullyReadAndParsedValidIniFile => flag_haveSuccesfullyReadAndParsedValidIniFile;

		private bool flag_haveSuccesfullyFoundOrCreatedTrialResultsDirectory = false;
		private bool flag_haveSuccesfullyCreatedTrialResultsFile = false;

		private bool flag_haveSuccesfullyFoundOrCreatedCurrentSessionDirectory = false;
        public bool Flag_haveSuccesfullyFoundOrCreatedCurrentSessionDirectory => flag_haveSuccesfullyFoundOrCreatedCurrentSessionDirectory;

        private bool flag_haveSuccesfullyCreatedSessionFile = false;

		private bool flag_haveSuccesfullyFoundOrCreatedLogFile = false;

		public MessageEvent Event_RecievedUDPString;

		[SerializeField] private string DBG_Class;

        /// <summary>Keeps track of the latest debug message. Use this in the host program to get more information about 
        /// what has happened in this manager.</summary>
        private string dbgReport;
		/// <summary>Keeps track of the latest debug message. Use this in the host program to get more information about 
		/// what has happened in this manager.</summary>
		public string DebugReport => dbgReport;

		[Header("[----------- FLAGS ----------]")]
		private bool flag_amClosing = false;

        private void OnEnable()
		{
			Event_RecievedUDPString = new MessageEvent();
		}

		private void OnDisable()
		{
			Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(OnDisable)}().");

			if( !flag_amClosing )
			{
				CloseMe();
			}

			Event_RecievedUDPString.RemoveAllListeners();
		}

		private void Awake()
		{
			//Note: Can't log until this awake is finished...

			DontDestroyOnLoad(this);

			if ( Instance == null )
			{
				NamruLogManager.Log( $"{nameof(Instance)} was null. Setting singleton reference to this...", LogDestination.Console );

				Instance = this;
			}
			else if (Instance != this)
			{
				Destroy(gameObject);
			}

			if( !AmPastFirstLoad )
			{
				NamruLogManager.Log( $"Not past first load. Setting up paths, directories, and writers...", LogDestination.Console );
				InitializePreliminaryPaths();
				TryToFindOrCreateNamruDirectory();

				if ( AmDoingLog )
				{
					TryToStartLogFileWriter();
				}

				if ( AmUsingIni )
				{
					TryToLoadIni();
				}

				if ( AmFennecSending)
				{
					StartFennec();
				}

				if( AmOutputtingTrialResults )
				{
					TryToFindOrCreateTrialResultsDirectory();
				}
			}
		}

		private void Start()
		{
			NamruLogManager.Log( $"[{nameof(NamruSessionManager)}].Start()", LogDestination.Hidden );
			NamruLogManager.IncrementTabLevel();

			amPastFirstLoad = true;

			flag_amClosing = false;

			MakeDebugFileString();

			NamruLogManager.Log($"End of [{nameof(NamruSessionManager)}].Start()...");
			NamruLogManager.DecrementTabLevel();
		}

		/// <summary>
		/// This initializes only the necessary path strings, such as the root namru directory path, the trial results path, the ini filepath and the log filepath. This is for all 
		/// the paths that are NOT specific to the session or individual participant.
		/// </summary>
		public void InitializePreliminaryPaths()
		{
			//Decided not to log this because currently this needs to complete before logs can happen.

			if ( Application.isEditor )
			{
				dirPath_NamruDirectory = namruDirectoryName; //will be one level outside of Assets
			}
			else
			{
				dirPath_NamruDirectory = Path.Combine( Application.dataPath, namruDirectoryName ); //Will be inside a folder inside the build folder with "_Data" at the end.
			}

			dirPath_TrialResultsDirectory = Path.Combine( dirPath_NamruDirectory, "TrialResults" );
			filePath_ini = Path.Combine( dirPath_NamruDirectory, "ini.txt" );
			filePath_log = Path.Combine( dirPath_NamruDirectory, "log.txt" );

		}

		public void TryToLoadIni()
		{
			NamruLogManager.Log( $"{nameof(TryToLoadIni)}()", LogDestination.Hidden );
			NamruLogManager.IncrementTabLevel();

			try
			{
				if ( File.Exists(filePath_ini) )
				{
					NamruLogManager.Log($"Found ini file at default path. Attempting to laod ini file...");
					string[] rslts = File.ReadAllLines(filePath_ini);

					if ( rslts.Length <= 0 )
					{
						NamruLogManager.LogError($"ini file only returned '{rslts.Length}' lines. Can't read ini file...");
						return;
					}
					else if ( rslts.Length < IniValues.Length )
					{
						NamruLogManager.LogError($"ini file only returned '{rslts.Length}' lines. This is different from the amount of ini values expected. Using default settings...");
						return;
					}
					else
					{
						IniValues = rslts;
						flag_haveSuccesfullyReadAndParsedValidIniFile = true;
					}
				}
				else
				{
					NamruLogManager.Log( $"ini file at path: '{filePath_ini}' does not exist. Using default settings.", LogDestination.Everywhere );
				}
			}
			catch ( Exception e )
			{
				NamruLogManager.LogException( e );
			}

			NamruLogManager.DecrementTabLevel();
		}

        #region DIRECTORY CREATION ---------------------------------------------------------------------
        /// <summary>
        /// Takes in a path string, checks if a directory exists at it's path, and if not, tries to create a directory.
        /// </summary>
        /// <param name="dirPath_passed"></param>
        /// <returns>true if it is succesful at either finding or creating a directory at the supplied path. False if not succesful at one of these.</returns>
        private bool TryToFindOrCreateDirectory( string dirPath_passed )
		{
			NamruLogManager.Log($"{nameof(TryToFindOrCreateDirectory)}({nameof(dirPath_passed)}: '{dirPath_passed}')", 
				LogDestination.Hidden);

			if ( Directory.Exists(dirPath_passed) )
			{
				NamruLogManager.Log( $"DID find already existing directory at: '{dirPath_passed}'.", LogDestination.Console );
				NamruLogManager.DecrementTabLevel();
				return true;
			}
			else
			{
				NamruLogManager.Log( $"Did NOT find '{nameof(dirPath_passed)}' creating directory...", LogDestination.Console );

				try
				{
					Directory.CreateDirectory(dirPath_passed);
					NamruLogManager.Log( $"Succesfully created directory at: '{dirPath_passed}'" );
				}
				catch ( Exception e )
				{
					NamruLogManager.LogException( e );
					NamruLogManager.DecrementTabLevel();
					return false;
				}
			}

			return true;
		}

		public void TryToFindOrCreateNamruDirectory()
		{
			NamruLogManager.Log($"{nameof(TryToFindOrCreateNamruDirectory)}({nameof(dirPath_NamruDirectory)}: '{dirPath_NamruDirectory}')", 
				LogDestination.Hidden );

			flag_haveSuccesfullyFoundOrCreatedNamruDirectory = TryToFindOrCreateDirectory( dirPath_NamruDirectory );

		}

		public void TryToFindOrCreateTrialResultsDirectory()
		{
			NamruLogManager.Log($"{nameof(TryToFindOrCreateTrialResultsDirectory)}({nameof(dirPath_TrialResultsDirectory)}: '{dirPath_TrialResultsDirectory}')",
				LogDestination.Hidden );
            NamruLogManager.IncrementTabLevel();

            flag_haveSuccesfullyFoundOrCreatedTrialResultsDirectory = TryToFindOrCreateDirectory(dirPath_TrialResultsDirectory);

            NamruLogManager.DecrementTabLevel();
        }

        public void TryToFindOrCreateCurrentSessionDirectory()
        {
            NamruLogManager.Log($"{nameof(TryToFindOrCreateCurrentSessionDirectory)}({nameof(dirPath_CurrentSessionDirectory)}: '{dirPath_CurrentSessionDirectory}')",
                LogDestination.Hidden);
            NamruLogManager.IncrementTabLevel();

            flag_haveSuccesfullyFoundOrCreatedCurrentSessionDirectory = TryToFindOrCreateDirectory( dirPath_CurrentSessionDirectory );

            NamruLogManager.DecrementTabLevel();
        }
        #endregion

        /// <summary>
        /// Call this method when the participant ID is set through the host program. Creates the full derived paths of the 
        /// current session directory and trial results, and creates the current session directory.
        /// </summary>
        /// <param name="prtcipntIdString"></param>
        /// <returns></returns>
        public bool SetParticipantId_action( string prtcipntIdString )
		{
			NamruLogManager.Log( $"{nameof(SetParticipantId_action)}({prtcipntIdString})", LogDestination.Hidden );

			participantID = prtcipntIdString;

			if ( string.IsNullOrEmpty(participantID) )
			{
				NamruLogManager.LogError($"supplied participant id was empty. Cannot calculate a session filepath without a valid participant ID. Returning early...");
				return false;
			}

			dirPath_CurrentSessionDirectory = Path.Combine( dirPath_TrialResultsDirectory, participantID );

            sessionString = $"{participantID}_{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}_session";
			int sessionNumb = 1;

			if ( Directory.Exists(dirPath_CurrentSessionDirectory) )
			{
				NamruLogManager.Log($"Directory '{dirPath_CurrentSessionDirectory}' already existed. Using this directory for trial data. Now attempting to generate a unique session string...");

				string tryPath = Path.Combine( dirPath_CurrentSessionDirectory, $"{sessionString}{sessionNumb}.{trialResultsFormat}" );

                if ( File.Exists(tryPath) )
				{
					NamruLogManager.Log($"file at '{tryPath}' already existed. Trying to find a unique trial number for this session...");

                    for ( int i = 0; i < 100; i++ )
					{
                        sessionNumb++;

                        if ( sessionNumb < 100 )
                        {
							tryPath = Path.Combine( dirPath_CurrentSessionDirectory, $"{sessionString}{sessionNumb}.{trialResultsFormat}" );

                            if ( !File.Exists(tryPath) )
							{
								NamruLogManager.Log( $"Found unique path at: '{tryPath}'" );
								break;
							}
                        }
						else
						{
                            NamruLogManager.LogError($"Found too many sessions for user on current day. Assuming infinite loop. Breaking early to prevent crash...");
                            return false;
						}
                    }
                }
			}
			else
			{
				NamruLogManager.Log( $"Directory '{dirPath_CurrentSessionDirectory}' did NOT already exist...", LogDestination.Console );
            }

            sessionString += sessionNumb;
            filePath_sessionTrialResults = Path.Combine( dirPath_CurrentSessionDirectory, sessionString + $".{trialResultsFormat}");

            TryToFindOrCreateCurrentSessionDirectory();

            NamruLogManager.Log($"Succesfully created session filepath string of: '{filePath_sessionTrialResults}.");
			
			return flag_haveSuccesfullyFoundOrCreatedCurrentSessionDirectory;
		}

        public bool TryToStartLogFileWriter()
        {
            NamruLogManager.Log( $"{nameof(TryToStartLogFileWriter)}()", LogDestination.Console );
            NamruLogManager.IncrementTabLevel();

			if ( !flag_haveSuccesfullyFoundOrCreatedNamruDirectory )
			{
				NamruLogManager.LogError( $"Never created namru directory for saving log. Returning early...");
                NamruLogManager.DecrementTabLevel();

                return false;
			}

            NamruLogManager.Log($"trying to create log at filepath: '{filePath_log}'...");
            try
            {
                streamWriter_log = new StreamWriter(filePath_log);
                streamWriter_log.WriteLine($"Created log at: '{DateTime.Now}'");

                //File.WriteAllText( FilePath_log, $"Created log at: '{System.DateTime.Now}'" );
                flag_haveSuccesfullyFoundOrCreatedLogFile = true;

            }
            catch ( Exception e )
            {
                NamruLogManager.LogException( e );
                NamruLogManager.DecrementTabLevel();

                return false;
            }

            NamruLogManager.DecrementTabLevel();

			return true;
        }

        /// <summary>
        /// Attempts to create the trial results file, then opens a streamwriter for writing to this file.
		/// You should manually call this in the host program, probably in the start/settings scene before 
		/// moving on to the task.
        /// </summary>
        /// <returns>true if file create and stream create was succesful, false if not.</returns>
        public bool TryToStartTrialResultsWriting()
        {
            NamruLogManager.Log( $"{nameof(TryToStartTrialResultsWriting)}()", LogDestination.Console );
            NamruLogManager.IncrementTabLevel();

			if ( !flag_haveSuccesfullyFoundOrCreatedTrialResultsDirectory )
			{
				dbgReport = $"Cannot write to trial results because trial results directory was never succesfully found or created.";
				NamruLogManager.LogError( dbgReport );
                NamruLogManager.DecrementTabLevel();
                return false;
			}

            if ( !flag_haveSuccesfullyFoundOrCreatedCurrentSessionDirectory )
            {
                dbgReport = $"Cannot write to trial results because current session directory was never succesfully found or created.";
                NamruLogManager.LogError(dbgReport);
                NamruLogManager.DecrementTabLevel();
                return false;
            }

            NamruLogManager.Log( $"trying to create trial results at filepath: '{filePath_sessionTrialResults}'...", LogDestination.Console );
            try
            {
				if ( !flag_haveSuccesfullyCreatedSessionFile )
				{
					NamruLogManager.Log($"trying to create session file at: '{filePath_sessionTrialResults}'...");
                    //File.Create( filePath_sessionTrialResults );
					flag_haveSuccesfullyCreatedSessionFile = true;
                }

				NamruLogManager.Log($"created session file.");

				
                streamWriter_trialResults = new StreamWriter(filePath_sessionTrialResults);

                if ( DataHeaders.Count > 0 )
                {
                    string headerString = "";
					for( int i = 0; i < DataHeaders.Count; i++ )
					{
						headerString += DataHeaders[i];
						if( i < DataHeaders.Count - 1 )
						{
							headerString += ",";
						}
					}

					NamruLogManager.Log($"Found there were headers. Writing headers to line...", LogDestination.Console );
					streamWriter_trialResults.WriteLine( headerString );
                    NamruLogManager.Log($"Wrote headers to line...", LogDestination.Console);

                }
                else
				{
					NamruLogManager.Log($"{nameof(DataHeaders)} was blank, so not writing initial line of dataheaders...", LogDestination.Console );
				}

                NamruLogManager.Log($"Created trial results at: '{System.DateTime.Now}'");
                flag_haveSuccesfullyCreatedTrialResultsFile = true;

            }
            catch ( Exception e )
            {
				dbgReport = e.ToString();
                NamruLogManager.LogException(e);
                NamruLogManager.DecrementTabLevel();
                return false;
            }

            NamruLogManager.DecrementTabLevel();

			return true;
        }

        public void WriteToLogFile(string msg)
		{
			if ( AmDoingLog && flag_haveSuccesfullyFoundOrCreatedLogFile && !flag_writersShouldBeDisposed && streamWriter_log != null )
			{
				streamWriter_log.WriteLine(msg);
			}
		}

		/// <summary>
		/// Use this to write trial event data in the moment.
		/// </summary>
		/// <param name="s"></param>
		public void WriteToTrialResults(string s)
		{
			NamruLogManager.Log($"{nameof(NamruSessionManager)}.{nameof(WriteToTrialResults)}('{s}')");

			if ( AmOutputtingTrialResults && flag_haveSuccesfullyFoundOrCreatedTrialResultsDirectory )
			{
				if( streamWriter_trialResults != null )
				{
					streamWriter_trialResults.WriteLine( s );
				}
				else
				{
					NamruLogManager.LogError( $"StreamWriter is closed, can't write to file. Attempting to write to error log..." );
				}
			}
		}

        #region SENDING/RECIEVING ------------------------------------------------------------------------
        public bool StartUDPListening()
		{
			NamruLogManager.Log( $"{nameof(StartUDPListening)}()", LogDestination.Hidden );
			// UDP init stuff-----------------
			int attemptCount = 0;
			bool isConnected = false;
			while (!isConnected)
			{
				try
				{
					udpClient = new UdpClient(port);
					ipEndPt = new IPEndPoint(IPAddress.Any, 0);

					isConnected = true;
				}
				catch (SocketException e)
				{
					attemptCount++;

					if (attemptCount == 10)
					{
						NamruLogManager.LogException( e );
						return false;
					}
				}
			}

			NamruLogManager.Log($"UDP client initialization did not error. Now initializing listener thread...");
			listeningThread = new Thread(new ThreadStart(ListenForUDPPacket_action));
			listeningThread.IsBackground = true;
			continueListeningThread = true;
			listeningThread.Start();

			NamruLogManager.Log($"{nameof(StartUDPListening)}() end. Returning true...", LogDestination.Hidden);
			return true;
		}

		public void ListenForUDPPacket_action()
		{
			while ( continueListeningThread )
			{
				try
				{
					receive_byte_array = udpClient.Receive(ref ipEndPt); //I believe that the event-like behavior of this is because this line blocks execution until it recieves something
					receiveString = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);

					if ( !string.IsNullOrEmpty(receiveString) )
					{
						Event_RecievedUDPString.Invoke( receiveString, LogType.Log );
					}
				}
				catch (SocketException)
				{
					NamruLogManager.LogWarning($"socket exception caught", true);
				}
				catch (Exception e)
				{
					NamruLogManager.LogError($"Got exception of type: '{e.GetType()}' attempting to listen to UDP. Exception says: '{e}'", true);
				}
			}
		}

		public void StartFennec()
		{
			NamruLogManager.Log($"{nameof(StartFennec)}()", LogDestination.Hidden);

			try
			{
				fennecSender = new UDPFennec(FennecIPAddress);

			}
			catch ( Exception e )
			{

				NamruLogManager.LogError( $"Caught exception of type: '{e.GetType()}' trying to initialize a fennec sender. Exception says: " + e.ToString() );
			}

		}

		public void SendToFennec(string name, double value, bool sendData = false)
		{
			//NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(SendToFennec)}('{name}', '{value}')" );
			fennecSender.AddData(name, value);

			if (sendData)
			{
				fennecSender.SendData();
			}

			//NAMRU_Debug.Log( $"{nameof(NamruSessionManager)}.{nameof(SendToFennec)}()" );
		}
        #endregion

        #region CLOSING --------------------------------------------------------------------------
        [ContextMenu("z call CloseMe()")]
		public void CloseMe()
		{
			NamruLogManager.Log($"{nameof(CloseMe)}()", LogDestination.Console );

			if( flag_amClosing )
			{
				NamruLogManager.Log( $"Decided already closing. Will not proceed with method. Returning early...", LogDestination.Console );
				return;
			}

			CloseUDPConnection();

			DisposeAllWriters();

			flag_amClosing = true;
			
		}

		[ContextMenu("z call DisposeAllWriters()")]
		public void DisposeAllWriters()
		{
			NamruLogManager.Log( $"{nameof(DisposeAllWriters)}()", LogDestination.Console );
			int spot = 0;

			flag_writersShouldBeDisposed = true;
			spot = 1;

			try
			{
				spot = 2;
				if ( streamWriter_log != null )
				{
					NamruLogManager.Log($"{nameof(streamWriter_log)} was not null. Disposing...", LogDestination.Console);
					streamWriter_log.Dispose(); //explicitely calling this because I'm keeping a persistant logwriter variable, vs using it in an auto-managed using statement.
					streamWriter_log = null; //I think the docs say if you're going to call dispose, you need to do this to truly release it.
				}
				else
				{
					NamruLogManager.Log($"{nameof(streamWriter_log)} was null...", LogDestination.Console);
				}
				spot = 3;

				if ( streamWriter_trialResults != null )
				{
					NamruLogManager.Log($"{nameof(streamWriter_trialResults)} was not null. Disposing...", LogDestination.Console);
					streamWriter_trialResults.Dispose(); //explicitely calling this because I'm keeping a persistant logwriter variable, vs using it in an auto-managed using statement.
					streamWriter_trialResults = null; //I think the docs say if you're going to call dispose, you need to do this to truly release it.
				}
				else
				{
					NamruLogManager.Log($"{nameof(streamWriter_trialResults)} was null...", LogDestination.Console );
				}
				spot = 4;
			}
			catch ( Exception e )
			{
				NamruLogManager.LogError($"Got exceptipon attempting to dispose writers after spot: '{spot}'. Exception says:");
				NamruLogManager.LogError(e.ToString());
			}
		}

		public void CloseUDPConnection()
		{
			NamruLogManager.Log($"{nameof(CloseUDPConnection)}()");
			continueListeningThread = false;

			if (udpClient != null)
			{
				try
				{
					udpClient.Close();
				}
				catch ( SocketException )
				{
					NamruLogManager.LogWarning($"socket exception caught");
				}
				catch ( Exception e )
				{
					NamruLogManager.LogError("Got exceptipon attempting to close udp listener. Exception says:");
					NamruLogManager.Log(e.ToString());
				}
			}
			else
			{
				NamruLogManager.Log($"Was going to close {nameof(udpClient)}, but found it was already null...");
			}
		}

        #endregion

        private void MakeDebugFileString()
		{
			DBG_Class = $"{nameof(amPastFirstLoad)}: '{amPastFirstLoad}'\n" +
				$"{nameof(participantID)}: '{participantID}'\n" +
					$"{nameof(sessionString)}: '{sessionString}'\n" +

					"\n[---- Directory stuff ----]\n" +
					$"{nameof(dirPath_NamruDirectory)}: '{dirPath_NamruDirectory}'\n" +
					$"{nameof(dirPath_TrialResultsDirectory)}: '{dirPath_TrialResultsDirectory}'\n" +
					$"{nameof(dirPath_CurrentSessionDirectory)}: '{dirPath_CurrentSessionDirectory}'\n" +

					"\n[---- File Paths ----]\n" +
					$"{nameof(filePath_ini)}: '{filePath_ini}'\n" +
					$"{nameof(filePath_sessionTrialResults)}: '{filePath_sessionTrialResults}'\n" +
					$"{nameof(filePath_log)}: '{filePath_log}'\n" +

					"\n[---- Flags ----]\n" +
					$"{nameof(flag_haveSuccesfullyFoundOrCreatedNamruDirectory)}: '{flag_haveSuccesfullyFoundOrCreatedNamruDirectory}'\n" +
					$"{nameof(flag_haveSuccesfullyReadAndParsedValidIniFile)}: '{flag_haveSuccesfullyReadAndParsedValidIniFile}'\n" +
					$"{nameof(flag_haveSuccesfullyFoundOrCreatedTrialResultsDirectory)}: '{flag_haveSuccesfullyFoundOrCreatedTrialResultsDirectory}'\n" +
					$"{nameof(flag_haveSuccesfullyCreatedTrialResultsFile)}: '{flag_haveSuccesfullyCreatedTrialResultsFile}'\n" +
					$"{nameof(flag_haveSuccesfullyFoundOrCreatedCurrentSessionDirectory)}: '{flag_haveSuccesfullyFoundOrCreatedCurrentSessionDirectory}'\n" +
					$"{nameof(flag_haveSuccesfullyCreatedSessionFile)}: '{flag_haveSuccesfullyCreatedSessionFile}'\n" +
					$"{nameof(flag_haveSuccesfullyFoundOrCreatedLogFile)}: '{flag_haveSuccesfullyFoundOrCreatedLogFile}'\n" +
				$"";

		}
	}
}
