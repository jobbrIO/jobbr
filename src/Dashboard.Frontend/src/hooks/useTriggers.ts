import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useApi } from '../context/ApiContext';
import { JobTriggerDto } from '../types';

// Query Keys
export const triggerKeys = {
  all: ['triggers'] as const,
  lists: () => [...triggerKeys.all, 'list'] as const,
  byJob: (jobId: number) => [...triggerKeys.all, 'byJob', jobId] as const,
  byJobWithPage: (jobId: number, page: number) => [...triggerKeys.byJob(jobId), { page }] as const,
  details: () => [...triggerKeys.all, 'detail'] as const,
  detail: (jobId: number, triggerId: number) => [...triggerKeys.details(), jobId, triggerId] as const,
};

// Query Hooks
export function useTriggersByJobId(
  jobId: number | string | undefined,
  page: number = 1
) {
  const { apiClient } = useApi();
  const parsedJobId = typeof jobId === 'string' ? parseInt(jobId) : jobId;

  return useQuery({
    queryKey: triggerKeys.byJobWithPage(parsedJobId!, page),
    queryFn: () => apiClient.getTriggersByJobId(parsedJobId!, page),
    enabled: !!parsedJobId && parsedJobId > 0,
    staleTime: 2 * 60 * 1000, // 2 minutes
  });
}

export function useTrigger(
  jobId: number | string | undefined,
  triggerId: number | string | undefined | null
) {
  const { apiClient } = useApi();
  const parsedJobId = typeof jobId === 'string' ? parseInt(jobId) : jobId;
  const parsedTriggerId = typeof triggerId === 'string' ? parseInt(triggerId) : triggerId;

  return useQuery({
    queryKey: triggerKeys.detail(parsedJobId!, parsedTriggerId!),
    queryFn: () => apiClient.getTrigger(parsedJobId!, parsedTriggerId!),
    enabled: !!parsedJobId && !!parsedTriggerId && parsedJobId > 0 && parsedTriggerId > 0,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

// Mutation Hooks
export function useCreateTrigger() {
  const { apiClient } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ trigger, jobId }: { 
      trigger: Omit<JobTriggerDto, 'id' | 'deleted'>; 
      jobId: number;
    }) => apiClient.createTrigger(trigger, jobId),
    onSuccess: (_, { jobId }) => {
      // Invalidate triggers for this job
      queryClient.invalidateQueries({ queryKey: triggerKeys.byJob(jobId) });
    },
  });
}

export function useUpdateTrigger() {
  const { apiClient } = useApi();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ trigger, jobId }: { 
      trigger: JobTriggerDto; 
      jobId: number;
    }) => apiClient.updateTrigger(trigger, jobId),
    onSuccess: (_, { trigger, jobId }) => {
      // Update the specific trigger in cache
      queryClient.setQueryData(
        triggerKeys.detail(jobId, trigger.id),
        trigger
      );
      
      // Invalidate triggers list for this job
      queryClient.invalidateQueries({ queryKey: triggerKeys.byJob(jobId) });
    },
  });
}

// Utility hook to invalidate trigger queries
export function useInvalidateTriggers() {
  const queryClient = useQueryClient();

  return {
    invalidateAll: () => queryClient.invalidateQueries({ queryKey: triggerKeys.all }),
    invalidateByJob: (jobId: number) => queryClient.invalidateQueries({ queryKey: triggerKeys.byJob(jobId) }),
    invalidateTrigger: (jobId: number, triggerId: number) => 
      queryClient.invalidateQueries({ queryKey: triggerKeys.detail(jobId, triggerId) }),
  };
}