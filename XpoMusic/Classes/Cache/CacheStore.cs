using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpoMusic.Classes.Cache
{
    public abstract class CacheStore<T>
    {
        private Dictionary<string, T> data = new Dictionary<string, T>();
        private SemaphoreQueue semaphore = new SemaphoreQueue(1, 1);

        public void Clear()
        {
            data.Clear();
        }

        protected abstract Task<T> RetrieveItem(string key);

        public async Task<T> GetItem(string key)
        {
            try
            {
                await semaphore.WaitAsync();
                if (!data.ContainsKey(key))
                {
                    data[key] = await RetrieveItem(key);
                }

                return data[key];
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
