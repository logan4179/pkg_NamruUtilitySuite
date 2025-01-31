using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NamruPlayerPrefsManager
{
    public class NamruPlayerprefsManager : MonoBehaviour
    {
        public static NamruPlayerprefsManager Instance;

        [Header("STRINGS")]
        public List<string> Keys_string = new List<string>();
        public List<string> DefaultValues_string = new List<string>();
        [HideInInspector] public List<string> Values_string = new List<string>();

        [Header("INTS")]
        public List<string> Keys_int = new List<string>();
        public List<int> DefaultValues_int = new List<int>();
        [HideInInspector] public List<int> Values_int = new List<int>();

        [Header("FLOATS")]
        public List<string> Keys_float = new List<string>();
        public List<float> DefaultValues_float = new List<float>();
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
                        log_protected( $"Key: '{Keys_string[i]}' was NOT saved in playerprefs. Using default value: '{DefaultValues_string[i]}'" );

                        PlayerPrefs.SetString( Keys_string[i], DefaultValues_string[i] );
                    }
                }
            }

            if ( Keys_int != null && Keys_int.Count > 0 )
            {
                Keys_int = new List<string>();
                for ( int i = 0; i < Keys_int.Count; i++ )
                {
                    if ( PlayerPrefs.HasKey(Keys_int[i]) )
                    {
                        Values_int.Add( PlayerPrefs.GetInt(Keys_int[i]) );
                    }
                    else
                    {
                        log_protected( $"Key: '{Keys_int[i]}' was NOT saved in playerprefs. Using default value: '{DefaultValues_int[i]}'" );

                        PlayerPrefs.SetInt( Keys_int[i], DefaultValues_int[i] );
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
                        log_protected($"NPPM: Key: '{Keys_float[i]}' was NOT saved in playerprefs. Using default value: '{DefaultValues_float[i]}'");

                        PlayerPrefs.SetFloat( Keys_float[i], DefaultValues_float[i] );
                    }
                }
            }
        }

        public void SetStringPref( string val, int index )
        {
            if( index > Keys_string.Count - 1 )
            {
                log_protected($"NPPM ERROR! You tried to set a string value at index: '{index}', which went beyond the count of {nameof(Keys_string)}. Returning early...");
                return;
            }

            try
            {
                Values_string[index] = val;
                PlayerPrefs.SetString( Keys_string[index], val );
            }
            catch ( System.Exception e )
            {
                log_protected($"NPPM >> Caught exception of type: '{e.GetType()}' while trying to set playerpref string '{val}'. exception says: \n{e.ToString()}");
                //throw;
            }
        }

        public void SetIntPref( int val, int index)
        {
            if ( index > Keys_int.Count - 1 )
            {
                log_protected($"NPPM ERROR! You tried to set a string value at index: '{index}', which went beyond the count of {nameof(Keys_int)}. Returning early...");
                return;
            }

            try
            {
                Values_int[index] = val;
                PlayerPrefs.SetInt( Keys_int[index], val );
            }
            catch ( System.Exception e )
            {
                log_protected($"NPPM >> Caught exception of type: '{e.GetType()}' while trying to set playerpref int '{val}'. exception says: \n{e.ToString()}");
                //throw;
            }
        }

        public void SetFloattPref( float val, int index )
        {
            if ( index > Keys_float.Count - 1 )
            {
                log_protected($"NPPM ERROR! You tried to set a string value at index: '{index}', which went beyond the count of {nameof(Keys_float)}. Returning early...");
                return;
            }

            try
            {
                Values_float[index] = val;
                PlayerPrefs.SetFloat( Keys_int[index], val );
            }
            catch ( System.Exception e )
            {
                log_protected($"NPPM >> Caught exception of type: '{e.GetType()}' while trying to set playerpref float '{val}'. exception says: \n{e.ToString()}");
                //throw;
            }
        }

        public bool CheckIfKosher()
        {
            if ( 
                (Keys_string != null && DefaultValues_string == null) || 
                (Keys_string.Count != DefaultValues_string.Count)
            )
            {
                log_protected( $"NPPM ERROR! {nameof(Keys_string)} or {nameof(DefaultValues_string)} had an inconsistency." );
                return false;
            }

            if (
                (Keys_int != null && DefaultValues_int == null) ||
                (Keys_int.Count != DefaultValues_int.Count)
            )
            {
                log_protected($"NPPM ERROR! {nameof(Keys_int)} or {nameof(DefaultValues_int)} had an inconsistency.");
                return false;
            }

            if (
                (Keys_float != null && DefaultValues_float == null) ||
                (Keys_float.Count != DefaultValues_float.Count)
            )
            {
                log_protected($"NPPM ERROR! {nameof(Keys_float)} or {nameof(DefaultValues_float)} had an inconsistency.");
                return false;
            }

            return true;
        }

        private void log_protected( string message )
        {
            Event_LogMessage.Invoke( message );
            if ( AmConsoleLoggingMessages )
            {
                Debug.Log( message );
            }
        }
    }
}