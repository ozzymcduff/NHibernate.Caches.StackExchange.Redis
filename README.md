NHibernate.Caches.StackExchange.Redis
=====================================

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

Wire up configuration section in app or web config:
```xml
  <configSections>
    <section name="rediscache" type="NHibernate.Caches.StackExchange.Redis.SectionHandler,NHibernate.Caches.StackExchange.Redis" />
```

And then add the settings:
```xml
  <rediscache>
    <cache region="foo_bar" expiration="999" priority="4" />
  </rediscache>
```

To set the connection settings using c#:
```csharp
RedisCacheProvider.ConnectionSettings = new RedisCacheConnection("localhost", 6379) { { "allowAdmin", "true" }, { "abortConnect", "false" } };
```

---

Happy caching!
