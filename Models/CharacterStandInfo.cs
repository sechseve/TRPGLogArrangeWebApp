using System.Collections.Generic;

namespace TRPGLogArrangeTool.Blazor.Models
{
    public class CharacterStandInfo
    {
        public string Name { get; set; }
        public Dictionary<string, string> StandDictionary { get; set; } = new Dictionary<string, string>();
    }
}
