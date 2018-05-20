﻿namespace PeregrineDb.Tests.Databases
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Pagination;
    using PeregrineDb.Dialects;
    using PeregrineDb.Tests.ExampleEntities;
    using PeregrineDb.Tests.Utils;
    using Xunit;

    [SuppressMessage("ReSharper", "StringLiteralAsInterpolationArgument")]
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public abstract partial class DefaultDatabaseConnectionCrudAsyncTests
    {
        public class GetPageAsync
            : DefaultDatabaseConnectionCrudAsyncTests
        {
            [Theory]
            [MemberData(nameof(TestDialects))]
            public async Task Returns_empty_list_when_there_are_no_entities(IDialect dialect)
            {
                using (var database = BlankDatabaseFactory.MakeDatabase(dialect))
                {
                    // Act
                    var pageBuilder = new PageIndexPageBuilder(1, 10);
                    var entities = await database.GetPageAsync<Dog>(pageBuilder, null, "Age");

                    // Assert
                    entities.Items.Count().Should().Be(0);
                }
            }

            [Theory]
            [MemberData(nameof(TestDialects))]
            public async Task Filters_result_by_conditions(IDialect dialect)
            {
                using (var database = BlankDatabaseFactory.MakeDatabase(dialect))
                {
                    // Arrange
                    database.Insert(new Dog { Name = "Some Name 1", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 2", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 3", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 4", Age = 11 });

                    // Act
                    var entities = await database.GetPageAsync<Dog>(
                        new PageIndexPageBuilder(1, 10),
                        $"WHERE Name LIKE CONCAT({"Some Name"}, '%') and Age = {10}",
                        "Age");

                    // Assert
                    entities.Items.Count().Should().Be(3);
                }
            }

            [Theory]
            [MemberData(nameof(TestDialects))]
            public async Task Gets_first_page(IDialect dialect)
            {
                using (var database = BlankDatabaseFactory.MakeDatabase(dialect))
                {
                    // Arrange
                    database.Insert(new Dog { Name = "Some Name 1", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 2", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 3", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 4", Age = 11 });

                    // Act
                    var entities = (await database.GetPageAsync<Dog>(
                        new PageIndexPageBuilder(1, 2),
                        $"WHERE Name LIKE CONCAT({"Some Name"}, '%') and Age = {10}",
                        "Age DESC")).Items;

                    // Assert
                    entities.Count().Should().Be(2);
                    entities[0].Name.Should().Be("Some Name 1");
                    entities[1].Name.Should().Be("Some Name 2");
                }
            }


            [Theory]
            [MemberData(nameof(TestDialects))]
            public async Task Gets_second_page(IDialect dialect)
            {
                using (var database = BlankDatabaseFactory.MakeDatabase(dialect))
                {
                    // Arrange
                    database.Insert(new Dog { Name = "Some Name 1", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 2", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 3", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 4", Age = 11 });

                    // Act
                    var entities = (await database.GetPageAsync<Dog>(
                        new PageIndexPageBuilder(2, 2),
                        $"WHERE Name LIKE CONCAT({"Some Name"}, '%') and Age = {10}",
                        "Age DESC")).Items;

                    // Assert
                    entities.Count().Should().Be(1);
                    entities[0].Name.Should().Be("Some Name 3");
                }
            }


            [Theory]
            [MemberData(nameof(TestDialects))]
            public async Task Returns_empty_set_past_last_page(IDialect dialect)
            {
                using (var database = BlankDatabaseFactory.MakeDatabase(dialect))
                {
                    // Arrange
                    database.Insert(new Dog { Name = "Some Name 1", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 2", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 3", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 4", Age = 11 });

                    // Act
                    var entities = (await database.GetPageAsync<Dog>(
                        new PageIndexPageBuilder(3, 2),
                        $"WHERE Name LIKE CONCAT({"Some Name"}, '%') and Age = {10}",
                        "Age DESC")).Items;

                    // Assert
                    entities.Should().BeEmpty();
                }
            }

            [Theory]
            [MemberData(nameof(TestDialects))]
            public async Task Returns_page_from_everything_when_conditions_is_null(IDialect dialect)
            {
                using (var database = BlankDatabaseFactory.MakeDatabase(dialect))
                {
                    // Arrange
                    database.Insert(new Dog { Name = "Some Name 1", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 2", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 3", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 4", Age = 11 });

                    // Act
                    var page = await database.GetPageAsync<Dog>(new PageIndexPageBuilder(2, 2), null, "Age DESC");
                    var entities = page.Items;

                    // Assert
                    entities.Count().Should().Be(2);
                }
            }
        }

        public class GetPageAsyncWhereObject
            : DefaultDatabaseConnectionCrudAsyncTests
        {
            [Theory]
            [MemberData(nameof(TestDialects))]
            public async Task Returns_empty_list_when_there_are_no_entities(IDialect dialect)
            {
                using (var database = BlankDatabaseFactory.MakeDatabase(dialect))
                {
                    // Act
                    var pageBuilder = new PageIndexPageBuilder(1, 10);
                    var entities = await database.GetPageAsync<Dog>(pageBuilder, new { Age = 10 }, "Age");

                    // Assert
                    entities.Items.Should().BeEmpty();
                }
            }


            [Theory]
            [MemberData(nameof(TestDialects))]
            public async Task Filters_result_by_conditions(IDialect dialect)
            {
                using (var database = BlankDatabaseFactory.MakeDatabase(dialect))
                {
                    // Arrange
                    database.Insert(new Dog { Name = "Some Name 1", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 2", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 3", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 4", Age = 11 });

                    // Act
                    var pageBuilder = new PageIndexPageBuilder(1, 10);
                    var entities = await database.GetPageAsync<Dog>(pageBuilder, new { Age = 10 }, "Age");

                    // Assert
                    entities.Items.Count().Should().Be(3);
                }
            }


            [Theory]
            [MemberData(nameof(TestDialects))]
            public async Task Gets_first_page(IDialect dialect)
            {
                using (var database = BlankDatabaseFactory.MakeDatabase(dialect))
                {
                    // Arrange
                    database.Insert(new Dog { Name = "Some Name 1", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 2", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 3", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 4", Age = 11 });

                    // Act
                    var pageBuilder = new PageIndexPageBuilder(1, 2);
                    var page = await database.GetPageAsync<Dog>(pageBuilder, new { Age = 10 }, "Age DESC");
                    var entities = page.Items;

                    // Assert
                    entities.Count().Should().Be(2);
                    entities[0].Name.Should().Be("Some Name 1");
                    entities[1].Name.Should().Be("Some Name 2");
                }
            }

            [Theory]
            [MemberData(nameof(TestDialects))]
            public async Task Gets_second_page(IDialect dialect)
            {
                using (var database = BlankDatabaseFactory.MakeDatabase(dialect))
                {
                    // Arrange
                    database.Insert(new Dog { Name = "Some Name 1", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 2", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 3", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 4", Age = 11 });

                    // Act
                    var pageBuilder = new PageIndexPageBuilder(2, 2);
                    var page = await database.GetPageAsync<Dog>(pageBuilder, new { Age = 10 }, "Age DESC");
                    var entities = page.Items;

                    // Assert
                    entities.Count().Should().Be(1);
                    entities[0].Name.Should().Be("Some Name 3");
                }
            }


            [Theory]
            [MemberData(nameof(TestDialects))]
            public async Task Returns_empty_set_past_last_page(IDialect dialect)
            {
                using (var database = BlankDatabaseFactory.MakeDatabase(dialect))
                {
                    // Arrange
                    database.Insert(new Dog { Name = "Some Name 1", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 2", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 3", Age = 10 });
                    database.Insert(new Dog { Name = "Some Name 4", Age = 11 });

                    // Act
                    var pageBuilder = new PageIndexPageBuilder(3, 2);
                    var page = await database.GetPageAsync<Dog>(pageBuilder, new { Age = 10 }, "Age DESC");
                    var entities = page.Items;

                    // Assert
                    entities.Should().BeEmpty();
                }
            }
        }
    }
}