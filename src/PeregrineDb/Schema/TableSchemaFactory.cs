namespace PeregrineDb.Schema
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using PeregrineDb.Dialects;
    using PeregrineDb.Utils;

    /// <summary>
    /// Methods to create an instance of a <see cref="TableSchema"/>.
    /// </summary>
    internal class TableSchemaFactory
    {
        private readonly ConcurrentDictionary<TableSchemaCacheIdentity, TableSchema> schemas =
            new ConcurrentDictionary<TableSchemaCacheIdentity, TableSchema>();

        private readonly ConcurrentDictionary<ConditionsColumnCacheIdentity, ImmutableArray<ConditionColumnSchema>> conditionColumns =
            new ConcurrentDictionary<ConditionsColumnCacheIdentity, ImmutableArray<ConditionColumnSchema>>();

        private readonly IDialect dialect;
        private readonly ITableNameFactory tableNameFactory;
        private readonly IColumnNameFactory columnNameFactory;
        private readonly Dictionary<Type, DbType> sqlTypeMappings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableSchemaFactory"/> class.
        /// </summary>
        public TableSchemaFactory(
            IDialect dialect,
            ITableNameFactory tableNameFactory,
            IColumnNameFactory columnNameFactory,
            Dictionary<Type, DbType> sqlTypeMappings)
        {
            Ensure.NotNull(dialect, nameof(dialect));
            Ensure.NotNull(tableNameFactory, nameof(tableNameFactory));
            Ensure.NotNull(columnNameFactory, nameof(columnNameFactory));

            this.dialect = dialect;
            this.tableNameFactory = tableNameFactory;
            this.columnNameFactory = columnNameFactory;
            this.sqlTypeMappings = sqlTypeMappings;
        }

        /// <summary>
        /// Gets the <see cref="TableSchema"/> for the specified entityType and dialect.
        /// </summary>
        public TableSchema GetTableSchema(Type entityType)
        {
            var key = new TableSchemaCacheIdentity(entityType);

            if (this.schemas.TryGetValue(key, out var result))
            {
                return result;
            }

            var schema = this.MakeTableSchema(entityType);
            this.schemas[key] = schema;
            return schema;
        }

        /// <summary>
        /// Gets the <see cref="ConditionColumnSchema"/>s for the specified conditionsType and dialect.
        /// </summary>
        public ImmutableArray<ConditionColumnSchema> GetConditionsSchema(
            Type entityType,
            TableSchema tableSchema,
            Type conditionsType)
        {
            var key = new ConditionsColumnCacheIdentity(conditionsType, entityType);

            if (this.conditionColumns.TryGetValue(key, out var result))
            {
                return result;
            }

            var column = this.MakeConditionsSchema(conditionsType, tableSchema);
            this.conditionColumns[key] = column;
            return column;
        }

        /// <summary>
        /// Create the table schema for an entity
        /// </summary>
        public TableSchema MakeTableSchema(Type entityType)
        {
            var tableName = this.tableNameFactory.GetTableName(entityType, this.dialect);
            var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                       .Where(this.CouldBeColumn)
                                       .Select(PropertySchema.MakePropertySchema)
                                       .Where(p => p.FindAttribute<NotMappedAttribute>() == null)
                                       .ToList();

            var explicitKeyDefined = properties.Any(p => p.FindAttribute<KeyAttribute>() != null);

            var columns = properties.Select(p => this.MakeColumnSchema(p, GetColumnUsage(explicitKeyDefined, p)));

            return new TableSchema(tableName, columns.ToImmutableArray());
        }

        /// <summary>
        /// Creates the <see cref="ConditionColumnSchema"/> for the <paramref name="conditionsType"/>.
        /// </summary>
        public ImmutableArray<ConditionColumnSchema> MakeConditionsSchema(Type conditionsType, TableSchema tableSchema)
        {
            return conditionsType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                 .Where(p => this.CouldBeColumn(p) && p.GetCustomAttribute<NotMappedAttribute>() == null && p.CanRead)
                                 .Select(p => MakeConditionSchema(tableSchema, p))
                                 .ToImmutableArray();
        }

        /// <summary>
        /// Creates a new <see cref="TableSchemaFactory"/> which generates table names with the <paramref name="factory"/>.
        /// </summary>
        public TableSchemaFactory WithTableNameFactory(ITableNameFactory factory)
        {
            return new TableSchemaFactory(this.dialect, factory, this.columnNameFactory, this.sqlTypeMappings);
        }

        /// <summary>
        /// Creates a new <see cref="TableSchemaFactory"/> which generates column names with the <paramref name="factory"/>.
        /// </summary>
        public TableSchemaFactory WithColumnNameFactory(IColumnNameFactory factory)
        {
            return new TableSchemaFactory(this.dialect, this.tableNameFactory, factory, this.sqlTypeMappings);
        }

        private bool CouldBeColumn(PropertyInfo property)
        {
            if (property.GetIndexParameters().Length != 0)
            {
                return false;
            }

            var propertyType = property.PropertyType.GetUnderlyingType();
            return propertyType.GetTypeInfo().IsEnum || this.sqlTypeMappings.ContainsKey(propertyType);
        }

        private static ColumnUsage GetColumnUsage(bool explicitKeyDefined, PropertySchema property)
        {
            var isPrimaryKey = explicitKeyDefined
                ? property.FindAttribute<KeyAttribute>() != null
                : string.Equals(property.Name, "Id", StringComparison.OrdinalIgnoreCase);

            if (!property.PropertyInfo.CanWrite)
            {
                return isPrimaryKey
                    ? ColumnUsage.ComputedPrimaryKey
                    : ColumnUsage.ComputedColumn;
            }

            var generatedAttribute = property.FindAttribute<DatabaseGeneratedAttribute>();
            return isPrimaryKey
                ? GetPrimaryKeyUsage(generatedAttribute)
                : GetColumnUsage(generatedAttribute);
        }

        private static ColumnUsage GetColumnUsage(DatabaseGeneratedAttribute generatedAttribute)
        {
            if (generatedAttribute == null)
            {
                return ColumnUsage.Column;
            }

            switch (generatedAttribute.DatabaseGeneratedOption)
            {
                case DatabaseGeneratedOption.None:
                    return ColumnUsage.Column;
                case DatabaseGeneratedOption.Identity:
                    return ColumnUsage.GeneratedColumn;
                case DatabaseGeneratedOption.Computed:
                    return ColumnUsage.ComputedColumn;
                default:
                    throw new ArgumentOutOfRangeException(
                        "Unknown DatabaseGeneratedOption: " + generatedAttribute.DatabaseGeneratedOption);
            }
        }

        private static ColumnUsage GetPrimaryKeyUsage(DatabaseGeneratedAttribute generatedAttribute)
        {
            if (generatedAttribute == null)
            {
                return ColumnUsage.ComputedPrimaryKey;
            }

            switch (generatedAttribute.DatabaseGeneratedOption)
            {
                case DatabaseGeneratedOption.None:
                    return ColumnUsage.NotGeneratedPrimaryKey;
                case DatabaseGeneratedOption.Identity:
                case DatabaseGeneratedOption.Computed:
                    return ColumnUsage.ComputedPrimaryKey;
                default:
                    throw new ArgumentOutOfRangeException(
                        "Unknown DatabaseGeneratedOption: " + generatedAttribute.DatabaseGeneratedOption);
            }
        }

        private static ConditionColumnSchema MakeConditionSchema(TableSchema tableSchema, PropertyInfo property)
        {
            var propertyName = property.Name;
            var possibleColumns = tableSchema.Columns.Where(c => string.Equals(c.ParameterName, propertyName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (possibleColumns.Count > 1)
            {
                possibleColumns = tableSchema.Columns.Where(c => string.Equals(c.ParameterName, propertyName, StringComparison.Ordinal)).ToList();

                if (possibleColumns.Count > 1)
                {
                    throw new InvalidConditionSchemaException($"Ambiguous property '{propertyName}' on table {tableSchema.Name}");
                }
            }

            if (possibleColumns.Count < 1)
            {
                throw new InvalidConditionSchemaException($"Target table {tableSchema.Name} does not have a property called {propertyName}");
            }

            return new ConditionColumnSchema(possibleColumns.Single(), property);
        }

        private ColumnSchema MakeColumnSchema(PropertySchema property, ColumnUsage columnUsage)
        {
            var propertyName = property.Name;

            return new ColumnSchema(this.dialect.MakeColumnName(this.columnNameFactory.GetColumnName(property)), this.dialect.MakeColumnName(propertyName),
                propertyName,
                columnUsage,
                this.GetDbType(property));
        }

        private DbTypeEx GetDbType(PropertySchema property)
        {
            if (property.EffectiveType.GetTypeInfo().IsEnum)
            {
                return new DbTypeEx(DbType.Int32, property.IsNullable, null);
            }

            if (this.sqlTypeMappings.TryGetValue(property.EffectiveType, out var dbType))
            {
                var allowNull = property.IsNullable || (!property.Type.GetTypeInfo().IsValueType && property.FindAttribute<RequiredAttribute>() == null);
                return new DbTypeEx(dbType, allowNull, this.GetMaxLength(property));
            }

            throw new NotSupportedException("Unknown property type: " + property.EffectiveType);
        }

        private int? GetMaxLength(PropertySchema property)
        {
            if (property.EffectiveType == typeof(char))
            {
                return 1;
            }

            return property.FindAttribute<MaxLengthAttribute>()?.Length;
        }
    }
}