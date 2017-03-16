using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ProjectAlice
{
    namespace Utilities
    {
        [AttributeUsage( AttributeTargets.Field, AllowMultiple = false )]
        public class ProfileKey : Attribute
        {
        }

        [Serialize]
        public class Profile
        {
            public static object GetKey( object obj )
            {
                BindingFlags flag = BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                FieldInfo[] fields = obj.GetType().GetFields( flag );
                for (int i = 0; i < fields.Length; ++i)
                {
                    object[] attributes = fields[i].GetCustomAttributes(true);
                    for (int j = 0; j < attributes.Length; ++j)
                    {
                        if (attributes[j] is ProfileKey)
                        {
                            return fields[i].GetValue( obj );
                        }
                    }
                }

                return null;
            }
        }

        public class ProfileManager<T> where T : Profile
        {        
            #region fields
            string cachedPath;
            IDictionary<object, T> profiles;
            #endregion

            public object[] Keys { get { return profiles.Keys as object[]; } }

            public static ProfileManager<T> LoadProfile( string path )
            {
                if( Path.GetExtension(path) != "json" )
                {
                    path = Path.GetFullPath( path ) + ".json";
                    Debug.Log(string.Format("Open profile at {0}", path));
                }

                FileStream stream = null;
                StreamReader reader = null;
                try
                {
                    stream = new FileStream(path, FileMode.OpenOrCreate);
                    reader = new StreamReader(stream);
                }
                catch
                {
                    Console.WriteLine(string.Format("Open profile {0} failed", path ));
                }

                ProfileManager<T> manager = new ProfileManager<T>();
                if (reader != null)
                {
                    JsonData data = JsonMapper.ToJsonData(reader);
                    manager.profiles = ObjectMapper.ToObject<Dictionary<object, T>>(data, true);
                    manager.cachedPath = path;

                    reader.Close();
                    stream.Close();
                }

                if (manager.profiles == null)
                {
                    manager.profiles = new Dictionary<object, T>();
                }

                return manager;
            }

            public static void SaveProfile( ProfileManager<T> manager, string path = null )
            {
                if( manager == null )
                {
                    return;
                }

                if ( string.IsNullOrEmpty(path))
                {
                    path = manager.cachedPath;
                }

                if (!string.IsNullOrEmpty(path))
                {
                    FileStream stream = new FileStream(path, FileMode.Create );
                    StreamWriter writer = new StreamWriter(stream);
                    JsonData data = ObjectMapper.ToJsonData( manager.profiles, true );
                    JsonMapper.ToJson(data, writer);

                    writer.Close();
                    stream.Close();
                }
            }

            public static object GetKey( Profile profile )
            {
                BindingFlags flag = BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                FieldInfo[] fields = profile.GetType().GetFields( flag );
                for (int i = 0; i < fields.Length; ++i)
                {
                    object[] attributes = fields[i].GetCustomAttributes(true);
                    for (int j = 0; j < attributes.Length; ++j)
                    {
                        if (attributes[j] is ProfileKey)
                        {
                            return fields[i].GetValue( profile );
                        }
                    }
                }

                return null;
            }

            public void AddProfile( T profile )
            {
                if( profile != null )
                {
                    object key = Profile.GetKey(profile);

                    if (key != null)
                    {
                        if (!profiles.ContainsKey(key))
                        {
                            profiles.Add(key, profile);
                        }
                    }
                }
            }

            public void RemoveProfile( object key )
            {
                if (key != null)
                {
                    profiles.Remove(key);
                }
            }

            public T GetProfile( object key )
            {
                T profile = null;
                profiles.TryGetValue( key, out profile );
                return profile;
            }

            public T GetOrCreateProfile( object key, T defaultProfile )
            {
                T profile = GetProfile(key);
                if (profile == null)
                {
                    profile = defaultProfile;
                    AddProfile(profile);
                }
                return profile;
            }
        }
    }
}