using Dapper;
using Dapper_WebAPI_Dynamic.Models.BaseModels;
using Dapper_WebAPI_Dynamic.Models.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapper_WebAPI_Dynamic.Helper.Code
{
    public class GridExtract
    {
        internal static MainResponseModel GetSimpleResp(SqlMapper.GridReader objDetails)
        {
            var obj = new SampleResponse();
            try
            {
                obj.Sample = objDetails.Read<Sample>().ToList();
            }
            catch (Exception)
            {
                obj.Sample = new List<Sample>();
            }

            return obj;
        }
    }
}