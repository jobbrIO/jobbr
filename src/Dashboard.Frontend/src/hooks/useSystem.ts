import { useQuery } from '@tanstack/react-query';
import { useApi } from '../context/ApiContext';

// Query Keys
export const systemKeys = {
  all: ['system'] as const,
  cpu: () => [...systemKeys.all, 'cpu'] as const,
  memory: () => [...systemKeys.all, 'memory'] as const,
  disks: () => [...systemKeys.all, 'disks'] as const,
};

// Query Hooks
export function useCpuInfo(options: { enabled?: boolean; refetchInterval?: number } = {}) {
  const { apiClient } = useApi();
  const { enabled = true, refetchInterval = 2000 } = options;

  return useQuery({
    queryKey: systemKeys.cpu(),
    queryFn: () => apiClient.getCpuInfo(),
    enabled,
    refetchInterval,
    refetchIntervalInBackground: true, // Continue monitoring in background
    staleTime: 1000, // 1 second
    gcTime: 5000, // 5 seconds
  });
}

export function useMemoryInfo(options: { enabled?: boolean; refetchInterval?: number } = {}) {
  const { apiClient } = useApi();
  const { enabled = true, refetchInterval = 2000 } = options;

  return useQuery({
    queryKey: systemKeys.memory(),
    queryFn: () => apiClient.getMemoryInfo(),
    enabled,
    refetchInterval,
    refetchIntervalInBackground: true, // Continue monitoring in background
    staleTime: 1000, // 1 second
    gcTime: 5000, // 5 seconds
  });
}

export function useDiskInfo(options: { enabled?: boolean } = {}) {
  const { apiClient } = useApi();
  const { enabled = true } = options;

  return useQuery({
    queryKey: systemKeys.disks(),
    queryFn: () => apiClient.getDiskInfo(),
    enabled,
    staleTime: 30 * 1000, // 30 seconds
    refetchInterval: 60 * 1000, // 1 minute
    refetchIntervalInBackground: true, // Continue monitoring in background
  });
}