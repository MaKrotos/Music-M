using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatSlyLib.Models
{
    public class Event
    {
        public class CustomDateTimeConverter : IsoDateTimeConverter
        {
            public CustomDateTimeConverter()
            {
                base.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            }
        }


        public Event(string name, DateTime? event_time = null, List<EventParams>? eventParams = null)
        {
            this.name = name;
            this.event_time = event_time;
            this.eventParams = eventParams;

            

        }
        [JsonProperty("eventName")]
        public string name { get; set; }


        [JsonProperty("event_time", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? event_time { get; set; }
        [JsonProperty("params")]
        public List<EventParams>? eventParams {get; set;}
    }
}
