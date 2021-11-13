using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

using UrlShorten.DataAccess.Interface;
using UrlShorten.DTO;
using UrlShorten.DataAccess.Entities;
using UrlShorten.DataAccess.Exceptions;

namespace UrlShorten.DataAccess.Mongo
{
    public class UrlDataAccess : IUrlDataAccess
    {
        private readonly IMongoCollection<UrlEntity> _urlCollection;

        public UrlDataAccess(string connStr, string database)
        {
            var mongo = new MongoClient(connStr);
            var shortenUrlDB = mongo.GetDatabase(database);
            _urlCollection = shortenUrlDB.GetCollection<UrlEntity>("url");
        }

        public async Task AddUrlAsync(AddUrlDTO url)
        {
            var urlEntity = new UrlEntity
            {
                UserId = url.UserId,
                OriginalUrl = url.Url,
                Token = url.UrlToken,
                ClickCount = 0,
                CreateTime = DateTime.Now
            };

            try
            {
                await _urlCollection.InsertOneAsync(urlEntity);
            }
            catch (MongoWriteException)
            {
                throw new DuplicateKeyException();
            }
        }

        public async Task DeleteUrlAsync(string urlToken)
        {
            await _urlCollection.DeleteOneAsync(url => url.Token == urlToken);
        }

        public async Task<bool> ExsitsTokenAsync(string urlToken, string userId)
        {
            var result = await _urlCollection.FindAsync(url => url.Token == urlToken && url.UserId == userId);
            return await result.AnyAsync();
        }

        public async Task<bool> ExsitsUrlAsync(string url, string userId)
        {
            var result = await _urlCollection.FindAsync(u => u.OriginalUrl == url && u.UserId == userId);
            return await result.AnyAsync();
        }

        public async Task<int?> GetClickCountAsync(string urlToken)
        {
            var result = await _urlCollection.FindAsync(url => url.Token == urlToken);
            var url = await result.FirstOrDefaultAsync();

            return url?.ClickCount;
        }


        public async Task IncreaseClickCountAsync(string urlToken)
        {
            await _urlCollection.UpdateOneAsync(url => url.Token == urlToken, 
                                                Builders<UrlEntity>.Update.Inc(url => url.ClickCount, 1));
        }

        public async Task<string> GetUrlAsync(string urlToken)
        {
            var result = await _urlCollection.FindAsync(url => url.Token == urlToken);
            var url = await result.FirstOrDefaultAsync();

            return url?.OriginalUrl;
        }

        public async Task<GetUrlDTO> GetUrlAsync(string urlToken, string userId)
        {
            var result = await _urlCollection.FindAsync(url => url.Token == urlToken && url.UserId == userId);
            var url = await result.FirstOrDefaultAsync();

            if (url == null)
            {
                return null;
            }

            return new GetUrlDTO
            {
                Url = url.OriginalUrl,
                Token = url.Token,
                CreatedTime = url.CreateTime
            };
        }
    }
}
