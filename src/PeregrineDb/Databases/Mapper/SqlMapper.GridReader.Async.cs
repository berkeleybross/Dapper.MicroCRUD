﻿namespace PeregrineDb.Databases.Mapper
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal static partial class SqlMapper
    {
        public partial class GridReader
        {
            private readonly CancellationToken cancel;
            internal GridReader(IDbCommand command, IDataReader reader, Identity identity, DynamicParameters dynamicParams, bool addToCache, CancellationToken cancel)
                : this(command, reader, identity, dynamicParams, addToCache)
            {
                this.cancel = cancel;
            }

            /// <summary>
            /// Read the next grid of results, returned as a dynamic object
            /// </summary>
            /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
            /// <param name="buffered">Whether to buffer the results.</param>
            public Task<IEnumerable<dynamic>> ReadAsync(bool buffered = true) => this.ReadAsyncImpl<dynamic>(typeof(DapperRow), buffered);

            /// <summary>
            /// Read an individual row of the next grid of results, returned as a dynamic object
            /// </summary>
            /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
            public Task<dynamic> ReadFirstAsync() => this.ReadRowAsyncImpl<dynamic>(typeof(DapperRow), Row.First);

            /// <summary>
            /// Read an individual row of the next grid of results, returned as a dynamic object
            /// </summary>
            /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
            public Task<dynamic> ReadFirstOrDefaultAsync() => this.ReadRowAsyncImpl<dynamic>(typeof(DapperRow), Row.FirstOrDefault);

            /// <summary>
            /// Read an individual row of the next grid of results, returned as a dynamic object
            /// </summary>
            /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
            public Task<dynamic> ReadSingleAsync() => this.ReadRowAsyncImpl<dynamic>(typeof(DapperRow), Row.Single);

            /// <summary>
            /// Read an individual row of the next grid of results, returned as a dynamic object
            /// </summary>
            /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
            public Task<dynamic> ReadSingleOrDefaultAsync() => this.ReadRowAsyncImpl<dynamic>(typeof(DapperRow), Row.SingleOrDefault);

            /// <summary>
            /// Read the next grid of results
            /// </summary>
            /// <param name="type">The type to read.</param>
            /// <param name="buffered">Whether to buffer the results.</param>
            /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
            public Task<IEnumerable<object>> ReadAsync(Type type, bool buffered = true)
            {
                if (type == null) throw new ArgumentNullException(nameof(type));
                return this.ReadAsyncImpl<object>(type, buffered);
            }

            /// <summary>
            /// Read an individual row of the next grid of results
            /// </summary>
            /// <param name="type">The type to read.</param>
            /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
            public Task<object> ReadFirstAsync(Type type)
            {
                if (type == null) throw new ArgumentNullException(nameof(type));
                return this.ReadRowAsyncImpl<object>(type, Row.First);
            }

            /// <summary>
            /// Read an individual row of the next grid of results.
            /// </summary>
            /// <param name="type">The type to read.</param>
            /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
            public Task<object> ReadFirstOrDefaultAsync(Type type)
            {
                if (type == null) throw new ArgumentNullException(nameof(type));
                return this.ReadRowAsyncImpl<object>(type, Row.FirstOrDefault);
            }

            /// <summary>
            /// Read an individual row of the next grid of results.
            /// </summary>
            /// <param name="type">The type to read.</param>
            /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
            public Task<object> ReadSingleAsync(Type type)
            {
                if (type == null) throw new ArgumentNullException(nameof(type));
                return this.ReadRowAsyncImpl<object>(type, Row.Single);
            }

            /// <summary>
            /// Read an individual row of the next grid of results.
            /// </summary>
            /// <param name="type">The type to read.</param>
            /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
            public Task<object> ReadSingleOrDefaultAsync(Type type)
            {
                if (type == null) throw new ArgumentNullException(nameof(type));
                return this.ReadRowAsyncImpl<object>(type, Row.SingleOrDefault);
            }

            /// <summary>
            /// Read the next grid of results.
            /// </summary>
            /// <typeparam name="T">The type to read.</typeparam>
            /// <param name="buffered">Whether the results should be buffered in memory.</param>
            public Task<IEnumerable<T>> ReadAsync<T>(bool buffered = true) => this.ReadAsyncImpl<T>(typeof(T), buffered);

            /// <summary>
            /// Read an individual row of the next grid of results.
            /// </summary>
            /// <typeparam name="T">The type to read.</typeparam>
            public Task<T> ReadFirstAsync<T>() => this.ReadRowAsyncImpl<T>(typeof(T), Row.First);

            /// <summary>
            /// Read an individual row of the next grid of results.
            /// </summary>
            /// <typeparam name="T">The type to read.</typeparam>
            public Task<T> ReadFirstOrDefaultAsync<T>() => this.ReadRowAsyncImpl<T>(typeof(T), Row.FirstOrDefault);

            /// <summary>
            /// Read an individual row of the next grid of results.
            /// </summary>
            /// <typeparam name="T">The type to read.</typeparam>
            public Task<T> ReadSingleAsync<T>() => this.ReadRowAsyncImpl<T>(typeof(T), Row.Single);

            /// <summary>
            /// Read an individual row of the next grid of results.
            /// </summary>
            /// <typeparam name="T">The type to read.</typeparam>
            public Task<T> ReadSingleOrDefaultAsync<T>() => this.ReadRowAsyncImpl<T>(typeof(T), Row.SingleOrDefault);

            private async Task NextResultAsync()
            {
                if (await ((DbDataReader)this.reader).NextResultAsync(this.cancel).ConfigureAwait(false))
                {
                    this.readCount++;
                    this.gridIndex++;
                    this.IsConsumed = false;
                }
                else
                {
                    // happy path; close the reader cleanly - no
                    // need for "Cancel" etc
                    this.reader.Dispose();
                    this.reader = null;
                    this.callbacks?.OnCompleted();
                    this.Dispose();
                }
            }

            private Task<IEnumerable<T>> ReadAsyncImpl<T>(Type type, bool buffered)
            {
                if (this.reader == null) throw new ObjectDisposedException(this.GetType().FullName, "The reader has been disposed; this can happen after all data has been consumed");
                if (this.IsConsumed) throw new InvalidOperationException("Query results must be consumed in the correct order, and each result can only be consumed once");
                var typedIdentity = this.identity.ForGrid(type, this.gridIndex);
                CacheInfo cache = GetCacheInfo(typedIdentity, null, this.addToCache);
                var deserializer = cache.Deserializer;

                int hash = GetColumnHash(this.reader);
                if (deserializer.Func == null || deserializer.Hash != hash)
                {
                    deserializer = new DeserializerState(hash, GetDeserializer(type, this.reader, 0, -1, false));
                    cache.Deserializer = deserializer;
                }
                this.IsConsumed = true;
                if (buffered && this.reader is DbDataReader)
                {
                    return this.ReadBufferedAsync<T>(this.gridIndex, deserializer.Func);
                }
                else
                {
                    var result = this.ReadDeferred<T>(this.gridIndex, deserializer.Func, type);
                    if (buffered) result = result.ToList(); // for the "not a DbDataReader" scenario
                    return Task.FromResult(result);
                }
            }

            private Task<T> ReadRowAsyncImpl<T>(Type type, Row row)
            {
                if (this.reader is DbDataReader dbReader) return this.ReadRowAsyncImplViaDbReader<T>(dbReader, type, row);

                // no async API available; use non-async and fake it
                return Task.FromResult(this.ReadRow<T>(type, row));
            }

            private async Task<T> ReadRowAsyncImplViaDbReader<T>(DbDataReader reader, Type type, Row row)
            {
                if (reader == null) throw new ObjectDisposedException(this.GetType().FullName, "The reader has been disposed; this can happen after all data has been consumed");
                if (this.IsConsumed) throw new InvalidOperationException("Query results must be consumed in the correct order, and each result can only be consumed once");

                this.IsConsumed = true;
                T result = default(T);
                if (await reader.ReadAsync(this.cancel).ConfigureAwait(false) && reader.FieldCount != 0)
                {
                    var typedIdentity = this.identity.ForGrid(type, this.gridIndex);
                    CacheInfo cache = GetCacheInfo(typedIdentity, null, this.addToCache);
                    var deserializer = cache.Deserializer;

                    int hash = GetColumnHash(reader);
                    if (deserializer.Func == null || deserializer.Hash != hash)
                    {
                        deserializer = new DeserializerState(hash, GetDeserializer(type, reader, 0, -1, false));
                        cache.Deserializer = deserializer;
                    }
                    result = (T)deserializer.Func(reader);
                    if ((row & Row.Single) != 0 && await reader.ReadAsync(this.cancel).ConfigureAwait(false)) ThrowMultipleRows(row);
                    while (await reader.ReadAsync(this.cancel).ConfigureAwait(false)) { /* ignore subsequent rows */ }
                }
                else if ((row & Row.FirstOrDefault) == 0) // demanding a row, and don't have one
                {
                    ThrowZeroRows(row);
                }
                await this.NextResultAsync().ConfigureAwait(false);
                return result;
            }

            private async Task<IEnumerable<T>> ReadBufferedAsync<T>(int index, Func<IDataReader, object> deserializer)
            {
                try
                {
                    var reader = (DbDataReader)this.reader;
                    var buffer = new List<T>();
                    while (index == this.gridIndex && await reader.ReadAsync(this.cancel).ConfigureAwait(false))
                    {
                        buffer.Add((T)deserializer(reader));
                    }
                    return buffer;
                }
                finally // finally so that First etc progresses things even when multiple rows
                {
                    if (index == this.gridIndex)
                    {
                        await this.NextResultAsync().ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
