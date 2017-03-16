using System;
using System.Collections;
using System.Collections.Generic;

namespace ProjectAlice
{
    namespace Utilities
    {
        public class JsonData : IList, IDictionary, IEquatable<JsonData>
        {   
            #region declaration

            public enum JsonType
            {
                Unknown,
                Object,
                Array,
                Boolean,
                String,
                Int,
                Long,
                Double
            }

            #endregion

            #region instances

            private JsonType type;
            private IDictionary<object, JsonData> inst_object;
            private IList<JsonData> inst_array;
            private string inst_string;
            private int inst_int;
            private long inst_long;
            private double inst_double;
            private bool inst_bool;

            #endregion

            #region type operation

            public JsonType GetJsonType()
            {
                return type;
            }

            public bool IsObject()
            {
                return type == JsonType.Object;
            }

            public bool IsArray()
            {
                return type == JsonType.Array;
            }

            public bool IsBoolean()
            {
                return type == JsonType.Boolean;
            }

            public bool IsNull()
            {
                return type == JsonType.Unknown;
            }

            public bool IsString()
            {
                return type == JsonType.String;
            }

            public bool IsInt()
            {
                return type == JsonType.Int;
            }

            public bool IsLong()
            {
                return type == JsonType.Long;
            }

            public bool IsDouble()
            {
                return type == JsonType.Double;
            }

            #endregion

            #region constructors

            public JsonData()
            {
                type = JsonType.Unknown;
            }

            public JsonData( string value )
            {
                type = JsonType.String;
                inst_string = value;
            }

            public JsonData( bool value)
            {
                type = JsonType.Boolean;
                inst_bool = value;
            }

            public JsonData( int value )
            {
                type = JsonType.Int;
                inst_int = value;
            }

            public JsonData( long value )
            {
                type = JsonType.Long;
                inst_long = value;
            }

            public JsonData( double value )
            {
                type = JsonType.Double;
                inst_double = value;
            }

            public JsonData( JsonData other )
            {
                type = other.type;
                inst_object = other.inst_object;
                inst_array = other.inst_array;
                inst_bool = other.inst_bool;
                inst_string = other.inst_string;
                inst_int = other.inst_int;
                inst_long = other.inst_long;
                inst_double = other.inst_double;
            }

            public JsonData( object value )
            {
                if (value == null)
                {
                    type = JsonType.Unknown;
                    return;
                }

                if (value is String)
                {
                    type = JsonType.String;
                    inst_string = (string)value;
                    return;
                }

                if (value is Boolean)
                {
                    type = JsonType.Boolean;
                    inst_bool = (bool)value;
                    return;
                }

                if (value is Int32)
                {
                    type = JsonType.Int;
                    inst_int = (int)value;
                    return;
                }

                if (value is Int64)
                {
                    type = JsonType.Long;
                    inst_long = (long)value;
                    return;
                }

                if (value is Single)
                {
                    type = JsonType.Double;
                    inst_double = (double)(float)value;
                    return;
                }

                if (value is Double )
                {
                    type = JsonType.Double;
                    inst_double = (double)value;
                    return;
                }

                throw new ArgumentException(
                    "Unsupport object type" );
            }

            #endregion

            #region implicit conversions

            public static implicit operator JsonData( String value )
            {
                return new JsonData(value);
            }

            public static implicit operator JsonData( Boolean value )
            {
                return new JsonData(value);
            }

            public static implicit operator JsonData( Int32 value )
            {
                return new JsonData(value);
            }

            public static implicit operator JsonData( Int64 value )
            {
                return new JsonData(value);
            }

            public static implicit operator JsonData( Double value )
            {
                return new JsonData(value);
            }

            #endregion

            #region explicit conversions

            public static explicit operator String( JsonData data )
            {
                if (data.type != JsonType.String)
                {
                    throw new InvalidCastException("JsonData is not string");
                }
                return data.inst_string;
            }

            public static explicit operator Boolean( JsonData data )
            {
                if (data.type != JsonType.Boolean)
                {
                    throw new InvalidCastException("JsonData is not bool");
                }
                return data.inst_bool;
            }

            public static explicit operator Int32( JsonData data )
            {
                if (data.type != JsonType.Int)
                {
                    throw new InvalidCastException("JsonData is not int");
                }
                return data.inst_int;
            }

