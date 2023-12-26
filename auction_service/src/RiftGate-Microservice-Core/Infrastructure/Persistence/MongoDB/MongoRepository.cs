using CodingPatterns.DomainLayer;
using Infrastructure.Persistence._Interfaces;
using MongoDB.Driver;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.MongoDB;

public abstract class MongoRepository<T> : IRepository<T> where T : Entity, IAggregateRoot
{
    private string CollectionName => typeof(T).Name + "s";
    private readonly IMongoDbManager _dbManager;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private string EntityName => typeof(T).Name;

    protected MongoRepository(IMongoDbManager dbManager, IDomainEventDispatcher domainEventDispatcher)
    {
        _dbManager = dbManager ?? throw new ArgumentNullException(nameof(dbManager));
        _domainEventDispatcher = domainEventDispatcher ?? throw new ArgumentNullException(nameof(domainEventDispatcher));
    }

    protected IMongoCollection<T> GetCollection() => _dbManager.GetCollection<T>(CollectionName);

    private FilterDefinition<T> IdFilter(string id) => Builders<T>.Filter.Eq("Id", id);

    public virtual async Task InsertAsync(T entity)
    {
        try
        {
            var collection = GetCollection();
            await collection.InsertOneAsync(entity);
            await _domainEventDispatcher.DispatchEventsAsync(entity);
        }
        catch (MongoException ex)
        {
            throw new MongoRepositoryConnectionException($"Error connecting to MongoDB on insert operation. Details: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new MongoRepositoryException($"Error inserting {EntityName} into {CollectionName}. Details: {ex.Message}", ex);
        }
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        try
        {
            var collection = GetCollection();
            return await collection.Find(_ => true).ToListAsync();
        }
        catch (MongoException ex)
        {
            throw new MongoRepositoryConnectionException($"Error connecting to MongoDB on get all operation. Details: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new MongoRepositoryException($"Error retrieving all {EntityName} from {CollectionName}. Details: {ex.Message}", ex);
        }
    }

    public virtual async Task<T> GetByIdAsync(string id)
    {
        try
        {
            var collection = GetCollection();
            var result = await collection.Find(IdFilter(id)).FirstOrDefaultAsync();

            if (result == null)
            {
                throw new MongoRepositoryNotFoundException($"{EntityName} with id {id} was not found.");
            }

            return result;
        }
        catch (MongoRepositoryNotFoundException)
        {
            // Rethrow the not found exception as it is
            throw;
        }
        catch (MongoException ex)
        {
            throw new MongoRepositoryConnectionException($"Error connecting to MongoDB when retrieving {EntityName} with id {id}. Details: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new MongoRepositoryException($"Error retrieving {EntityName} by id {id} from {CollectionName}. Details: {ex.Message}", ex);
        }
    }
    
    public virtual async Task UpdateAsync(T entity)
    {
        try
        {
            var collection = GetCollection();
            var filter = Builders<T>.Filter.Eq(doc => doc.Id, entity.Id);
            await collection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = false });
            await _domainEventDispatcher.DispatchEventsAsync(entity);
        }
        catch (MongoException ex)
        {
            throw new MongoRepositoryConnectionException($"Error connecting to MongoDB when updating {EntityName} with id {entity.Id}. Details: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new MongoRepositoryException($"Error updating {EntityName} with id {entity.Id}. Details: {ex.Message}", ex);
        }
    }

    // public virtual async Task UpdateAsync(T entity)
    // {
    //     try
    //     {
    //         var collection = GetCollection();
    //         var filter = IdFilter(entity.Id);
    //         var updateDefinition = CreateUpdateDefinition(entity);
    //
    //         if (updateDefinition != null)
    //         {
    //             await collection.UpdateOneAsync(filter, updateDefinition);
    //         }
    //         await _domainEventDispatcher.DispatchEventsAsync(entity);
    //     }
    //     catch (MongoException ex)
    //     {
    //         throw new MongoRepositoryConnectionException($"Error connecting to MongoDB when updating entity with id {entity.Id}. Details: {ex.Message}", ex);
    //     }
    //     catch (Exception ex)
    //     {
    //         throw new MongoRepositoryException($"Error updating entity by id {entity.Id} in {CollectionName}. Details: {ex.Message}", ex);
    //     }
    // }
    //
    // private UpdateDefinition<T>? CreateUpdateDefinition(T entity)
    // {
    //     var updateProps = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
    //     var updateDefinitionBuilder = Builders<T>.Update;
    //     UpdateDefinition<T>? updateDefinition = null;
    //
    //     foreach (var prop in updateProps)
    //     {
    //         if (Attribute.IsDefined(prop, typeof(BsonElementAttribute)))
    //         {
    //             var bsonElementAttribute = Attribute.GetCustomAttribute(prop, typeof(BsonElementAttribute)) as BsonElementAttribute;
    //             var propName = bsonElementAttribute?.ElementName;
    //             var propValue = prop.GetValue(entity);
    //             var update = updateDefinitionBuilder.Set(propName, propValue);
    //             updateDefinition = updateDefinition == null ? update : Builders<T>.Update.Combine(updateDefinition, update);
    //         }
    //     }
    //
    //     return updateDefinition;
    // }

    public virtual async Task SoftDeleteAsync(T entity)
    {
        try
        {
            var collection = GetCollection();
            
            entity.MarkAsDeleted<T>();
            
            var deleteResult = await collection.DeleteOneAsync(IdFilter(entity.Id));

            if (deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0)
            {
                await _domainEventDispatcher.DispatchEventsAsync(entity);
            }
        }
        catch (MongoException ex)
        {
            throw new MongoRepositoryConnectionException($"Error connecting to MongoDB when deleting {EntityName} with id {entity.Id}. Details: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new MongoRepositoryException($"Error deleting {EntityName} by id {entity.Id} from {CollectionName}. Details: {ex.Message}", ex);
        }
    }
    
    public virtual async Task DeleteAsync(T entity)
    {
        try
        {
            var collection = GetCollection();
            
            entity.TriggerDeleteNotification<T>();
            
            var deleteResult = await collection.DeleteOneAsync(IdFilter(entity.Id));

            if (deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0)
            {
                await _domainEventDispatcher.DispatchEventsAsync(entity);
            }
        }
        catch (MongoException ex)
        {
            throw new MongoRepositoryConnectionException($"Error connecting to MongoDB when deleting {EntityName} with id {entity.Id}. Details: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new MongoRepositoryException($"Error deleting {EntityName} by id {entity.Id} from {CollectionName}. Details: {ex.Message}", ex);
        }
    }
}
