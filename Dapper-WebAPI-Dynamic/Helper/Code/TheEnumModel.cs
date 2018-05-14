namespace Dapper_WebAPI_Dynamic.Helper.Code
{
    public class TheEnumModel
    {
        /// <summary>
        /// This will be used in the switch to differentiate which classes to use for mapping.
        /// </summary>
        public enum ResponseMode
        {
            /// <summary>
            /// Just the response object with result, message and additional parameter as output
            /// </summary>
            ResponseOnly
        }

        /// <summary>
        /// The stored procedures list.
        /// </summary>
        public enum SP
        {
            SP_SampleStoredProcedure
        }
    }
}