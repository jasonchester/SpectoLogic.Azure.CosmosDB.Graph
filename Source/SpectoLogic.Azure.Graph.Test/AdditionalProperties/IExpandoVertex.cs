using System.Dynamic;
using System.Linq.Expressions;
using Dynamitey;
using Dynamitey.DynamicObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpectoLogic.Azure.Graph.Test.Delivery;

namespace SpectoLogic.Azure.Graph.Test.AdditionalProperties
{
    public interface IExpandoVertex : IVertex<IExpandoEdge, IExpandoEdge>
    {
    }
}
