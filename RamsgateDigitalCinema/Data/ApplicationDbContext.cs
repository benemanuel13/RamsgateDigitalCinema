using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RamsgateDigitalCinema.Models.Entities;
using RamsgateDigitalCinema.Models.Localisation;
using RamsgateDigitalCinema.Models.PayPal;
using System;
using System.Collections.Generic;
using System.Text;

namespace RamsgateDigitalCinema.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Member> Members { get; set; }

        public DbSet<FilmCategory> FilmCategories { get; set; }
        public DbSet<Film> Films { get; set; }
        public DbSet<FilmCollection> FilmCollections { get; set; }

        public DbSet<FilmDetails> FilmDetails { get; set; }

        public DbSet<MemberFilm> MemberFilms { get; set; }

        public DbSet<Region> Regions { get; set; }
        public DbSet<SubRegion> SubRegions { get; set; }
        public DbSet<Country> Countries { get; set; }

        public DbSet<BlockedFilm> BlockedFilms { get; set; }

        public DbSet<Donation> Donations { get; set; }

        public DbSet<PayPalDetails> PayPalDetails { get; set; }
    }
}
