﻿namespace Dapper.MicroCRUD.Tests.Databases
{
    using System;
    using System.Data;
    using Dapper.MicroCRUD.Databases;
    using FluentAssertions;
    using Moq;
    using Xunit;

    public class DefaultDatabaseTests
    {
        public class Dispose
        {
            [Fact]
            public void Disposes_of_connection()
            {
                // Arrange
                var database = new Mock<IDbConnection>();

                var sut = new DefaultDatabase(database.Object, Dialect.PostgreSql);

                // Act
                sut.Dispose();

                // Assert
                database.Verify(d => d.Dispose());
            }
        }

        public class StartUnitOfWork
        {
            [Fact]
            public void Does_not_dispose_of_itself_when_an_error_occurs()
            {
                // Arrange
                var database = new Mock<IDbConnection>();

                var sut = new DefaultDatabase(database.Object, Dialect.PostgreSql);

                // Act
                Action act = () => sut.StartUnitOfWork();

                // Assert
                act.ShouldThrowExactly<ArgumentNullException>();
                database.Verify(d => d.Dispose(), Times.Never);
            }

            [Fact]
            public void Returns_a_unit_of_work_with_right_transaction()
            {
                // Arrange
                var database = new Mock<IDbConnection>();
                var transaction = new Mock<IDbTransaction>();
                database.Setup(d => d.BeginTransaction()).Returns(transaction.Object);

                var sut = new DefaultDatabase(database.Object, Dialect.PostgreSql);

                // Act
                var result = sut.StartUnitOfWork();

                // Assert
                result.Transaction.Should().BeSameAs(transaction.Object);
            }

            [Fact]
            public void Does_not_dispose_of_database_when_unit_of_work_is_disposed()
            {
                // Arrange
                var database = new Mock<IDbConnection>();
                var transaction = new Mock<IDbTransaction>();
                database.Setup(d => d.BeginTransaction()).Returns(transaction.Object);

                var sut = new DefaultDatabase(database.Object, Dialect.PostgreSql);

                // Act
                using (sut.StartUnitOfWork())
                {
                }

                // Assert
                database.Verify(d => d.Dispose(), Times.Never);
            }
        }

        public class StartUnitOfWorkWithIsolationLevel
        {
            [Fact]
            public void Does_not_dispose_of_itself_when_an_error_occurs()
            {
                // Arrange
                var database = new Mock<IDbConnection>();

                var sut = new DefaultDatabase(database.Object, Dialect.PostgreSql);

                // Act
                Action act = () => sut.StartUnitOfWork(IsolationLevel.ReadCommitted);

                // Assert
                act.ShouldThrowExactly<ArgumentNullException>();
                database.Verify(d => d.Dispose(), Times.Never);
            }

            [Fact]
            public void Returns_a_unit_of_work_with_right_transaction()
            {
                // Arrange
                var database = new Mock<IDbConnection>();
                var transaction = new Mock<IDbTransaction>();
                database.Setup(d => d.BeginTransaction(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

                var sut = new DefaultDatabase(database.Object, Dialect.PostgreSql);

                // Act
                var result = sut.StartUnitOfWork(IsolationLevel.ReadCommitted);

                // Assert
                result.Transaction.Should().BeSameAs(transaction.Object);
            }

            [Fact]
            public void Does_not_dispose_of_database_when_unit_of_work_is_disposed()
            {
                // Arrange
                var database = new Mock<IDbConnection>();
                var transaction = new Mock<IDbTransaction>();
                database.Setup(d => d.BeginTransaction(IsolationLevel.ReadCommitted)).Returns(transaction.Object);

                var sut = new DefaultDatabase(database.Object, Dialect.PostgreSql);

                // Act
                using (sut.StartUnitOfWork(IsolationLevel.ReadCommitted))
                {
                }

                // Assert
                database.Verify(d => d.Dispose(), Times.Never);
            }
        }
    }
}