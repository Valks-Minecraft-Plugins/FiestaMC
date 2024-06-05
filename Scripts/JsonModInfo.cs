using System.Text.Json.Serialization;

namespace FiestaMC;

public class JsonModInfo
{
    public string FileName { get; set; }
    public string FolderName { get; set; }

    // JSON Values
    public string Id { get; set; }
    public string Version { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public object[] Authors { get; set; }
    public Dictionary<string, string> Contact { get; set; } = new();

    // Please note Value must be of type object because some versions could be like "1.2.0" or ["1.1.0", "1.3.0"]
    public Dictionary<string, object> Depends { get; set; } = new();
}
