﻿// <copyright file="DbConnectionExtensions.cs" company="Berkeleybross">
//   Copyright (c) Berkeleybross. All rights reserved.
// </copyright>
// ReSharper disable once CheckNamespace
namespace Dapper
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Dapper.MicroCRUD;
    using Dapper.MicroCRUD.Dialects;
    using Dapper.MicroCRUD.SqlCommands;
    using Dapper.MicroCRUD.Utils;
    using Pagination;

    /// <summary>
    /// CRUD extensions to the <see cref="IDbConnection"/>.
    /// </summary>
    public static class DbConnectionExtensions
    {
        /// <summary>
        /// Counts how many entities in the <typeparamref name="TEntity"/> table match the <paramref name="conditions"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        ///
        ///     public int Age { get; set; }
        /// }
        /// ...
        /// this.connection.Count<UserEntity>("WHERE Age > @MinAge", new { MinAge = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static int Count<TEntity>(
            this IDbConnection connection,
            string conditions = null,
            object parameters = null,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeCountCommand<TEntity>(conditions, parameters, transaction, dialect, commandTimeout);
            return connection.ExecuteScalar<int>(command);
        }

        /// <summary>
        /// Counts how many entities in the <typeparamref name="TEntity"/> table match the <paramref name="conditions"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        ///
        ///     public int Age { get; set; }
        /// }
        /// ...
        /// this.connection.Count<UserEntity>(new { Age = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static int Count<TEntity>(
            this IDbConnection connection,
            object conditions,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeCountCommand<TEntity>(conditions, transaction, dialect, commandTimeout);
            return connection.ExecuteScalar<int>(command);
        }

        /// <summary>
        /// Finds a single entity from the <typeparamref name="TEntity"/> table by it's primary key, or the default value if not found.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// var entity = this.connection.Find<UserEntity>(12);
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity Find<TEntity>(
            this IDbConnection connection,
            object id,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeFindCommand<TEntity>(id, transaction, dialect, commandTimeout);
            return connection.Query<TEntity>(command).FirstOrDefault();
        }

        /// <summary>
        /// Gets a single entity from the <typeparamref name="TEntity"/> table by it's primary key, or throws an exception if not found
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// var entity = this.connection.Get<UserEntity>(12);
        /// ]]>
        /// </code>
        /// </example>
        /// <exception cref="InvalidOperationException">The entity was not found.</exception>
        public static TEntity Get<TEntity>(
            this IDbConnection connection,
            object id,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
            where TEntity : class
        {
            var result = connection.Find<TEntity>(id, transaction, dialect, commandTimeout);
            if (result != null)
            {
                return result;
            }

            throw new InvalidOperationException($"An entity with id {id} was not found");
        }

        /// <summary>
        /// Gets the first matching entity from the <typeparamref name="TEntity"/> table which matches the <paramref name="conditions"/>,
        /// or the default value if none match.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var user = this.connection.GetFirstOrDefault<UserEntity>("WHERE Age > @MinAge", new { MinAge = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity GetFirstOrDefault<TEntity>(
            this IDbConnection connection,
            string conditions,
            string orderBy,
            object parameters = null,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeGetTopNCommand<TEntity>(1, conditions, orderBy, parameters, transaction, dialect, commandTimeout);
            return connection.Query<TEntity>(command).FirstOrDefault();
        }

        /// <summary>
        /// Gets the first matching entity from the <typeparamref name="TEntity"/> table which matches the <paramref name="conditions"/>,
        /// or the default value if none match.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var user = this.connection.GetFirstOrDefault<UserEntity>(new { Age = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity GetFirstOrDefault<TEntity>(
            this IDbConnection connection,
            object conditions,
            string orderBy,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeGetTopNCommand<TEntity>(1, conditions, orderBy, transaction, dialect, commandTimeout);
            return connection.Query<TEntity>(command).FirstOrDefault();
        }

        /// <summary>
        /// Gets the first matching entity from the <typeparamref name="TEntity"/> table which matches the <paramref name="conditions"/>,
        /// or throws an <see cref="InvalidOperationException"/> if none match.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var user = this.connection.GetFirst<UserEntity>("WHERE Age > @MinAge", new { MinAge = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity GetFirst<TEntity>(
            this IDbConnection connection,
            string conditions,
            string orderBy,
            object parameters = null,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            var result = connection.GetFirstOrDefault<TEntity>(conditions, orderBy, parameters, transaction, dialect, commandTimeout);
            if (result != null)
            {
                return result;
            }

            throw new InvalidOperationException($"No entity matching {conditions} was found");
        }

        /// <summary>
        /// Gets the first matching entity from the <typeparamref name="TEntity"/> table which matches the <paramref name="conditions"/>,
        /// or throws an <see cref="InvalidOperationException"/> if none match.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var user = this.connection.GetFirst<UserEntity>(new { Age = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity GetFirst<TEntity>(
            this IDbConnection connection,
            object conditions,
            string orderBy,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            var result = connection.GetFirstOrDefault<TEntity>(conditions, orderBy, transaction, dialect, commandTimeout);
            if (result != null)
            {
                return result;
            }

            throw new InvalidOperationException($"No entity matching {conditions} was found");
        }

        /// <summary>
        /// Gets the only matching entity from the <typeparamref name="TEntity"/> table which matches the <paramref name="conditions"/>,
        /// or the default value if none match. Throws an <see cref="InvalidOperationException"/> if multiple entities match.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var user = this.connection.GetSingleOrDefault<UserEntity>("WHERE Age > @MinAge", new { MinAge = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity GetSingleOrDefault<TEntity>(
            this IDbConnection connection,
            string conditions,
            object parameters = null,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeGetTopNCommand<TEntity>(2, conditions, null, parameters, transaction, dialect, commandTimeout);
            return connection.Query<TEntity>(command).SingleOrDefault();
        }

        /// <summary>
        /// Gets the only matching entity from the <typeparamref name="TEntity"/> table which matches the <paramref name="conditions"/>,
        /// or the default value if none match. Throws an <see cref="InvalidOperationException"/> if multiple entities match.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var user = this.connection.GetSingleOrDefault<UserEntity>(new { Age = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity GetSingleOrDefault<TEntity>(
            this IDbConnection connection,
            object conditions,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeGetTopNCommand<TEntity>(2, conditions, null, transaction, dialect, commandTimeout);
            return connection.Query<TEntity>(command).SingleOrDefault();
        }

        /// <summary>
        /// Gets the only matching entity from the <typeparamref name="TEntity"/> table which matches the <paramref name="conditions"/>,
        /// or throws an <see cref="InvalidOperationException"/> if no entries, or multiple entities match.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var user = this.connection.GetSingle<UserEntity>("WHERE Age > @MinAge", new { MinAge = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity GetSingle<TEntity>(
            this IDbConnection connection,
            string conditions,
            object parameters = null,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            var result = connection.GetSingleOrDefault<TEntity>(conditions, parameters, transaction, dialect, commandTimeout);
            if (result != null)
            {
                return result;
            }

            throw new InvalidOperationException($"No entity matching {conditions} was found");
        }

        /// <summary>
        /// Gets the only matching entity from the <typeparamref name="TEntity"/> table which matches the <paramref name="conditions"/>,
        /// or throws an <see cref="InvalidOperationException"/> if no entries, or multiple entities match.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// var user = this.connection.GetSingle<UserEntity>(new { Age = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static TEntity GetSingle<TEntity>(
            this IDbConnection connection,
            object conditions,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            var result = connection.GetSingleOrDefault<TEntity>(conditions, transaction, dialect, commandTimeout);
            if (result != null)
            {
                return result;
            }

            throw new InvalidOperationException($"No entity matching {conditions} was found");
        }

        /// <summary>
        /// Gets a collection of entities from the <typeparamref name="TEntity"/> table which match the <paramref name="conditions"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        ///
        ///     public int Age { get; set; }
        /// }
        /// ...
        /// var users = this.connection.GetRange<UserEntity>("WHERE Age > @MinAge", new { MinAge = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<TEntity> GetRange<TEntity>(
            this IDbConnection connection,
            string conditions,
            object parameters = null,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeGetRangeCommand<TEntity>(conditions, parameters, transaction, dialect, commandTimeout);
            return connection.Query<TEntity>(command);
        }

        /// <summary>
        /// Gets a collection of entities from the <typeparamref name="TEntity"/> table which match the <paramref name="conditions"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        ///
        ///     public int Age { get; set; }
        /// }
        /// ...
        /// var users = this.connection.GetRange<UserEntity>(new { Age = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static IEnumerable<TEntity> GetRange<TEntity>(
            this IDbConnection connection,
            object conditions,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeGetRangeCommand<TEntity>(conditions, transaction, dialect, commandTimeout);
            return connection.Query<TEntity>(command);
        }

        /// <summary>
        /// Gets a collection of entities from the <typeparamref name="TEntity"/> table which match the <paramref name="conditions"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        ///
        ///     public int Age { get; set; }
        /// }
        /// ...
        /// var pageBuilder = new PageIndexPageBuilder(3, 10);
        /// var users = this.connection.GetPage<UserEntity>(pageBuilder, "WHERE Age > @MinAge", "Age DESC", new { MinAge = 18 });
        /// ]]>
        /// </code>
        /// </example>
        public static PagedList<TEntity> GetPage<TEntity>(
            this IDbConnection connection,
            IPageBuilder pageBuilder,
            string conditions,
            string orderBy,
            object parameters = null,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));

            var totalNumberOfItems = connection.Count<TEntity>(conditions, parameters, transaction, dialect, commandTimeout);
            var page = pageBuilder.GetCurrentPage(totalNumberOfItems);
            if (page.IsEmpty)
            {
                return PagedList<TEntity>.Empty(totalNumberOfItems, page);
            }

            var itemsCommand = CommandFactory.MakeGetPageCommand<TEntity>(page, conditions, orderBy, parameters, transaction, dialect, commandTimeout);
            var items = connection.Query<TEntity>(itemsCommand);
            return PagedList<TEntity>.Create(totalNumberOfItems, page, items);
        }

        /// <summary>
        /// Gets a collection of entities from the <typeparamref name="TEntity"/> table which match the <paramref name="conditions"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        ///
        ///     public int Age { get; set; }
        /// }
        /// ...
        /// var pageBuilder = new PageIndexPageBuilder(3, 10);
        /// var users = this.connection.GetPage<UserEntity>(pageBuilder, new { Age = 10 }, "Age DESC");
        /// ]]>
        /// </code>
        /// </example>
        public static PagedList<TEntity> GetPage<TEntity>(
            this IDbConnection connection,
            IPageBuilder pageBuilder,
            object conditions,
            string orderBy,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));

            var totalNumberOfItems = connection.Count<TEntity>(conditions, transaction, dialect, commandTimeout);
            var page = pageBuilder.GetCurrentPage(totalNumberOfItems);
            if (page.IsEmpty)
            {
                return PagedList<TEntity>.Empty(totalNumberOfItems, page);
            }

            var itemsCommand = CommandFactory.MakeGetPageCommand<TEntity>(page, conditions, orderBy, transaction, dialect, commandTimeout);
            var items = connection.Query<TEntity>(itemsCommand);
            return PagedList<TEntity>.Create(totalNumberOfItems, page, items);
        }

        /// <summary>
        /// Gets all the entities in the <typeparamref name="TEntity"/> table.
        /// </summary>
        public static IEnumerable<TEntity> GetAll<TEntity>(
            this IDbConnection connection,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeGetAllCommand<TEntity>(transaction, dialect, commandTimeout);
            return connection.Query<TEntity>(command);
        }

        /// <summary>
        /// Inserts the <paramref name="entity"/> into the database.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// var entity = new User { Name = "Little bobby tables" };
        /// this.connection.Insert(entity);
        /// ]]>
        /// </code>
        /// </example>
        public static void Insert(
            this IDbConnection connection,
            object entity,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null,
            bool? verifyAffectedRowCount = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var config = MicroCRUDConfig.Current;
            var command = CommandFactory.MakeInsertCommand(entity, transaction, dialect, commandTimeout, config);

            var result = connection.ExecuteCommand(command);
            if (config.ShouldVerifyAffectedRowCount(verifyAffectedRowCount))
            {
                result.ExpectingAffectedRowCountToBe(1);
            }
        }

        /// <summary>
        /// Inserts the <paramref name="entity"/> into the database, and returns the auto-generated identity (or the default if invalid)
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// var entity = new User { Name = "Little bobby tables" };
        /// entity.Id = this.connection.Insert<int>(entity);
        /// ]]>
        /// </code>
        /// </example>
        public static TPrimaryKey Insert<TPrimaryKey>(
            this IDbConnection connection,
            object entity,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeInsertReturningPrimaryKeyCommand<TPrimaryKey>(entity, transaction, dialect, commandTimeout);
            return connection.ExecuteScalar<TPrimaryKey>(command);
        }

        /// <summary>
        /// <para>Efficiently inserts multiple <paramref name="entities"/> into the database.</para>
        /// <para>For performance, it's recommended to always perform this action inside of a transaction.</para>
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// var entities = new []
        ///     {
        ///         new User { Name = "Little bobby tables" },
        ///         new User { Name = "Jimmy" };
        ///     };
        ///
        /// using (var transaction = this.connection.BeginTransaction())
        /// {
        ///     this.connection.InsertRange(entities, transaction);
        ///
        ///     transaction.Commit();
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public static SqlCommandResult InsertRange<TEntity>(
            this IDbConnection connection,
            IEnumerable<TEntity> entities,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeInsertRangeCommand(entities, transaction, dialect, commandTimeout);
            return connection.ExecuteCommand(command);
        }

        /// <summary>
        /// <para>
        /// Efficiently inserts multiple <paramref name="entities"/> into the database,
        /// and for each one calls <paramref name="setPrimaryKey"/> allowing the primary key to be recorded.
        /// </para>
        /// <para>For performance, it's recommended to always perform this action inside of a transaction.</para>
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// var entities = new []
        ///     {
        ///         new User { Name = "Little bobby tables" },
        ///         new User { Name = "Jimmy" };
        ///     };
        ///
        /// using (var transaction = this.connection.BeginTransaction())
        /// {
        ///     this.connection.InsertRange<User, int>(entities, (e, k) => { e.Id = k; }, transaction);
        ///
        ///     transaction.Commit();
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "Ensure doesn't enumerate")]
        public static void InsertRange<TEntity, TPrimaryKey>(
            this IDbConnection connection,
            IEnumerable<TEntity> entities,
            Action<TEntity, TPrimaryKey> setPrimaryKey,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            Ensure.NotNull(setPrimaryKey, nameof(setPrimaryKey));

            var sql = CommandFactory.MakeInsertRangeCommand<TEntity, TPrimaryKey>(entities, dialect);

            foreach (var entity in entities)
            {
                var command = new CommandDefinition(sql, entity, transaction, commandTimeout);
                var id = connection.ExecuteScalar<TPrimaryKey>(command);
                setPrimaryKey(entity, id);
            }
        }

        /// <summary>
        /// Updates the <paramref name="entity"/> in the database.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// var entity = this.connection.Find<UserEntity>(5);
        /// entity.Name = "Little bobby tables";
        /// this.connection.Update(entity);
        /// ]]>
        /// </code>
        /// </example>
        /// <exception cref="AffectedRowCountException">The update command didn't change any record, or changed multiple records.</exception>
        public static void Update<TEntity>(
            this IDbConnection connection,
            TEntity entity,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null,
            bool? verifyAffectedRowCount = null)
        {
            Ensure.NotNull(connection, nameof(connection));

            var config = MicroCRUDConfig.Current;
            var command = CommandFactory.MakeUpdateCommand<TEntity>(entity, transaction, dialect, commandTimeout, config);
            var result = connection.ExecuteCommand(command);

            if (config.ShouldVerifyAffectedRowCount(verifyAffectedRowCount))
            {
                result.ExpectingAffectedRowCountToBe(1);
            }
        }

        /// <summary>
        /// Efficiently updates multiple <paramref name="entities"/> in the database.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// using (var transaction = this.connection.BeginTransaction())
        /// {
        ///     var entities = this.connection.GetRange<UserEntity>("WHERE @Age = 10", transaction);
        ///
        ///     foreach (var entity in entities)
        ///     {
        ///         entity.Name = "Little bobby tables";
        ///     }
        ///
        ///     this.connection.UpdateRange(entities, transaction);
        ///     transaction.Commit();
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>The number of affected records.</returns>
        public static SqlCommandResult UpdateRange<TEntity>(
            this IDbConnection connection,
            IEnumerable<TEntity> entities,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeUpdateRangeCommand(entities, transaction, dialect, commandTimeout);
            return connection.ExecuteCommand(command);
        }

        /// <summary>
        /// Deletes the entity in the <typeparamref name="TEntity"/> table, identified by its primary key.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// var entity = this.connection.Find<UserEntity>(5);
        /// this.connection.Delete(entity);
        /// ]]>
        /// </code>
        /// </example>
        /// <exception cref="AffectedRowCountException">The delete command didn't delete anything, or deleted multiple records.</exception>
        public static void Delete<TEntity>(
            this IDbConnection connection,
            TEntity entity,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null,
            bool? verifyAffectedRowCount = null)
        {
            Ensure.NotNull(connection, nameof(connection));

            var config = MicroCRUDConfig.Current;
            var command = CommandFactory.MakeDeleteCommand<TEntity>(entity, transaction, dialect, commandTimeout, config);
            var result = connection.ExecuteCommand(command);

            if (config.ShouldVerifyAffectedRowCount(verifyAffectedRowCount))
            {
                result.ExpectingAffectedRowCountToBe(1);
            }
        }

        /// <summary>
        /// Deletes the entity in the <typeparamref name="TEntity"/> table which has the <paramref name="id"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// this.connection.Delete(5);
        /// ]]>
        /// </code>
        /// </example>
        /// <exception cref="AffectedRowCountException">The delete command didn't delete anything, or deleted multiple records.</exception>
        public static void Delete<TEntity>(
            this IDbConnection connection,
            object id,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null,
            bool? verifyAffectedRowCount = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var config = MicroCRUDConfig.Current;
            var command = CommandFactory.MakeDeleteByPrimaryKeyCommand<TEntity>(id, transaction, dialect, commandTimeout, config);
            var result = connection.ExecuteCommand(command);

            if (config.ShouldVerifyAffectedRowCount(verifyAffectedRowCount))
            {
                result.ExpectingAffectedRowCountToBe(1);
            }
        }

        /// <summary>
        /// <para>Deletes all the entities in the <typeparamref name="TEntity"/> table which match the <paramref name="conditions"/>.</para>
        /// <para>Note: <paramref name="conditions"/> must contain a WHERE clause. Use <see cref="DeleteAll{TEntity}"/> if you want to delete all entities.</para>
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// this.connection.DeleteRange<UserEntity>("WHERE Name LIKE '%Foo%'");
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>The number of deleted entities.</returns>
        public static SqlCommandResult DeleteRange<TEntity>(
            this IDbConnection connection,
            string conditions,
            object parameters = null,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeDeleteRangeCommand<TEntity>(conditions, parameters, transaction, dialect, commandTimeout);
            return connection.ExecuteCommand(command);
        }

        /// <summary>
        /// <para>Deletes all the entities in the <typeparamref name="TEntity"/> table which match the <paramref name="conditions"/>.</para>
        /// <para>Note: <paramref name="conditions"/> must contain a WHERE clause. Use <see cref="DeleteAll{TEntity}"/> if you want to delete all entities.</para>
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// this.connection.DeleteRange<UserEntity>("WHERE Name LIKE '%Foo%'");
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>The number of deleted entities.</returns>
        public static SqlCommandResult DeleteRange<TEntity>(
            this IDbConnection connection,
            object conditions,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeDeleteRangeCommand<TEntity>(conditions, transaction, dialect, commandTimeout);
            return connection.ExecuteCommand(command);
        }

        /// <summary>
        /// Deletes all the entities in the <typeparamref name="TEntity"/> table.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// [Table("Users")]
        /// public class UserEntity
        /// {
        ///     [Key]
        ///     public int Id { get; set; }
        ///
        ///     public string Name { get; set; }
        /// }
        /// ...
        /// this.connection.DeleteAll<UserEntity>();
        /// ]]>
        /// </code>
        /// </example>
        /// <returns>The number of deleted entities.</returns>
        public static SqlCommandResult DeleteAll<TEntity>(
            this IDbConnection connection,
            IDbTransaction transaction = null,
            IDialect dialect = null,
            int? commandTimeout = null)
        {
            Ensure.NotNull(connection, nameof(connection));
            var command = CommandFactory.MakeDeleteAllCommand<TEntity>(transaction, dialect, commandTimeout);
            return connection.ExecuteCommand(command);
        }

        private static SqlCommandResult ExecuteCommand(this IDbConnection connection, CommandDefinition command)
        {
            return new SqlCommandResult(connection.Execute(command));
        }
    }
}