            public static explicit operator Int64( JsonData data )
            {
                if (data.type != JsonType.Long)
                {
                    throw new InvalidCastException("JsonData is not long");
                }
                return data.inst_long;
            }

            public static explicit operator Double( JsonData data )
            {
                if (data.type != JsonType.Double)
                {
                    throw new InvalidCastException("JsonData is not double");
                }
                return data.inst_double;
            }

            #endregion

            #region IEquatable methods

            public bool Equals( JsonData other )
            {
                if (other == null)
                    return false;

                if (other.type != this.type)
                {
                    return false;
                }

                switch ( type )
                {
                    case JsonType.Unknown:
                        return false;
                    case JsonType.Object:
                        return inst_object.Equals(other.inst_object);
                    case JsonType.Array:
                        return inst_array.Equals(other.inst_array);
                    case JsonType.Boolean:
                        return inst_bool.Equals(other.inst_bool);
                    case JsonType.String:
                        return inst_string.Equals(other.inst_string);
                    case JsonType.Int:
                        return inst_int.Equals(other.inst_int);
                    case JsonType.Long:
                        return inst_long.Equals(other.inst_long);
                    case JsonType.Double:
                        return inst_double.Equals(other.inst_double);
                }

                return false;
            }

            #endregion

            #region IEnumerator methods

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetCollection().GetEnumerator();
            }

            #endregion

            #region ICollection methods

            void ICollection.CopyTo( Array array, int index )
            {
                GetCollection().CopyTo(array, index);
            }

            int ICollection.Count
            {
                get { return GetCollection().Count; }
            }

            bool ICollection.IsSynchronized
            {
                get { return GetCollection().IsSynchronized; }
            }

            object ICollection.SyncRoot
            {
                get { return GetCollection().SyncRoot; }
            }

            #endregion

            #region IDictionary methods

            void IDictionary.Add( object key, object value )
            {
                if (key is String)
                {
                    GetDictionary().Add(key, value);
                }
                else
                {
                    GetDictionary().Add((string)key, value);
                }
            }

            void IDictionary.Remove( object key )
            {
                if (key is string)
                {
                    GetDictionary().Remove(key);
                }
                else
                {
                    GetDictionary().Remove((string)key);
                }
            }

            void IDictionary.Clear()
            {
                GetDictionary().Clear();
            }

            bool IDictionary.Contains( object value )
            {
                return GetDictionary().Contains( value );
            }

            IDictionaryEnumerator IDictionary.GetEnumerator()
            {
                return GetDictionary().GetEnumerator();
            }

            bool IDictionary.IsFixedSize
            {
                get { return GetDictionary().IsFixedSize; }
            }

            bool IDictionary.IsReadOnly
            {
                get { return GetDictionary().IsReadOnly; }
            }

            ICollection IDictionary.Keys
            {
                get { return GetDictionary().Keys; }
            }

            ICollection IDictionary.Values
            {            
                get
                {                
                    GetDictionary();
                    IList<JsonData> values = new List<JsonData>();
                    foreach (KeyValuePair<object, JsonData> pair in inst_object)
                    {
                        values.Add(pair.Value);
                    }

                    return (ICollection)values;
                }
            }

            object IDictionary.this[object key]
            {
                get { return GetDictionary()[key]; }

                set
                {
                    JsonData data = ToJsonData(value);
                    this[(string)key] = data;
                }
            }

            #endregion

            #region IList methods
            int IList.Add( object value )
            {
                return Add(value);
            }

            void IList.Clear()
            {
                GetList().Clear();
            }

            bool IList.Contains( object value )
            {
                return GetList().Contains(value);
            }

            int IList.IndexOf( object value )
            {
                return GetList().IndexOf(value);
            }

            void IList.Insert( int index, object value )
            {
                GetList().Insert(index, value);
            }

            void IList.Remove( object value )
            {
                GetList().Remove(value);
            }

            void IList.RemoveAt( int index )
            {
                GetList().RemoveAt(index);
            }

            bool IList.IsFixedSize
            {
                get { return GetList().IsFixedSize; }
            }

            bool IList.IsReadOnly
            {
                get { return GetList().IsReadOnly; }
            }

            object IList.this[int index]
            {
                get { return GetList()[index]; }

                set
                {
                    GetList();
                    JsonData data = ToJsonData(value);
                    this[index] = data;
                }
            }

