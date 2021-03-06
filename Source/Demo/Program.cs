﻿using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Graphs;
using Microsoft.Azure.Graphs.Elements;
using SpectoLogic.Azure.CosmosDB;
using SpectoLogic.Azure.Graph;
using SpectoLogic.Azure.Graph.Extensions;
using SpectoLogic.Azure.Graph.Serializer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Demo
{
    class Program
    {
        #region Configuration
        static string Account_DemoBuild_Hobbit;
        static string Account_DemoBuild_Hobbit_Graph;
        static string Account_DemoBuild_Hobbit_Key;
        #endregion

        static void Main(string[] args)
        {
            #region Read Configuration
            Account_DemoBuild_Hobbit = ConfigurationManager.AppSettings["Account_DemoBuild_Hobbit"];
            Account_DemoBuild_Hobbit_Graph = ConfigurationManager.AppSettings["Account_DemoBuild_Hobbit_Graph"];
            Account_DemoBuild_Hobbit_Key = ConfigurationManager.AppSettings["Account_DemoBuild_Hobbit_Key"];
            #endregion

            Task.Run(async () =>
            {
                try
                {
                    await Program.Demo();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed with {ex.Message}.");
                }
            }).Wait();

            Console.WriteLine("Demo completed. Please press THE ANY-key to continue!");
            Console.ReadLine();
        }
        static async Task Demo()
        {
            try
            {
                Console.WriteLine("=========================================================");
                Console.WriteLine("Demo for the SpectoLogic.Azure.CosmosDB Extension Library");
                Console.WriteLine("(c) by SpectoLogic e.U. 2017");
                Console.WriteLine("written by Andreas Pollak");
                Console.WriteLine("Licensed under the MIT License");
                Console.WriteLine("=========================================================");

                // Connect with DocumentClient and create necessary Database and Collection 
                Console.Write("Creating Collection 'thehobbit'...");
                DocumentClient client = await CosmosDBHelper.ConnectToCosmosDB(Account_DemoBuild_Hobbit, Account_DemoBuild_Hobbit_Key);
                Database db = await CosmosDBHelper.CreateOrGetDatabase(client, "demodb");
                DocumentCollection collection = await CosmosDBHelper.CreateCollection(client, db, "thehobbit", 400, null, null, false);
                Console.WriteLine("Done");

                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine("DEMO: Delivery Demo ");
                Console.WriteLine("---------------------------------------------------------");

                Delivery.Demo d = new Delivery.Demo();
                await d.Execute(client, collection);
                
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine("DEMO: Create custom objects and populate cosmosdb graph");
                Console.WriteLine("---------------------------------------------------------");

                Place cave = new Place() { name = "Cave of Hobbit" };
                Place restaurant = new Place() { name = "Restaurant Green Dragon" };
                Place europe = new Place() { name = "Europe",
                                             country = GraphProperty.Create("country", "AT", "MetaTag1", "Austria").AddValue("FI", "MetaTag1", "Finnland")
                };
                Path hobbitPath = new Path(cave, restaurant, 2); //TODO: find out why 2.3 has an issue

                await client.CreateGraphDocumentAsync<Place>(collection, cave);
                await client.CreateGraphDocumentAsync<Place>(collection, restaurant);
                await client.CreateGraphDocumentAsync<Place>(collection, europe);
                await client.CreateGraphDocumentAsync<Path>(collection, hobbitPath);

                Console.WriteLine("----------------------------------------------------------------------------------");
                Console.WriteLine("DEMO: Usage of 'ExecuteNextAsyncAsPOCO<T>'-Extension Method on typed Gremlin Query");
                Console.WriteLine("----------------------------------------------------------------------------------");

                MemoryGraph partialGraph = new MemoryGraph();

                string gremlinQueryStatement = "g.V().hasLabel('place')";
                Console.WriteLine($"Executing gremlin query as string: {gremlinQueryStatement}");
                var germlinQuery = client.CreateGremlinQuery<Vertex>(collection, gremlinQueryStatement);
                while (germlinQuery.HasMoreResults)
                {
                    // It is not required to pass in a context like partialGraph here. This parameter can be omitted.
                    foreach (var result in await germlinQuery.ExecuteNextAsyncAsPOCO<Place>(partialGraph))
                    {
                        Console.WriteLine($"Vertex ==> Label:{result.Label} Name:{result.name}");
                    }
                }

                #region EXPERIMENTAL DEMO
                /// =================================================================================================
                /// IMPORTANT: The following code makes use of the internal GraphTraversal class, which should not
                /// be used according to the documentation of Microsofts Graph Library. Use at your own risk.
                /// =================================================================================================
                Console.WriteLine("--------------------------------------------------------------------");
                Console.WriteLine("DEMO: Usage of 'NextAsPOCO<T>' with GraphCommand and GraphTraversal ");
                Console.WriteLine("--------------------------------------------------------------------");
                Console.Write("Connecting with GraphConnection object...");
                // Connect with GraphConnection
                object graphConnection = GraphConnectionFactory.Create(client, collection);
                Console.WriteLine("Done");

                // Drop previous context (optional if the same graph)
                partialGraph.Drop();

                Microsoft.Azure.Graphs.GraphCommand cmd = GraphCommandFactory.Create(graphConnection);

                GraphTraversal placeTrav = cmd.g().V().HasLabel("place"); 
                GraphTraversal edgeTrav = cmd.g().E().HasLabel("path");
                {
                    Console.WriteLine("Retrieving all places with 'NextAsPOCO'-Extension on GraphTraversal ");
                    // Returns a list of all vertices for place
                    var places = await placeTrav.NextAsPOCO<Place>(partialGraph);
                    foreach (Place place in places)
                    {
                        Console.WriteLine($"Vertex ==> Label:{place.Label} Name:{place.name}");
                    }
                }

                Console.WriteLine("--------------------------------------------------------------------");
                Console.WriteLine("DEMO: Direct Usage of GraphSerializer<T> with GraphTraversal  ");
                Console.WriteLine("--------------------------------------------------------------------");
                // Drop previous context (optional if the same graph)
                partialGraph.Drop();

                Console.WriteLine("Iterating over GraphTraversal Places (Vertices) and deserializing GraphSON to custom object ");
                IGraphSerializer<Place> placeGraphSerializer = GraphSerializerFactory.CreateGraphSerializer<Place>(partialGraph);
                foreach (var p in placeTrav)
                {
                    IList<Place> places = placeGraphSerializer.DeserializeGraphSON(p); // Returns more than one result in each call
                    foreach(Place place in places)
                    {
                        Console.WriteLine($"Vertex ==> Label:{place.Label} Name:{place.name}");
                        Console.WriteLine("Serializing to CosmosDB internal represenation: ");
                        string docDBJson = placeGraphSerializer.ConvertToDocDBJObject(place).ToString();
                        Console.WriteLine($"JSON ==> {docDBJson}");
                    }
                }

                Console.WriteLine("Iterating over GraphTraversal Paths (Edges) and deserializing GraphSON to custom object ");
                IGraphSerializer<Path> pathGraphSerializer = GraphSerializerFactory.CreateGraphSerializer<Path>(partialGraph);
                foreach (var p in edgeTrav)
                {
                    IList<Path> paths = pathGraphSerializer.DeserializeGraphSON(p); // Returns more than one result in each loop
                    foreach (Path path in paths)
                    {
                        Console.WriteLine($"Edge ==> Label:{path.Label} Weight:{path.weight}");
                        Console.WriteLine("Serializing to CosmosDB internal represenation: ");
                        string docDBJson = pathGraphSerializer.ConvertToDocDBJObject(path).ToString();
                        Console.WriteLine($"JSON ==> {docDBJson}");
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError:");
                Console.WriteLine($"Demo failed with {ex.Message}.");
            }
        }
    }
}
