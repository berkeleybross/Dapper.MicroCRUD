// <copyright file="Identity.cs" company="Berkeleybross">
// Copyright (c) Berkeleybross. All rights reserved.
// </copyright>

namespace PeregrineDb.Databases.Mapper
{
    using System;
    using System.Data;

    /// <summary>
    /// Identity of a cached query in Dapper, used for extensibility.
    /// </summary>
    internal class Identity : IEquatable<Identity>
    {
        internal Identity(string sql, CommandType? commandType, IDbConnection connection, Type type, Type parametersType, Type[] otherTypes)
            : this(sql, commandType, connection.ConnectionString, type, parametersType, otherTypes, 0)
        { /* base call */
        }

        private Identity(string sql, CommandType? commandType, string connectionString, Type type, Type parametersType, Type[] otherTypes, int gridIndex)
        {
            this.sql = sql;
            this.commandType = commandType;
            this.connectionString = connectionString;
            this.type = type;
            this.parametersType = parametersType;
            this.gridIndex = gridIndex;
            unchecked
            {
                this.hashCode = 17; // we *know* we are using this in a dictionary, so pre-compute this
                this.hashCode = (this.hashCode * 23) + commandType.GetHashCode();
                this.hashCode = (this.hashCode * 23) + gridIndex.GetHashCode();
                this.hashCode = (this.hashCode * 23) + (sql?.GetHashCode() ?? 0);
                this.hashCode = (this.hashCode * 23) + (type?.GetHashCode() ?? 0);
                if (otherTypes != null)
                {
                    foreach (var t in otherTypes)
                    {
                        this.hashCode = (this.hashCode * 23) + (t?.GetHashCode() ?? 0);
                    }
                }

                this.hashCode = (this.hashCode * 23) + (connectionString == null ? 0 : SqlMapper.connectionStringComparer.GetHashCode(connectionString));
                this.hashCode = (this.hashCode * 23) + (parametersType?.GetHashCode() ?? 0);
            }
        }

        /// <summary>
        /// Create an identity for use with DynamicParameters, internal use only.
        /// </summary>
        /// <param name="type">The parameters type to create an <see cref="Identity"/> for.</param>
        public Identity ForDynamicParameters(Type type) => new Identity(this.sql, this.commandType, this.connectionString, this.type, type, null, -1);

        /// <summary>
        /// Whether this <see cref="Identity"/> equals another.
        /// </summary>
        /// <param name="obj">The other <see cref="object"/> to compare to.</param>
        public override bool Equals(object obj) => this.Equals(obj as Identity);

        /// <summary>
        /// The raw SQL command.
        /// </summary>
        public readonly string sql;

        /// <summary>
        /// The SQL command type.
        /// </summary>
        public readonly CommandType? commandType;

        /// <summary>
        /// The hash code of this Identity.
        /// </summary>
        public readonly int hashCode;

        /// <summary>
        /// The grid index (position in the reader) of this Identity.
        /// </summary>
        public readonly int gridIndex;

        /// <summary>
        /// This <see cref="Type"/> of this Identity.
        /// </summary>
        public readonly Type type;

        /// <summary>
        /// The connection string for this Identity.
        /// </summary>
        public readonly string connectionString;

        /// <summary>
        /// The type of the parameters object for this Identity.
        /// </summary>
        public readonly Type parametersType;

        /// <summary>
        /// Gets the hash code for this identity.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => this.hashCode;

        /// <summary>
        /// Compare 2 Identity objects
        /// </summary>
        /// <param name="other">The other <see cref="Identity"/> object to compare.</param>
        /// <returns>Whether the two are equal</returns>
        public bool Equals(Identity other)
        {
            return other != null
                   && this.gridIndex == other.gridIndex
                   && this.type == other.type
                   && this.sql == other.sql
                   && this.commandType == other.commandType
                   && SqlMapper.connectionStringComparer.Equals(this.connectionString, other.connectionString)
                   && this.parametersType == other.parametersType;
        }
    }
}
