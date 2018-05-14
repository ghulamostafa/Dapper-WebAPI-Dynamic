namespace Dapper_WebAPI_Dynamic.Models.SideModels
{
    public class ResponseModel
    {
        public string result { get; set; }
        public string message { get; set; }
        public string additional { get; set; }

        public ResponseModel(string Result, string Message, string Additional)
        {
            result = Result;
            message = Message;
            additional = Additional;
        }
        public ResponseModel()
        {

        }
    }
}