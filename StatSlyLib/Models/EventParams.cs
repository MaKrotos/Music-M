using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatSlyLib.Models
{
    public class EventParams
    {
        public EventParams(string paramName, string? paramValueString)
        {
            ParamName = paramName;
            ParamValueString = paramValueString;
        }
        public EventParams(string paramName, int? paramValueINT)
        {
            ParamName = paramName;
            ParamValueINT = paramValueINT;
        }
        public EventParams(string paramName, int? paramValueINT, string? paramValueString)
        {
            ParamName = paramName;
            ParamValueINT = paramValueINT;
            ParamValueString = paramValueString;
        }

        [JsonProperty("paramName")]
        public string ParamName { get; set; }
        [JsonProperty("number")]
        public int? ParamValueINT { get; set; }
        [JsonProperty("text")]
        public string? ParamValueString { get; set; }
    }
}
