using System;
using NHibernate.Cfg;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Tool.hbm2ddl;
using System.IO;
using Xunit;
namespace NHibernate.Caches.StackExchange.Redis.Tests
{
    public class RedisCacheIntegrationTests : RedisTest
    {
        private static Configuration configuration;

        public RedisCacheIntegrationTests()
        {
            RedisCacheProvider.ConnectionSettings = ConnectionSettings;

            if (File.Exists("tests.db")) { File.Delete("tests.db"); }

            if (configuration == null)
            {
                configuration = Fluently.Configure()
                    .Database(
                        SQLiteConfiguration.Standard.UsingFile("tests.db")
                    )
                    .Mappings(m => m.FluentMappings.Add(typeof(PersonMapping)))
                    .ExposeConfiguration(cfg => cfg.SetProperty(NHibernate.Cfg.Environment.GenerateStatistics, "true"))
                    .Cache(c => c.UseQueryCache().UseSecondLevelCache().ProviderClass<RedisCacheProvider>())
                    .BuildConfiguration();
            }

            new SchemaExport(configuration).Create(false, true);
        }

        [Fact]
        public void Entity_cache()
        {
            using (var sf = CreateSessionFactory())
            {
                object personId = null;
                
                UsingSession(sf, session =>
                {
                    personId = session.Save(new Person("Foo", 1));
                });

                sf.Statistics.Clear();

                UsingSession(sf, session =>
                {
                    session.Get<Person>(personId);
                    Assert.Equal(1, sf.Statistics.SecondLevelCacheMissCount);
                    Assert.Equal(1, sf.Statistics.SecondLevelCachePutCount);
                });

                sf.Statistics.Clear();

                UsingSession(sf, session =>
                {
                    session.Get<Person>(personId);
                    Assert.Equal(1, sf.Statistics.SecondLevelCacheHitCount);
                    Assert.Equal(0, sf.Statistics.SecondLevelCacheMissCount);
                    Assert.Equal(0, sf.Statistics.SecondLevelCachePutCount);
                });
            }
        }

        private ISessionFactory CreateSessionFactory()
        {
            return configuration.BuildSessionFactory();
        }

        private void UsingSession(ISessionFactory sessionFactory, Action<ISession> action)
        {
            using (var session = sessionFactory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                action(session);
                transaction.Commit();
            }
        }
    }
}
