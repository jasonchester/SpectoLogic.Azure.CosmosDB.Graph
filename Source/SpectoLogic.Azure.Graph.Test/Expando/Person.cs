using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Dynamitey;
using Dynamitey.DynamicObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpectoLogic.Azure.Graph.Test.Delivery;

namespace SpectoLogic.Azure.Graph.Test.Expando
{
    public interface IExpandoVertex : IVertex<IExpandoEdge, IExpandoEdge>
    {
    }

    [GraphClass(ElementType =  GraphElementType.Vertex, SerializeTypeInformation = true)]
    public class User : IExpandoVertex
    {
        public User()
        {
            Id = Guid.NewGuid().ToString("D");
            Label = "Person";
            InE = new List<IExpandoEdge>();
            OutE = new List<IExpandoEdge>();
            AdditionalProperties = new List<GraphProperty>();
        }
        public IList<IExpandoEdge> InE { get; set; }
        public IList<IExpandoEdge> OutE { get; set; }
        public string Id { get; set; }
        public string Label { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [GraphProperty(DefinedProperty = GraphDefinedPropertyType.Extension)]
        public IList<GraphProperty> AdditionalProperties { get; set; }
    }

    public interface IExpandoEdge : IEdge<IExpandoVertex, IExpandoVertex>
    {
    }

    [GraphClass(ElementType = GraphElementType.Edge, SerializeTypeInformation = true)]
    public class Acquaintance : IExpandoEdge
    {
        public Acquaintance(string id, string label, IExpandoVertex inV, IExpandoVertex outV)
        {
            Id = id;
            Label = label;
            InV = inV;
            OutV = outV;
        }

        public string Id { get; set; }
        public string Label { get; set; }
        public IExpandoVertex InV { get; set; }
        public IExpandoVertex OutV { get; set; }
    }
}
