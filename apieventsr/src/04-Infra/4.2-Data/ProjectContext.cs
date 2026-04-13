using System.Reflection;
using apieventsr.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace apieventsr.Data
{
    public class ProjectContext : DbContext
    {
        public DbSet<DomainEntity> DomainEntity { get; set; }

        // Módulo Eventos
        public DbSet<Event> Events { get; set; }
        public DbSet<EventDocument> EventDocuments { get; set; }
        public DbSet<EventEnrollment> EventEnrollments { get; set; }
        public DbSet<EnrollmentFile> EnrollmentFiles { get; set; }
        public DbSet<Segment> Segments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<UserSegment> UserSegments { get; set; }

        public ProjectContext(DbContextOptions<ProjectContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}