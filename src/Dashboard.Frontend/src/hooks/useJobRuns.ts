import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useApi } from '../context/ApiContext';
import { JobRunDto } from '../types';

// Query Keys
export const jobRunKeys = {
  all: ['jobRuns'] as const,
  lists: () => [...jobRunKeys.all, 'list'] as const,
  list: (filters: Record<string, any>) => [...jobRunKeys.lists(), { filters }] as const,
  details: () => [...jobRunKeys.all, 'detail'] as const,
  detail: (id: number) => [...jobRunKeys.details(), id] as const,
  running: () => [...jobRunKeys.all, 'running'] as const,
  failed: () => [...jobRunKeys.all, 'failed'] as const,
  byJob: (jobId: number) => [...jobRunKeys.all, 'byJob', jobId] as const,
  byJobWithFilters: (jobId: number, filters: Record<string, any>) => 
    [...jobRunKeys.byJob(jobId), { filters }] as const,
};

// Query Hooks
export function useJobRuns(filters: {
  page?: number;
  sort?: string;
  query?: string;
  states?: string[];
  showDeleted?: boolean;
  pageSize?: number;
} = {}) {
  const { apiClient } = useApi();
  const {
    page = 1,
    sort = '',
    query = '',
    states,
    showDeleted = false,
    pageSize
  } = filters;

  return useQuery({
    queryKey: jobRunKeys.list({ page, sort, query, states, showDeleted, pageSize }),
    queryFn: () => apiClient.getJobRuns(page, sort, query, states, showDeleted, pageSize),
    staleTime: 30 * 1000, // 30 seconds
  });
}

export function useJobRunsByJobId(
  jobId: number | string | undefined,
  filters: {
    page?: number;
    sort?: string;
    pageSize?: number;
    showDeleted?: boolean;
  } = {}
) {
  const { apiClient } = useApi();
  const parsedJobId = typeof jobId === 'string' ? parseInt(jobId) : jobId;
  const { page = 1, sort = '', pageSize, showDeleted = false } = filters;

  return useQuery({
    queryKey: jobRunKeys.byJobWithFilters(parsedJobId!, { page, sort, pageSize, showDeleted }),
    queryFn: () => apiClient.getJobRunsByJobId(parsedJobId!, page, sort, pageSize, showDeleted),
    enabled: !!parsedJobId && parsedJobId > 0,
    staleTime: 30 * 1000, // 30 seconds
  });
}

export function useJobRun(id: number | string | undefined) {
  const { apiClient } = useApi();
  const jobRunId = typeof id === 'string' ? parseInt(id) : id;

  return useQuery({
    queryKey: jobRunKeys.detail(jobRunId!),
    queryFn: () => apiClient.getJobRun(jobRunId!),
    enabled: !!jobRunId && jobRunId > 0,
    staleTime: 30 * 1000, // 30 seconds
  });
}

export function useRunningJobRuns() {
  const { apiClient } = useApi();

  return useQuery({
    queryKey: jobRunKeys.running(),
    queryFn: () => apiClient.getRunningJobRuns(),
    staleTime: 2 * 1000, // 2 seconds
    refetchInterval: 2 * 1000, // Refetch every 2 seconds
  });
}

export function useLastFailedJobRuns() {
  const { apiClient } = useApi();

  return useQuery({
    queryKey: jobRunKeys.failed(),
    queryFn: () => apiClient.getLastFailedJobRuns(),
    staleTime: 5 * 1000, // 5 seconds
    refetchInterval: 5 * 1000, // Refetch every 5 seconds
  });
}

// Mutation Hooks
export function useDeleteJobRun() {
  const { apiClient } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => apiClient.deleteJobRun(id),
    onSuccess: (_, deletedId) => {
      // Remove the deleted job run from cache
      queryClient.removeQueries({ queryKey: jobRunKeys.detail(deletedId) });
      
      // Invalidate job runs lists
      queryClient.invalidateQueries({ queryKey: jobRunKeys.lists() });
      queryClient.invalidateQueries({ queryKey: jobRunKeys.running() });
      queryClient.invalidateQueries({ queryKey: jobRunKeys.failed() });
    },
  });
}

export function useRetryJobRun() {
  const { apiClient } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (jobRun: JobRunDto) => apiClient.retryJobRun(jobRun),
    onSuccess: (_, jobRun) => {
      // Invalidate related queries
      queryClient.invalidateQueries({ queryKey: jobRunKeys.lists() });
      queryClient.invalidateQueries({ queryKey: jobRunKeys.byJob(jobRun.jobId) });
      queryClient.invalidateQueries({ queryKey: jobRunKeys.running() });
      queryClient.invalidateQueries({ queryKey: jobRunKeys.failed() });
      
      // Invalidate trigger queries since a new trigger is created
      queryClient.invalidateQueries({ queryKey: ['triggers', 'byJob', jobRun.jobId] });
    },
  });
}

// Utility hook to invalidate job run queries
export function useInvalidateJobRuns() {
  const queryClient = useQueryClient();

  return {
    invalidateAll: () => queryClient.invalidateQueries({ queryKey: jobRunKeys.all }),
    invalidateList: () => queryClient.invalidateQueries({ queryKey: jobRunKeys.lists() }),
    invalidateJobRun: (id: number) => queryClient.invalidateQueries({ queryKey: jobRunKeys.detail(id) }),
    invalidateByJob: (jobId: number) => queryClient.invalidateQueries({ queryKey: jobRunKeys.byJob(jobId) }),
    invalidateRunning: () => queryClient.invalidateQueries({ queryKey: jobRunKeys.running() }),
    invalidateFailed: () => queryClient.invalidateQueries({ queryKey: jobRunKeys.failed() }),
  };
}