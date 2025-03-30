
using Newtonsoft.Json.Linq;

namespace AutoCADMCP
{
    public class Command
    {
        public string Type { get; set; }
        public JObject Parameters { get; set; }
    }
}
