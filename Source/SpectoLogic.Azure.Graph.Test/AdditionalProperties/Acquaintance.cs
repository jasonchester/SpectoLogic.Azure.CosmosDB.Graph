using System;

namespace SpectoLogic.Azure.Graph.Test.AdditionalProperties
{
    [GraphClass(ElementType = GraphElementType.Edge, SerializeTypeInformation = true)]
    public class Acquaintance : IExpandoEdge
    {

        public Acquaintance()
        {
            Id = Guid.NewGuid().ToString("D");
            Label = nameof(Acquaintance);
        }

        public string Id { get; set; }
        public string Label { get; set; }
        public IExpandoVertex InV { get; set; }
        public IExpandoVertex OutV { get; set; }
    }
}
