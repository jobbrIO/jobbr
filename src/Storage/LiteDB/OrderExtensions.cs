using System;
using System.Collections.Generic;
using System.Linq;
using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.Storage.LiteDB.Entities;

namespace Jobbr.Storage.LiteDB
{
    public static class OrderExtensions
    {
        public static IEnumerable<T> ApplyOrdering<T>(this IEnumerable<T> source, string[] sort)
        {
            if (sort == null || sort.Length == 0)
            {
                return source;
            }

            IOrderedEnumerable<T> orderedSource = null;

            foreach (var sortField in sort)
            {
                var descending = false;
                var fieldName = sortField;

                if (sortField.StartsWith("-"))
                {
                    descending = true;
                    fieldName = sortField.Substring(1);
                }

                if (orderedSource == null)
                {
                    orderedSource = descending 
                        ? source.OrderByDescending(GetSortSelector<T>(fieldName))
                        : source.OrderBy(GetSortSelector<T>(fieldName));
                }
                else
                {
                    orderedSource = descending 
                        ? orderedSource.ThenByDescending(GetSortSelector<T>(fieldName))
                        : orderedSource.ThenBy(GetSortSelector<T>(fieldName));
                }
            }

            return orderedSource ?? source;
        }

        private static Func<T, object> GetSortSelector<T>(string fieldName)
        {
            return fieldName switch
            {
                nameof(Job.Id) when typeof(T) == typeof(JobEntity) => entity => ((JobEntity)(object)entity).Id,
                nameof(Job.CreatedDateTimeUtc) when typeof(T) == typeof(JobEntity) => entity => ((JobEntity)(object)entity).CreatedDateTimeUtc,
                nameof(Job.UpdatedDateTimeUtc) when typeof(T) == typeof(JobEntity) => entity => ((JobEntity)(object)entity).UpdatedDateTimeUtc,
                nameof(Job.UniqueName) when typeof(T) == typeof(JobEntity) => entity => ((JobEntity)(object)entity).UniqueName,
                nameof(Job.Title) when typeof(T) == typeof(JobEntity) => entity => ((JobEntity)(object)entity).Title,
                nameof(Job.Type) when typeof(T) == typeof(JobEntity) => entity => ((JobEntity)(object)entity).Type,
                
                nameof(JobRun.Id) when typeof(T) == typeof(JobRunEntity) => entity => ((JobRunEntity)(object)entity).Id,
                nameof(JobRun.PlannedStartDateTimeUtc) when typeof(T) == typeof(JobRunEntity) => entity => ((JobRunEntity)(object)entity).PlannedStartDateTimeUtc,
                nameof(JobRun.ActualStartDateTimeUtc) when typeof(T) == typeof(JobRunEntity) => entity => ((JobRunEntity)(object)entity).ActualStartDateTimeUtc,
                nameof(JobRun.ActualEndDateTimeUtc) when typeof(T) == typeof(JobRunEntity) => entity => ((JobRunEntity)(object)entity).ActualEndDateTimeUtc,
                nameof(JobRun.EstimatedEndDateTimeUtc) when typeof(T) == typeof(JobRunEntity) => entity => ((JobRunEntity)(object)entity).EstimatedEndDateTimeUtc,
                nameof(JobRun.State) when typeof(T) == typeof(JobRunEntity) => entity => ((JobRunEntity)(object)entity).State,
                nameof(JobRun.Progress) when typeof(T) == typeof(JobRunEntity) => entity => ((JobRunEntity)(object)entity).Progress,
                
                nameof(JobTriggerBase.Id) when typeof(T) == typeof(TriggerEntity) => entity => ((TriggerEntity)(object)entity).Id,
                nameof(JobTriggerBase.CreatedDateTimeUtc) when typeof(T) == typeof(TriggerEntity) => entity => ((TriggerEntity)(object)entity).CreatedDateTimeUtc,
                nameof(JobTriggerBase.IsActive) when typeof(T) == typeof(TriggerEntity) => entity => ((TriggerEntity)(object)entity).IsActive,
                
                _ => entity => null // Default fallback
            };
        }
    }
}