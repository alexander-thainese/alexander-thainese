using System.Collections.Generic;

namespace CMT.BO
{
    public static class DataRowErrorCode
    {
        public const int MaxApplicationErrorCode = 50;

        //todo move
        public const int ApplicationErrorsGroupCode = -1234;
        public const string ApplicationErrorsGroupName = "Application";

        public const int UnableToParseValue = 1;
        public const int InvalidRowLength = 2;
        public const int NullValueInRequiredColumn = 3;
        public const int ColumnNotFound = 4;
        public const int ColumnDuplicated = 5;
        public const int ValueOutOfRange = 6;

        public static readonly Dictionary<int, string> ParserErrorsDictionary
            = new Dictionary<int, string>
              {
                  {DataRowErrorCode.UnableToParseValue, "Unable to parse value."},
                  {DataRowErrorCode.InvalidRowLength, "Invalid row length."},
                  {DataRowErrorCode.NullValueInRequiredColumn, "Column is required."},
                  {DataRowErrorCode.ColumnNotFound, "Column not found."},
                  {DataRowErrorCode.ColumnDuplicated, "Column is duplicated."},
                  {DataRowErrorCode.ValueOutOfRange, "Value out of range."},
              };
    }
}