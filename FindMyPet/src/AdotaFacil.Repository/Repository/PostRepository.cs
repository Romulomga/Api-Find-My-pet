using AdotaFacil.Business.Interfaces;
using AdotaFacil.Business.Models;
using AdotaFacil.Repository.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdotaFacil.Repository.Repository
{
    public class PostRepository : Repository<Post>, IPostRepository
    {
        public PostRepository(MyDbContext context) : base(context) { }

        public async Task<Post> ObterProdutoFornecedor(Guid id)
        {
            return await Db.Posts.AsNoTracking().Include(f => f.WktPolygon)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Post>> ObterProdutosFornecedores()
        {
            return await Db.Posts.AsNoTracking().Include(f => f.WktPolygon).ToListAsync();
        }

        public async Task<IEnumerable<Post>> ObterProdutosPorFornecedor(Guid fornecedorId)
        {
            return await Buscar(p => p.Id == fornecedorId);
        }
    }
}
