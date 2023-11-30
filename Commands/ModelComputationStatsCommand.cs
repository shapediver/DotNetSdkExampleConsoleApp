using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

using ShapeDiver.SDK.PlatformBackend;
using PDTO = ShapeDiver.SDK.PlatformBackend.DTO;
using GDTO = ShapeDiver.SDK.GeometryBackend.DTO;
using ShapeDiver.SDK.GeometryBackend.Resources.Interfaces;

using CommandLine;
using System.Linq;
using ShapeDiver.SDK.GeometryBackend.DTO;

namespace DotNetSdkSampleConsoleApp.Commands
{
    /// <summary>
    /// Fetch data from the log of computations of a ShapeDiver model,
    /// display some statistics of the Grasshopper components taking 
    /// most of the computation time. 
    /// </summary>
    [Verb("model-computation-stats", isDefault: false, HelpText = "Fetch data from the log of computations of a ShapeDiver model, display some statistics.")]
    class ModelComputationStatsCommand : BaseCommand, ICommand
    {
        [Option('i', "identifier", HelpText = "Identifier of the model (slug, model id, geometry backend model id, or ticket)", Required = false)]
        public string Identifier { get; set; }

        [Option('d', "days", HelpText = "Number of past days to inspect computation stats for")]
        public int Days { get; set; }

        public async Task Execute()
        {
            await WrapExceptions(async () =>
            {
                // use at least one day
                Days = Days <= 0 ? 1 : Days;

                // get identifier
                if (String.IsNullOrEmpty(Identifier))
                {
                    Console.Write("Model identifier (slug, model id, geometry backend model id): ");
                    Identifier = Console.ReadLine();
                }
                if (String.IsNullOrEmpty(Identifier))
                {
                    throw new Exception($"Refusing to proceed without a model identifier");
                }

                // in case the identifier is a url, guess the slug from it
                if (Identifier.StartsWith("https://"))
                    Identifier = Identifier.Split('/').Last();

                // get authenticated SDK
                var sdk = await GetAuthenticatedSDK();

                // in case a ticket was given, try to decrypt it
                if (Identifier.Length > 100 && Identifier.Split('-').Count() == 2 && Identifier.ToLowerInvariant() == Identifier)
                {
                    Console.WriteLine($"Looks like a ticket, trying to decrypt it...");
                    var ticketDecrypted = await sdk.PlatformClient.ModelApi.DecryptTicket(Identifier);
                    Identifier = ticketDecrypted.Model.Id;
                }

                // get model call
                Console.WriteLine($"Get model information...");
                var model = (await sdk.PlatformClient.ModelApi.Get<PDTO.ModelDto>(Identifier, ModelGetEmbeddableFields.Backend_System)).Data;
                var token = (await sdk.PlatformClient.TokenApi.Create(model.Id, new List<PDTO.ModelTokenScopeEnum>() { PDTO.ModelTokenScopeEnum.GroupAnalytics }));

                // get instance of low-level geometry backend SDK
                var gbSdk = sdk.GeometryBackendClient.CreateLowLevelApi(model.BackendSystem.ModelViewUrl, token.Data.AccessToken);

                // query model computations
                var timestampFrom = DateTime.UtcNow.AddDays(-1.0 * Days).ToString("yyyyMMddHHmmssfff");
                var timestampTo = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                var order = RequestModelComputationQueryOrder.Desc;

                // keep a dictionary of components taking using most of the computation time
                Dictionary<string, ComponentStats> Components = new Dictionary<string, ComponentStats>();

                Console.WriteLine($"Fetch computations from log between {timestampFrom} and {timestampTo} ...");
                var response = await gbSdk.Model.QueryComputations(model.GeometryBackendId, timestampFrom, timestampTo, order: order);

                while ( true ) 
                {
                    foreach (var computation in response.Computations)
                    {
                        Console.WriteLine($"{computation.Timestamp}, {computation.ComputeRequestId}, {computation.Status}");

                        foreach (var component in computation.Stats.Model.Components.Computed)
                        {
                            if (!Components.ContainsKey(component.Instance))
                            {
                                Components[component.Instance] = new ComponentStats() { Component = component };
                            }
                            Components[component.Instance].RegisterComputation(component.Time);
                        }
                    }

                    if (String.IsNullOrEmpty(response.Pagination.NextOffset))
                        break;
                 
                    response = await gbSdk.Model.QueryComputations(model.GeometryBackendId, timestampFrom, timestampTo, order: order, offset: response.Pagination.NextOffset);
                }

                IEnumerable<ComponentStats> componentsOrderedByAvgTimeDesc = Components.Select(c => c.Value).OrderBy(c => c.AvgTime).Reverse();

                Console.WriteLine($"Components by decreasing average computation time:");
                Console.WriteLine($"Name,InstanceId,NickName,AvgTime,MaxTime,Count,TotalTime");
                foreach ( var c in componentsOrderedByAvgTimeDesc)
                {
                    Console.WriteLine($"{c.Component.Name},{c.Component.Instance},{c.Component.NickName},{c.AvgTime},{c.MaxTime},{c.Count},{c.TotalTime}");
                }
  
            });

        }
    }

    class ComponentStats
    {
        public GeometryBackendModelComputationComponentComputedDto Component { get; set; }
        public double TotalTime { get; private set; }
        public double MaxTime { get; private set; }
        public int Count { get; private set; }

        public double AvgTime => TotalTime / Count;

        public void RegisterComputation(double time)
        {
            TotalTime += time;
            MaxTime = Math.Max(MaxTime, time);
            Count++;
        }
    }

}
