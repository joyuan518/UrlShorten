using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Base62;
using System.Security.Cryptography;
using System.Text;

namespace UrlShorten.WebAPI.UrlTokenGenerator
{
    public class Md5UrlTokenGenerator : IUrlTokenGenerator
    {
        private bool disposedValue;
        private readonly MD5 _md5 = MD5.Create();
        private Random rand = new Random();
        private Base62Converter base62 = new Base62Converter();

        string IUrlTokenGenerator.GetUrlToken(string originalUrl)
        {
            //Use an 4 digits of random number as a factor to mitigate the probability of collision
            var factor = rand.Next(1000, 10000);
            var hash = _md5.ComputeHash(Encoding.UTF8.GetBytes(originalUrl + factor.ToString()));  
            //Use base62 to get rid of the characters which are not allowed in url 
            var result = base62.Encode(Convert.ToBase64String(hash));

            return result.Substring(0, 7);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                _md5.Dispose();

                rand = null;
                base62 = null;

                disposedValue = true;
            }
        }

        ~Md5UrlTokenGenerator()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
