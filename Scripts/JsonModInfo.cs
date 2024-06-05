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
    public string[] Authors { get; set; }
    public Dictionary<string, string> Contact { get; set; }
    public Dictionary<string, string> Depends { get; set; }
}
