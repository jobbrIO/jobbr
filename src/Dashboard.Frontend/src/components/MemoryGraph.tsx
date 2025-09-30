import React, { useEffect, useRef, useState } from 'react';
import { SmoothieChart, TimeSeries } from 'smoothie';
import { useApi } from '../context/ApiContext';
import { MemoryInfoDto } from '../types';

interface MemoryGraphProps {
  height?: number;
  enabled?: boolean;
}

const MemoryGraph: React.FC<MemoryGraphProps> = ({ height = 120, enabled = true }) => {
  const { apiClient } = useApi();
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const chartRef = useRef<SmoothieChart | null>(null);
  const timeSeriesRef = useRef<TimeSeries | null>(null);
  const intervalRef = useRef<NodeJS.Timeout | null>(null);
  const [totalMemory, setTotalMemory] = useState<number>(0);

  useEffect(() => {
    // Add global CSS for Smoothie tooltips if not already added
    if (!document.querySelector('.smoothie-tooltip-styles')) {
      const style = document.createElement('style');
      style.className = 'smoothie-tooltip-styles';
      style.textContent = `
        .smoothie-chart-tooltip {
          background: rgba(34, 37, 43, 0.95) !important;
          color: rgba(255, 197, 51, 1) !important;
          border: 1px solid rgba(255, 255, 255, 0.2) !important;
          border-radius: 4px !important;
          padding: 6px 8px !important;
          font-size: 12px !important;
          font-family: monospace !important;
          box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3) !important;
        }
      `;
      document.head.appendChild(style);
    }

    if (!canvasRef.current) return;

    // Create the smoothie chart with Aurelia-like styling
    const chart = new SmoothieChart({
      responsive: true,
      millisPerPixel: 50,
      interpolation: 'linear',
      maxValueScale: 1.1,
      minValueScale: 1.1,
      scaleSmoothing: 0.125,
      grid: {
        strokeStyle: 'rgba(57, 67, 79, 0.8)',
        fillStyle: 'rgba(34, 37, 43, 1)',
        lineWidth: 1,
        millisPerLine: 5000,
        verticalSections: 4,
      },
      labels: {
        fillStyle: 'rgba(255, 197, 51, 0.8)',
        fontSize: 11,
        fontFamily: 'monospace',
        precision: 1,
      },
      tooltip: true,
      yMaxFormatter: (max: number) => `${(max / 1024).toFixed(1)} GB`,
      yMinFormatter: (min: number) => `${(min / 1024).toFixed(1)} GB`,
      timestampFormatter: (date: Date) => date.toLocaleTimeString(),
    });

    const timeSeries = new TimeSeries();

    chart.addTimeSeries(timeSeries, {
      strokeStyle: 'rgba(38, 108, 179, 1)',
      fillStyle: 'rgba(38, 108, 179, 0.1)',
      lineWidth: 1,
    });

    // Set up the chart to render to the canvas every second
    chart.streamTo(canvasRef.current, 1000);

    chartRef.current = chart;
    timeSeriesRef.current = timeSeries;

    return () => {
      if (chartRef.current) {
        chartRef.current.stop();
      }
    };
  }, []);

  const updateMemoryData = async () => {
    if (!timeSeriesRef.current) return;

    try {
      const memoryInfo: MemoryInfoDto = await apiClient.getMemoryInfo();
      
      // Set total memory on first load
      if (!totalMemory && memoryInfo.totalPhysicalMemory) {
        setTotalMemory(memoryInfo.totalPhysicalMemory);
      }

      const usedMemory = memoryInfo.totalPhysicalMemory - memoryInfo.freeMemory;
      const now = new Date().getTime();
      
      timeSeriesRef.current.append(now, usedMemory);
    } catch (error) {
      console.error('Error fetching memory data:', error);
    }
  };

  useEffect(() => {
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }

    if (!enabled) {
      return;
    }

    updateMemoryData();

    intervalRef.current = setInterval(updateMemoryData, 1000);

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
    };
  }, [enabled]);

  return (
    <div style={{ height: `${height}px`, width: '100%' }}>
      <canvas
        ref={canvasRef}
        style={{ 
          width: '100%', 
          height: '100%',
          backgroundColor: 'rgba(34, 37, 43, 1)',
          border: '1px solid rgba(57, 67, 79, 0.8)'
        }}
        width={400}
        height={height}
      />
    </div>
  );
};

export default MemoryGraph;