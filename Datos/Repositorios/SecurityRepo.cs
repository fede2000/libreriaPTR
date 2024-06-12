using Entidades.Seguridad;
using Microsoft.Extensions.Logging;

using Datos.Contextos;
using Microsoft.EntityFrameworkCore;

namespace Datos.Repositorios;

public class SecurityRepo : ISecurityRepo
{
  private readonly SecurityContext _ctx;
  private readonly ILogger<SecurityContext> _logger;

  public SecurityRepo(SecurityContext ctx, ILogger<SecurityContext> logger)
  {
    _ctx = ctx;
    _logger = logger;
  }

  public IEnumerable<Perfil> GetPerfiles()
  {
    return _ctx.Perfiles.AsEnumerable();
  }

  public Usuario CrearUsuario(Usuario nuevo, string pass)
  {
    using var transaccion = _ctx.Database.BeginTransaction();

    try
    {
      _ctx.Usuarios.Add(nuevo);
      _ctx.SaveChanges();

      //  int filas = _ctx.Database.ExecuteSqlInterpolated($"call CambiarPassword( {nuevo.Login}, {pass})");
      int filas = _ctx.Database.ExecuteSqlInterpolated($"execute CambiarPassword {nuevo.Login}, {pass}");

      if (filas != 1)
        throw new ApplicationException("No se pudo setear la password");
    }
    catch (Exception ex)
    {
      transaccion.Rollback();

      _logger.LogCritical("*** ROLLBACK!!! ***");
      throw new ApplicationException("No se pudo ingresar el nuevo usuario", ex);
    }

    transaccion.Commit();
    return nuevo;
  }

  public bool SetPassword(Guid user, string pass)
  {
    return true;
  }

  public Usuario GetUsuarioFromLogin(string login)
  {
    try
    {
      var user = _ctx.Usuarios
                     .Include(u => u.Perfiles)
                     .SingleOrDefault(usr => usr.Login == login);

      return user;
    }
    catch (Exception ex)
    {
      throw new ApplicationException("Problema desconocido", ex);
    }
  }

  public bool ValidarPassword(Guid user, string pass)
  {
    //  observar que la comparamos en el servidor, no traemos la pass a la aplicacion!!
    //
    //  EF Core 7 re-introdujo la posibilidad de consultar sobre tipos simples, sin necesidad que
    //  formen parte del modelo
    //
    var resultado = _ctx.Database
                        .SqlQueryRaw<Guid>(
                          "select Identificador from Usuarios where Identificador = {0} and Hashed_Password = {1}",
                          user, pass)
                        .AsEnumerable()
                        .SingleOrDefault();

    //  si no devuelve nada, o sea si la password es incorrecta,
    //  el contenido es el default (un Guid = 000000000000-0000-0000-0000-00000000)

    return (resultado == user);

    //  Otra alternativa es usar un stored que retorne "OK" o "ERROR" por ejemplo
    //var resultado = _ctx.Database
    //  .SqlQueryRaw<string>("execute ChequearPassword {0}, {1}", user, pass)
    //  .AsEnumerable()
    //  .Single();
  }

    public async Task<Direccion> AgregarDireccionAsync(Direccion direccion)
    {
        using var transaccion = await _ctx.Database.BeginTransactionAsync();

        try
        {
            _ctx.Direcciones.Add(direccion);
            await _ctx.SaveChangesAsync();
            _logger.LogInformation("Transacción completada correctamente. Guardando cambios en la base de datos...");


            await transaccion.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaccion.RollbackAsync();
            _logger.LogCritical("*** ROLLBACK!!! ***");
            throw new ApplicationException("No se pudo ingresar la direccion", ex);
        }

        return direccion;
    }

    public async Task<IEnumerable<Direccion>> GetDireccionesByUsuarioIdAsync(Guid idUsuario)
    {
        return await _ctx.Direcciones
                             .Where(d => d.ID_Usuario == idUsuario)
                             .ToListAsync();
    }

    public async Task<Direccion> GetDireccionByIdAsync(int direccionId)
    {
        return await _ctx.Direcciones.FindAsync(direccionId);
    }

    public async Task EliminarDireccionAsync(int direccionId)
    {
        var direccion = await _ctx.Direcciones.FindAsync(direccionId);
        if (direccion != null)
        {
            _ctx.Direcciones.Remove(direccion);
            await _ctx.SaveChangesAsync();
        }
    }
}


