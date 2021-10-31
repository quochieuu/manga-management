using MangaBook.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MangaBook.Data.Configurations
{
    public class MangaInGenreConfiguration : IEntityTypeConfiguration<MangaInGenre>
    {
        public void Configure(EntityTypeBuilder<MangaInGenre> builder)
        {
            builder.ToTable("MangaInGenres");
            builder.HasKey(x => x.Id);
        }
    }
}
