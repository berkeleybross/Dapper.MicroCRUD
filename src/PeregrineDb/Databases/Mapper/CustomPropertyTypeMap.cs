﻿namespace PeregrineDb.Databases.Mapper
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Implements custom property mapping by user provided criteria (usually presence of some custom attribute with column to member mapping)
    /// </summary>
    internal sealed class CustomPropertyTypeMap 
        : ITypeMap
    {
        private readonly Type _type;
        private readonly Func<Type, string, PropertyInfo> _propertySelector;

        /// <summary>
        /// Creates custom property mapping
        /// </summary>
        /// <param name="type">Target entity type</param>
        /// <param name="propertySelector">Property selector based on target type and DataReader column name</param>
        public CustomPropertyTypeMap(Type type, Func<Type, string, PropertyInfo> propertySelector)
        {
            this._type = type ?? throw new ArgumentNullException(nameof(type));
            this._propertySelector = propertySelector ?? throw new ArgumentNullException(nameof(propertySelector));
        }

        /// <summary>
        /// Always returns default constructor
        /// </summary>
        /// <param name="names">DataReader column names</param>
        /// <param name="types">DataReader column types</param>
        /// <returns>Default constructor</returns>
        public ConstructorInfo FindConstructor(string[] names, Type[] types) => this._type.GetConstructor(new Type[0]);

        /// <summary>
        /// Always returns null
        /// </summary>
        /// <returns></returns>
        public ConstructorInfo FindExplicitConstructor() => null;

        /// <summary>
        /// Not implemented as far as default constructor used for all cases
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns property based on selector strategy
        /// </summary>
        /// <param name="columnName">DataReader column name</param>
        /// <returns>Poperty member map</returns>
        public IMemberMap GetMember(string columnName)
        {
            var prop = this._propertySelector(this._type, columnName);
            return prop != null ? new SimpleMemberMap(columnName, prop) : null;
        }
    }
}
