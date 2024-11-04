using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Jobbr.ComponentModel.JobStorage.Model;
using ServiceStack.OrmLite;

namespace Jobbr.Storage.MsSql
{
    public static class OrderExtensions
    {
        private static IDictionary<string, Expression<Func<Entities.JobRun, object>>> JobRunMapping { get; } = new Dictionary<string, Expression<Func<Entities.JobRun, object>>>
        {
            { nameof(JobRun.Id), run => run.Id },
            { nameof(JobRun.InstanceParameters), run => run.InstanceParameters },
            { nameof(JobRun.JobParameters), run => run.JobParameters },
            { nameof(JobRun.Pid), run => run.Pid },
            { nameof(JobRun.PlannedStartDateTimeUtc), run => run.PlannedStartDateTimeUtc },
            { nameof(JobRun.Progress), run => run.Progress },
            { nameof(JobRun.ActualEndDateTimeUtc), run => run.ActualEndDateTimeUtc },
            { nameof(JobRun.ActualStartDateTimeUtc), run => run.ActualStartDateTimeUtc },
            { nameof(JobRun.EstimatedEndDateTimeUtc), run => run.EstimatedEndDateTimeUtc },
            { nameof(JobRun.State), run => run.State }
        };

        private static IDictionary<string, Expression<Func<Entities.Job, object>>> JobMapping { get; } = new Dictionary<string, Expression<Func<Entities.Job, object>>>
        {
            { nameof(Job.Id), run => run.Id },
            { nameof(Job.CreatedDateTimeUtc), run => run.CreatedDateTimeUtc },
            { nameof(Job.Parameters), run => run.Parameters },
            { nameof(Job.Title), run => run.Title },
            { nameof(Job.Type), run => run.Type },
            { nameof(Job.UniqueName), run => run.UniqueName },
            { nameof(Job.UpdatedDateTimeUtc), run => run.UpdatedDateTimeUtc },
        };

        private class OrderByEntry
        {
            public string Field { get; set; }

            public SortOrder SortOrder { get; set; }
        }

        private static List<OrderByEntry> GetOrderByEntries(string[] orderBy)
        {
            if (orderBy == null)
            {
                return new List<OrderByEntry>();
            }

            return orderBy.Select(
                f =>
                {
                    var entry = new OrderByEntry();

                    if (f.StartsWith("-"))
                    {
                        entry.SortOrder = SortOrder.Descending;
                        entry.Field = f.Substring(1);
                    }
                    else
                    {
                        entry.SortOrder = SortOrder.Ascending;
                        entry.Field = f;
                    }

                    return entry;
                }).ToList();
        }

        public static void Ordered(this SqlExpression<Entities.Job> sqlExpression, string[] orderBy)
        {
            sqlExpression.ApplyOrder(orderBy, JobMapping);
        }

        public static void Ordered(this SqlExpression<Entities.JobRun> sqlExpression, string[] orderBy)
        {
            sqlExpression.ApplyOrder(orderBy, JobRunMapping);
        }

        private static void ApplyOrder<TModel, TEntity>(this SqlExpression<TEntity> sqlExpression, string[] orderBy, IDictionary<string, Expression<Func<TModel, object>>> orderByMapping)
        {
            var orderByEntries = GetOrderByEntries(orderBy);

            if (orderByEntries.Any())
            {
                var hasFirstOrderBy = false;

                foreach (var orderByEntry in orderByEntries)
                {
                    if (!orderByMapping.TryGetValue(orderByEntry.Field, out var orderByExpression))
                    {
                        // field is not in the mapping dictionary --> we skip it
                        continue;
                    }

                    if (orderByEntry.SortOrder == SortOrder.Ascending)
                    {
                        if (hasFirstOrderBy == false)
                        {
                            sqlExpression.OrderBy(orderByExpression);
                            hasFirstOrderBy = true;
                        }
                        else
                        {
                            sqlExpression.ThenBy(orderByExpression);
                        }
                    }
                    else
                    {
                        if (hasFirstOrderBy == false)
                        {
                            sqlExpression.OrderByDescending(orderByExpression);
                            hasFirstOrderBy = true;
                        }
                        else
                        {
                            sqlExpression.ThenByDescending(orderByExpression);
                        }
                    }
                }
            }
        }
    }
}