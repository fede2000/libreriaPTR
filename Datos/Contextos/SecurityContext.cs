using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades.Seguridad;
using Microsoft.EntityFrameworkCore;

namespace Datos.Contextos;

public class SecurityContext : DbContext
{
	public DbSet<Usuario> Usuarios { get; set; }

	public DbSet<Perfil> Perfiles { get; set; }

   
    public DbSet<Direccion> Direcciones { get; set; }

    public SecurityContext(DbContextOptions<SecurityContext> options)
	  : base(options)
	{

	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Perfil>(builder =>
		{
			builder.Property(per => per.TipoUsuario).HasColumnName("Tipo_Usuario");
		});

		modelBuilder.Entity<Usuario>(builder =>
		{
			builder.HasKey(usr => usr.Clave);
			builder.Property(usr => usr.Clave).HasColumnName("Identificador");

			builder.Property(usr => usr.TipoUsuario).HasColumnName("Tipo_Usuario");
			builder.Property(usr => usr.FechaAlta).HasColumnName("Fecha_Alta");
			builder.Property(usr => usr.Nacimiento).HasColumnName("Fecha_Nacimiento");
			builder.Property(usr => usr.Correo).HasColumnName("Email");
			builder.Property(usr => usr.LastLogin).HasColumnName("Ultimo_Ingreso");


			// Configurar la relación Perfil *----* Usuario
			builder
				.HasMany(usr => usr.Perfiles) // <-- parado en Perfil
				.WithMany()
				.UsingEntity<Dictionary<string, object>>("Relacion_Perfiles_Usuarios",
					right => right.HasOne<Perfil>().WithMany().HasForeignKey("ID_Perfil"),
					left => left.HasOne<Usuario>().WithMany().HasForeignKey("ID_Usuario"))
				.ToTable("Usuarios_Perfiles");

            builder
                   .HasMany(usr => usr.Direcciones)
                   .WithOne(dir => dir.Usuario)
                   .HasForeignKey(dir => dir.ID_Usuario);
        });

        modelBuilder.Entity<Direccion>(builder =>
        {
            builder.HasKey(dir => dir.ID);
            builder.Property(dir => dir.Linea1).HasColumnName("Linea1");
            builder.Property(dir => dir.Linea2).HasColumnName("Linea2");
            builder.Property(dir => dir.Codigo_Postal).HasColumnName("Codigo_Postal");
            builder.Property(dir => dir.Provincia).HasColumnName("Provincia");
            builder.Property(dir => dir.Localidad).HasColumnName("Localidad");
            builder.Property(dir => dir.Pais).HasColumnName("Pais");
            builder.Property(dir => dir.Verificada).HasColumnName("Verificada");
            builder.Property(dir => dir.ID_Usuario).HasColumnName("ID_Usuario");
        });


    }
}