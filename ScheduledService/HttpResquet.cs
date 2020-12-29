using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ScheduledService
{
    class HttpResquet
    {
        public string HttpGet(string getUrl)
        {
            string str = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(getUrl);
            request.ContentType = "application/json";
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    str = reader.ReadToEnd().ToString();
                }
            }
            return str;
        }
        public string HttpPost(string postUrl, string paramData, Encoding dataEncode)
        {
            string str = string.Empty;
            try
            {
                byte[] bytes = dataEncode.GetBytes(paramData);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default))
                    {
                        return reader.ReadToEnd().ToString();
                    }
                }
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
            return str;
        }
    }
}
