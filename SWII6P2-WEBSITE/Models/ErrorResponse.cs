using System.Text.Json.Serialization;

namespace SWII6P2_WEBSITE.Models
{
    public class ErrorResponse
    {
        [JsonPropertyName("message")] 
        public string Message { get; set; }
    }

}
