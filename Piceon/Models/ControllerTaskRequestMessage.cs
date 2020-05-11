using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Services.Maps;

namespace Piceon.Models
{
    public class ControllerTaskRequestMessage
    {
        public int taskid { get; set; }
        public int type { get; set; }
        public List<string> images { get; set; } = new List<string>();

        public string ToJson()
        {
            string result = "";

            result = JsonConvert.SerializeObject(this);

            return result;
        }

        public static ControllerTaskRequestMessage FromJson(string json)
        {
            var result = JsonConvert.DeserializeObject<ControllerTaskRequestMessage>(json);

            return result;
        }
    }
}
