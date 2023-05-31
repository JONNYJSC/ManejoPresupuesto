using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QueryFirstAsync<int>("SP_Transacciones_Insertar",
                new
                {
                    transaccion.UsuarioId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota
                },
            commandType: CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(
                    @"
                        SELECT
	                        t.Id
                           ,t.Monto
                           ,t.FechaTransaccion
                           ,c.Nombre AS Categoria
                           ,cu.Nombre AS Cuenta
                           ,c.TipoOperacionId
                        FROM Transacciones t
                        INNER JOIN Categorias c
	                        ON t.CategoriaId = c.Id
                        INNER JOIN Cuentas cu
	                        ON t.CuentaId = cu.Id
                        WHERE t.CuentaId = @CuentaId
                        AND t.UsuarioId = @UsuarioId
                        AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                    ", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(
                    @"
                        SELECT
	                        t.Id
                           ,t.Monto
                           ,t.FechaTransaccion
                           ,c.Nombre AS Categoria
                           ,cu.Nombre AS Cuenta
                           ,c.TipoOperacionId
                        FROM Transacciones t
                        INNER JOIN Categorias c
	                        ON t.CategoriaId = c.Id
                        INNER JOIN Cuentas cu
	                        ON t.CuentaId = cu.Id
                        WHERE t.UsuarioId = @UsuarioId
                        AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
                        ORDER BY t.FechaTransaccion DESC
                    ", modelo);
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("SP_Transacciones_Actualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(
                                @"
                                    SELECT
	                                    t.*, c.TipoOperacionId
                                    FROM Transacciones t
                                    INNER JOIN Categorias c
	                                    ON t.CategoriaId = c.Id
                                    WHERE t.Id = @Id
                                    AND t.UsuarioId = @UsuarioId",
                                new { id, usuarioId });
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>
                (@"
                    SELECT
	                    DATEDIFF(d, @fechaInicio, t.FechaTransaccion) / 7 + 1 AS Semana
                       ,SUM(t.Monto) AS Monto
                       ,c.TipoOperacionId
                    FROM Transacciones t
                    INNER JOIN Categorias c
	                    ON t.CategoriaId = c.Id
                    WHERE t.UsuarioId = @usuarioId
                    AND t.FechaTransaccion BETWEEN @fechaInicio AND @fechaFin
                    GROUP BY DATEDIFF(d, @fechaInicio, t.FechaTransaccion) / 7
		                    ,c.TipoOperacionId
                 ", modelo);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("SP_Transacciones_Borrar",
                new { id },
                commandType: CommandType.StoredProcedure
                );
        }
    }
}
