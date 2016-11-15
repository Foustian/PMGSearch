using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Net;


namespace PMGSearch
{
    static class RestClient
    {
        public static String getXML(String URL, List<KeyValuePair<string, string>> vars, bool IsPMGLogging, string PMGLogFileLocation, Int32? timeOutPeriod = null, string RawParam = null)
        {
            try
            {

                Uri address = new Uri(URL);
                String ret = string.Empty;

                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = timeOutPeriod == null ? 210000 : (Int32)timeOutPeriod;

                StringBuilder data = new StringBuilder();
                int c = 0;
                foreach (KeyValuePair<String, String> kvp in vars)
                {
                    if (c > 0) data.Append("&");
                    data.Append(kvp.Key + "=" + HttpUtility.UrlEncode(kvp.Value));
                    c++;
                }

                data = data.Append(!string.IsNullOrWhiteSpace(RawParam) ? RawParam : string.Empty);

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                string _URL = URL + data.ToString(); 

                CommonFunction.LogInfo(_URL, IsPMGLogging, PMGLogFileLocation);


                request.ContentLength = byteData.Length;

                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    ret = reader.ReadToEnd();

                    //CommonFunction.LogInfo(ret,IsPMGLogging,PMGLogFileLocation);

                    return ret;
                }
            }
            catch (TimeoutException ex)
            {
                throw;

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public static String getXML(String URL, List<KeyValuePair<string, string>> vars, bool IsPMGLogging, string PMGLogFileLocation, out string RequestURL, string RawParam = null)
        {
            try
            {

                Uri address = new Uri(URL);
                String ret = string.Empty;

                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 210000;

                StringBuilder data = new StringBuilder();
                int c = 0;
                foreach (KeyValuePair<String, String> kvp in vars)
                {
                    if (c > 0) data.Append("&");
                    data.Append(kvp.Key + "=" + HttpUtility.UrlEncode(kvp.Value));
                    c++;
                }

                data = data.Append(!string.IsNullOrWhiteSpace(RawParam) ? RawParam : string.Empty);

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                string _URL = URL + data.ToString();

                CommonFunction.LogInfo(_URL, IsPMGLogging, PMGLogFileLocation);
                RequestURL = _URL.Remove(URL.LastIndexOf("/")) + "?" + data.ToString();

                request.ContentLength = byteData.Length;

                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    ret = reader.ReadToEnd();

                    //CommonFunction.LogInfo(ret,IsPMGLogging,PMGLogFileLocation);

                    return ret;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static String getFacet(String URL, List<KeyValuePair<string, string>> vars, bool IsPMGLogging, string PMGLogFileLocation)
        {
            try
            {

                Uri address = new Uri(URL);
                String ret = string.Empty;

                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 210000;

                StringBuilder data = new StringBuilder();
                int c = 0;
                foreach (KeyValuePair<String, String> kvp in vars)
                {
                    if (c > 0) data.Append("&");
                    data.Append(kvp.Key + "=" + HttpUtility.UrlEncode(kvp.Value));
                    c++;
                }

                byte[] byteData = UTF8Encoding.UTF8.GetBytes(data.ToString());

                string _URL = URL + data.ToString();

                CommonFunction.LogInfo(_URL, IsPMGLogging, PMGLogFileLocation);


                request.ContentLength = byteData.Length;

                using (Stream postStream = request.GetRequestStream())
                {
                    postStream.Write(byteData, 0, byteData.Length);
                }

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    ret = reader.ReadToEnd();

                    //CommonFunction.LogInfo(ret,IsPMGLogging,PMGLogFileLocation);

                    return ret;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

    }
}
