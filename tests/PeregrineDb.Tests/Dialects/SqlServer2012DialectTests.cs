﻿// <copyright file="SqlServer2012DialectTests.cs" company="Berkeleybross">
// Copyright (c) Berkeleybross. All rights reserved.
// </copyright>
namespace Dapper.MicroCRUD.Tests.Dialects
{
    using System;
    using System.Collections.Immutable;
    using System.Data;
    using Dapper.MicroCRUD.Dialects;
    using Dapper.MicroCRUD.Schema;
    using Dapper.MicroCRUD.Tests.ExampleEntities;
    using Dapper.MicroCRUD.Tests.Utils;
    using Moq;
    using Pagination;
    using Xunit;

    public class SqlServer2012DialectTests
    {
        private readonly IDialect dialect = Dialect.SqlServer2012;

        public class MakeCountStatement
            : SqlServer2012DialectTests
        {
            [Fact]
            public void Selects_from_given_table()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeCountStatement(schema, null);

                // Assert
                var expected = @"SELECT COUNT(*)
FROM [Users]";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Adds_conditions()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeCountStatement(schema, "WHERE Foo IS NOT NULL");

                // Assert
                var expected = @"SELECT COUNT(*)
FROM [Users]
WHERE Foo IS NOT NULL";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }
        }

