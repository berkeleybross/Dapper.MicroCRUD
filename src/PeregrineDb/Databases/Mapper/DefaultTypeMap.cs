﻿namespace PeregrineDb.Databases.Mapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using PeregrineDb.Utils;

    /// <summary>
    /// Represents default type mapping strategy used by Dapper
    /// </summary>
    internal sealed class DefaultTypeMap 
        : ITypeMap
    {
        /// <summary>
        /// Creates default type map
        /// </summary>
        /// <param name="type">Entity type</param>
        public DefaultTypeMap(Type type)
        {
            Ensure.NotNull(type, nameof(type));

            this.Properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                  .Where(p => p.GetSetMethod(false) != null)
                                  .ToList();
        }

        /// <summary>
        /// Gets member mapping for column
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Mapping implementation</returns>
        public IMemberMap GetMember(string columnName)
        {
            var property = this.Properties.Find(p => string.Equals(p.Name, columnName, StringComparison.Ordinal))
               ?? this.Properties.Find(p => string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

            if (property == null && MatchNamesWithUnderscores)
            {
                property = this.Properties.Find(p => string.Equals(p.Name, columnName.Replace("_", ""), StringComparison.Ordinal))
                    ?? this.Properties.Find(p => string.Equals(p.Name, columnName.Replace("_", ""), StringComparison.OrdinalIgnoreCase));
            }

            return property != null
                ? new SimpleMemberMap(columnName, property)
                : null;
        }

        /// <summary>
        /// Should column names like User_Id be allowed to match properties/fields like UserId ?
        /// </summary>
        public static bool MatchNamesWithUnderscores { get; set; } = true;

        /// <summary>
        /// The settable properties for this typemap
        /// </summary>
        public List<PropertyInfo> Properties { get; }
    }
}
