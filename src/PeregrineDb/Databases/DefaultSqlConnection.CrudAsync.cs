// <copyright file="DefaultSqlConnection.CrudAsync.cs" company="Berkeleybross">
// Copyright (c) Berkeleybross. All rights reserved.
// </copyright>

namespace PeregrineDb.Databases
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Pagination;
    using PeregrineDb.SqlCommands;
    using PeregrineDb.Utils;

    public partial class DefaultSqlConnection
    {
        public Task<int> CountAsync<TEntity>(string conditions, object parameters, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeCountCommand<TEntity>(conditions, parameters);
            return this.ExecuteScalarAsync<int>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public Task<int> CountAsync<TEntity>(object conditions, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeCountCommand<TEntity>(conditions);
            return this.ExecuteScalarAsync<int>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public async Task<bool> ExistsAsync<TEntity>(string conditions, object parameters, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var count = await this.CountAsync<TEntity>(conditions, parameters, commandTimeout, cancellationToken).ConfigureAwait(false);
            return count > 0;
        }

        public async Task<bool> ExistsAsync<TEntity>(object conditions, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var count = await this.CountAsync<TEntity>(conditions, commandTimeout, cancellationToken).ConfigureAwait(false);
            return count > 0;
        }

        public Task<TEntity> FindAsync<TEntity>(object id, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeFindCommand<TEntity>(id);
            return this.QueryFirstOrDefaultAsync<TEntity>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public async Task<TEntity> GetAsync<TEntity>(object id, int? commandTimeout = null, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var result = await this.FindAsync<TEntity>(id, commandTimeout, cancellationToken).ConfigureAwait(false);
            return result ?? throw new InvalidOperationException($"An entity with id {id} was not found");
        }

        public Task<TEntity> FindFirstAsync<TEntity>(string orderBy, string conditions, object parameters, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeGetFirstNCommand<TEntity>(1, conditions, parameters, orderBy);
            return this.QueryFirstOrDefaultAsync<TEntity>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public Task<TEntity> FindFirstAsync<TEntity>(string orderBy, object conditions, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeGetFirstNCommand<TEntity>(1, conditions, orderBy);
            return this.QueryFirstOrDefaultAsync<TEntity>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public Task<TEntity> GetFirstAsync<TEntity>(string orderBy, string conditions, object parameters, int? commandTimeout = null, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var command = this.commandFactory.MakeGetFirstNCommand<TEntity>(1, conditions, parameters, orderBy);
            return this.QueryFirstAsync<TEntity>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public Task<TEntity> GetFirstAsync<TEntity>(string orderBy, object conditions, int? commandTimeout = null, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var command = this.commandFactory.MakeGetFirstNCommand<TEntity>(1, conditions, orderBy);
            return this.QueryFirstAsync<TEntity>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public Task<TEntity> FindSingleAsync<TEntity>(string conditions, object parameters, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeGetFirstNCommand<TEntity>(2, conditions, parameters, null);
            return this.QuerySingleOrDefaultAsync<TEntity>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public Task<TEntity> FindSingleAsync<TEntity>(object conditions, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeGetFirstNCommand<TEntity>(2, conditions, null);
            return this.QuerySingleOrDefaultAsync<TEntity>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public async Task<TEntity> GetSingleAsync<TEntity>(string conditions, object parameters, int? commandTimeout = null, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var result = await this.FindSingleAsync<TEntity>(conditions, parameters, commandTimeout, cancellationToken);
            return result ?? throw new InvalidOperationException("No entity matching given conditions was found");
        }

        public async Task<TEntity> GetSingleAsync<TEntity>(object conditions, int? commandTimeout = null, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var result = await this.FindSingleAsync<TEntity>(conditions, commandTimeout, cancellationToken);
            return result ?? throw new InvalidOperationException("No entity matching given conditions was found");
        }

        public Task<IReadOnlyList<TEntity>> GetRangeAsync<TEntity>(string conditions, object parameters, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeGetRangeCommand<TEntity>(conditions, parameters);
            return this.QueryAsync<TEntity>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public Task<IReadOnlyList<TEntity>> GetRangeAsync<TEntity>(object conditions, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeGetRangeCommand<TEntity>(conditions);
            return this.QueryAsync<TEntity>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public Task<IReadOnlyList<TEntity>> GetTopAsync<TEntity>(int count, string orderBy, string conditions, object parameters, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            Ensure.NotNullOrWhiteSpace(orderBy, nameof(orderBy));

            var command = this.commandFactory.MakeGetFirstNCommand<TEntity>(count, conditions, parameters, orderBy);
            return this.QueryAsync<TEntity>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public Task<IReadOnlyList<TEntity>> GetTopAsync<TEntity>(int count, string orderBy, object conditions, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            Ensure.NotNullOrWhiteSpace(orderBy, nameof(orderBy));

            var command = this.commandFactory.MakeGetFirstNCommand<TEntity>(count, conditions, orderBy);
            return this.QueryAsync<TEntity>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public async Task<PagedList<TEntity>> GetPageAsync<TEntity>(IPageBuilder pageBuilder, string orderBy, string conditions, object parameters, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var totalNumberOfItems = await this.CountAsync<TEntity>(conditions, parameters, commandTimeout, cancellationToken).ConfigureAwait(false);
            var page = pageBuilder.GetCurrentPage(totalNumberOfItems);
            if (page.IsEmpty)
            {
                return PagedList<TEntity>.Empty(totalNumberOfItems, page);
            }

            var itemsCommand = this.commandFactory.MakeGetPageCommand<TEntity>(page, conditions, parameters, orderBy);
            var items = await this.QueryAsync<TEntity>(itemsCommand.CommandText, itemsCommand.Parameters, CommandType.Text, commandTimeout, cancellationToken).ConfigureAwait(false);
            return PagedList<TEntity>.Create(totalNumberOfItems, page, items);
        }

        public async Task<PagedList<TEntity>> GetPageAsync<TEntity>(IPageBuilder pageBuilder, string orderBy, object conditions, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var totalNumberOfItems = await this.CountAsync<TEntity>(conditions, commandTimeout, cancellationToken).ConfigureAwait(false);
            var page = pageBuilder.GetCurrentPage(totalNumberOfItems);
            if (page.IsEmpty)
            {
                return PagedList<TEntity>.Empty(totalNumberOfItems, page);
            }

            var itemsCommand = this.commandFactory.MakeGetPageCommand<TEntity>(page, conditions, orderBy);
            var items = await this.QueryAsync<TEntity>(itemsCommand.CommandText, itemsCommand.Parameters, CommandType.Text, commandTimeout, cancellationToken).ConfigureAwait(false);
            return PagedList<TEntity>.Create(totalNumberOfItems, page, items);
        }

        public Task<IReadOnlyList<TEntity>> GetAllAsync<TEntity>(int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeGetRangeCommand<TEntity>(null, null);
            return this.QueryAsync<TEntity>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public async Task InsertAsync(object entity, int? commandTimeout = null, bool? verifyAffectedRowCount = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeInsertCommand(entity);
            var result = await this.ExecuteAsync(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken).ConfigureAwait(false);
            if (this.Config.ShouldVerifyAffectedRowCount(verifyAffectedRowCount))
            {
                result.ExpectingAffectedRowCountToBe(1);
            }
        }

        public Task<TPrimaryKey> InsertAsync<TPrimaryKey>(object entity, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            Ensure.NotNull(entity, nameof(entity));

            var command = this.commandFactory.MakeInsertReturningPrimaryKeyCommand<TPrimaryKey>(entity);
            return this.ExecuteScalarAsync<TPrimaryKey>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public Task<CommandResult> InsertRangeAsync<TEntity>(IEnumerable<TEntity> entities, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeInsertRangeCommand(entities);
            return this.ExecuteMultipleAsync(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "Doesn't actually enumerate twice")]
        public async Task InsertRangeAsync<TEntity, TPrimaryKey>(IEnumerable<TEntity> entities, Action<TEntity, TPrimaryKey> setPrimaryKey, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            Ensure.NotNull(setPrimaryKey, nameof(setPrimaryKey));
            Ensure.NotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                var command = this.commandFactory.MakeInsertReturningPrimaryKeyCommand<TPrimaryKey>(entity);
                var id = await this.ExecuteScalarAsync<TPrimaryKey>(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken).ConfigureAwait(false);
                setPrimaryKey(entity, id);
            }
        }

        public async Task UpdateAsync<TEntity>(TEntity entity, int? commandTimeout = null, bool? verifyAffectedRowCount = null, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var command = this.commandFactory.MakeUpdateCommand(entity);
            var result = await this.ExecuteAsync(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken)
                                   .ConfigureAwait(false);

            if (this.Config.ShouldVerifyAffectedRowCount(verifyAffectedRowCount))
            {
                result.ExpectingAffectedRowCountToBe(1);
            }
        }

        public Task<CommandResult> UpdateRangeAsync<TEntity>(IEnumerable<TEntity> entities, int? commandTimeout = null, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var command = this.commandFactory.MakeUpdateRangeCommand(entities);
            return this.ExecuteMultipleAsync(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public async Task DeleteAsync<TEntity>(TEntity entity, int? commandTimeout = null, bool? verifyAffectedRowCount = null, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var command = this.commandFactory.MakeDeleteCommand(entity);
            var result = await this.ExecuteAsync(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken)
                                   .ConfigureAwait(false);

            if (this.Config.ShouldVerifyAffectedRowCount(verifyAffectedRowCount))
            {
                result.ExpectingAffectedRowCountToBe(1);
            }
        }

        public async Task DeleteAsync<TEntity>(object id, int? commandTimeout = null, bool? verifyAffectedRowCount = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeDeleteByPrimaryKeyCommand<TEntity>(id);
            var result = await this.ExecuteAsync(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken)
                                   .ConfigureAwait(false);

            if (this.Config.ShouldVerifyAffectedRowCount(verifyAffectedRowCount))
            {
                result.ExpectingAffectedRowCountToBe(1);
            }
        }

        public Task<CommandResult> DeleteRangeAsync<TEntity>(string conditions, object parameters, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeDeleteRangeCommand<TEntity>(conditions, parameters);
            return this.ExecuteAsync(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public Task<CommandResult> DeleteRangeAsync<TEntity>(object conditions, int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeDeleteRangeCommand<TEntity>(conditions);
            return this.ExecuteAsync(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }

        public Task<CommandResult> DeleteAllAsync<TEntity>(int? commandTimeout = null, CancellationToken cancellationToken = default)
        {
            var command = this.commandFactory.MakeDeleteAllCommand<TEntity>();
            return this.ExecuteAsync(command.CommandText, command.Parameters, CommandType.Text, commandTimeout, cancellationToken);
        }
    }
}