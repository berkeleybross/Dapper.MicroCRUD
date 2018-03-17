﻿// <copyright file="SqlStringComparer.cs" company="Berkeleybross">
// Copyright (c) Berkeleybross. All rights reserved.
// </copyright>
namespace Dapper.MicroCRUD.Tests.Utils
{
    using System;
    using System.Collections.Generic;

    public class SqlStringComparer
        : IComparer<string>, IEqualityComparer<string>
    {
        private readonly IComparer<string> comparer;

        public SqlStringComparer(StringComparer comparer)
        {
            this.comparer = comparer;
        }

        public static SqlStringComparer Instance { get; } = new SqlStringComparer(StringComparer.Ordinal);

        public int Compare(string x, string y)
        {
            x = x?.Replace("\r\n", "\n");
            y = y?.Replace("\r\n", "\n");

            return this.comparer.Compare(x, y);
        }

        public bool Equals(string x, string y)
        {
            x = x?.Replace("\r\n", "\n");
            y = y?.Replace("\r\n", "\n");

            return ((IEqualityComparer<string>)this.comparer).Equals(x, y);
        }

        public int GetHashCode(string obj)
        {
            throw new NotSupportedException();
        }
    }
}