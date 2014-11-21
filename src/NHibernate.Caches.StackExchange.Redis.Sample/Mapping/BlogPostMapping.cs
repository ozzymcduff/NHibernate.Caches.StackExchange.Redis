using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using NHibernate.Caches.StackExchange.Redis.Sample.Models;

namespace NHibernate.Caches.StackExchange.Redis.Sample.Mapping
{
    public class BlogPostMapping : ClassMap<BlogPost>
    {
        public BlogPostMapping()
        {
            Id(x => x.Id);
            Map(x => x.Title);
            Map(x => x.Body);
            Map(x => x.Created);

            Cache.ReadWrite();
        }
    }
}