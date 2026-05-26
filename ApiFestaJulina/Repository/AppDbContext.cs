using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiFestaJulina.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiFestaJulina.Repository
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuarios> Usuarios {get; set;}
        public DbSet<Pedidos> Pedidos {get; set;}
        public DbSet<Lotes> Lotes {get; set;}
        public DbSet<Ingressos> Ingressos {get; set;}
        public DbSet<Eventos> Eventos {get; set;}
        public DbSet<Perfil> Perfil {get; set;}
        public DbSet<Sessao> Sessoes {get; set;}
        public DbSet<TipoUsuario> TipoUsuario {get; set;}
        public DbSet<Tipo> Tipo {get; set;}
        public DbSet<Perguntas> Perguntas {get; set;}
        public DbSet<Resposta> Respostas {get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuarios>()
                .HasOne(p => p.Perfil).WithMany()
                .HasForeignKey(p => p.IdPerfil);

            modelBuilder.Entity<Sessao>()
                .HasOne(s => s.Usuario).WithMany()
                .HasForeignKey(s => s.IdUsuario);

            modelBuilder.Entity<Ingressos>()
                .HasOne(i => i.Usuario).WithMany(u => u.Ingressos)
                .HasForeignKey(i => i.IdUsuario);

            modelBuilder.Entity<Ingressos>()
                .HasOne(i => i.Pedido).WithMany(p => p.Ingressos)
                .HasForeignKey(i => i.IdPedido);

            modelBuilder.Entity<Ingressos>()
                .HasOne(i => i.Lote).WithMany(l => l.Ingressos)
                .HasForeignKey(i => i.IdLote);
        }
    }

}
/*
Usuarios
pedidos
lotes
evento
ingressos
perfil
==============
notificacao
sessao
perguntas
tipo(ingresos)
*/