using System.Collections.Generic;

namespace Dapper_WebAPI_Dynamic.Models.ResponseModels
{
    public class Sample
    {

    }
    public class SampleResponse : BaseModels.MainResponseModel
    {
        //The classes are kept as List objects
        public List<Sample> Sample { get; set; }
    }
}