import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useApi } from '../context/ApiContext';

// Query Keys
export const jobKeys = {
  all: ['jobs'] as const,
  lists: () => [...jobKeys.all, 'list'] as const,
  list: (filters: Record<string, any>) => [...jobKeys.lists(), { filters }] as const,
  details: () => [...jobKeys.all, 'detail'] as const,
  detail: (id: number) => [...jobKeys.details(), id] as const,
};

// Query Hooks
export function useJobs() {
  const { apiClient } = useApi();

  return useQuery({
    queryKey: jobKeys.lists(),
    queryFn: () => apiClient.getAllJobs(),
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

export function useJob(id: number | string | undefined) {
  const { apiClient } = useApi();
  const jobId = typeof id === 'string' ? parseInt(id) : id;

  return useQuery({
    queryKey: jobKeys.detail(jobId!),
    queryFn: () => apiClient.getJob(jobId!),
    enabled: !!jobId && jobId > 0,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

// Utility hook to invalidate job queries
export function useInvalidateJobs() {
  const queryClient = useQueryClient();

  return {
    invalidateAll: () => queryClient.invalidateQueries({ queryKey: jobKeys.all }),
    invalidateList: () => queryClient.invalidateQueries({ queryKey: jobKeys.lists() }),
    invalidateJob: (id: number) => queryClient.invalidateQueries({ queryKey: jobKeys.detail(id) }),
  };
}