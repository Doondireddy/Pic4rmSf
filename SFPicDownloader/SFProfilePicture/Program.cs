using Common_SalesforceFunction.SFDC;
using Newtonsoft.Json;
using SFProfilePicture.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace SFProfilePicture
{
    static class Program
    {
        public static string UserName = "cnusim@gmail.com";
        public static string Password = "Appa!2019";
        public static string SecurityToken = "3L4YAOfNGvbebVv0maLnqvwy";//sDhNhhlEo1snwGMK4L0KRXitT

        static void Main(string[] args)
        {
            ImageDownload();
            Console.ReadKey();
        }
        public static void ImageDownload()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            LoginResult currentLoginResult = null;
            SforceService sfdcBinding = new SforceService();
            currentLoginResult = sfdcBinding.login(UserName, Password + SecurityToken);

            //Change the binding to the new endpoint
            sfdcBinding.Url = currentLoginResult.serverUrl;
            //Create a new session header object and set the session id to that returned by the login
            sfdcBinding.SessionHeaderValue = new SessionHeader();
            sfdcBinding.SessionHeaderValue.sessionId = currentLoginResult.sessionId;
            string sfSessionId = currentLoginResult.sessionId;
            QueryResult queryResult = null;
            if (sfdcBinding != null)
            {
                //String SOQL = "SELECT id,IsProfilePhotoActive,FullPhotoUrl,LastName ,FirstName FROM User where isActive = true and IsProfilePhotoActive=true";
                String SOQL = "select Id, name, Phone__c,fee__c,Head_Master__c, City__c, Board__c,Languages_offered__c,State__c from School__c";
                queryResult = sfdcBinding.query(SOQL);
            }
            if (queryResult.size > 0)
            {
               // SFImages(queryResult, sfSessionId);
                SchoolData(queryResult);
            }
        }


        public static void SFImages(QueryResult queryResult, string sfSessionId)
        {
            //Please provide a local folder path to download the Picture.
            string localFolder = @"D:\My_SF\SF_PP\";
            for (int i = 0; i < queryResult.size; i++)
            {
                User users = (User)queryResult.records[i];
                using (WebClient client = new WebClient())
                {
                    HttpWebRequest lxRequest = (HttpWebRequest)WebRequest.Create(users.FullPhotoUrl);
                    lxRequest.Headers.Add("Authorization", "Bearer " + sfSessionId);
                    lxRequest.Method = HttpMethod.Get.ToString();
                    // returned values are returned as a stream, then read into a string
                    String lsResponse = string.Empty;
                    using (HttpWebResponse lxResponse = (HttpWebResponse)lxRequest.GetResponse())
                    {
                        using (BinaryReader reader = new BinaryReader(lxResponse.GetResponseStream()))
                        {
                            Byte[] lnByte = reader.ReadBytes(1 * 1024 * 1024 * 10);
                            using (FileStream lxFS = new FileStream(localFolder + users.FirstName + " " + users.LastName + ".png", FileMode.Create))
                            {
                                lxFS.Write(lnByte, 0, lnByte.Length);
                            }
                        }
                    }
                }
            }
        }

        public static void SchoolData(QueryResult queryResult)
        {
            List<School> map = new List<School>();
            for (int i = 0; i < queryResult.size; i++)
            {
                School__c school = (School__c)queryResult.records[i];
                School sch = new School();
                sch.id = school.Id;
                sch.Name = school.Name;
                sch.Phone = school.Phone__c;
                sch.fee = school.Fee__c?.ToString();
                sch.Head_Master = school.Head_Master__c;
                sch.City = school.City__c;
                sch.Board = school.Board__c;
                sch.State = school.State__c;
                sch.Languages_offered = school.Languages_Offered__c;
                map.Add(sch);
            }
            string localFolder = @"D:\My_SF\SF_PP\";
            string fileName = "Schools.csv";
            string delimiter = ",";
            //DeserializeObject 
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(map);
            //Constructing DataTable
            StringBuilder sBuilder = new StringBuilder();
            DataTable SchoolData = JsonConvert.DeserializeObject<DataTable>(json);
            if (!SchoolData.Equals(null))
            {
                string[] columnNames = SchoolData.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
                sBuilder.AppendLine(string.Join(delimiter, columnNames));
                foreach (DataRow row in SchoolData.Rows)
                {
                    string[] fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                    sBuilder.AppendLine(string.Join(delimiter, fields));
                }//foreach
            }//if
            File.WriteAllText(localFolder + fileName, sBuilder.ToString());
        }
    }
}


