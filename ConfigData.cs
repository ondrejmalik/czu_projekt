public class ConfigData
{
    public required Colors colors { get; set; }

    public required CustomRegex custom_regex { get; set; }

    public class Colors
    {
        public required string highlightA { get; set; }
        public required string highlightB { get; set; }
        public required string highlightC { get; set; }
    }

    public class CustomRegex
    {
        public required string custom_1 { get; set; }
        public required string custom_2 { get; set; }
        public required string custom_3 { get; set; }
    }
}
