using Dapper;
using Dapper_WebAPI_Dynamic.Models.BaseModels;
using Dapper_WebAPI_Dynamic.Models.SideModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using static Dapper_WebAPI_Dynamic.Helper.Code.TheEnumModel;

namespace Dapper_WebAPI_Dynamic.Helper.Code
{
    public class Utility
    {
        #region The Resources

        static string theConString = "Entity";
        static string thedevConString = "Entity";

        #endregion

        #region List to Table Conversions

        internal static DataTable listToDataTable(List<object> _obj)
        {
            var dt = new DataTable();

            //Get the name of the columns from the Class defined
            foreach (var property in _obj[0].GetType().GetProperties())
            {
                dt.Columns.Add(property.Name);
            }

            foreach (var transaction in _obj)
            {
                var dr = dt.NewRow();

                foreach (var subItem in transaction.GetType().GetProperties())
                {
                    dr[subItem.Name] = subItem.GetValue(transaction, null);
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        internal static List<paramWithValues> formDataToParamList(object _obj)
        {
            var paramList = new List<paramWithValues>();

            foreach (var propVal in _obj.GetType().GetProperties())
            {
                paramList.Add(new paramWithValues(propVal.Name, propVal.GetValue(_obj, null).ToString()));
            }

            return paramList;
        }

        #endregion

        /// <summary>
        /// This is the platform specific method copy of DapperRepo above This method uses the Dapper Library to get multiple result sets from SQL Server and return it in a single Base Class Object so that one method is all what is required. Stored Procedure as Enum
        /// </summary>
        /// <param name="StoredProcedure">The Stored Procedure Name as Enum</param>
        /// <param name="Mode">The types of responses from a dapper SqlMapper.QueryMultiple query</param>
        /// <param name="headers">Request Headers</param>
        /// <param name="paramList">The paramwithvalues class array</param>
        /// <returns></returns>
        public static object DapperRepoPlatformSpecific(SP StoredProcedure,
                                                                    ResponseMode Mode,
                                                                    HttpRequestHeaders headers = null,
                                                                    params paramWithValues[] paramList)
        {
            //List<Models.ResponseModels.PushNotificationResponse> MainResponseModel2 = new List<Models.ResponseModels.PushNotificationResponse>();

            //return MainResponseModel2;            

            #region Request Header
            string Lang = "en";
            string secureToken = "";
            string env = "prod";

            List<MainResponseModel> MainResponseModel = new List<MainResponseModel>();

            if (headers != null)
            {
                Lang = headers.Contains("Lang") ? headers.GetValues("Lang").First() : "en";
                secureToken = headers.Contains("secureToken") ? headers.GetValues("secureToken").First() : "";
                env = headers.Contains("env") ? ((headers.GetValues("env").First() == "dev") ? "dev" : "prod") : "prod";
            }
            //To check if the given request is for developer or not. This is for cases when headers sending is not possible
            foreach (var item in paramList)
            {
                if (item.param == "@Dev")
                {
                    var index = paramList.ToList().IndexOf(item);
                    env = item.value;
                    var tempParamList = paramList.ToList();
                    tempParamList.RemoveAt(index);
                    paramList = tempParamList.ToArray();
                }
            }
            #endregion

            #region Security Token Check            
            if (!IsAuthenticated(secureToken)) //Enable After everything goes OKAY.
            {
                return new List<MainResponseModel>()
                    {
                        new MainResponseModel()
                        {
                            Response = new ResponseModel("-1", "Authentication Failed", "Authentication Failed")
                        }
                    };
            }
            #endregion

            #region SQL Server Stuff

            // This is SQL Connection only, do not mess with it
            SqlConnection con;
            // Have a check if the env (environment) header contains either Production or Development. Go to according database.
            string constr = ConfigurationManager.ConnectionStrings[(env == "dev" ? thedevConString : theConString)].ToString();
            con = new SqlConnection(constr);
            con.Open();


            // These are argument related only
            DynamicParameters args = new DynamicParameters(new { });
            string result = "0";
            string message = "";
            string additional = "";

            // Take all from paramWithValues list and turn in to Dapper Arguments
            foreach (paramWithValues item in paramList)
            {
                if (item.dbType.ToString() == "Object")
                {
                    args.Add(item.param, item.valueDT, item.dbType, item.paramDirection);
                }
                else
                {
                    args.Add(item.param, item.value, item.dbType, item.paramDirection);
                }
            }
            args.Add("@Lang", Lang);
            args.Add("@result", result, DbType.String, ParameterDirection.Output);
            args.Add("@message", message, DbType.String, ParameterDirection.Output);
            args.Add("@additional", additional, DbType.String, ParameterDirection.Output);

            #endregion

            var objDetails = SqlMapper.QueryMultiple(con, StoredProcedure.ToString(), param: args, commandType: CommandType.StoredProcedure);

            MainResponseModel ObjMaster = new MainResponseModel();

            #region Switch Statement
            // This will switch and call the appropriate function
            switch (Mode)
            {
                case ResponseMode.ResponseOnly:
                    break;
                default:
                    break;
            }
            #endregion

            ObjMaster.Response = GetResponseObject(args);

            MainResponseModel.Add(ObjMaster);
            con.Close();
            return ObjMaster; //It is always Android. :)
            //if (platform == "android") { return ObjMaster; } else { return MainResponseModel; }
        }

        public static ResponseModel GetResponseObject(DynamicParameters responseObj)
        {
            var resp = new ResponseModel();
            try
            {
                var _result = responseObj.Get<string>("result").ToString();
                var _message = responseObj.Get<string>("message").ToString();
                var _additional = responseObj.Get<string>("additional").ToString();
                resp = new ResponseModel(_result, _message, _additional);
            }
            catch (Exception e)
            {
                resp = new ResponseModel("0", e.ToString(), "Happy Coding with Error");
            }

            return resp;
        }

        #region Security

        /// <summary>
        /// Any request from client side will have an Authentication check using the MD5 Secure Token
        /// </summary>
        /// <param name="secureToken"></param>
        /// <returns></returns>
        public static bool IsAuthenticated(string secureToken)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                string hash = GetMd5Hash(md5Hash, "HULKSMASH");

                if (secureToken == hash)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        #endregion
    }
}