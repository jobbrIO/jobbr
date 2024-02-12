using System;
using System.Collections.Generic;
using Raven.Client;
using Raven.Client.Linq;

namespace Jobbr.Storage.RavenDB
{
    public static class RavenExtensions
    {
        public static IEnumerable<T> GetAll<T>(this IDocumentSession session)
        {
            var list = new List<T>();

            RavenQueryStatistics statistics;

            list.AddRange(session.Query<T>().Statistics(out statistics));

            if (statistics.TotalResults <= 128)
            {
                return list;
            }

            var toTake = statistics.TotalResults - 128;
            var taken = 128;

            while (toTake > 0)
            {
                list.AddRange(session.Query<T>().Skip(taken).Take(toTake > 1024 ? 1024 : toTake));
                toTake -= 1024;
                taken += 1024;
            }

            return list;
        }

        public static string ToRavenId(this long id, string collectionPrefix)
        {
            return $"{collectionPrefix}/{id}";
        }

        public static long ParseId(this string id)
        {
            var split = id.Split('/');

            if (split.Length != 2)
            {
                throw new ArgumentException("Id is not in the format collection/id", nameof(id));
            }

            return Convert.ToInt64(split[1]);
        }

        public static IEnumerable<T> StreamAll<T>(this IDocumentSession session, IRavenQueryable<T> query)
        {
            var enumerator = session.Advanced.Stream(query);

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current.Document;
            }
        }
    }
}