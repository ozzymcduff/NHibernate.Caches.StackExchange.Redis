NHibernate.Caches.StackExchange.Redis [![Build status](https://ci.appveyor.com/api/projects/status/d6rdani2sq4yt5wc/branch/master?svg=true)](https://ci.appveyor.com/project/wallymathieu/nhibernate-caches-stackexchange-redis/branch/master)
=====================================

This is a [Redis](http://redis.io/) based [ICacheProvider](http://www.nhforge.org/doc/nh/en/#configuration-optional-cacheprovider) 
for [NHibernate](http://nhforge.org/) written in C# using [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis/).

Installation
------------

1. git submodule add https://github.com/wallymathieu/NHibernate.Caches.StackExchange.Redis.git 
2. Fork and make a submodule (you might want a different version of StackExchange.Redis)
3. Copy the relevant code (it's MIT)
4. Private nuget stream

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
