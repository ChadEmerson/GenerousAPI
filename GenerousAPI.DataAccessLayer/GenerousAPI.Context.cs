﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GenerousAPI.DataAccessLayer
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class GenerousAPIEntities : DbContext
    {
        public GenerousAPIEntities()
            : base("name=GenerousAPIEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CardType> CardTypes { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
        public virtual DbSet<TransactionMode> TransactionModes { get; set; }
        public virtual DbSet<PaymentGatewayConfig> PaymentGatewayConfigs { get; set; }
        public virtual DbSet<PaymentGatewayType> PaymentGatewayTypes { get; set; }
        public virtual DbSet<BankAccount> BankAccounts { get; set; }
        public virtual DbSet<PaymentProfile> PaymentProfiles { get; set; }
    }
}