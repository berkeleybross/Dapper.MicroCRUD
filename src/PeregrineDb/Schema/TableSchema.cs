﻿// <copyright file="TableSchema.cs" company="Berkeleybross">
//   Copyright (c) Berkeleybross. All rights reserved.
// </copyright>
namespace Dapper.MicroCRUD.Schema
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Dapper.MicroCRUD.Utils;

    /// <summary>
    /// Represents a table in the database, as derived from the definition of an entity.
    /// </summary>
    public class TableSchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableSchema"/> class.
        /// </summary>
        public TableSchema(string name, ImmutableArray<ColumnSchema> columns)
        {
            this.Name = name;
            this.Columns = columns;
            this.PrimaryKeyColumns = columns.Where(c => c.Usage.IsPrimaryKey).ToImmutableArray();
        }

        /// <summary>
        /// Gets the name of the database table.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the columns which form the Primary Key of this table.
        /// </summary>
        public ImmutableArray<ColumnSchema> PrimaryKeyColumns { get; }

        /// <summary>
        /// Gets the columns in the table.
        /// </summary>
        public ImmutableArray<ColumnSchema> Columns { get; }

        /// <summary>
        /// Gets whether this table can generate a primary key with the specified type.
        /// </summary>
        public bool CanGeneratePrimaryKey(Type type)
        {
            if (this.PrimaryKeyColumns.Length != 1)
            {
                return false;
            }

            var keyType = type.GetUnderlyingType();
            return keyType == typeof(int) || keyType == typeof(long);
        }

        /// <summary>
        /// Gets the columns which form the Primary Key of this table.
        /// Throws an exception if there is no key.
        /// </summary>
        /// <exception cref="InvalidPrimaryKeyException">This table doesn't have a primary key.</exception>
        public ImmutableArray<ColumnSchema> GetPrimaryKeys()
        {
            var result = this.PrimaryKeyColumns;
            if (result.Length == 0)
            {
                throw new InvalidPrimaryKeyException("This method only supports an entity with a [Key] or Id property");
            }

            return result;
        }

        /// <summary>
        /// Gets the parameters to be used as the primary key for dapper.
        /// </summary>
        public object GetPrimaryKeyParameters(object id)
        {
            var primaryKeys = this.GetPrimaryKeys();
            if (primaryKeys.Length > 1)
            {
                return id;
            }

            var parameters = new DynamicParameters();
            parameters.Add("@" + primaryKeys.First().ParameterName, id);
            return parameters;
        }
    }
}