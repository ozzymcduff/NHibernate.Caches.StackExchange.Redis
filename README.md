NHibernate.Caches.Redis
=======================

This is a [Redis](http://redis.io/) based [ICacheProvider](http://www.nhforge.org/doc/nh/en/#configuration-optional-cacheprovider) 
for [NHibernate](http://nhforge.org/) written in C# using [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis/).

Installation
------------

1. Build/install from source: `msbuild .\build\build.proj` and then look
   inside the `bin` directory.

Usage
-----

Configure NHibernate to use the custom cache provider:

```xml
<property name="cache.use_second_level_cache">true</property>
<property name="cache.use_query_cache">true</property>
<property name="cache.provider_class">NHibernate.Caches.Redis.RedisCacheProvider, 
    NHibernate.Caches.Redis</property>
```

Set the `IRedisClientsManager` (pooled, basic, etc) on the `RedisCacheProvider`
*before* creating your `ISessionFactory`:

```csharp
// Or use your IoC container to wire this up.
var connMultiplexer = ConnectionMultiplexer.Connect("localhost:6379")
RedisCacheProvider.ConnectionMultiplexer = connMultiplexer;

using (var sessionFactory = ...)
{
    // ...
}

connMultiplexer.Dispose();
```

---

Happy caching!
