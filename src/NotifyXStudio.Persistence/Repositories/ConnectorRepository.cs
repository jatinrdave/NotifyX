using Microsoft.EntityFrameworkCore;
using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence
{
    public class ConnectorRepository : IConnectorRepository
    {
        private readonly NotifyXStudioDbContext _context;

        public ConnectorRepository(NotifyXStudioDbContext context)
        {
            _context = context;
        }

        public async Task<ConnectorRegistryEntry?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.Connectors
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<List<ConnectorRegistryEntry>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Connectors
                .ToListAsync(cancellationToken);
        }

        public async Task<ConnectorRegistryEntry> CreateAsync(ConnectorRegistryEntry connector, CancellationToken cancellationToken = default)
        {
            _context.Connectors.Add(connector);
            await _context.SaveChangesAsync(cancellationToken);
            return connector;
        }

        public async Task<ConnectorRegistryEntry> UpdateAsync(ConnectorRegistryEntry connector, CancellationToken cancellationToken = default)
        {
            _context.Connectors.Update(connector);
            await _context.SaveChangesAsync(cancellationToken);
            return connector;
        }

        public async System.Threading.Tasks.Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var connector = await GetByIdAsync(id, cancellationToken);
            if (connector != null)
            {
                _context.Connectors.Remove(connector);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
