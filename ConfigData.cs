public class ConfigData
{
    public Colors colors { get; set; } = new Colors();
    public CustomRegex custom_regex { get; set; } = new CustomRegex();

    public class Colors
    {
        public string highlightA { get; set; } = string.Empty;
        public string highlightB { get; set; } = string.Empty;
        public string highlightC { get; set; } = string.Empty;
    }

    public class CustomRegex
    {
        public string custom_1 { get; set; } = string.Empty;
        public string custom_2 { get; set; } = string.Empty;
        public string custom_3 { get; set; } = string.Empty;
    }
}
