using System;
using System.Collections.Generic;
using StackExchange.Redis;
using NUnit.Framework;
using Fact = NUnit.Framework.TestAttribute;


namespace NHibernate.Caches.StackExchange.Redis.Tests
{
    [TestFixture]
    public class RedisCacheTests : RedisTest
    {
        [Fact]
        public void Put_should_serialize_item_and_set_with_expiry()
        {
            // Arrange
            var cache = GetRedisCacheWithRegion("region");

            // Act
            cache.Put(999, new Person("Foo", 10));
            // Assert
            var cacheKey = cache.KeyAsString(999);
            
            var expiry = Redis.KeyTimeToLive(cacheKey);
            Assert.NotNull(expiry);

            var person = cache.Get(999) as Person;
            Assert.NotNull(person);
            Assert.AreEqual("Foo", person.Name);
            Assert.AreEqual(10, person.Age);
        }

        [Fact]
        public void Get_should_deserialize_data()
        {
            // Arrange
            var cache = GetRedisCacheWithRegion("region");
            cache.Put(999, new Person("Foo", 10));

            // Act
            var person = cache.Get(999) as Person;

            // Assert
            Assert.NotNull(person);
            Assert.AreEqual("Foo", person.Name);
            Assert.AreEqual(10, person.Age);
        }

        [Fact]
        public void Get_should_return_null_if_not_exists()
        {
            // Arrange
            var cache = GetRedisCacheWithRegion("region");

            // Act
            var person = cache.Get(99999) as Person;

            // Assert
            Assert.Null(person);
        }

        [Fact]
        public void Put_and_Get_into_different_cache_regions()
        {
            // Arrange
            const int key = 1;
            var cache1 = GetRedisCacheWithRegion("region_A");
            var cache2 = GetRedisCacheWithRegion("region_B");

            // Act
            cache1.Put(key, new Person("A", 1));
            cache2.Put(key, new Person("B", 1));

            // Assert
            Assert.AreEqual("A", ((Person)cache1.Get(1)).Name);
            Assert.AreEqual("B", ((Person)cache2.Get(1)).Name);
        }

        [Fact]
        public void Remove_should_remove_from_cache()
        {
            // Arrange
            var cache = GetRedisCacheWithRegion("region");
            cache.Put(999, new Person("Foo", 10));

            // Act
            cache.Remove(999);

            // Assert
            var result = Redis.StringGet(cache.KeyAsString(999));
            Assert.False(result.HasValue);
        }

    }
}
