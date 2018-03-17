﻿// <copyright file="SqlBuilderExtensions.cs" company="Berkeleybross">
// Copyright (c) Berkeleybross. All rights reserved.
// </copyright>
namespace Dapper.MicroCRUD.Dialects
{
    using System;
    using System.Collections.Immutable;
    using System.Text;
    using Dapper.MicroCRUD.Schema;

    /// <summary>
    /// Helpers to generate SQL statements
    /// </summary>
    internal static class SqlBuilderExtensions
    {
        /// <summary>
        /// Appends a WHERE clause which selects equality of primary keys.
        /// </summary>
        public static void AppendWherePrimaryKeysClause(this StringBuilder sql, TableSchema tableSchema)
        {
            sql.AppendClause("WHERE ")
               .AppendColumnNamesEqualParameterNames(tableSchema.GetPrimaryKeys(), " AND ", p => true);
        }

        /// <summary>
        /// Appends a SQL clause which lists all the propertys and their aliases
        /// </summary>
        public static StringBuilder AppendSelectPropertiesClause(
            this StringBuilder sql,
            ImmutableArray<ColumnSchema> properties)
        {
            var isFirst = true;

            // ReSharper disable once ForCanBeConvertedToForeach
            // PERF: This method can be called in a very tight loop so should be as fast as possible
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                if (!isFirst)
                {
                    sql.Append(", ");
                }

                sql.Append(property.ColumnName);

                if (property.ColumnName != property.SelectName)
                {
                    sql.Append(" AS " + property.SelectName);
                }

                isFirst = false;
            }

            return sql;
        }

        /// <summary>
        /// Appends a list of properties in the form of ColumnName = @ParameterName {Seperator} ColumnName = @ParameterName ...
        /// </summary>
        public static StringBuilder AppendColumnNamesEqualParameterNames(
            this StringBuilder sql,
            ImmutableArray<ColumnSchema> properties,
            string seperator,
            Func<ColumnSchema, bool> include)
        {
            var isFirst = true;

            // ReSharper disable once ForCanBeConvertedToForeach
            // PERF: This method can be called in a very tight loop so should be as fast as possible
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                if (!include(property))
                {
                    continue;
                }

                if (!isFirst)
                {
                    sql.Append(seperator);
                }

                sql.Append(property.ColumnName).Append(" = @").Append(property.ParameterName);
                isFirst = false;
            }

            return sql;
        }

        /// <summary>
        /// Appends a list of properties in the form of @ParameterName, @ParameterName ...
        /// </summary>
        public static StringBuilder AppendParameterNames(
            this StringBuilder sql,
            ImmutableArray<ColumnSchema> properties,
            Func<ColumnSchema, bool> include)
        {
            var isFirst = true;

            // ReSharper disable once ForCanBeConvertedToForeach
            // PERF: This method can be called in a very tight loop so should be as fast as possible
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                if (!include(property))
                {
                    continue;
                }

                if (!isFirst)
                {
                    sql.Append(", ");
                }

                sql.Append("@").Append(property.ParameterName);
                isFirst = false;
            }

            return sql;
        }

        /// <summary>
        /// Appends a list of properties in the form of ColumnName, ColumnName ...
        /// </summary>
        public static StringBuilder AppendColumnNames(
            this StringBuilder sql,
            ImmutableArray<ColumnSchema> properties,
            Func<ColumnSchema, bool> include)
        {
            var isFirst = true;

            // ReSharper disable once ForCanBeConvertedToForeach
            // PERF: This method can be called in a very tight loop so should be as fast as possible
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];

                if (!include(property))
                {
                    continue;
                }

                if (!isFirst)
                {
                    sql.Append(", ");
                }

                sql.Append(property.ColumnName);
                isFirst = false;
            }

            return sql;
        }

        /// <summary>
        /// Appends an arbtitrary clause of SQL to the string. Adds a new line at the start if <paramref name="clause"/> is not empty.
        /// </summary>
        public static StringBuilder AppendClause(this StringBuilder sql, string clause)
        {
            return string.IsNullOrEmpty(clause)
                ? sql
                : sql.AppendLine().Append(clause);
        }
    }
}