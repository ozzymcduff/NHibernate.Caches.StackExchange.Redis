using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NHibernate.Caches.Redis
{
    public class ObjectSerializer 
    {
        protected readonly BinaryFormatter Bf = new BinaryFormatter();

        public virtual byte[] Serialize(object value)
        {
            if (value == null)
                return null;
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Seek(0, 0);
                Bf.Serialize(memoryStream, value);
                memoryStream.Seek(0, 0);
                var buffer = new byte[(int)memoryStream.Length];
                memoryStream.Read(buffer, 0, (int) memoryStream.Length);
                return buffer;
            }
        }

        public virtual object Deserialize(byte[] bytes)
        {
            if (bytes == null)
                return null;
            
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, 0);
                return Bf.Deserialize(memoryStream);
            }
            
        }
    }
}