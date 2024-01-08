using System.Collections.Generic;

namespace ConvertXML
{
    public struct customTable
    {
        public customTable(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }

        public override string ToString() => $"({Id}, {Name})";
    }
}
