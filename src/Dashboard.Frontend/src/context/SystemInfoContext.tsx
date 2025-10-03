import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { useApi } from './ApiContext';
import { MemoryInfoDto } from '../types';

interface SystemInfoContextType {
  cpuUsage: number;
  memoryInfo: MemoryInfoDto | null;
  loading: boolean;
}

const SystemInfoContext = createContext<SystemInfoContextType | undefined>(undefined);

interface SystemInfoProviderProps {
  children: ReactNode;
  enabled?: boolean;
}

export const SystemInfoProvider: React.FC<SystemInfoProviderProps> = ({ children, enabled = true }) => {
  const { apiClient } = useApi();
  const [cpuUsage, setCpuUsage] = useState<number>(0);
  const [memoryInfo, setMemoryInfo] = useState<MemoryInfoDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!enabled) {
      setLoading(false);
      return;
    }

    const updateSystemInfo = async () => {
      try {
        const [cpu, memory] = await Promise.all([
          apiClient.getCpuInfo(),
          apiClient.getMemoryInfo()
        ]);
        
        const usedMemory = memory.totalPhysicalMemory - memory.freeMemory;
        const usagePercentage = (usedMemory / memory.totalPhysicalMemory) * 100;
        
        setCpuUsage(cpu);
        setMemoryInfo({
          ...memory,
          usagePercentage: usagePercentage
        });
        setLoading(false);
      } catch (error) {
        console.error('Error updating system info:', error);
        setLoading(false);
      }
    };

    updateSystemInfo();

    // Set up interval for updates every 2 seconds
    const interval = setInterval(updateSystemInfo, 2000);

    return () => clearInterval(interval);
  }, [apiClient, enabled]);

  useEffect(() => {
    if (!enabled) {
      setCpuUsage(0);
      setMemoryInfo(null);
      setLoading(false);
    }
  }, [enabled]);

  return (
    <SystemInfoContext.Provider value={{ cpuUsage, memoryInfo, loading }}>
      {children}
    </SystemInfoContext.Provider>
  );
};

export const useSystemInfo = (): SystemInfoContextType => {
  const context = useContext(SystemInfoContext);
  if (!context) {
    throw new Error('useSystemInfo must be used within a SystemInfoProvider');
  }
  return context;
};