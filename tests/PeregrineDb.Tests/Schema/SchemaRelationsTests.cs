﻿namespace Dapper.MicroCRUD.Tests.Schema
{
    using System;
    using Dapper.MicroCRUD.Schema.Relations;
    using FluentAssertions;
    using Xunit;

    public class SchemaRelationsTests
    {
        public class GetClearDataCommands
            : SchemaRelationsTests
        {
            [Fact]
            public void Returns_no_commands_when_there_are_no_tables()
            {
                // Arrange
                var sut = new SchemaRelations();

                // Act
                var result = sut.GetClearDataCommands();

                // Assert
                result.Should().BeEmpty();
            }

            [Fact]
            public void Ignores_relationships_for_tables_which_dont_exist()
            {
                // Arrange
                var sut = new SchemaRelations();
                sut.AddTable("A");

                sut.AddRelationship("B", "A", "A -> B", false);

                // Assert
                var result = sut.GetClearDataCommands();

                // Assert
                result.ShouldAllBeEquivalentTo(new[]
                        {
                            new ClearTableCommand("A")
                        },
                    o => o.RespectingRuntimeTypes());
            }

            [Fact]
            public void Returns_clear_table_commands_when_there_are_no_tables_with_relations()
            {
                // Arrange
                var sut = new SchemaRelations();
                sut.AddTable("Foo");
                sut.AddTable("Bar");

                // Act
                var result = sut.GetClearDataCommands();

                // Assert
                result.ShouldAllBeEquivalentTo(new[]
                        {
                            new ClearTableCommand("Foo"),
                            new ClearTableCommand("Bar")
                        },
                    o => o.RespectingRuntimeTypes());
            }

            /// <summary>
            /// Bar -> Foo
            /// 
            /// Therefore Bar needs to be deleted first
            /// </summary>
            [Fact]
            public void Returns_clear_table_command_for_referencing_table_before_referenced_table()
            {
                // Arrange
                var sut = new SchemaRelations();
                sut.AddTable("Foo");
                sut.AddTable("Bar");

                sut.AddRelationship("Foo", "Bar", "Foo_Bar", false);

                // Act
                var result = sut.GetClearDataCommands();

                // Assert
                result.ShouldAllBeEquivalentTo(new[]
                        {
                            new ClearTableCommand("Bar"),
                            new ClearTableCommand("Foo")
                        },
                    o => o.RespectingRuntimeTypes().WithStrictOrdering());
            }

            /// <summary>
            /// Baz -> Bar -> Foo
            /// 
            /// Therefore Baz needs to be deleted first, then Bar and then Foo
            /// </summary>
            [Fact]
            public void Returns_clear_table_command_for_referencing_tables_before_referenced_tables()
            {
                // Arrange
                var sut = new SchemaRelations();
                sut.AddTable("Foo");
                sut.AddTable("Bar");
                sut.AddTable("Baz");

                sut.AddRelationship("Bar", "Baz", "Bar_Baz", false);
                sut.AddRelationship("Foo", "Bar", "Foo_Bar", false);

                // Act
                var result = sut.GetClearDataCommands();

                // Assert
                result.ShouldAllBeEquivalentTo(new[]
                        {
                            new ClearTableCommand("Baz"),
                            new ClearTableCommand("Bar"),
                            new ClearTableCommand("Foo")
                        },
                    o => o.RespectingRuntimeTypes().WithStrictOrdering());
            }

            /// <summary>
            /// B -> A
            /// C -^ 
            /// 
            /// Therefore B and C need to be deleted before A
            /// </summary>
            [Fact]
            public void Returns_clear_table_command_for_multiple_referencing_tables_before_referenced_table()
            {
                // Arrange
                var sut = new SchemaRelations();
                sut.AddTable("A");
                sut.AddTable("B");
                sut.AddTable("C");

                sut.AddRelationship("A", "B", "B_A", false);
                sut.AddRelationship("A", "C", "C_A", false);

                // Act
                var result = sut.GetClearDataCommands();

                // Assert
                result.ShouldAllBeEquivalentTo(new[]
                        {
                            new ClearTableCommand("B"),
                            new ClearTableCommand("C"),
                            new ClearTableCommand("A")
                        },
                    o => o.RespectingRuntimeTypes().WithStrictOrdering());
            }

            /// <summary>
            /// A ~> B -> A
            /// 
            /// Therefore A needs to be set to null before B can be deleted, and last A can be deleted
            /// </summary>
            [Fact]
            public void Clears_foreign_key_columns_before_clearing_recursive_relations()
            {
                // Arrange
                var sut = new SchemaRelations();
                sut.AddTable("A");
                sut.AddTable("B");

                sut.AddRelationship("A", "B", "B -> A", false);
                sut.AddRelationship("B", "A", "A ~> B", true);

                // Act
                var result = sut.GetClearDataCommands();

                // Assert
                result.ShouldAllBeEquivalentTo(new object[]
                        {
                            new NullColumnCommand("A", "A ~> B"),
                            new ClearTableCommand("B"),
                            new ClearTableCommand("A")
                        },
                    o => o.RespectingRuntimeTypes().WithStrictOrdering());
            }

            /// <summary>
            /// C -> A ~> B -> A
            /// </summary>
            [Fact]
            public void Clears_leaf_tables_before_nulling_foreign_keys()
            {
                // Arrange
                var sut = new SchemaRelations();
                sut.AddTable("A");
                sut.AddTable("B");
                sut.AddTable("C");

                sut.AddRelationship("A", "B", "B -> A", false);
                sut.AddRelationship("B", "A", "A ~> B", true);
                sut.AddRelationship("A", "C", "C -> A", false);

                // Act
                var result = sut.GetClearDataCommands();

                // Assert
                result.ShouldAllBeEquivalentTo(new object[]
                        {
                            new ClearTableCommand("C"),
                            new NullColumnCommand("A", "A ~> B"),
                            new ClearTableCommand("B"),
                            new ClearTableCommand("A")
                        },
                    o => o.RespectingRuntimeTypes().WithStrictOrdering());
            }


            /// <summary>
            /// A ~> B -> A
            ///   C -^
            /// </summary>
            [Fact]
            public void Clears_leaf_tables_before_nulling_foreign_keys_at_any_point_in_chain()
            {
                // Arrange
                var sut = new SchemaRelations();
                sut.AddTable("A");
                sut.AddTable("B");
                sut.AddTable("C");

                sut.AddRelationship("A", "B", "B -> A", false);
                sut.AddRelationship("B", "A", "A ~> B", true);
                sut.AddRelationship("B", "C", "C -> B", false);

                // Act
                var result = sut.GetClearDataCommands();

                // Assert
                result.ShouldAllBeEquivalentTo(new object[]
                        {
                            new ClearTableCommand("C"),
                            new NullColumnCommand("A", "A ~> B"),
                            new ClearTableCommand("B"),
                            new ClearTableCommand("A")
                        },
                    o => o.RespectingRuntimeTypes().WithStrictOrdering());
            }

            /// <summary>
            /// A -> B -> A
            /// </summary>
            [Fact]
            public void Throws_exception_if_there_are_no_nullable_foreign_keys()
            {
                // Arrange
                var sut = new SchemaRelations();
                sut.AddTable("A");
                sut.AddTable("B");
                sut.AddTable("C");

                sut.AddRelationship("A", "B", "B -> A", false);
                sut.AddRelationship("B", "A", "A -> B", false);

                // Act
                Action act = () => sut.GetClearDataCommands();

                // Assert
                act.ShouldThrow<InvalidOperationException>();
            }

            /// <summary>
            ///    A ~> B -> A -> C
            ///         ^~~~~~~~~~|
            /// </summary>
            [Fact]
            public void Nulls_multiple_foreign_keys()
            {
                // Arrange
                var sut = new SchemaRelations();
                sut.AddTable("A");
                sut.AddTable("B");
                sut.AddTable("C");

                sut.AddRelationship("A", "B", "B -> A", false);
                sut.AddRelationship("C", "A", "A -> C", false);
                sut.AddRelationship("B", "A", "A ~> B", true);
                sut.AddRelationship("B", "C", "C ~> B", true);

                // Assert
                var result = sut.GetClearDataCommands();

                // Assert
                result.ShouldAllBeEquivalentTo(new object[]
                        {
                            new NullColumnCommand("A", "A ~> B"),
                            new NullColumnCommand("C", "C ~> B"),
                            new ClearTableCommand("B"),
                            new ClearTableCommand("A"),
                            new ClearTableCommand("C")
                        },
                    o => o.RespectingRuntimeTypes().WithStrictOrdering());
            }

            /// <summary>
            ///   A -> B ~> C
            ///   ^~~~~|
            /// </summary>
            [Fact]
            public void Only_nulls_out_minimum_necessary()
            {
                // Arrange
                var sut = new SchemaRelations();
                sut.AddTable("A");
                sut.AddTable("B");
                sut.AddTable("C");

                sut.AddRelationship("B", "A", "A -> B", false);
                sut.AddRelationship("A", "B", "B ~> A", true);
                sut.AddRelationship("C", "B", "B ~> C", true);

                // Assert
                var result = sut.GetClearDataCommands();

                // Assert
                result.ShouldAllBeEquivalentTo(new object[]
                        {
                            new NullColumnCommand("B", "B ~> A"),
                            new ClearTableCommand("A"),
                            new ClearTableCommand("B"),
                            new ClearTableCommand("C")
                        },
                    o => o.RespectingRuntimeTypes().WithStrictOrdering());
            }

            /// <summary>
            ///   A -> B ~> C ~> D
            ///   ^~~~~|
            /// </summary>
            [Fact]
            public void Only_nulls_out_minimum_necessary_regardless_of_order_added()
            {
                // Arrange
                var sut = new SchemaRelations();
                sut.AddTable("A");
                sut.AddTable("B");
                sut.AddTable("C");
                sut.AddTable("D");

                sut.AddRelationship("B", "A", "A -> B", false);
                sut.AddRelationship("C", "B", "B ~> C", true);
                sut.AddRelationship("A", "B", "B ~> A", true);
                sut.AddRelationship("D", "C", "C ~> D", true);

                // Assert
                var result = sut.GetClearDataCommands();

                // Assert
                result.ShouldAllBeEquivalentTo(new object[]
                        {
                            new NullColumnCommand("B", "B ~> A"),
                            new ClearTableCommand("A"),
                            new ClearTableCommand("B"),
                            new ClearTableCommand("C"),
                            new ClearTableCommand("D")
                        },
                    o => o.RespectingRuntimeTypes().WithStrictOrdering());
            }
        }
    }
}