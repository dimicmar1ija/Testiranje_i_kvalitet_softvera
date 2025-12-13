using ForumAPI;
using ForumAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;


public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserRepository(IOptions<MongoDbSettings> mongoSettings)
    {
        var mongoClient = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(mongoSettings.Value.DatabaseName);
        _usersCollection = database.GetCollection<User>("Users");
    }

    public async Task CreateAsync(User user)
    {
        await _usersCollection.InsertOneAsync(user);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _usersCollection.Find(_ => true).ToListAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _usersCollection.Find(u => u.Id == id.ToString()).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(User user)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
        await _usersCollection.ReplaceOneAsync(filter, user);
    }

    public async Task DeleteAsync(string userId)
    {
        await _usersCollection.DeleteOneAsync(u => u.Id == userId);
    }
}

