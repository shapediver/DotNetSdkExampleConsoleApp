using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

using ShapeDiver.SDK.PlatformBackend;
using ShapeDiver.Newtonsoft.Json;
using PDTO = ShapeDiver.SDK.PlatformBackend.DTO;
using GDTO = ShapeDiver.SDK.GeometryBackend.DTO;
using ShapeDiver.SDK.GeometryBackend.Resources.Interfaces;

using CommandLine;
using System.Linq;
using ShapeDiver.SDK.GeometryBackend.DTO;
using System.Text;

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

                // keep a log of message and export it to a file
                var log = new StringBuilder();
                Action<string> logMessage = (string message) =>
                {
                    Console.WriteLine(message);
                    log.AppendLine(message);
                };

                // get model call
                logMessage($"Get model information for identifier {Identifier} ...");
                var model = (await sdk.PlatformClient.ModelApi.Get<PDTO.ModelDto>(Identifier, ModelGetEmbeddableFields.Backend_System)).Data;
                var token = (await sdk.PlatformClient.TokenApi.Create(model.Id, new List<PDTO.ModelTokenScopeEnum>() { PDTO.ModelTokenScopeEnum.GroupAnalytics }));

                // get instance of low-level geometry backend SDK
                var gbSdk = sdk.GeometryBackendClient.CreateLowLevelApi(model.BackendSystem.ModelViewUrl, token.Data.AccessToken);

                // query model computations
                var timestampFrom = DateTime.UtcNow.AddDays(-1.0 * Days).ToString("yyyyMMddHHmmssfff");
                var timestampTo = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                var order = RequestModelComputationQueryOrder.Desc;

                // collect computation stats for exporting a csv and json file
                List<GeometryBackendModelComputationDto> computations = new List<GeometryBackendModelComputationDto>();

                // keep a dictionary of components using most of the computation time
                Dictionary<string, ComponentStats> Components = new Dictionary<string, ComponentStats>();

                logMessage($"Fetch computations from log between {timestampFrom} and {timestampTo} ...");
                var response = await gbSdk.Model.QueryComputations(model.GeometryBackendId, timestampFrom, timestampTo, order: order);

                while ( true ) 
                {
                    if (response.Computations.Count > 0)
                    {
                        logMessage($"{response.Computations.First().Timestamp} - {response.Computations.Last().Timestamp}: {response.Computations.Count} computations");
                    }

                    computations.AddRange(response.Computations);

                    foreach (var computation in response.Computations)
                    {
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

                    // workaroung for timestamp filter bug
                    if (response.Computations.Count > 0)
                    {
                        if (response.Computations.Last().Timestamp < long.Parse(timestampFrom))
                        {
                            logMessage($"Workaround for timestamp filter bug: stop querying computations.");
                            break;
                        }
                    }

                    response = await gbSdk.Model.QueryComputations(model.GeometryBackendId, timestampFrom, timestampTo, order: order, offset: response.Pagination.NextOffset);
                }

                if (computations.Count == 0)
                {
                    logMessage($"No computations found.");
                    return;
                }

                // timestamp of the youngest computation
                var lastComputationTimestamp = computations.First().Timestamp.ToString();
                var prefix = $"{model.Slug}--{lastComputationTimestamp}";

                // build a csv file containing the component stats
                IEnumerable<ComponentStats> componentsOrderedByAvgTimeDesc = Components.Select(c => c.Value).OrderBy(c => c.AvgTime).Reverse();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Name,InstanceId,NickName,AvgTime,MaxTime,Count,TotalTime");
                foreach ( var c in componentsOrderedByAvgTimeDesc)
                {
                    sb.AppendLine($"{c.Component.Name},{c.Component.Instance},{c.Component.NickName},{N2S(c.AvgTime)},{N2S(c.MaxTime)},{c.Count},{N2S(c.TotalTime)}");
                }
                var componentCsv = sb.ToString();
                var componentCsvFilename = $"{prefix}--components-stats.csv";
                logMessage($"{Environment.NewLine}Exported information about {componentsOrderedByAvgTimeDesc.Count()} components sorted by decreasing average computation time to {componentCsvFilename}.");
                File.WriteAllText(componentCsvFilename, componentCsv);
           
                // csv file containing the stats for successful computations without exports
                var computationsCsvFilename = $"{prefix}--computations-stats.csv";
                var computationsSelected = computations.Where(c => c.Status == ModelComputationStatusEnum.Success && c.Exports.Count == 0);
                ExportComputationsSummaryToCsv(computationsCsvFilename, computationsSelected);
                logMessage($"{Environment.NewLine}Exported stats of {computationsSelected.Count()} successful computations to {componentCsvFilename}.");

                // csv file containing the stats for successful exports
                var exportsCsvFilename = $"{prefix}--exports-stats.csv";
                computationsSelected = computations.Where(c => c.Status == ModelComputationStatusEnum.Success && c.Exports.Count != 0);
                ExportComputationsSummaryToCsv(exportsCsvFilename, computationsSelected);
                logMessage($"{Environment.NewLine}Exported stats of {computationsSelected.Count()} successful exports to {exportsCsvFilename}.");

                // csv file containing the stats for failed computations and exports
                var failedCsvFilename = $"{prefix}--failed-stats.csv";
                computationsSelected = computations.Where(c => c.Status != ModelComputationStatusEnum.Success);
                ExportComputationsSummaryToCsv(failedCsvFilename, computationsSelected);
                logMessage($"{Environment.NewLine}Exported stats of {computationsSelected.Count()} failed computations and exports to {failedCsvFilename}.");

                // json file containing the stats for all computations
                var jsonFilename = $"{prefix}--computations-stats.json";
                File.WriteAllText(jsonFilename, JsonConvert.SerializeObject(computations, Formatting.Indented));
                logMessage($"{Environment.NewLine}Exported stats of all computations to {jsonFilename}.");

                // find and report extreme computations
                logMessage($"{Environment.NewLine}Summary statistics of successful computations:");
                var successfullComputations = computations.Where(c => c.Status == ModelComputationStatusEnum.Success);
                PrintSummaryStatistic(successfullComputations, c => c.Stats.TimeSolver, "Milliseconds used by Grasshopper solver (time_solver)", logMessage);
                PrintSummaryStatistic(successfullComputations, c => c.Stats.TimeSolverCollect, "Milliseconds used to collect data after solution (time_solver_collect)", logMessage);
                PrintSummaryStatistic(successfullComputations, c => c.Stats.TimeStorage, "Milliseconds used to store data (time_storage)", logMessage);
                PrintSummaryStatistic(successfullComputations, c => c.Stats.TimeProcessing, "Milliseconds used to process the request (time_processing)", logMessage);
                PrintSummaryStatistic(successfullComputations, c => c.Stats.TimeWait, "Milliseconds the request waited before being processed (time_wait)", logMessage);
                PrintSummaryStatistic(successfullComputations, c => c.Stats.TimeCompletion, "Milliseconds used to answer the request (time_completion)", logMessage);
                PrintSummaryStatistic(successfullComputations, c => c.Stats.SizeAssets, "Size of resulting data in bytes (size_assets)", logMessage);

                File.WriteAllText($"{prefix}--log.txt", log.ToString());
            });
        }

        void ExportComputationsSummaryToCsv(string filename, IEnumerable<GeometryBackendModelComputationDto> computations)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"timestamp,time_solver,time_solver_collect,time_storage,time_processing,time_wait,time_completion,size_assets,status");
            foreach (var computation in computations)
            {
                sb.AppendLine($"{computation.Timestamp},{computation.Stats.TimeSolver},{computation.Stats.TimeSolverCollect},{computation.Stats.TimeStorage},{computation.Stats.TimeProcessing},{computation.Stats.TimeWait},{computation.Stats.TimeCompletion},{computation.Stats.SizeAssets},{computation.Status}");
            }
            var csv = sb.ToString();
            File.WriteAllText(filename, csv);
        }

        void PrintSummaryStatistic(IEnumerable<GeometryBackendModelComputationDto> computations, Func<GeometryBackendModelComputationDto, int> selector, string description, Action<string> logMessage)
        {
            var values = computations.Select(c => selector(c)).ToList();
            var min = values.Min();
            var max = values.Max();
            var avg = values.Average();

            var sortedNumbers = values.OrderBy(n => n).ToList();
            Func<double, string> p = (double d) => N2S(CalculatePercentile(sortedNumbers, d));

            logMessage($"{description} - Min,Avg,Mag: {min},{N2S(avg)},{max} - p01,p05,p10,p25,p50,p75,p90,p95,p99: {p(0.01)},{p(0.05)},{p(0.1)},{p(0.25)},{p(0.5)},{p(0.75)},{p(0.9)},{p(0.95)},{p(0.99)}");
        }

        string N2S(double n)
        {
            return n.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Calculate the percentile of a sequence of numbers.
        /// </summary>
        /// <param name="sortedNumbers">must be ordered!</param>
        /// <param name="percentile"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        static double CalculatePercentile(List<int> sortedNumbers, double percentile)
        {
            if (sortedNumbers == null || sortedNumbers.Count < 2)
            {
                throw new ArgumentException("The sequence must contain at least two elements.", nameof(sortedNumbers));
            }

            // Calculate the index for the percentile
            double index = (percentile * (sortedNumbers.Count - 1)) + 1;

            // Interpolate to get the value at the calculated index
            int lowerIndex = (int)index;
            int upperIndex = lowerIndex + 1;

            double lowerValue = sortedNumbers[lowerIndex - 1];
            double upperValue = sortedNumbers[upperIndex - 1];

            double interpolatedValue = lowerValue + (index - lowerIndex) * (upperValue - lowerValue);

            return interpolatedValue;
        }

    }

    /// <summary>
    /// Stats about a component of the model.
    /// </summary>
    class ComponentStats
    {
        /// <summary>
        /// Information about the component. 
        /// </summary>
        public GeometryBackendModelComputationComponentComputedDto Component { get; set; }

        /// <summary>
        /// Total computation time for this component (sum of all computation times).
        /// </summary>
        public double TotalTime { get; private set; }

        /// <summary>
        /// Maximum computation time for this component
        /// </summary>
        public double MaxTime { get; private set; }

        /// <summary>
        /// Number of computations for this component.
        /// Note that the computation logs only record the computations of those 10 components which 
        /// took most computation time. 
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Average computation time for this component.
        /// </summary>
        public double AvgTime => TotalTime / Count;

        public void RegisterComputation(double time)
        {
            TotalTime += time;
            MaxTime = Math.Max(MaxTime, time);
            Count++;
        }
    }

}
