﻿// <copyright file="SqlCommandResult.cs" company="Berkeleybross">
// Copyright (c) Berkeleybross. All rights reserved.
// </copyright>
namespace Dapper.MicroCRUD.SqlCommands
{
    /// <summary>
    /// Represents the result of a SQL command.
    /// </summary>
    public struct SqlCommandResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCommandResult"/> struct.
        /// </summary>
        public SqlCommandResult(int numRowsAffected)
        {
            this.NumRowsAffected = numRowsAffected;
        }

        /// <summary>
        /// Gets the number of rows affected
        /// </summary>
        public int NumRowsAffected { get; }

        /// <summary>
        /// Throws an exception if the <see cref="NumRowsAffected"/> does not match the <paramref name="expectedCount"/>
        /// </summary>
        public void ExpectingAffectedRowCountToBe(int expectedCount)
        {
            if (this.NumRowsAffected != expectedCount)
            {
                throw new AffectedRowCountException(
                    $"Expected {expectedCount} rows to be updated, but was actually {this.NumRowsAffected}");
            }
        }
    }
}