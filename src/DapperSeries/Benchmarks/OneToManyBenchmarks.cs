
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using DapperSeries.Entities;
using Microsoft.Extensions.Configuration;

namespace DapperSeries.Benchmarks
{
    [MemoryDiagnoser]
    public class OneToManyBenchmarks
    {
        private Controllers.ScheduledFlightController controller;

        public OneToManyBenchmarks() {
            controller = new Controllers.ScheduledFlightController(null);

        }
        [Benchmark]
        public async Task<IEnumerable<ScheduledFlight>> MultiMapping()
        {
            return await controller.Get("YYC");
        }

        [Benchmark]
        public async Task<IEnumerable<ScheduledFlight>> MultipleResultSets()
        {
            return await controller.GetAlt("YYC");
        }

    }
}