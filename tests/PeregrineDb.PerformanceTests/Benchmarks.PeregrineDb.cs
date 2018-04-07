﻿namespace PeregrineDb.PerformanceTests
{
    using System.Data.SqlClient;
    using System.Linq;
    using BenchmarkDotNet.Attributes;
    using PeregrineDb.Databases;

    public class PeregrineDbBenchmarks
        : BenchmarkBase
    {
        private IDatabase<SqlConnection> database;

        [GlobalSetup]
        public void Setup()
        {
            this.BaseSetup();
            this.database = DefaultDatabase.From(this._connection, PeregrineConfig.SqlServer2012, true);
        }

        [Benchmark(Description = "Query<T>")]
        public Post QueryUnbuffered()
        {
            this.Step();
            return this.database.Query<Post>($"select * from Posts where Id = {this.i}").First();
        }

        [Benchmark(Description = "Query<dyanmic> (unbuffered)")]
        public dynamic QueryUnbufferedDynamic()
        {
            this.Step();
            return this.database.Query<dynamic>($"select * from Posts where Id = {this.i}").First();
        }

        [Benchmark(Description = "QueryFirstOrDefault<T>")]
        public Post QueryFirstOrDefault()
        {
            this.Step();
            return this.database.QueryFirstOrDefault<Post>($"select * from Posts where Id = {this.i}");
        }

        [Benchmark(Description = "QueryFirstOrDefault<dyanmic>")]
        public dynamic QueryFirstOrDefaultDynamic()
        {
            this.Step();
            return this.database.QueryFirstOrDefault<dynamic>($"select * from Posts where Id = {this.i}").First();
        }

        [Benchmark(Description = "Get<T>")]
        public Post ContribGet()
        {
            this.Step();
            return this.database.Get<Post>(this.i);
        }
    }
}