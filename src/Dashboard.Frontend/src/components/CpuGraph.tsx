import React, { useEffect, useRef } from 'react';
import { SmoothieChart, TimeSeries } from 'smoothie';
import { useApi } from '../context/ApiContext';

interface CpuGraphProps {
  height?: number;
  enabled?: boolean;
}

const CpuGraph: React.FC<CpuGraphProps> = ({ height = 120, enabled = true }) => {
  const { apiClient } = useApi();
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const chartRef = useRef<SmoothieChart | null>(null);
  const timeSeriesRef = useRef<TimeSeries | null>(null);
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    // Add global CSS for Smoothie tooltips
    const style = document.createElement('style');
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

    if (!canvasRef.current) return;

    return () => {
      document.head.removeChild(style);
    };
  }, []);

  useEffect(() => {
    if (!canvasRef.current) return;

    // Create the smoothie chart with Aurelia-like styling
    const chart = new SmoothieChart({
      responsive: true,
      millisPerPixel: 50,
      interpolation: 'linear',
      maxValue: 100,
      minValue: 0,
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
      yMaxFormatter: (max: number) => `${max.toFixed(1)}%`,
      yMinFormatter: (min: number) => `${min.toFixed(1)}%`,
      timestampFormatter: (date: Date) => date.toLocaleTimeString(),
    });

    const timeSeries = new TimeSeries();

    chart.addTimeSeries(timeSeries, {
      strokeStyle: 'rgba(0, 255, 0, 1)',
      fillStyle: 'rgba(0, 255, 0, 0.1)',
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

  const updateCpuData = async () => {
    if (!timeSeriesRef.current) return;

    try {
      const cpuUsage = await apiClient.getCpuInfo();
      const now = new Date().getTime();
      
      timeSeriesRef.current.append(now, cpuUsage);
    } catch (error) {
      console.error('Error fetching CPU data:', error);
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

    updateCpuData();

    intervalRef.current = setInterval(updateCpuData, 1000);

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

export default CpuGraph;