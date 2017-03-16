using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace ProjectAlice
{
    namespace Utilities
    {
        public class JsonMapper
        {
            #region interface

            public static JsonData ToJsonData( string json )
            {
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                reader = new StreamReader(stream);
                return Parse();
            }

            public static JsonData ToJsonData(StreamReader input )
            {
                reader = input;
                return Parse();
            }

            public static string ToJson(JsonData data )
            {
                MemoryStream stream = new MemoryStream();
                writer = new StreamWriter(stream);
                Serialize( data );
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                StreamReader r = new StreamReader(stream);
                return r.ReadToEnd();
            }

            public static void ToJson( JsonData data, StreamWriter output )
            {
                writer = output;
                Serialize( data );
            }

            #endregion

            #region internal

            enum ValueType
            {
                Unknown,
                Object,
                Array,
                String,
                Number,
                True,
                False,
                Null
            }

            enum Token
            {
                ObjectBegin,
                ObjectEnd,
                ArrayBegin,
                ArrayEnd,
                StringBegin,
                StringEnd,
            }

            static StreamReader reader;
            static int offset;
            static int lineCount;
            static int columnCount;
            static char ch = '\0';

            static StreamWriter writer;
            static int indentStep = 1;
            static Stack<int> indentStack = new Stack<int>();

            static void Read()
            {
                ++offset;
                int c = reader.Read();
                ch = (c >= 0) ? (char)c : '\0';

                if (ch == '\n')
                {
                    ++lineCount;
                    columnCount = 0;
                }
                else
                {
                    ++columnCount;
                }
            }

            static bool ReadAllWhiteSpace()
            {
                while (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r')
                {
                    Read();
                }

                return ch != '\0';
            }

            static JsonData Parse()
            {
                if (reader != null)
                {
                    Read();

                    // allow empty document
                    return ch != '\0' ? ParseValue() : null;
                }
                return null;
            }

            static JsonData ParseObject()
            {
                int memberCount = 0;

                // eat '{'
                Read();

                //Console.Write("ObjectBegin: {\n");

                JsonData objectData = new JsonData();

                if (ch != '}')
                {
                    bool loop = true;
                    while (loop)
                    {
                        if (!ReadAllWhiteSpace())
                        {
                            ParseError(JsonError.ErrorCode.UnexpectedEnd);
                            return null;
                        }

                        // expect '"'
                        if (ch != '"')
                        {
                            ParseError(JsonError.ErrorCode.ExpectName);
                            return null;
                        }

                        JsonData keyData = ParseString();
                        if (keyData == null)
                        {
                            return null;
                        }

                        // expect pair seperator
                        if (!ReadAllWhiteSpace())
                        {
                            ParseError(JsonError.ErrorCode.UnexpectedEnd);
                            return null;
                        }

                        if (ch != ':')
                        {
                            ParseError(JsonError.ErrorCode.ExpectPairSeperator);
                            return null;
                        }

                        // eat ':'
                        Read();

                        JsonData valueData = ParseValue();

                        objectData.Add( (string)keyData, valueData);

                        ++memberCount;

                        // expect ',' and '}'
                        if (!ReadAllWhiteSpace())
                        {
                            ParseError(JsonError.ErrorCode.UnexpectedEnd);
                            return null;
                        }

                        switch (ch)
                        {
                            case ',':
                                // eat ','
                                Read();
                                break;
                            case '}':
                                loop = false;
                                break;
                            default:
                                {
                                    ParseError(JsonError.ErrorCode.ExpectObjectEndOrValueSeperator);
                                    return null;
                                }
                        }
                    }
                }

                // eat '}'
                Read();

                //Console.Write("ObjectEnd: }\n");

                // extract value

                return objectData;
            }

            static JsonData ParseArray()
            {
                int elementCount = 0;

                // eat '['
                Read();

                JsonData arrayData = new JsonData();

                //Console.Write("ArrayBegin: {\n");

                bool loop = true;
                while (loop)
                {
                    // allow empty array
                    if (ch != ']')
                    {
                        // read array element
                        JsonData valueData = ParseValue();
                        if (valueData != null)
                        {
                            arrayData.Add( valueData );
                            ++elementCount;
                        }

                        // expect ']'
                        if (!ReadAllWhiteSpace())
                        {
                            ParseError(JsonError.ErrorCode.UnexpectedEnd);
                            return null;
                        }

                        switch (ch)
                        {
                            case ',':
                                // eat ','
                                Read();
                                break;
                            case ']':
                                loop = false;
                                break;
                            default:
                                {
                                    ParseError(JsonError.ErrorCode.ExpectArrayEndOrValueSeperator);
                                    return null;
                                }
                        }
                    }
                    else
                    {
                        loop = false;
                    }
                }

                // eat ']'
                Read();

                //Console.Write("ArrayEnd: }\n");

                // extract value

                return arrayData;
            }

            static JsonData ParseValue()
            {            
                if (!ReadAllWhiteSpace())
                {
                    ParseError(JsonError.ErrorCode.UnexpectedEnd);
                    return null;
                }

                switch (ch)
                {
                    case '{':
                        return ParseObject();
                    case '[':
                        return ParseArray();
                    case '\"':
                        return ParseString();
                    case '-':
                        return ParseNumber();
                    case 't':
                    case 'f':
                    case 'n':
                        return ParseConst();
                    default:
                        {
                            if (ch >= '0' && ch <= '9')
                            {
                                return ParseNumber();
                            }
                        }
                        break;
                }

                ParseError(JsonError.ErrorCode.InvalidValue);
                return null;
            }

            static JsonData ParseString()
            {
                StringBuilder sb = new StringBuilder();
                // eat '"'
                Read();

                bool loop = true;
                while (loop)
                {
                    if (ch == '\\')
                    {
                        Read();
                        switch (ch)
                        {
                            case '"':
                                sb.Append('"');
                                Read();
                                break;
                            case '\\':
                                sb.Append('\\');
                                Read();
                                break;
                            case '/':
                                sb.Append('/');
                                Read();
                                break;
                            case 'b':
                                sb.Append('\b');
                                Read();
                                break;
                            case 'f':
                                sb.Append('\f');
                                Read();
                                break;
                            case 'n':
                                sb.Append('\n');
                                Read();
                                break;
                            case 'r':
                                sb.Append('\r');
                                Read();
                                break;
                            case 't':
                                sb.Append('\t');
                                Read();
                                break;
                            case 'u':
                                {
                                    uint x = 0;
                                    Read();
                                    x += CharToHex(ch) * 0x1000;
                                    Read();
                                    x += CharToHex(ch) * 0x0100;
                                    Read();
                                    x += CharToHex(ch) * 0x0010;
                                    Read();
                                    x += CharToHex(ch) * 0x0001;
                                    sb.Append((char)x);
                                }
                                break;
                        }
                    }
                    else if (ch == '"')
                    {
                        loop = false;
                    }
                    else if (ch == '\0')
                    {
                        ParseError(JsonError.ErrorCode.UnexpectedEnd);
                    }
                    else if (ch != '"')
                    {
                        sb.Append(ch);
                        Read();
                    }
                }   


                // eat '"'
                Read();

                // extract value
                string line = sb.ToString();
                //Console.Write("ParsedString: {0}\n", line);

                return new JsonData( line );
            }

            static uint CharToHex(char x)
            {
                if (x >= '0' && x <= '9')
                {
                    return (uint)(x - '0');
                }
                else if (x >= 'a' && x <= 'z')
                { 
                    return (uint)(x - 'a');
                }
                else if (x >= 'A' && x <= 'Z')
                {
                    return (uint)(x - 'A');
                }

                ParseError(JsonError.ErrorCode.InvalidHexValueInString);
                return 0;
            }

            static JsonData ParseNumber()
            {
                int sign = 1;
                long integerPart = 0;
                double decimalPart = 0.0;
                int exponentialSign = 1;
                int exponentialPart = 0;

                bool isDecimal = false;
                bool isExp = false;

                // expect '-'
                if (ch == '-')
                {
                    sign = 1;
                    Read();
                }

                // expect '0' to '9'
                if (ch >= '0' && ch <= '9')
                {                
                    // parse integer
                    while (ch >= '0' && ch <= '9')
                    {
                        integerPart *= 10;
                        integerPart += ch - '0';
                        Read();
                    }

                    if (ch == '.')
                    {
                        isDecimal = true;

                        // eat '.'
                        Read();

                        // parse decimal
                        Stack<int> digits = new Stack<int>();
                        while (ch >= '0' && ch <= '9')
                        {
                            digits.Push(ch - '0');
                            Read();
                        }

                        while (digits.Count > 0)
                        {
                            decimalPart *= 0.1;
                            decimalPart += digits.Pop() * 0.1;
                        }
                    }

                    if (ch == 'e' || ch == 'E')
                    {
                        isExp = true;

                        // eat 'e' or 'E'
                        Read();

                        // expect '+' or '-'
                        if (ch == '+')
                        {
                            exponentialSign = 1;
                            // eat '+'
                            Read();
                        }
                        else if (ch == '-')
                        {
                            exponentialSign = -1;
                            // eat '-'
                            Read();
                        }

                        if (ch >= '0' || ch <= '9')
                        {
                            while (ch >= '0' && ch <= '9')
                            {
                                exponentialPart *= 10;
                                exponentialPart += ch - '0';
                                Read();
                            }
                        }
                        else
                        {
                            ParseError(JsonError.ErrorCode.InvalidNumber);
                            return null;
                        }
                    }
                }
                else
                {
                    ParseError(JsonError.ErrorCode.InvalidNumber);
                    return null;
                }

                // extract value

                JsonData numberData = new JsonData();

                if (sign < 0)
                {
                    integerPart = -integerPart;
                    decimalPart = -decimalPart;
                }

                if (isDecimal == false)
                {
                    if (isExp)
                    {
                        integerPart = (long)((double)integerPart * global::System.Math.Pow(exponentialSign > 0 ? 10.0 : 0.1, exponentialPart));
                    }

                    if (integerPart >= int.MinValue && integerPart <= int.MaxValue)
                    {
                        numberData.Set((int)integerPart);
                        //Console.Write(string.Format("ParsedNumber: {0}\n", numberData.GetInt()));
                    }
                    else
                    {
                        numberData.Set(integerPart);
                        //Console.Write(string.Format("ParsedNumber: {0}\n", numberData.GetLong()));
                    }
                }
                else
                {
                    if (isExp)
                    {
                        numberData.Set(((double)integerPart + decimalPart) * global::System.Math.Pow(exponentialSign > 0 ? 10.0 : 0.1, exponentialPart));
                    }
                    else
                    {
                        numberData.Set((double)integerPart + decimalPart);
                    }
                    //Console.Write(string.Format("ParsedNumber: {0}\n", numberData.GetDouble()));
                }

                return numberData;
            }

            static JsonData ParseConst()
            {
                ValueType valueType = ValueType.Unknown;

                bool valid = false;
                if (ch == 't')
                {
                    Read();
                    if (ch == 'r')
                    {
                        Read();
                        if (ch == 'u')
                        {
                            Read();
                            if (ch == 'e')
                            {
                                valueType = ValueType.True;
                                valid = true;

                                // eat 'e'
                                Read();
                            }
                        }
                    }
                }
                else if (ch == 'f')
                {
                    Read();
                    if (ch == 'a')
                    {
                        Read();
                        if (ch == 'l')
                        {
                            Read();
                            if (ch == 's')
                            {
                                Read();
                                if (ch == 'e')
                                {
                                    valueType = ValueType.False;
                                    valid = true;

                                    // eat 'e'
                                    Read();
                                }
                            }
                        }
                    }
                }
                else if (ch == 'n')
                {
                    Read();
                    if (ch == 'u')
                    {
                        Read();
                        if (ch == 'l')
                        {
                            Read();
                            if (ch == 'l')
                            {
                                valueType = ValueType.Null;
                                valid = true;

                                // eat 'l'
                                Read();
                            }
                        }
                    }
                }

                if (!valid)
                {
                    ParseError(JsonError.ErrorCode.InvalidValue);
                    return null;
                }

                // extract value

                JsonData constData = new JsonData();
                switch (valueType)
                {
                    case ValueType.True:
                        constData.Set(true);
                        break;
                    case ValueType.False:
                        constData.Set(false);
                        break;
                    case ValueType.Null:
                        constData = null;
                        break;
                    default:
                        {
                            constData = null;
                        }
                        break;
                }

                return constData;
            }

            static void ParseError(JsonError.ErrorCode errorCode)
            {
                Console.Write(string.Format("ParseError({0}, {1}): {2}, ch = {3}\n", lineCount+1, columnCount+1, JsonError.GetErrorMessage(errorCode), ch ));
                Console.Write( string.Format( "at {0}", GetParsedString(0, offset) ) );
            }

            static string GetParsedString(int start, int end)
            {
                int size = end - start;
                if (size > 0)
                {
                    long oldPosition = reader.BaseStream.Position;
                    reader.BaseStream.Position = start;
                    char[] buffer = new char[size];
                    reader.ReadBlock(buffer, 0, size);
                    reader.BaseStream.Position = oldPosition;
                    return new string(buffer);
                }

                return string.Empty;
            }

            static void Serialize( JsonData data )
            {
                PushIndent(0);
                SerializeValue(data, 0);
                PopIndent();
                writer.Write("\n");
            }

            static void SerializeObject( JsonData data )
            {
                writer.Write("{");
                PushIndent( indentStep );

                ICollection keys = data.Keys;
                object[] keyObjects = new object[keys.Count];
                keys.CopyTo(keyObjects, 0);
                string[] stringKeys = data.StringKeys;
                for (int i = 0; i < stringKeys.Length; ++i)
                {
                    WriteIndent();
                    writer.Write("\"");
                    writer.Write(stringKeys[i]);
                    writer.Write("\"");
                    writer.Write(" : ");
                    int offset = stringKeys[i].Length + 1 + 3 + 1;
                    SerializeValue(data[keyObjects[i]], offset );
                    if (i != stringKeys.Length - 1)
                    {
                        writer.Write(",");
                    }
                }

                PopIndent();
                //writer.Write("\n");
                WriteIndent();
                writer.Write("}");
            }

            static void SerializeArray( JsonData data )
            {
                writer.Write("[");
                PushIndent( indentStep );

                int count = data.Count;
                for (int i = 0; i < count; ++i)
                {
                    WriteIndent();
                    SerializeValue(data[i], 0);
                    if (i != count - 1)
                    {
                        writer.Write(",");
                    }
                }

                PopIndent();
                WriteIndent();
                writer.Write( "]" );
            }

            static void SerializeValue( JsonData data, int offset )
            {
                if (data == null)
                {
                    writer.Write("null");
                    return;
                }

                switch (data.GetJsonType())
                {
                    case JsonData.JsonType.Object:
                        {
                            PushIndent(offset);
                            SerializeObject(data);
                            PopIndent();
                        }
                        break;
                    case JsonData.JsonType.Array:
                        {
                            PushIndent(offset);
                            SerializeArray(data);
                            PopIndent();
                        }
                        break;
                    case JsonData.JsonType.String:
                        writer.Write("\"");
                        string line = data.GetString();
                        for (int i = 0; i < line.Length; ++i)
                        {
                            switch (line[i])
                            {
                                case '\t':
                                    writer.Write("\\t");
                                    break;
                                case '\r':
                                    writer.Write("\\r");
                                    break;
                                case '\n':
                                    writer.Write("\\n");
                                    break;
                                case '"':
                                    writer.Write( "\"" );
                                    break;
                                case '\\':
                                    writer.Write("\\\\");
                                    break;
                                default:
                                    writer.Write(line[i]);
                                    break;
                            }
                        }
                        //writer.Write(line);
                        writer.Write("\"");
                        break;
                    case JsonData.JsonType.Int:
                        writer.Write(data.GetInt().ToString());
                        break;
                    case JsonData.JsonType.Long:
                        writer.Write(data.GetLong().ToString());
                        break;
                    case JsonData.JsonType.Double:
                        writer.Write(data.GetDouble().ToString());
                        break;
                    case JsonData.JsonType.Boolean:
                        writer.Write(data.GetBool() ? "true" : "false");
                        break;
                }

                return;
            }

            static void PushIndent( int offset )
            {
                int indentCount = indentStack.Count > 0 ? indentStack.Peek() : 0;
                indentStack.Push(indentCount + offset);
            }

            static void PopIndent()
            {
                indentStack.Pop();
            }

            static void WriteIndent()
            {
                int indentCount = indentStack.Count > 0 ? indentStack.Peek() : 0;
                writer.Write("\n");
                for (int i = 0; i < indentCount; ++i)
                {
                    writer.Write(' ');
                }
            }

            #endregion
        }
    }
}

