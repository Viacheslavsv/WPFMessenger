﻿using ChatDAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatDAL.Configurations
{
    public class ChatEntityConfiguration : IEntityTypeConfiguration<ChatEntity>
    {
        public void Configure(EntityTypeBuilder<ChatEntity> builder)
        {
            builder
                .HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .IsRequired();

            builder
                .Property(x=>x.Title)
                .IsRequired();

            builder
                .HasMany(x => x.Users)
                .WithMany(x => x.Chats);

            builder
                .HasMany(x=>x.Messages)
                .WithOne(x=>x.Chat)
                .HasForeignKey(x=>x.ChatId);
        }
    }
}