        public class MakeFindStatement
            : SqlServer2012DialectTests
        {
            [Fact]
            public void Selects_from_given_table()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeFindStatement(schema);

                // Assert
                var expected = @"SELECT [Id], [Name], [Age]
FROM [Users]
WHERE [Id] = @Id";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Uses_non_default_primary_key_name()
            {
                // Arrange
                var schema = this.dialect.KeyExplicit();

                // Act
                var sql = this.dialect.MakeFindStatement(schema);

                // Assert
                var expected = @"SELECT [Key], [Name]
FROM [KeyExplicit]
WHERE [Key] = @Key";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Uses_each_key_in_composite_key()
            {
                // Arrange
                var schema = this.dialect.CompositeKeys();

                // Act
                var sql = this.dialect.MakeFindStatement(schema);

                // Assert
                var expected = @"SELECT [Key1], [Key2], [Name]
FROM [CompositeKeys]
WHERE [Key1] = @Key1 AND [Key2] = @Key2";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Adds_alias_when_primary_key_is_aliased()
            {
                // Arrange
                var schema = this.dialect.KeyAlias();

                // Act
                var sql = this.dialect.MakeFindStatement(schema);

                // Assert
                var expected = @"SELECT [Key] AS [Id], [Name]
FROM [KeyAlias]
WHERE [Key] = @Id";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Adds_alias_when_column_name_is_aliased()
            {
                // Arrange
                var schema = this.dialect.PropertyAlias();

                // Act
                var sql = this.dialect.MakeFindStatement(schema);

                // Assert
                var expected = @"SELECT [Id], [YearsOld] AS [Age]
FROM [PropertyAlias]
WHERE [Id] = @Id";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }
        }

        public class MakeGetRangeStatement
            : SqlServer2012DialectTests
        {
            [Fact]
            public void Selects_from_given_table()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeGetRangeStatement(schema, null);

                // Assert
                var expected = @"SELECT [Id], [Name], [Age]
FROM [Users]";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Adds_conditions_clause()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeGetRangeStatement(schema, "WHERE Age > @Age");

                // Assert
                var expected = @"SELECT [Id], [Name], [Age]
FROM [Users]
WHERE Age > @Age";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Uses_explicit_primary_key_name()
            {
                // Arrange
                var schema = this.dialect.KeyExplicit();

                // Act
                var sql = this.dialect.MakeGetRangeStatement(schema, null);

                // Assert
                var expected = @"SELECT [Key], [Name]
FROM [KeyExplicit]";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Adds_alias_when_primary_key_is_aliased()
            {
                // Arrange
                var schema = this.dialect.KeyAlias();

                // Act
                var sql = this.dialect.MakeGetRangeStatement(schema, null);

                // Assert
                var expected = @"SELECT [Key] AS [Id], [Name]
FROM [KeyAlias]";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Adds_alias_when_column_name_is_aliased()
            {
                // Arrange
                var schema = this.dialect.PropertyAlias();

                // Act
                var sql = this.dialect.MakeGetRangeStatement(schema, null);

                // Assert
                var expected = @"SELECT [Id], [YearsOld] AS [Age]
FROM [PropertyAlias]";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }
        }

        public class MakeGetTopNStatement
            : SqlServer2012DialectTests
        {
            [Fact]
            public void Selects_from_given_table()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeGetTopNStatement(schema, 1, null, "Name");

                // Assert
                var expected = @"SELECT TOP 1 [Id], [Name], [Age]
FROM [Users]
ORDER BY Name";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Adds_conditions_clause()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeGetTopNStatement(schema, 1, "WHERE Name LIKE 'Foo%'", "Name");

                // Assert
                var expected = @"SELECT TOP 1 [Id], [Name], [Age]
FROM [Users]
WHERE Name LIKE 'Foo%'
ORDER BY Name";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Adds_alias_when_column_name_is_aliased()
            {
                // Arrange
                var schema = this.dialect.PropertyAlias();

                // Act
                var sql = this.dialect.MakeGetTopNStatement(schema, 1, null, "Name");

                // Assert
                var expected = @"SELECT TOP 1 [Id], [YearsOld] AS [Age]
FROM [PropertyAlias]
ORDER BY Name";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void Does_not_order_when_no_orderby_given(string orderBy)
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeGetTopNStatement(schema, 1, null, orderBy);

                // Assert
                var expected = @"SELECT TOP 1 [Id], [Name], [Age]
FROM [Users]";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }
        }

        public class MakeGetPageStatement
            : SqlServer2012DialectTests
        {
            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            public void Throws_exception_when_order_by_is_empty(string orderBy)
            {
                // Arrange
                var schema = this.dialect.User();

                // Act / Assert
                Assert.Throws<ArgumentException>(
                    () => this.dialect.MakeGetPageStatement(schema, new Page(1, 10, true, 0, 9), null, orderBy));
            }

            [Fact]
            public void Selects_from_given_table()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeGetPageStatement(schema, new Page(1, 10, true, 0, 9), null, "Name");

                // Assert
                var expected = @"SELECT [Id], [Name], [Age]
FROM [Users]
ORDER BY Name
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Adds_conditions_clause()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeGetPageStatement(schema, new Page(1, 10, true, 0, 9), "WHERE Name LIKE 'Foo%'", "Name");

                // Assert
                var expected = @"SELECT [Id], [Name], [Age]
FROM [Users]
WHERE Name LIKE 'Foo%'
ORDER BY Name
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Adds_alias_when_column_name_is_aliased()
            {
                // Arrange
                var schema = this.dialect.PropertyAlias();

                // Act
                var sql = this.dialect.MakeGetPageStatement(schema, new Page(1, 10, true, 0, 9), null, "Name");

                // Assert
                var expected = @"SELECT [Id], [YearsOld] AS [Age]
FROM [PropertyAlias]
ORDER BY Name
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Selects_second_page()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeGetPageStatement(schema, new Page(2, 10, true, 10, 19), null, "Name");

                // Assert
                var expected = @"SELECT [Id], [Name], [Age]
FROM [Users]
ORDER BY Name
OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Selects_appropriate_number_of_rows()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeGetPageStatement(schema, new Page(2, 5, true, 5, 9), null, "Name");

                // Assert
                var expected = @"SELECT [Id], [Name], [Age]
FROM [Users]
ORDER BY Name
OFFSET 5 ROWS FETCH NEXT 5 ROWS ONLY";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }
        }

        public class MakeInsertStatement
            : SqlServer2012DialectTests
        {
            [Fact]
            public void Inserts_into_given_table()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeInsertStatement(schema);

                // Assert
                var expected = @"INSERT INTO [Users] ([Name], [Age])
VALUES (@Name, @Age);";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Adds_primary_key_if_its_not_generated_by_database()
            {
                // Arrange
                var schema = this.dialect.KeyNotGenerated();

                // Act
                var sql = this.dialect.MakeInsertStatement(schema);

                // Assert
                var expected = @"INSERT INTO [KeyNotGenerated] ([Id], [Name])
VALUES (@Id, @Name);";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Does_not_include_computed_columns()
            {
                // Arrange
                var schema = this.dialect.PropertyComputed();

                // Act
                var sql = this.dialect.MakeInsertStatement(schema);

                // Assert
                var expected = @"INSERT INTO [PropertyComputed] ([Name])
VALUES (@Name);";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Does_not_include_generated_columns()
            {
                // Arrange
                var schema = this.dialect.PropertyGenerated();

                // Act
                var sql = this.dialect.MakeInsertStatement(schema);

                // Assert
                var expected = @"INSERT INTO [PropertyGenerated] ([Name])
VALUES (@Name);";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }
        }

        public class MakeInsertReturningIdentityStatement
            : SqlServer2012DialectTests
        {
            [Fact]
            public void Inserts_into_given_table()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeInsertReturningIdentityStatement(schema);

                // Assert
                var expected = @"INSERT INTO [Users] ([Name], [Age])
VALUES (@Name, @Age);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS [id]";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Adds_primary_key_if_its_not_generated_by_database()
            {
                // Arrange
                var schema = this.dialect.KeyNotGenerated();

                // Act
                var sql = this.dialect.MakeInsertReturningIdentityStatement(schema);

                // Assert
                var expected = @"INSERT INTO [KeyNotGenerated] ([Id], [Name])
VALUES (@Id, @Name);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS [id]";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Does_not_include_computed_columns()
            {
                // Arrange
                var schema = this.dialect.PropertyComputed();

                // Act
                var sql = this.dialect.MakeInsertReturningIdentityStatement(schema);

                // Assert
                var expected = @"INSERT INTO [PropertyComputed] ([Name])
VALUES (@Name);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS [id]";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Does_not_include_generated_columns()
            {
                // Arrange
                var schema = this.dialect.PropertyGenerated();

                // Act
                var sql = this.dialect.MakeInsertReturningIdentityStatement(schema);

                // Assert
                var expected = @"INSERT INTO [PropertyGenerated] ([Name])
VALUES (@Name);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS [id]";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }
        }

        public class MakeUpdateStatement
            : SqlServer2012DialectTests
        {
            [Fact]
            public void Updates_given_table()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeUpdateStatement(schema);

                // Assert
                var expected = @"UPDATE [Users]
SET [Name] = @Name, [Age] = @Age
WHERE [Id] = @Id";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Uses_each_key_in_composite_key()
            {
                // Arrange
                var schema = this.dialect.CompositeKeys();

                // Act
                var sql = this.dialect.MakeUpdateStatement(schema);

                // Assert
                var expected = @"UPDATE [CompositeKeys]
SET [Name] = @Name
WHERE [Key1] = @Key1 AND [Key2] = @Key2";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Does_not_update_primary_key_even_if_its_not_auto_generated()
            {
                // Arrange
                var schema = this.dialect.KeyNotGenerated();

                // Act
                var sql = this.dialect.MakeUpdateStatement(schema);

                // Assert
                var expected = @"UPDATE [KeyNotGenerated]
SET [Name] = @Name
WHERE [Id] = @Id";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Uses_aliased_property_names()
            {
                // Arrange
                var schema = this.dialect.PropertyAlias();

                // Act
                var sql = this.dialect.MakeUpdateStatement(schema);

                // Assert
                var expected = @"UPDATE [PropertyAlias]
SET [YearsOld] = @Age
WHERE [Id] = @Id";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Uses_aliased_key_name()
            {
                // Arrange
                var schema = this.dialect.KeyAlias();

                // Act
                var sql = this.dialect.MakeUpdateStatement(schema);

                // Assert
                var expected = @"UPDATE [KeyAlias]
SET [Name] = @Name
WHERE [Key] = @Id";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Uses_explicit_key_name()
            {
                // Arrange
                var schema = this.dialect.KeyExplicit();

                // Act
                var sql = this.dialect.MakeUpdateStatement(schema);

                // Assert
                var expected = @"UPDATE [KeyExplicit]
SET [Name] = @Name
WHERE [Key] = @Key";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Does_not_include_computed_columns()
            {
                // Arrange
                var schema = this.dialect.PropertyComputed();

                // Act
                var sql = this.dialect.MakeUpdateStatement(schema);

                // Assert
                var expected = @"UPDATE [PropertyComputed]
SET [Name] = @Name
WHERE [Id] = @Id";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Includes_generated_columns()
            {
                // Arrange
                var schema = this.dialect.PropertyGenerated();

                // Act
                var sql = this.dialect.MakeUpdateStatement(schema);

                // Assert
                var expected = @"UPDATE [PropertyGenerated]
SET [Name] = @Name, [Created] = @Created
WHERE [Id] = @Id";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }
        }

        public class MakeDeleteByPrimaryKeyStatement
            : SqlServer2012DialectTests
        {
            [Fact]
            public void Deletes_from_given_table()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeDeleteByPrimaryKeyStatement(schema);

                // Assert
                var expected = @"DELETE FROM [Users]
WHERE [Id] = @Id";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Uses_each_key_in_composite_key()
            {
                // Arrange
                var schema = this.dialect.CompositeKeys();

                // Act
                var sql = this.dialect.MakeDeleteByPrimaryKeyStatement(schema);

                // Assert
                var expected = @"DELETE FROM [CompositeKeys]
WHERE [Key1] = @Key1 AND [Key2] = @Key2";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Uses_primary_key_even_if_its_not_auto_generated()
            {
                // Arrange
                var schema = this.dialect.KeyNotGenerated();

                // Act
                var sql = this.dialect.MakeDeleteByPrimaryKeyStatement(schema);

                // Assert
                var expected = @"DELETE FROM [KeyNotGenerated]
WHERE [Id] = @Id";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Uses_aliased_key_name()
            {
                // Arrange
                var schema = this.dialect.KeyAlias();

                // Act
                var sql = this.dialect.MakeDeleteByPrimaryKeyStatement(schema);

                // Assert
                var expected = @"DELETE FROM [KeyAlias]
WHERE [Key] = @Id";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Uses_explicit_key_name()
            {
                // Arrange
                var schema = this.dialect.KeyExplicit();

                // Act
                var sql = this.dialect.MakeDeleteByPrimaryKeyStatement(schema);

                // Assert
                var expected = @"DELETE FROM [KeyExplicit]
WHERE [Key] = @Key";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }
        }

        public class MakeDeleteRangeStatement
            : SqlServer2012DialectTests
        {
            [Fact]
            public void Deletes_from_given_table()
            {
                // Arrange
                var schema = this.dialect.User();

                // Act
                var sql = this.dialect.MakeDeleteRangeStatement(schema, "WHERE [Age] > 10");

                // Assert
                var expected = @"DELETE FROM [Users]
WHERE [Age] > 10";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }
        }

        public class MakeWhereClause
            : SqlServer2012DialectTests
        {
            [Fact]
            public void Returns_empty_string_for_empty_conditions_object()
            {
                // Arrange
                var conditions = new { };
                var schema = this.GetConditionsSchema<User>(conditions);

                // Act
                var sql = this.dialect.MakeWhereClause(schema, conditions);

                // Assert
                var expected = string.Empty;
                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Includes_column_in_where_clause()
            {
                // Arrange
                var conditions = new { Name = "Bobby" };
                var schema = this.GetConditionsSchema<User>(conditions);

                // Act
                var sql = this.dialect.MakeWhereClause(schema, conditions);

                // Assert
                var expected = @"WHERE [Name] = @Name";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Includes_all_columns_in_where_clause()
            {
                // Arrange
                var conditions = new { Name = "Bobby", Age = 5 };
                var schema = this.GetConditionsSchema<User>(conditions);

                // Act
                var sql = this.dialect.MakeWhereClause(schema, conditions);

                // Assert
                var expected = @"WHERE [Name] = @Name AND [Age] = @Age";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            [Fact]
            public void Checks_for_null_when_condition_value_is_null()
            {
                // Arrange
                var conditions = new { Name = (string)null };
                var schema = this.GetConditionsSchema<User>(conditions);

                // Act
                var sql = this.dialect.MakeWhereClause(schema, conditions);

                // Assert
                var expected = @"WHERE [Name] IS NULL";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }

            private ImmutableArray<ConditionColumnSchema> GetConditionsSchema<TEntity>(object value)
            {
                var config = new MicroCRUDConfig(this.dialect, new TableSchemaFactory(new DefaultTableNameFactory(), new DefaultColumnNameFactory()), true);
                var tableSchema = TableSchemaFactory.GetTableSchema(typeof(TEntity), config.Dialect, config.SchemaFactory);
                return TableSchemaFactory.GetConditionsSchema(typeof(TEntity), tableSchema, value.GetType(), config.Dialect, config.SchemaFactory);
            }
        }

        public class MakeCreateTempTableStatement
            : SqlServer2012DialectTests
        {
            private readonly Mock<ITableNameFactory> tableNameFactory;

            public MakeCreateTempTableStatement()
            {
                MicroCRUDConfig.AddTypeMap(typeof(DateTime), DbType.DateTime2);
                this.tableNameFactory = new Mock<ITableNameFactory>();

                var defaultTableNameFactory = new DefaultTableNameFactory();
                this.tableNameFactory.Setup(f => f.GetTableName(It.IsAny<Type>(), It.IsAny<IDialect>()))
                    .Returns((Type type, IDialect d) => "[#" + defaultTableNameFactory.GetTableName(type, d).Substring(1));
            }

            [Fact]
            public void Throws_exception_when_tablename_doesnt_begin_with_a_hash()
            {
                // Arrange
                this.tableNameFactory.Setup(f => f.GetTableName(It.IsAny<Type>(), It.IsAny<IDialect>()))
                    .Returns((Type type, IDialect d) => "table");
                var tableSchema = this.dialect.MakeSchema<User>(this.tableNameFactory.Object);

                // Act
                Assert.Throws<ArgumentException>(() => this.dialect.MakeCreateTempTableStatement(tableSchema));
            }

            [Fact]
            public void Throws_exception_if_there_are_no_columns()
            {
                // Arrange
                var tableSchema = this.dialect.MakeSchema<NoColumns>(this.tableNameFactory.Object);

                // Act
                Assert.Throws<ArgumentException>(() => this.dialect.MakeCreateTempTableStatement(tableSchema));
            }

            [Fact]
            public void Creates_table_with_all_possible_types()
            {
                // Arrange
                var tableSchema = this.dialect.MakeSchema<TempAllPossibleTypes>(this.tableNameFactory.Object);

                // Act
                var sql = this.dialect.MakeCreateTempTableStatement(tableSchema);

                // Assert
                var expected = @"CREATE TABLE [#TempAllPossibleTypes]
(
    [Id] INT NOT NULL,
    [Int16Property] SMALLINT NOT NULL,
    [NullableInt16Property] SMALLINT NULL,
    [Int32Property] INT NOT NULL,
    [NullableInt32Property] INT NULL,
    [Int64Property] BIGINT NOT NULL,
    [NullableInt64Property] BIGINT NULL,
    [SingleProperty] REAL NOT NULL,
    [NullableSingleProperty] REAL NULL,
    [DoubleProperty] FLOAT NOT NULL,
    [NullableDoubleProperty] FLOAT NULL,
    [DecimalProperty] NUMERIC NOT NULL,
    [NullableDecimalProperty] NUMERIC NULL,
    [BoolProperty] BIT NOT NULL,
    [NullableBoolProperty] BIT NULL,
    [StringProperty] NVARCHAR(MAX) NOT NULL,
    [NullableStringProperty] NVARCHAR(MAX) NULL,
    [FixedLengthStringProperty] NVARCHAR(50) NULL,
    [CharProperty] NCHAR(1) NOT NULL,
    [NullableCharProperty] NCHAR(1) NULL,
    [GuidProperty] UNIQUEIDENTIFIER NOT NULL,
    [NullableGuidProperty] UNIQUEIDENTIFIER NULL,
    [DateTimeProperty] DATETIME2(7) NOT NULL,
    [NullableDateTimeProperty] DATETIME2(7) NULL,
    [DateTimeOffsetProperty] DATETIMEOFFSET NOT NULL,
    [NullableDateTimeOffsetProperty] DATETIMEOFFSET NULL,
    [ByteArrayProperty] VARBINARY(MAX) NOT NULL,
    [Color] INT NOT NULL,
    [NullableColor] INT NULL
);";

                Assert.Equal(expected, sql, SqlStringComparer.Instance);
            }
        }
    }
}