using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCategorias
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCategorias : IRepositorioCategorias
    {
        private readonly string connectionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
                                    INSERT INTO Categorias(Nombre, TipoOperacionId, UsuarioId)
                                    VALUES (@Nombre, @TipoOperacionId, @UsuarioId);

                                    SELECT SCOPE_IDENTITY();
                                    ", categoria);
            categoria.Id = id;
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(@"
                                SELECT * FROM Categorias c WHERE c.UsuarioId = @UsuarioId",
                                new { usuarioId });
        }

        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(@"
                                    SELECT * FROM Categorias c WHERE c.Id = @Id AND c.UsuarioId = @UsuarioId",
                                    new { id, usuarioId });
        }

        public async Task Actualizar(Categoria categoria)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                                    UPDATE Categorias
                                       SET Nombre = @Nombre,
                                        TipoOperacionId = @TipoOperacionID
                                         WHERE Id = @Id",
                                       categoria);
        }

        public async Task Borrar(int id)
        {
            using var conection = new SqlConnection(connectionString);
            await conection.ExecuteAsync("DELETE Categorias WHERE Id = @Id", new { id });
        }
    }
}
