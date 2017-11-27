using System;
using System.Collections.Generic;

namespace SpectoLogic.Azure.Graph.Test.AdditionalProperties
{
    [GraphClass(ElementType =  GraphElementType.Vertex, SerializeTypeInformation = true)]
    public class ExtendablePerson : IExpandoVertex
    {
        public ExtendablePerson()
        {
            Id = Guid.NewGuid().ToString("D");
            Label = nameof(ExtendablePerson);
            InE = new List<IExpandoEdge>();
            OutE = new List<IExpandoEdge>();
            Version = 1;
            AdditionalProperties = new List<GraphProperty>();
        }
        public IList<IExpandoEdge> InE { get; set; }
        public IList<IExpandoEdge> OutE { get; set; }
        public string Id { get; set; }
        public string Label { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long Version { get; set; }

        [GraphProperty(DefinedProperty = GraphDefinedPropertyType.AdditionalProperties)]
        public IList<GraphProperty> AdditionalProperties { get; set; }
    }
}