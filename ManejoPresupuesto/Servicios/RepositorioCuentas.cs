using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCuentas : IRepositorioCuentas
    {
        private readonly string connectionString;
        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var conection = new SqlConnection(connectionString);
            var id = await conection.QuerySingleAsync<int>
                (@"INSERT INTO Cuentas (Nombre, TipoCuentaId, Balance, Descripcion)
	                VALUES (@Nombre, @TipoCuentaId, @Balance, @Descripcion);

                SELECT
	                SCOPE_IDENTITY();", cuenta);

            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            var conection = new SqlConnection(connectionString);
            return await conection.QueryAsync<Cuenta>
                                                (@"SELECT
	                                                c.Id
                                                   ,c.Nombre
                                                   ,c.Balance
                                                   ,tc.Nombre AS TipoCuenta
                                                FROM Cuentas c
                                                INNER JOIN TiposCuentas tc
	                                                ON c.TipoCuentaId = tc.Id
                                                WHERE tc.UsuarioId = @UsuarioId
                                                ORDER BY tc.Orden", new { usuarioId });
        }

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            var conection = new SqlConnection(connectionString);
            return await conection.QueryFirstOrDefaultAsync<Cuenta>
                                                (@"SELECT
	                                                c.Id
                                                   ,c.Nombre
                                                   ,c.Balance
                                                   ,c.Descripcion
                                                   ,c.TipoCuentaId
                                                FROM Cuentas c
                                                INNER JOIN TiposCuentas tc
	                                                ON c.TipoCuentaId = tc.Id
                                                WHERE  tc.UsuarioId = @UsuarioId AND c.Id = @Id",
                                                new { id, usuarioId });
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            var conection = new SqlConnection(connectionString);
            await conection.ExecuteAsync
                                        (@"UPDATE Cuentas
                                           SET Nombre = @Nombre
                                            ,TipoCuentaId = @TipoCuentaId
                                            ,Balance = @Balance
                                            ,Descripcion = @Descripcion
                                            WHERE Id = @Id;", cuenta);
        }
    }
}