            #endregion

            #region interface

            public static JsonData ToJsonData( object value )
            {
                return value != null ? new JsonData(value) : null;
            }

            public void Set( string value )
            {
                type = JsonType.String;
                inst_string = value;
            }

            public void Set( bool value )
            {
                type = JsonType.Boolean;
                inst_bool = value;
            }

            public void Set( int value )
            {
                type = JsonType.Int;
                inst_int = value;
            }

            public void Set( long value )
            {
                type = JsonType.Long;
                inst_long = value;
            }

            public void Set( double value )
            {
                type = JsonType.Double;
                inst_double = value;
            }

            public int Add( object value )
            {
                JsonData data = ToJsonData(value);
                return GetList().Add(data);
            }

            public int Add( JsonData value )
            {
                return GetList().Add(value);
            }

            public void Add( object key, object value )
            {
                JsonData data = ToJsonData( value );
                GetDictionary().Add(key, data);
            }

            public void Add( object key, JsonData value )
            {
                // JsonData data = ToJsonData( value );
                GetDictionary().Add(key, value);
            }

            public string GetString()
            {
                if (type == JsonType.String)
                {
                    return inst_string;
                }

                throw new InvalidOperationException("JsonData is not string");
            }

            public bool GetBool()
            {
                if (type == JsonType.Boolean)
                {
                    return inst_bool;
                }

                throw new InvalidOperationException("JsonData is not bool");
            }

            public int GetInt()
            {
                if (type == JsonType.Int)
                {
                    return inst_int;
                }

                throw new InvalidOperationException("JsonData is not int");
            }

            public long GetLong()
            {
                if (type == JsonType.Long)
                {
                    return inst_long;
                }

                throw new InvalidOperationException("JsonData is not long");
            }

            public double GetDouble()
            {
                if (type == JsonType.Double)
                {
                    return inst_double;
                }

                throw new InvalidOperationException("JsonData is not double");
            }

            public ICollection Keys
            {
                get { return GetDictionary().Keys; }
            }

            public string[] StringKeys
            {
                get
                {
                    string[] keys = new string[Keys.Count];

                    IEnumerable objects = Keys as IEnumerable;
                    int count = 0;
                    foreach (object o in objects)
                    {
                        if (o.GetType().IsAssignableFrom(typeof(string)))
                        {
                            keys[count++] = (string)o;
                        }
                        else
                        {
                            keys[count++] = o.ToString();
                        }
                    }

                    return keys;
                }
            }

            public int Count
            {
                get { return GetCollection().Count; }
            }

            public JsonData this[object key]
            {
                get
                {
                    GetDictionary();
                    JsonData data = null;
                    inst_object.TryGetValue(key, out data);
                    return data;
                }

                set
                {
                    GetDictionary();
                    inst_object[key] = value;
                }
            }

            public JsonData this[int index]
            {
                get
                {                
                    GetList();
                    return inst_array[index];
                }

                set
                {
                    GetList();
                    inst_array[index] = value;
                }
            }

            #endregion

            #region internal methods

            private ICollection GetCollection()
            {
                if (type == JsonType.Object)
                    return (IDictionary)inst_object;
                if (type == JsonType.Array)
                    return (ICollection)inst_array;
                throw new InvalidOperationException(
                    "JsonData is not a collection" );
            }

            private IDictionary GetDictionary()
            {
                if (type == JsonType.Object)
                    return (IDictionary)inst_object;
                if (type == JsonType.Unknown)
                {
                    type = JsonType.Object;
                    inst_object = new Dictionary<object, JsonData>();
                    return (IDictionary)inst_object;
                }

                throw new InvalidOperationException(
                    "JsonData is not a object");
            }

            private IList GetList()
            {
                if (type == JsonType.Array)
                    return (IList)inst_array;
                if (type == JsonType.Unknown)
                {
                    type = JsonType.Array;
                    inst_array = new List<JsonData>();
                    return (IList)inst_array;
                }

                throw new InvalidOperationException(
                    "JsonData is not a array");
            }

            private void Clear()
            {
                if (type == JsonType.Object)
                {
                    inst_object.Clear();
                }
                else if (type == JsonType.Array)
                {
                    inst_array.Clear();
                }
            }

            #endregion
        }
    }
}

