// <copyright file="KeyInt64.cs" company="Berkeleybross">
// Copyright (c) Berkeleybross. All rights reserved.
// </copyright>
namespace Dapper.MicroCRUD.Tests.ExampleEntities
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table(nameof(KeyInt64))]
    public class KeyInt64
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }
}