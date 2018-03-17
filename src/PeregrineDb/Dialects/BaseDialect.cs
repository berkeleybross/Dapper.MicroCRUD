﻿// <copyright file="BaseDialect.cs" company="Berkeleybross">
// Copyright (c) Berkeleybross. All rights reserved.
// </copyright>
namespace Dapper.MicroCRUD.Dialects
{
    using System.Collections.Immutable;
    using System.Text;
    using Dapper.MicroCRUD.Schema;
    using Pagination;

    /// <summary>
    /// Simple implementation of a SQL dialect which performs most SQL generation.
    /// </summary>
    public abstract class BaseDialect
        : IDialect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDialect"/> class.
        /// </summary>
        /// <param name="name"></param>
        protected BaseDialect(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the dialect
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public string MakeCountStatement(TableSchema tableSchema, string conditions)
        {
            var sql = new StringBuilder("SELECT COUNT(*)");
            sql.AppendClause("FROM ").Append(tableSchema.Name);
            sql.AppendClause(conditions);
            return sql.ToString();
        }

        /// <inheritdoc />
        public string MakeFindStatement(TableSchema tableSchema)
        {
            var sql = new StringBuilder("SELECT ").AppendSelectPropertiesClause(tableSchema.Columns);
            sql.AppendClause("FROM ").Append(tableSchema.Name);
            sql.AppendWherePrimaryKeysClause(tableSchema);
            return sql.ToString();
        }

        public abstract string MakeGetTopNStatement(TableSchema tableSchema, int take, string conditions, string orderBy);

        /// <inheritdoc />
        public string MakeGetRangeStatement(TableSchema tableSchema, string conditions)
        {
            var sql = new StringBuilder("SELECT ").AppendSelectPropertiesClause(tableSchema.Columns);
            sql.AppendClause("FROM ").Append(tableSchema.Name);
            sql.AppendClause(conditions);
            return sql.ToString();
        }

        /// <inheritdoc />
        public abstract string MakeGetPageStatement(TableSchema tableSchema, Page page, string conditions, string orderBy);

        /// <inheritdoc />
        public string MakeInsertStatement(TableSchema tableSchema)
        {
            bool Include(ColumnSchema p) => p.Usage.IncludeInInsertStatements;
            var columns = tableSchema.Columns;

            var sql = new StringBuilder("INSERT INTO ")
                .Append(tableSchema.Name)
                .Append(" (").AppendColumnNames(columns, Include).Append(")");
            sql.AppendClause("VALUES (").AppendParameterNames(columns, Include).Append(");");
            return sql.ToString();
        }

        /// <inheritdoc />
        public string MakeUpdateStatement(TableSchema tableSchema)
        {
            bool Include(ColumnSchema p) => p.Usage.IncludeInUpdateStatements;

            var sql = new StringBuilder("UPDATE ").Append(tableSchema.Name);
            sql.AppendClause("SET ").AppendColumnNamesEqualParameterNames(tableSchema.Columns, ", ", Include);
            sql.AppendWherePrimaryKeysClause(tableSchema);
            return sql.ToString();
        }

        /// <inheritdoc />
        public string MakeDeleteByPrimaryKeyStatement(TableSchema tableSchema)
        {
            var sql = new StringBuilder("DELETE FROM ").Append(tableSchema.Name);
            sql.AppendWherePrimaryKeysClause(tableSchema);
            return sql.ToString();
        }

        /// <summary>
        /// Generates a SQL Delete statement which chooses which row to delete by the <paramref name="conditions"/>.
        /// </summary>
        public string MakeDeleteRangeStatement(TableSchema tableSchema, string conditions)
        {
            var sql = new StringBuilder("DELETE FROM ").Append(tableSchema.Name);
            sql.AppendClause(conditions);
            return sql.ToString();
        }

        /// <inheritdoc />
        public string MakeWhereClause(ImmutableArray<ConditionColumnSchema> conditionsSchema, object conditions)
        {
            if (conditionsSchema.IsEmpty)
            {
                return string.Empty;
            }

            var sql = new StringBuilder("WHERE ");
            var isFirst = true;

            foreach (var condition in conditionsSchema)
            {
                if (!isFirst)
                {
                    sql.Append(" AND ");
                }

                if (condition.IsNull(conditions))
                {
                    sql.Append(condition.Column.ColumnName).Append(" IS NULL");
                }
                else
                {
                    sql.Append(condition.Column.ColumnName).Append(" = @").Append(condition.Column.ParameterName);
                }

                isFirst = false;
            }

            return sql.ToString();
        }

        /// <inheritdoc />
        public abstract string MakeCreateTempTableStatement(TableSchema tableSchema);

        /// <inheritdoc />
        public abstract string MakeDropTempTableStatement(TableSchema tableSchema);

        /// <inheritdoc />
        public abstract string MakeInsertReturningIdentityStatement(TableSchema tableSchema);

        /// <inheritdoc />
        public abstract string MakeColumnName(string name);

        /// <inheritdoc />
        public abstract string MakeTableName(string tableName);

        /// <inheritdoc />
        public abstract string MakeTableName(string schema, string tableName);

        /// <inheritdoc />
        public override string ToString()
        {
            return "Dialect " + this.Name;
        }
    }
}