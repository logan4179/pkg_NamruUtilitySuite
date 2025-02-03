using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NamruUtilitySuite
{
    public class NamruPlayerprefsManager : MonoBehaviour
    {
        public static NamruPlayerprefsManager Instance;

        [Header("STRINGS")]
        public List<string> Keys_string = new List<string>();
        [SerializeField] private List<string> defaultValues_string = new List<string>();
        [HideInInspector] public List<string> Values_string = new List<string>();

        [Header("INTS")]
        public List<string> Keys_int = new List<string>();
        [SerializeField] private List<int> defaultValues_int = new List<int>();
        [HideInInspector] public List<int> Values_int = new List<int>();

        [Header("FLOATS")]
        public List<string> Keys_float = new List<string>();
        [SerializeField] private List<float> defaultValues_float = new List<float>();
        [HideInInspector] public List<float> Values_float = new List<float>();

        public MessageEvent Event_LogMessage = new MessageEvent();

        [Header("TRUTH")]
        public bool AmConsoleLoggingMessages = true;

        private void OnEnable()
        {
            Event_LogMessage = new MessageEvent();
        }

        private void OnDisable()
        {
            Event_LogMessage.RemoveAllListeners();
        }

        private void Awake()
        {
            Instance = this;
        }

        public void FetchValues()
        {
            if( !CheckIfKosher() )
            {
                return;
            }

            if ( Keys_string != null && Keys_string.Count > 0 )
            {
                Values_string = new List<string>();
                for ( int i = 0; i < Keys_string.Count; i++ )
                {
                    if ( PlayerPrefs.HasKey(Keys_string[i]) )
                    {
                        Values_string.Add( PlayerPrefs.GetString(Keys_string[i]) );
                    }
                    else
                    {
                        log_protected( $"Key: '{Keys_string[i]}' was NOT saved in playerprefs. Using default value: '{defaultValues_string[i]}'", LogType.Warning );

                        PlayerPrefs.SetString( Keys_string[i], defaultValues_string[i] );
                    }
                }
            }

            if ( Keys_int != null && Keys_int.Count > 0 )
            {
                Values_int = new List<int>();
                for ( int i = 0; i < Keys_int.Count; i++ )
                {
                    if ( PlayerPrefs.HasKey(Keys_int[i]) )
                    {
                        Values_int.Add( PlayerPrefs.GetInt(Keys_int[i]) );
                    }
                    else
                    {
                        log_protected( $"Key: '{Keys_int[i]}' was NOT saved in playerprefs. Using default value: '{defaultValues_int[i]}'", LogType.Warning);

                        PlayerPrefs.SetInt( Keys_int[i], defaultValues_int[i] );
                    }
                }
            }

            if ( Keys_float != null && Keys_float.Count > 0 )
            {
                Values_float = new List<float>();
                for ( int i = 0; i < Keys_float.Count; i++ )
                {
                    if ( PlayerPrefs.HasKey(Keys_float[i]) )
                    {
                        Values_float.Add( PlayerPrefs.GetFloat(Keys_float[i]) );
                    }
                    else
                    {
                        log_protected($"NPPM: Key: '{Keys_float[i]}' was NOT saved in playerprefs. Using default value: '{defaultValues_float[i]}'", LogType.Warning);

                        PlayerPrefs.SetFloat( Keys_float[i], defaultValues_float[i] );
                    }
                }
            }
        }

        public void SetStringPref( string val, int index )
        {
            if( index > Keys_string.Count - 1 )
            {
                log_protected($"NPPM ERROR! You tried to set a string value at index: '{index}', which went beyond the count of {nameof(Keys_string)}. Returning early...", LogType.Error );
                return;
            }

            try
            {
                Values_string[index] = val;
                PlayerPrefs.SetString( Keys_string[index], val );
            }
            catch ( System.Exception e )
            {
                log_protected($"NPPM >> Caught exception of type: '{e.GetType()}' while trying to set playerpref string '{val}'. exception says: \n{e.ToString()}", LogType.Exception);
                //throw;
            }
        }

        public void SetIntPref( int val, int index)
        {
            if ( index > Keys_int.Count - 1 )
            {
                log_protected($"NPPM ERROR! You tried to set a string value at index: '{index}', which went beyond the count of {nameof(Keys_int)}. Returning early...", LogType.Error );
                return;
            }

            try
            {
                Values_int[index] = val;
                PlayerPrefs.SetInt( Keys_int[index], val );
            }
            catch ( System.Exception e )
            {
                log_protected($"NPPM >> Caught exception of type: '{e.GetType()}' while trying to set playerpref int '{val}'. exception says: \n{e.ToString()}", LogType.Exception );
                //throw;
            }
        }

        public void SetFloattPref( float val, int indx )
        {
            if ( indx > Keys_float.Count - 1 )
            {
                log_protected($"NPPM ERROR! You tried to set a string value at index: '{indx}', which went beyond the count of {nameof(Keys_float)}. Returning early...", LogType.Error );
                return;
            }

            try
            {
                Values_float[indx] = val;
                PlayerPrefs.SetFloat( Keys_float[indx], val );
            }
            catch ( System.Exception e )
            {
                log_protected( $"NPPM >> Caught exception of type: '{e.GetType()}' while trying to set playerpref float '{val}', with index: '{indx}'. exception says: \n{e.ToString()}", LogType.Exception );
                //throw;
            }
        }

        public bool CheckIfKosher()
        {
            if ( 
                (Keys_string != null && defaultValues_string == null) || 
                (Keys_string.Count != defaultValues_string.Count)
            )
            {
                log_protected( $"NPPM ERROR! {nameof(Keys_string)} or {nameof(defaultValues_string)} had an inconsistency.", LogType.Error );
                return false;
            }

            if (
                (Keys_int != null && defaultValues_int == null) ||
                (Keys_int.Count != defaultValues_int.Count)
            )
            {
                log_protected($"NPPM ERROR! {nameof(Keys_int)} or {nameof(defaultValues_int)} had an inconsistency.", LogType.Error );
                return false;
            }

            if (
                (Keys_float != null && defaultValues_float == null) ||
                (Keys_float.Count != defaultValues_float.Count)
            )
            {
                log_protected($"NPPM ERROR! {nameof(Keys_float)} or {nameof(defaultValues_float)} had an inconsistency.", LogType.Error );
                return false;
            }

            return true;
        }

        private void log_protected( string message, LogType logtype )
        {
            Event_LogMessage.Invoke( message, logtype );
            if ( AmConsoleLoggingMessages )
            {
                Debug.Log( message );
            }
        }
    }
}