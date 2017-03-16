using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace ProjectAlice
{
    namespace Utilities
    {
        [AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true )]
        public class Serialize : Attribute
        {
            public bool DefaultOption = true;
        }

        [AttributeUsage( AttributeTargets.Field )]
        public class SerializeField : Attribute
        {
        }

        [AttributeUsage( AttributeTargets.Field )]
        public class NonSerializeField : Attribute
        {
        }

        public class ObjectMapper
        {
            #region declarations
            public delegate JsonData ExportFunc( object obj );
            public delegate object ImportFunc( JsonData data );
            public delegate object ImportFactory();
            #endregion

            #region members
            static Dictionary<int, ExportFunc>          customExporters =  new Dictionary<int, ExportFunc>();
            static Dictionary<int, Type>                customExportersTypeIndex = new Dictionary<int, Type>();
            static Dictionary<int, ImportFunc>          customImporters = new Dictionary<int, ImportFunc>();
            static Dictionary<int, Type>                customImportersTypeIndex = new Dictionary<int, Type>();

            static Dictionary<Type, ImportFactory>      factories = new Dictionary<Type, ImportFactory>();
            #endregion

            #region interface

            public static JsonData ToJsonData( object obj, bool forceSerialize = false )
            {
                return ConvertObjectToJsonData(obj, forceSerialize);
            }

            public static T ToObject<T>( JsonData data, bool forceSerialize = false )
            {
                return (T)ConvertJsonDataToObject(data, typeof( T ), forceSerialize);
            }

            public static void RegisterFactory<T>( ImportFactory factory )
            {            
                if (factory != null)
                {
                    Type type = typeof(T);
                    factories.Add(type, factory);
                }
            }

            public static void UnregisterAutoType<T>()
            {
                Type type = typeof( T );
                factories.Remove(type);
            }

            public static void RegisterExporter<T>( ExportFunc exporter )
            {
                Type type = typeof(T);
                int hashValue = type.GetHashCode();

                if (!customExporters.ContainsKey(hashValue))
                {
                    customExporters.Add(hashValue, exporter);
                    customExportersTypeIndex.Add(hashValue, type);
                }
            }

            public static void UnregisterExporter<T>()
            {
                Type type = typeof(T);
                int hashValue = type.GetHashCode();
                customExporters.Remove(hashValue);
                customExportersTypeIndex.Remove(hashValue);
            }

            public static void RegisterImporter<T>( ImportFunc importer )
            {
                Type type = typeof(T);
                int hashValue = type.GetHashCode();

                if (!customImporters.ContainsKey(hashValue))
                {
                    customImporters.Add( hashValue, importer);
                    customImportersTypeIndex.Add(hashValue, type);
                }
            }

            public static void UnregisterImporter<T>()
            {
                Type type = typeof(T);
                int hashValue = type.GetHashCode();
                customImporters.Remove(hashValue);
                customImportersTypeIndex.Remove(hashValue);
            }

            #endregion

            #region internal

            private static JsonData ConvertObjectToJsonData( object obj, bool forceSerialize = false )
            {            
                if (obj == null)
                {
                    return null;
                }

                Type type = obj.GetType();

                // try custom exporters
                ExportFunc func = null;
                customExporters.TryGetValue( type.GetHashCode(), out func );
                if (func != null)
                {
                    JsonData data = func(obj);
                    if (data != null && data.GetJsonType() == JsonData.JsonType.Object )
                    {
                        data.Add("@TypeHash", type.GetHashCode());
                    }
                    return data;
                }

                if (type == typeof(bool))
                {
                    return new JsonData((bool)obj);
                }
                else if (type == typeof(string))
                {
                    return new JsonData((string)obj);
                }
                else if (type == typeof(int))
                {
                    return new JsonData((int)obj);
                }
                else if (
                    type == typeof(char) ||
                    type == typeof(Byte) ||
                    type == typeof(SByte) ||
                    type == typeof(Int16) ||
                    type == typeof(UInt16)
                    )
                {
                    return new JsonData(Convert.ToInt32(obj));
                }
                else if (
                    type == typeof( UInt32 ) ||
                    type == typeof(long))
                {
                    return new JsonData(Convert.ToInt64(obj));
                }
                else if (
                    type == typeof(Int64) ||
                    type == typeof(UInt64) ||
                    type == typeof(IntPtr) ||
                    type == typeof(UIntPtr))
                {
                    return new JsonData(Convert.ToInt64(obj));
                }
                else if (type == typeof(Single))
                {
                    return new JsonData((double)(float)obj);
                }
                else if (type == typeof(Double))
                {
                    return new JsonData((double)obj);
                }
                else if (type.IsEnum)
                {
                    return new JsonData(obj.ToString());
                }
                else if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) )
                {
                    return ConvertListToJsonDataObject(obj, forceSerialize);
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    return ConvertDictionaryToJsonDataObject(obj, forceSerialize);
                }
                else if (type.IsAssignableFrom(typeof(IList)))
                {
                    return ConvertListToJsonDataArray(obj, forceSerialize);
                }
                else if (type.IsArray)
                {
                    return ConvertArrayToJsonDataArray(obj, forceSerialize);
                }
                else if (type.IsValueType)
                {
                    return ConvertClassStructToJsonDataObject(obj, forceSerialize);
                }
                else if (type.IsClass)
                {
                    // FIXME: may lead to stack overflow here
                    return ConvertClassStructToJsonDataObject(obj, forceSerialize);
                }
                else if( type != null )
                {
                    //Console.Write(string.Format( "Not support type {0}", type ));
                }

                return null;
            }

            private static JsonData ConvertListToJsonDataObject( object obj, bool forceSerialize = false )
            {
                IList list = (IList)obj;
                if (list.Count> 0)
                {
                    JsonData data = new JsonData();
                    for (int i = 0; i < list.Count; ++i)
                    {
                        data.Add(ConvertObjectToJsonData(list[i], forceSerialize));
                    }
                    return data;
                }

                return null;
            }

            private static JsonData ConvertClassStructToJsonDataObject( object obj, bool forceSerialize = false )
            {   
                Type type = obj.GetType();

                MemberInfo memberInfo = type;
                object[] attributes = memberInfo.GetCustomAttributes(true);

                JsonData data = null;

                bool needSerialize = forceSerialize;
                if (!needSerialize)
                {
                    for (int iClassAttribute = 0; iClassAttribute < attributes.Length; ++iClassAttribute)
                    {
                        if (attributes[iClassAttribute] is Serialize)
                        {
                            needSerialize = true;
                            break;
                        }
                    }
                }

                BindingFlags flag = BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                FieldInfo[] fields = type.GetFields( flag );
                for (int iField = 0; iField < fields.Length; ++iField)
                {
                    FieldInfo field = fields[iField];
                    if ( IsFieldNeedToSerialize( field, needSerialize) )
                    {
                        if (data == null)
                        {
                            data = new JsonData();
                        }

                        data.Add(field.Name, ConvertObjectToJsonData( field.GetValue( obj ), forceSerialize ));   
                    }
                }

                if (data != null )
                {
                    data.Add("@TypeHash", type.GetHashCode());
                }

                return data;
            }

            private static bool IsFieldNeedToSerialize( FieldInfo field, bool objectSerializeFlag )
            {
                object[] attributes = field.GetCustomAttributes(true) as object[];
                bool nonSerializeField = false;
                bool serializeField = false;
                for (int i = 0; i < attributes.Length; ++i)
                {
                    object attribute = attributes[i];
                    if (attribute is NonSerializeField)
                    {
                        nonSerializeField = true;
                        break;
                    }
                    else if (attribute is SerializeField)
                    {
                        serializeField = true;
                        break;
                    }
                }

                if (field.DeclaringType.IsGenericType)
                {
                    if (field.Name == "_size" || field.Name == "_version" || field.Name == "_syncRoot")
                        return false;
                }

                return ((objectSerializeFlag && !nonSerializeField) || (!objectSerializeFlag && serializeField));
            }

            private static JsonData ConvertDictionaryToJsonDataObject( object obj , bool forceSerialize )
            {
                IDictionary dict = (IDictionary)obj;
                ICollection collection = dict.Keys;
                if (collection.Count> 0)
                {
                    object[] keys = new object[collection.Count];
                    collection.CopyTo(keys, 0);
                    JsonData data = new JsonData();
                    for (int i = 0; i < keys.Length; ++i)
                    {
                        data.Add(keys[i], ConvertObjectToJsonData(dict[keys[i]], forceSerialize));
                    }
                    return data;
                }

                return null;
            }

            private static JsonData ConvertListToJsonDataArray( object obj, bool forceSerialize )
            {
                IList list = (IList)obj;
                if (list.Count > 0)
                {
                    JsonData data = new JsonData();
                    for (int i = 0; i < list.Count; ++i)
                    {
                        data.Add(ConvertObjectToJsonData( list[i], forceSerialize));
                    }
                    return data;
                }
                return null;
            }

            private static JsonData ConvertArrayToJsonDataArray( object obj, bool forceSerialize )
            {
                IEnumerable array = obj as IEnumerable;
                JsonData data = null;
                foreach( object o in array )
                {
                    if (data == null)
                    {
                        data = new JsonData();
                    }

                    data.Add( ConvertObjectToJsonData( o, forceSerialize ) );
                }

                return data;
            }

            private static object ConvertJsonDataToObject( JsonData data, Type type, bool forceSerialize = false )
            {
                if (data == null)
                {
                    return null;
                }

                // try custom importers
                if (data.GetJsonType() == JsonData.JsonType.Object )
                {
                    JsonData hashData = data["@TypeHash"];
                    if (hashData != null)
                    {
                        ImportFunc func = null;
                        customImporters.TryGetValue(hashData.GetInt(), out func);
                        if (func != null)
                        {
                            return func(data);
                        }
                    }
                }

                // default conversions

                JsonData.JsonType jsonType = data.GetJsonType();
                switch (jsonType)
                {
                    case JsonData.JsonType.Object:
                        {
                            if (type == typeof(IDictionary) ||
                                type.IsSubclassOf( typeof( IDictionary )) ||
                                type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                            {
                                return ConvertJsonDataObjectToDictionary(data, type);
                            }
                            else if (type.IsValueType || type.IsClass)
                            {
                                return ConvertJsonDataObjectToClassStruct(data, type, forceSerialize);
                            }
                        }
                        break;
                    case JsonData.JsonType.Array:
                        {
                            if (type.IsArray)
                            {
                                return ConvertJsonDataObjectToArray( data, type, forceSerialize );
                            }
                            else if (type == typeof( IList ) ||
                                type.IsSubclassOf( typeof( IList ) ) ||
                                type.IsGenericType && type.GetGenericTypeDefinition() == typeof( List<> ))
                            {
                                return ConvertJsonDataObjectToList( data, type, forceSerialize );
                            }
                        }
                        break;
                    case JsonData.JsonType.Boolean:
                    case JsonData.JsonType.String:
                    case JsonData.JsonType.Int:
                    case JsonData.JsonType.Long:
                    case JsonData.JsonType.Double:
                        {
                            return ConvertPrimitive(data, type);
                        }
                }

                return null;
            }

            private static object ConvertJsonDataObjectToClassStruct( JsonData data, Type type, bool forceSerialize = false )
            {
                // get class serialize flag
                MemberInfo memberInfo = type;
                object[] attributes = memberInfo.GetCustomAttributes(true);

                bool needSerialize = forceSerialize;
                if (!needSerialize)
                {
                    for (int iClassAttribute = 0; iClassAttribute < attributes.Length; ++iClassAttribute)
                    {
                        if (attributes[iClassAttribute] is Serialize)
                        {
                            needSerialize = true;
                            break;
                        }
                    }
                }

                object retObject = null;

                // try find each field in JsonData. If found, fill value
                object obj = InstantiateObject(type);
                BindingFlags flag = BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                FieldInfo[] fields = type.GetFields(flag);
                for (int iField = 0; iField < fields.Length; ++iField)
                {
                    FieldInfo field = fields[iField];
                    if (IsFieldNeedToSerialize(field, needSerialize))
                    {
                        if (retObject == null)
                        {
                            retObject = InstantiateObject(type);
                        }

                        JsonData fieldData = data[field.Name];
                        if (fieldData != null)
                        {
                            object fieldValue = field.GetValue(obj);
                            Type fieldType = field.FieldType;
                            if (fieldValue != null)
                            {
                                fieldType = fieldValue.GetType();
                            }

                            Object objValue = ConvertJsonDataToObject(fieldData, fieldType, forceSerialize);
                            if (objValue != null)
                            {
                                field.SetValue(obj, objValue);
                            }
                        }
                        else
                        {
                            retObject = null;
                            continue;
                        }
                    }
                }

                return obj;
            }

            private static object InstantiateObject( Type type )
            {
                ImportFactory factory = null;
                factories.TryGetValue(type, out factory);
                if (factory != null)
                {
                    return factory();
                }

                return Activator.CreateInstance(type);
            }

            private static object ConvertJsonDataObjectToDictionary( JsonData data, Type type )
            {
                Type[] argumentTypes = type.GetGenericArguments();

                string[] stringKeys = data.StringKeys;
                object[] Keys = new object[stringKeys.Length];
                data.Keys.CopyTo(Keys, 0);

                if (Keys.Length > 0)
                {
                    IDictionary dict = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(argumentTypes)) as IDictionary;

                    Type profileKeyType = TryGetProfileKey( argumentTypes[1] );

                    for (int i = 0; i < stringKeys.Length; ++i)
                    {
                        dict.Add(
                            // try to find a convertion from JsonData( string ) to key type
                            ConvertJsonDataToObject( new JsonData( Keys[i] ), profileKeyType != null ? profileKeyType : argumentTypes[0], true ),
                            ConvertJsonDataToObject( data[Keys[i]], argumentTypes[1], true )
                        );
                    }
                    return dict;
                }

                return null;
            }

            private static Type TryGetProfileKey( Type profileType )
            {
                BindingFlags flag = BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                FieldInfo[] fields = profileType.GetFields(flag);
                for (int iField = 0; iField < fields.Length; ++iField)
                {
                    FieldInfo field = fields[iField];

                    object[] attributes = field.GetCustomAttributes( true ) as object[];
                    for (int iAttribute = 0; iAttribute < attributes.Length; ++iAttribute)
                    {
                        if (attributes[iAttribute] is ProfileKey)
                        {
                            return field.FieldType;
                        }
                    }
                }

                return null;
            }

            private static object ConvertJsonDataObjectToArray( JsonData data, Type type, bool forceSerialize = false )
            {
                Type elementType = type.GetElementType();
                int elementCount = data.Count;
                if (elementCount > 0)
                {
                    Array array = Array.CreateInstance(elementType, data.Count);
                    for (int i = 0; i < data.Count; ++i)
                    {
                        array.SetValue(ConvertJsonDataToObject(data[i], elementType, forceSerialize), i);
                    }
                    return array;
                }

                return null;
            }

            private static object ConvertJsonDataObjectToList( JsonData data, Type type, bool forceSerialize = false )
            {
                Type[] argumentType = type.GetGenericArguments();
                Type elementType = argumentType[0];

                if (data.Count > 0)
                {
                    IList list = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
                    for (int i = 0; i < data.Count; ++i)
                    {
                        list.Add(ConvertJsonDataToObject(data[i], elementType, forceSerialize));
                    }
                    return list;
                }

                return null;
            }

            private static object ConvertPrimitive( JsonData data, Type type )
            {
                JsonData.JsonType jsonType = data.GetJsonType();
                switch (jsonType)
                {
                    case JsonData.JsonType.Boolean:
                        {
                            if (type == typeof(Boolean))
                            {
                                return data.GetBool();
                            }
                            else if (type == typeof(string))
                            {
                                return data.GetBool().ToString();
                            }
                            else if (type == typeof(Byte))
                            {
                                return Convert.ToByte(data.GetBool());
                            }
                            else if (type == typeof(SByte))
                            {
                                return Convert.ToSByte(data.GetBool());
                            }
                            else if (type == typeof(Int16))
                            {
                                return Convert.ToInt16(data.GetBool());
                            }
                            else if (type == typeof(UInt16))
                            {
                                return Convert.ToInt16(data.GetBool());
                            }
                            else if (type == typeof(Int32))
                            {
                                return Convert.ToInt32(data.GetBool());
                            }
                            else if (type == typeof(UInt32))
                            {
                                return Convert.ToUInt32(data.GetBool());
                            }
                            else if (type == typeof(Int64))
                            {
                                return Convert.ToInt64(data.GetBool());
                            }
                            else if (type == typeof(UInt64))
                            {
                                return Convert.ToUInt64(data.GetBool());
                            }
                            else if (type == typeof(object))
                            {
                                return (object)data.GetBool();
                            }
                        }
                        break;
                    case JsonData.JsonType.String:
                        {
                            if (type == typeof(String))
                            {
                                return data.GetString();
                            }
                            else if (type == typeof(Boolean))
                            {
                                return data.GetString();
                            }
                            else if (type == typeof(Byte))
                            {
                                return Convert.ToByte(data.GetString());
                            }
                            else if (type == typeof(SByte))
                            {
                                return Convert.ToSByte(data.GetString());
                            }
                            else if (type == typeof(Int16))
                            {
                                return Convert.ToInt16(data.GetString());
                            }
                            else if (type == typeof(UInt16))
                            {
                                return Convert.ToInt16(data.GetString());
                            }
                            else if (type == typeof(Int32))
                            {
                                return Convert.ToInt32(data.GetString());
                            }
                            else if (type == typeof(UInt32))
                            {
                                return Convert.ToUInt32(data.GetString());
                            }
                            else if (type == typeof(Int64))
                            {
                                return Convert.ToInt64(data.GetString());
                            }
                            else if (type == typeof(UInt64))
                            {
                                return Convert.ToUInt64(data.GetString());
                            }
                            else if (type == typeof(Double))
                            {
                                return Convert.ToDouble(data.GetString());
                            }
                            else if (type == typeof(Single))
                            {
                                return Convert.ToSingle(data.GetString());
                            }
                            else if (type.IsEnum)
                            {
                                return Enum.Parse(type, data.GetString());
                            }
                            else if (type == typeof(object))
                            {
                                return (object)data.GetString();
                            }
                        }
                        break;
                    case JsonData.JsonType.Int:
                        {
                            if (type == typeof(Int32))
                            {
                                return data.GetInt();
                            }
                            else if (type == typeof(Boolean))
                            {
                                return Convert.ToBoolean(data.GetInt());
                            }
                            else if (type == typeof(String))
                            {
                                return Convert.ToString(data.GetInt());
                            }
                            else if (type == typeof(Byte))
                            {
                                return Convert.ToByte(data.GetInt());
                            }
                            else if (type == typeof(SByte))
                            {
                                return Convert.ToSByte(data.GetInt());
                            }
                            else if (type == typeof(Int16))
                            {
                                return Convert.ToInt16(data.GetInt());
                            }
                            else if (type == typeof(UInt16))
                            {
                                return Convert.ToInt16(data.GetInt());
                            }
                            else if (type == typeof(UInt32))
                            {
                                return Convert.ToUInt32(data.GetInt());
                            }
                            else if (type == typeof(Int64))
                            {
                                return Convert.ToInt64(data.GetInt());
                            }
                            else if (type == typeof(UInt64))
                            {
                                return Convert.ToUInt64(data.GetInt());
                            }
                            else if (type == typeof(Double))
                            {
                                return Convert.ToDouble(data.GetInt());
                            }
                            else if (type == typeof(Single))
                            {
                                return Convert.ToSingle(data.GetInt());
                            }
                            else if (type == typeof(IntPtr))
                            {
                                return new IntPtr(data.GetInt());
                            }
                            else if (type == typeof(object))
                            {
                                return (object)data.GetInt();
                            }
                        }
                        break;
                    case JsonData.JsonType.Long:
                        {
                            if (type == typeof(Int64))
                            {
                                return data.GetLong();
                            }
                            else if (type == typeof(Boolean))
                            {
                                return Convert.ToBoolean(data.GetLong());
                            }
                            else if (type == typeof(String))
                            {
                                return Convert.ToString(data.GetLong());
                            }
                            else if (type == typeof(Byte))
                            {
                                return Convert.ToByte(data.GetLong());
                            }
                            else if (type == typeof(SByte))
                            {
                                return Convert.ToSByte(data.GetLong());
                            }
                            else if (type == typeof(Int16))
                            {
                                return Convert.ToInt16(data.GetLong());
                            }
                            else if (type == typeof(UInt16))
                            {
                                return Convert.ToInt16(data.GetLong());
                            }
                            else if (type == typeof(Int32))
                            {
                                return Convert.ToInt32(data.GetLong());
                            }
                            else if (type == typeof(UInt32))
                            {
                                return Convert.ToUInt32(data.GetLong());
                            }
                            else if (type == typeof(UInt64))
                            {
                                return Convert.ToUInt64(data.GetLong());
                            }
                            else if (type == typeof(Double))
                            {
                                return Convert.ToDouble(data.GetLong());
                            }
                            else if (type == typeof(Single))
                            {
                                return Convert.ToSingle(data.GetLong());
                            }
                            else if (type == typeof(IntPtr))
                            {
                                return new IntPtr(data.GetLong());
                            }
                            else if (type == typeof(object))
                            {
                                return (object)data.GetLong();
                            }
                        }
                        break;
                    case JsonData.JsonType.Double:
                        {
                            if (type == typeof(Double))
                            {
                                return data.GetDouble();
                            }
                            else if (type == typeof(Boolean))
                            {
                                return Convert.ToBoolean(data.GetDouble());
                            }
                            else if (type == typeof(String))
                            {
                                return Convert.ToString(data.GetDouble());
                            }
                            else if (type == typeof(Byte))
                            {
                                return Convert.ToByte(data.GetDouble());
                            }
                            else if (type == typeof(SByte))
                            {
                                return Convert.ToSByte(data.GetDouble());
                            }
                            else if (type == typeof(Int16))
                            {
                                return Convert.ToInt16(data.GetDouble());
                            }
                            else if (type == typeof(UInt16))
                            {
                                return Convert.ToInt16(data.GetDouble());
                            }
                            else if (type == typeof(UInt32))
                            {
                                return Convert.ToUInt32(data.GetDouble());
                            }
                            else if (type == typeof(Int64))
                            {
                                return Convert.ToInt64(data.GetDouble());
                            }
                            else if (type == typeof(UInt64))
                            {
                                return Convert.ToUInt64(data.GetDouble());
                            }
                            else if (type == typeof(Single))
                            {
                                return Convert.ToSingle(data.GetDouble());
                            }
                            else if (type == typeof(object))
                            {
                                return (object)data.GetDouble();
                            }
                        }
                        break;
                }

                return null;
            }

            #endregion
        }
    }
}

