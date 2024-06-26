﻿using Ksiegarnia.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace Ksiegarnia.Data
{
    public class KsiegarniaDbContext : IdentityDbContext<ApplicationUser>
    {
        public KsiegarniaDbContext(DbContextOptions<KsiegarniaDbContext> options) : base(options)
        {

        }

        public DbSet<Book> Books { get; set; }
    }
}