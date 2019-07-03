using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;

namespace FunctionTestUtils
{
    public static class Utils
    {
        public static HttpRequest CreateMockRequest(object body)
        {            
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
 
            var json = JsonConvert.SerializeObject(body);
 
            sw.Write(json);
            sw.Flush();
 
            ms.Position = 0;

            var mockRequest = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = ms
            };
 
            return mockRequest;
        }
    }
}