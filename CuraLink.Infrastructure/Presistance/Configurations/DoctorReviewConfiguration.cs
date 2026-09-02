using CuraLink.Domain.Entities.Doctors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CuraLink.Infrastructure.Presistance.Configurations
{
    public class DoctorReviewConfiguration
    : IEntityTypeConfiguration<DoctorReview>
    {
        public void Configure(
            EntityTypeBuilder<DoctorReview> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Rating)
                .IsRequired();

            builder.ToTable(t => t.HasCheckConstraint(

                "CK_DoctorReview_Rating",
                "[Rating] >= 1 AND [Rating] <= 5"));


            builder.Property(x => x.Comment)
                .HasMaxLength(1000);

            builder.Property(x => x.PatientId)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.Doctor)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
