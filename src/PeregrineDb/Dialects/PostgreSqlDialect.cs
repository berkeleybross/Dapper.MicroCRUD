// <copyright file="PostgreSqlDialect.cs" company="Berkeleybross">
// Copyright (c) Berkeleybross. All rights reserved.
// </copyright>

namespace PeregrineDb.Dialects
{
    using System;
    using System.Collections.Immutable;
    using System.Data;
    using Pagination;
    using PeregrineDb.Schema;
    using PeregrineDb.Schema.Relations;

    /// <summary>
    /// Implementation of <see cref="IDialect"/> for the PostgreSQL DBMS.
    /// </summary>
    public class PostgreSqlDialect
        : StandardDialect, ISchemaQueryDialect
    {
        private static readonly ImmutableArray<string> ColumnTypes;

        static PostgreSqlDialect()
        {
            var types = new string[28];
            types[(int)DbType.Byte] = null;
            types[(int)DbType.Boolean] = "BOOL";
            types[(int)DbType.Currency] = null;
            types[(int)DbType.Date] = "DATE";
            types[(int)DbType.DateTime] = "TIMESTAMP";
            types[(int)DbType.Decimal] = "NUMERIC";
            types[(int)DbType.Double] = "DOUBLE PRECISION";
            types[(int)DbType.Guid] = "UUID";
            types[(int)DbType.Int16] = "SMALLINT";
            types[(int)DbType.Int32] = "INT";
            types[(int)DbType.Int64] = "BIGINT";
            types[(int)DbType.Object] = null;
            types[(int)DbType.SByte] = null;
            types[(int)DbType.Single] = "REAL";
            types[(int)DbType.Time] = "TIME";
            types[(int)DbType.UInt16] = null;
            types[(int)DbType.UInt32] = null;
            types[(int)DbType.UInt64] = null;
            types[(int)DbType.VarNumeric] = null;
            types[(int)DbType.Xml] = null;
            types[(int)DbType.DateTime2] = "TIMESTAMP";
            types[(int)DbType.DateTimeOffset] = "TIMESTAMP WITH TIME ZONE";
            ColumnTypes = types.ToImmutableArray();
        }

        /// <inheritdoc />
        public override SqlCommand MakeInsertReturningPrimaryKeyCommand(object entity, TableSchema tableSchema)
        {
            Func<ColumnSchema, bool> include = p => p.Usage.IncludeInInsertStatements;
            var columns = tableSchema.Columns;

            var sql = new SqlCommandBuilder("INSERT INTO ").Append(tableSchema.Name).Append(" (").AppendColumnNames(columns, include).Append(")");
            sql.AppendClause("VALUES (").AppendParameterNames(columns, include).Append(")");
            sql.AppendClause("RETURNING ").AppendSelectPropertiesClause(tableSchema.PrimaryKeyColumns);
            return sql.ToCommand(entity);
        }

        public override SqlCommand MakeGetFirstNCommand(int take, string conditions, object parameters, string orderBy, TableSchema tableSchema)
        {
            var sql = new SqlCommandBuilder("SELECT ").AppendSelectPropertiesClause(tableSchema.Columns);
            sql.AppendClause("FROM ").Append(tableSchema.Name);
            sql.AppendClause(conditions);
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                sql.AppendClause("ORDER BY ").Append(orderBy);
            }

            sql.AppendLine().Append("LIMIT ").Append(take);
            return sql.ToCommand(parameters);
        }

        /// <inheritdoc />
        public override SqlCommand MakeGetPageCommand(Page page, string conditions, object parameters, string orderBy, TableSchema tableSchema)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                throw new ArgumentException("orderBy cannot be empty");
            }

            var sql = new SqlCommandBuilder("SELECT ").AppendSelectPropertiesClause(tableSchema.Columns);
            sql.AppendClause("FROM ").Append(tableSchema.Name);
            sql.AppendClause(conditions);
            sql.AppendClause("ORDER BY ").Append(orderBy);
            sql.AppendLine().AppendFormat("LIMIT {1} OFFSET {0}", page.FirstItemIndex, page.PageSize);
            return sql.ToCommand(parameters);
        }

        /// <inheritdoc />
        public override SqlCommand MakeCreateTempTableCommand(TableSchema tableSchema)
        {
            if (tableSchema.Columns.IsEmpty)
            {
                throw new ArgumentException("Temporary tables must have columns");
            }

            var sql = new SqlCommandBuilder("CREATE TEMP TABLE ").Append(tableSchema.Name).AppendLine();
            sql.AppendLine("(");

            var isFirst = true;
            foreach (var column in tableSchema.Columns)
            {
                if (!isFirst)
                {
                    sql.AppendLine(",");
                }

                sql.Append(new string(' ', 4));
                sql.Append(column.ColumnName);
                sql.Append(" ").Append(GetColumnType(column));

                isFirst = false;
            }

            sql.AppendLine();
            sql.Append(")");
            return sql.ToCommand(null);
        }

        /// <inheritdoc />
        public override SqlCommand MakeDropTempTableCommand(TableSchema tableSchema)
        {
            return new SqlCommand("DROP TABLE " + tableSchema.Name);
        }

        private static string GetColumnType(ColumnSchema column)
        {
            var nullability = column.ColumnType.AllowNull
                ? " NULL"
                : " NOT NULL";

            return GetColumnType(column.ColumnType) + nullability;
        }

        private static string GetColumnType(DbTypeEx dbType)
        {
            switch (dbType.Type)
            {
                case DbType.AnsiStringFixedLength:
                    return "TEXT";
                case DbType.Binary:
                    return "BYTEA";
                case DbType.String:
                    return "TEXT";
                case DbType.StringFixedLength:
                    return "TEXT";
                default:
                    var index = (int)dbType.Type;
                    if (index >= 0 && index < ColumnTypes.Length)
                    {
                        var result = ColumnTypes[index];
                        if (result != null)
                        {
                            return result;
                        }
                    }

                    throw new NotSupportedException("Unknown DbType: " + dbType.Type);
            }
        }

        public SqlCommand MakeGetAllTablesStatement()
        {
            return new SqlCommand(@"
SELECT table_schema || '.' || table_name AS Name
FROM information_schema.tables
WHERE table_type='BASE TABLE' AND table_schema <> 'information_schema' AND table_schema NOT LIKE 'pg_%';");
        }

        public SqlCommand MakeGetAllRelationsStatement()
        {
            return new SqlCommand(@"
SELECT target_table.table_schema || '.' || target_table.table_name AS TargetTable,
       source_table.table_schema || '.' || source_table.table_name AS SourceTable,
       source_column.column_name AS SourceColumn,
       source_column.is_nullable::boolean AS SourceColumnIsOptional
FROM information_schema.table_constraints AS source_table
JOIN information_schema.key_column_usage AS kcu ON source_table.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS target_table ON target_table.constraint_name = source_table.constraint_name
JOIN information_schema.columns AS source_column ON kcu.table_name = source_column.table_name AND kcu.table_schema = source_column.table_schema AND kcu.column_name = source_column.column_name
WHERE constraint_type = 'FOREIGN KEY';");
        }

        public SqlCommand MakeSetColumnNullStatement(string tableName, string columnName)
        {
            var sql = new SqlCommandBuilder("UPDATE ").Append(tableName);
            sql.AppendClause("SET ").Append(columnName).Append(" = NULL");
            return sql.ToCommand(null);
        }

        public SqlCommand MakeDeleteAllCommand(string tableName)
        {
            return new SqlCommand("DELETE FROM " + tableName);
        }
    }
}