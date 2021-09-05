using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ex12_Identity_JWT.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ex12_Identity_JWT.Database
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<Subscription> Subscriptions { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // do not delete this!

            builder.Entity<Subscription>(SubscriptionConfigure);

            builder.Entity<User>()
                .HasOne(user => user.Subscription)
                .WithMany(subscription => subscription.Users)
                .HasForeignKey(user => user.SubscriptionId);
        }

        private void SubscriptionConfigure(EntityTypeBuilder<Subscription> builder)
        {
            builder.Property(s => s.Name).IsRequired();

            // add more configurations
        }
    }
}
