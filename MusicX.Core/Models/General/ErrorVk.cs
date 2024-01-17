using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicX.Core.Models.General
{
    public class ErrorVk
    {
        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("error_msg")]
        public string ErrorMsg { get; set; }

        [JsonProperty("request_params")]
        public List<RequestParam> RequestParams { get; set; }
    }

    public class RequestParam
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
