using System;

namespace ProjectAlice
{
    namespace Utilities
    {
        public class JsonError
        {
            public enum ErrorCode
            {
                OK,
                UnexpectedEnd,
                InvalidValue,
                ExpectName,
                ExpectPairSeperator,
                ExpectValue,
                ExpectObjectEndOrValueSeperator,
                ExpectArrayEndOrValueSeperator,
                InvalidHexValueInString,
                InvalidNumber,
            }

            public static string[] ErrorMessages =
                {
                    "OK",
                    "Unexpected end of json",
                    "Invalid json value",
                    "Expect name of pair",
                    "Expect pair seperator \":\"",
                    "Expect value of pair",
                    "Expect object end \"}\" or value seperator \",\"",
                    "Expect array end \"]\" or value seperator \",\"",
                    "Invalid hex value in string",
                    "Invalid number",
                };

            public static string GetErrorMessage( ErrorCode errorCode )
            {
                return ErrorMessages[(int)errorCode];
            }
        }
    }
}

