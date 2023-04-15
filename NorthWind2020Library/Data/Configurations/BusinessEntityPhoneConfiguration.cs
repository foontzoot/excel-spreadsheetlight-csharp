﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthWind2020Library.Models;

namespace NorthWind2020Library.Data.Configurations
{
    public partial class BusinessEntityPhoneConfiguration : IEntityTypeConfiguration<BusinessEntityPhone>
    {
        public void Configure(EntityTypeBuilder<BusinessEntityPhone> entity)
        {
            entity.Property(e => e.BusinessEntityPhoneId).HasColumnName("BusinessEntityPhoneID");

            entity.Property(e => e.ModifiedDate).HasDefaultValueSql("(getdate())");

            entity.Property(e => e.PhoneNumberTypeId).HasColumnName("PhoneNumberTypeID");

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<BusinessEntityPhone> entity);
    }
}
