using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WolcenEditor
{
    public static class OnlineBuildRequest
    {

        public static string RequestBuild(string buildID)
        {
            string output = "";
            WebRequest request = WebRequest.Create("https://wolcen-universe.com/graphql");
            request.Method = "POST";

            var query = "{\"operationName\":\"BuildsIdQuery\",\"variables\":{\"buildId\":\""+ buildID +"\"},\"query\":\"query BuildsIdQuery($buildId: ResourceId!) {  build(buildId: $buildId) { passiveSkillTree   }}\"}";
            byte[] byteArray = Encoding.UTF8.GetBytes(query);

            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            WebResponse response = request.GetResponse();

            using (dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.  
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.  
                string responseFromServer = reader.ReadToEnd();
                // Display the content.  
                output = responseFromServer;
            }

            return output;
        }
    }
}
