using System.Reflection;
using BenchmarkDotNet.Running;

// Two experiments live in this assembly (see docs/journal/span-first-experiments.md). Run one at a time
// with a filter, e.g. `dotnet run -c Release --project bench/Lexi.Benchmarks -- --filter *Experiment1*`.
_ = BenchmarkSwitcher
    .FromAssembly(Assembly.GetExecutingAssembly())
    .Run(args);
