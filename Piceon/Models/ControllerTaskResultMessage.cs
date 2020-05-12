using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Piceon.Models
{
    public class ControllerTaskResultMessage
    {
        public int taskid { get; set; }
        public string result { get; set; }
        public string error_message { get; set; }
        public List<List<int>> images { get; set; } = new List<List<int>>();

        private static JSchema MessageJsonSchema = JSchema.Parse(
            "{" +
                "\"description\": \"ControllerTaskResultMessage\"," +
                "\"type\": \"object\"," +
                "\"properties\": {" +
                    "\"taskid\": {\"type\": \"number\"}," +
                    "\"result\": {\"type\": \"string\"}," +
                    "\"error_message\": {\"type\": [\"string\",\"null\"]}," +
                    "\"images\": {" +
                        "\"type\": \"array\"," +
                        "\"items\": {" +
                            "\"type\": \"array\"," +
                            "\"items\": {\"type\": \"string\"}" +
                        "}" +
                    "}" +
                "}" +
            "}");

        public static ControllerTaskResultMessage FromJson(string json)
        {
            var result = JsonConvert.DeserializeObject<ControllerTaskResultMessage>(json);

            return result;
        }

        public static bool IsJsonValidMessage(string json)
        {
            JObject jobject = JObject.Parse(json);

            return jobject.IsValid(MessageJsonSchema);
        }
    }
}
