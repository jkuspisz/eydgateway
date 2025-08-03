using EYDGateway.Data;
using EYDGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace EYDGateway.Services
{
    public interface IEPAService
    {
        Task<List<EPA>> GetAllActiveEPAsAsync();
        Task<EPA?> GetEPAByIdAsync(int epaId);
        Task<List<EPA>> GetEPAsByIdsAsync(List<int> epaIds);
        Task<bool> ValidateEPASelectionAsync(List<int> epaIds);
        Task<bool> ValidateSLEEPASelectionAsync(string sleType, List<int> epaIds);
        Task SaveEPAMappingAsync(string entityType, int entityId, List<int> epaIds, string userId);
        Task<List<EPAMapping>> GetEPAMappingsAsync(string entityType, int entityId);
        Task UpdateEPAMappingAsync(string entityType, int entityId, List<int> newEpaIds, string userId);
        Task DeleteEPAMappingAsync(string entityType, int entityId);
    }

    public class EPAService : IEPAService
    {
        private readonly ApplicationDbContext _context;
        
        public EPAService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EPA>> GetAllActiveEPAsAsync()
        {
            return await _context.EPAs
                .Where(e => e.IsActive)
                .OrderBy(e => e.Code)
                .ToListAsync();
        }

        public async Task<EPA?> GetEPAByIdAsync(int epaId)
        {
            return await _context.EPAs
                .FirstOrDefaultAsync(e => e.Id == epaId && e.IsActive);
        }

        public async Task<List<EPA>> GetEPAsByIdsAsync(List<int> epaIds)
        {
            if (epaIds == null || !epaIds.Any())
                return new List<EPA>();

            return await _context.EPAs
                .Where(e => epaIds.Contains(e.Id) && e.IsActive)
                .OrderBy(e => e.Code)
                .ToListAsync();
        }

        public async Task<bool> ValidateEPASelectionAsync(List<int> epaIds)
        {
            if (epaIds == null || epaIds.Count < 1 || epaIds.Count > 2)
                return false;

            // Check if all provided EPA IDs exist and are active
            var existingEPACount = await _context.EPAs
                .CountAsync(e => epaIds.Contains(e.Id) && e.IsActive);

            return existingEPACount == epaIds.Count;
        }

        public async Task<bool> ValidateSLEEPASelectionAsync(string sleType, List<int> epaIds)
        {
            if (epaIds == null || epaIds.Count < 1)
                return false;

            // SLE types that require exactly 1 EPA
            var singleEPATypes = new[] { SLETypes.MiniCEX, SLETypes.DOPS, SLETypes.DOPSSim };
            
            if (singleEPATypes.Contains(sleType))
            {
                if (epaIds.Count != 1)
                    return false;
            }
            else
            {
                // CBD, DENTL, DCT require 1-2 EPAs
                if (epaIds.Count > 2)
                    return false;
            }

            // Check if all provided EPA IDs exist and are active
            var existingEPACount = await _context.EPAs
                .CountAsync(e => epaIds.Contains(e.Id) && e.IsActive);

            return existingEPACount == epaIds.Count;
        }

        public async Task SaveEPAMappingAsync(string entityType, int entityId, List<int> epaIds, string userId)
        {
            if (!await ValidateEPASelectionAsync(epaIds))
                throw new ArgumentException("Invalid EPA selection");

            // Remove existing mappings for this entity
            await DeleteEPAMappingAsync(entityType, entityId);

            // Add new mappings
            var mappings = epaIds.Select(epaId => new EPAMapping
            {
                EPAId = epaId,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.EPAMappings.AddRange(mappings);
            await _context.SaveChangesAsync();
        }

        public async Task<List<EPAMapping>> GetEPAMappingsAsync(string entityType, int entityId)
        {
            return await _context.EPAMappings
                .Include(m => m.EPA)
                .Where(m => m.EntityType == entityType && m.EntityId == entityId)
                .OrderBy(m => m.EPA.Code)
                .ToListAsync();
        }

        public async Task UpdateEPAMappingAsync(string entityType, int entityId, List<int> newEpaIds, string userId)
        {
            await SaveEPAMappingAsync(entityType, entityId, newEpaIds, userId);
        }

        public async Task DeleteEPAMappingAsync(string entityType, int entityId)
        {
            var existingMappings = await _context.EPAMappings
                .Where(m => m.EntityType == entityType && m.EntityId == entityId)
                .ToListAsync();

            if (existingMappings.Any())
            {
                _context.EPAMappings.RemoveRange(existingMappings);
                await _context.SaveChangesAsync();
            }
        }
    }
}
