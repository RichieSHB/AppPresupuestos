using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Borrar(int id);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
		Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
	}
    public class RepositorioTipoCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public RepositorioTipoCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>
                                                   ("TiposCuentas_Insertar", 
                                                   new { usuarioId = tipoCuenta.UsuarioId,
                                                        nombre = tipoCuenta.Nombre },
                                                   commandType: System.Data.CommandType.StoredProcedure);
            tipoCuenta.id = id;

        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var conn = new SqlConnection(connectionString);
            var existe = await conn.QueryFirstOrDefaultAsync<int>(
                                    @"SELECT 1
                                    FROM TiposCuentas
                                    WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId;",
                                    new { nombre, usuarioId });
            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId)
        {
            using var conn = new SqlConnection(connectionString);
            return await conn.QueryAsync<TipoCuenta>("SELECT Id, Nombre, Orden " +
                                                     "FROM TiposCuentas " +
                                                     "WHERE UsuarioId = @UsuarioID " +
                                                     "ORDER BY Orden", new { usuarioId });
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var conn = new SqlConnection(connectionString);
            await conn.ExecuteAsync("UPDATE TiposCuentas " +
                                    "SET Nombre = @Nombre " +
                                    "WHERE Id = @Id", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var conn = new SqlConnection(connectionString);
            return await conn.QueryFirstOrDefaultAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden
                                                                    FROM TiposCuentas
                                                                    WHERE Id = @Id AND UsuarioId = @UsuarioId",
                                                                    new { id, usuarioId });
        }

        public async Task Borrar(int id)
        {
            using var conn = new SqlConnection(connectionString);
            await conn.ExecuteAsync("DELETE TiposCuentas WHERE Id = @Id", new { id });
        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados)
        {
            var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE Id = @Id;";

            using var conn = new SqlConnection(connectionString);
            await conn.ExecuteAsync(query, tipoCuentasOrdenados);
        }
    }
}
