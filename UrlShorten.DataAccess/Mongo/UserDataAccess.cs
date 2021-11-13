using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver;

using UrlShorten.DataAccess.Interface;
using UrlShorten.DataAccess.Entities;
using UrlShorten.DTO;

namespace UrlShorten.DataAccess.Mongo
{
    public class UserDataAccess : IUserDataAccess
    {
        private readonly IMongoCollection<UserEntity> _userCollection;

        public UserDataAccess(string connStr, string database)
        {
            var mongo = new MongoClient(connStr);
            var shortenUrlDB = mongo.GetDatabase(database);
            _userCollection = shortenUrlDB.GetCollection<UserEntity>("user");
        }

        public async Task<GetUserDTO> GetUserAsync(string userId)
        {
            var result = await _userCollection.FindAsync(user => user.UserId == userId);
            var user = await result.FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            return new GetUserDTO
            {
                 UserId = user.UserId,
                 Name = user.Name,
                 Email = user.Email,
                 PasswordHash = user.PasswordHash,
                 CreateTime = user.CreateTime
            };
        }

        public async Task<bool> ExistsUserAsync(string userId)
        {
            var result = await _userCollection.FindAsync(user => user.UserId == userId);
            return await result.AnyAsync();
        }

        public async Task AddUserAsync(AddUserDTO user)
        {
            var userEntity = new UserEntity
            {
                UserId = user.UserId,
                Name = user.UserName,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                CreateTime = DateTime.Now
            };

            await _userCollection.InsertOneAsync(userEntity);
        }
    }
}
