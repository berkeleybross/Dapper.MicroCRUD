﻿// <copyright file="TempAllPossibleTypes.cs" company="Berkeleybross">
// Copyright (c) Berkeleybross. All rights reserved.
// </copyright>
namespace Dapper.MicroCRUD.Tests.ExampleEntities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <remarks>
    /// Except enum, which is a different class :/
    /// </remarks>
    [Table(nameof(TempAllPossibleTypes))]
    public class TempAllPossibleTypes
    {
        public int Id { get; set; }

        public short Int16Property { get; set; }

        public short? NullableInt16Property { get; set; }

        public int Int32Property { get; set; }

        public int? NullableInt32Property { get; set; }

        public long Int64Property { get; set; }

        public long? NullableInt64Property { get; set; }

        public float SingleProperty { get; set; }

        public float? NullableSingleProperty { get; set; }

        public double DoubleProperty { get; set; }

        public double? NullableDoubleProperty { get; set; }

        public decimal DecimalProperty { get; set; }

        public decimal? NullableDecimalProperty { get; set; }

        public bool BoolProperty { get; set; }

        public bool? NullableBoolProperty { get; set; }

        [Required]
        public string StringProperty { get; set; }

        public string NullableStringProperty { get; set; }

        [MaxLength(50)]
        public string FixedLengthStringProperty { get; set; }

        public char CharProperty { get; set; }

        public char? NullableCharProperty { get; set; }

        public Guid GuidProperty { get; set; }

        public Guid? NullableGuidProperty { get; set; }

        public DateTime DateTimeProperty { get; set; }

        public DateTime? NullableDateTimeProperty { get; set; }

        public DateTimeOffset DateTimeOffsetProperty { get; set; }

        public DateTimeOffset? NullableDateTimeOffsetProperty { get; set; }

        [Required]
        public byte[] ByteArrayProperty { get; set; }

        public Color Color { get; set; }

        public Color? NullableColor { get; set; }
    }
}