using Microsoft.EntityFrameworkCore;
using Quiz.Api.Models;

namespace Quiz.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<PersonRecord> People => Set<PersonRecord>();
    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<It03Document> It03Documents => Set<It03Document>();
    public DbSet<It05QueueState> It05QueueStates => Set<It05QueueState>();
    public DbSet<It06Product> It06Products => Set<It06Product>();
    public DbSet<It07ProductCode> It07ProductCodes => Set<It07ProductCode>();
    public DbSet<It08Question> It08Questions => Set<It08Question>();
    public DbSet<It09Comment> It09Comments => Set<It09Comment>();
    public DbSet<It10ExamResult> It10ExamResults => Set<It10ExamResult>();
    public DbSet<It04Profile> It04Profiles => Set<It04Profile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersonRecord>(entity =>
        {
            entity.ToTable("people");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Remark).HasMaxLength(500);
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("user_accounts");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.NormalizedUsername).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(60).IsRequired();
            entity.Property(x => x.NormalizedUsername).HasMaxLength(60).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PasswordSalt).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<It03Document>(entity =>
        {
            entity.ToTable("it03_documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<It05QueueState>(entity =>
        {
            entity.ToTable("it05_queue_state");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LastIssuedIndex).IsRequired();
        });

        modelBuilder.Entity<It06Product>(entity =>
        {
            entity.ToTable("it06_products");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ProductCode).IsUnique();
            entity.Property(x => x.ProductCode).HasMaxLength(19).IsRequired();
        });

        modelBuilder.Entity<It07ProductCode>(entity =>
        {
            entity.ToTable("it07_product_codes");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ProductCode).IsUnique();
            entity.Property(x => x.ProductCode).HasMaxLength(35).IsRequired();
        });

        modelBuilder.Entity<It08Question>(entity =>
        {
            entity.ToTable("it08_questions");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.DisplayOrder).IsUnique();
            entity.Property(x => x.QuestionText).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Choice1).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Choice2).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Choice3).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Choice4).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<It09Comment>(entity =>
        {
            entity.ToTable("it09_comments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Commenter).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(300).IsRequired();
        });

        modelBuilder.Entity<It10ExamResult>(entity =>
        {
            entity.ToTable("it10_exam_results");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(160).IsRequired();
        });

        modelBuilder.Entity<It04Profile>(entity =>
        {
            entity.ToTable("it04_profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ProfileBase64).IsRequired();
            entity.Property(x => x.Occupation).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Sex).HasMaxLength(20).IsRequired();
        });
    }
}
