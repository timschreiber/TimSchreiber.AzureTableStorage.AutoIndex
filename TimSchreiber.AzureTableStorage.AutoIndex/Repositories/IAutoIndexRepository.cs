using Azure.Data.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TimSchreiber.AzureTableStorage.AutoIndex.Repositories
{
    public interface IAutoIndexRepository<T> where T : class, ITableEntity, new()
    {
        Task AddAsync(T entity);
        Task DeleteAsync(T entity);
        Task<IEnumerable<T>> GetAsync();
        Task<IEnumerable<T>> GetAsync(string partitionKey);
        Task<T> GetAsync(string partitionKey, string rowKey);
        Task<IEnumerable<T>> GetByIndexedPropertyAsync(string propertyName, object propertyValue);
        Task<T> GetFirstByIndexedPropertyAsync(string propertyName, object propertyValue);
        Task<T> GetFirstOrDefaultByIndexedPropertyAsync(string propertyName, object propertyValue);
        Task<T> GetSingleByIndexedPropertyAsync(string propertyName, object propertyValue);
        Task<T> GetSingleOrDefaultByIndexedPropertyAsync(string propertyName, object propertyValue);
        Task<IEnumerable<T>> QueryAsync(string filter);
        Task UpdateAsync(T entity);
    }
}