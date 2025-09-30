/**
 * Utility functions for safe data formatting
 */

/**
 * Safely formats a number with toFixed, handling undefined/null values
 * @param value - The number to format (can be undefined/null)
 * @param decimals - Number of decimal places (default: 1)
 * @param fallback - Fallback value when input is invalid (default: '0')
 * @returns Formatted string with specified decimals
 */
export const safeToFixed = (
  value: number | undefined | null, 
  decimals: number = 1, 
  fallback: string = '0'
): string => {
  if (value === null || value === undefined || isNaN(value)) {
    return fallback;
  }
  return value.toFixed(decimals);
};

/**
 * Safely formats file sizes
 * @param bytes - Size in bytes
 * @param decimals - Number of decimal places
 * @returns Formatted size string
 */
export const formatFileSize = (
  bytes: number | undefined | null, 
  decimals: number = 1
): string => {
  if (!bytes || isNaN(bytes)) return '0 B';
  
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return `${safeToFixed(bytes / Math.pow(k, i), decimals)} ${sizes[i]}`;
};

/**
 * Safely formats dates
 * @param dateString - Date string to format
 * @param fallback - Fallback string if date is invalid
 * @returns Formatted date string in DD.MM.YYYY HH:mm:ss format
 */
export const safeFormatDate = (
  dateString: string | undefined | null, 
  fallback: string = '-'
): string => {
  if (!dateString) return fallback;
  
  try {
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return fallback;
    
    // Format as DD.MM.YYYY HH:mm:ss
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    const seconds = date.getSeconds().toString().padStart(2, '0');
    
    return `${day}.${month}.${year} ${hours}:${minutes}:${seconds}`;
  } catch {
    return fallback;
  }
};

/**
 * Safely renders parameter values that might be objects for display in JSX
 * @param param - The parameter value to render
 * @returns String representation suitable for JSX rendering
 */
export const renderParameter = (param: any): string => {
  if (param === null || param === undefined) {
    return '';
  }
  
  if (typeof param === 'string') {
    return param;
  }
  
  if (typeof param === 'object') {
    try {
      return JSON.stringify(param, null, 2);
    } catch {
      return String(param);
    }
  }
  
  return String(param);
};

/**
 * Calculates time difference and returns a human-readable string like "5 minutes later"
 * @param fromDate - The starting date/time
 * @param toDate - The ending date/time  
 * @returns Human-readable time difference or empty string if dates are invalid
 */
export const formatTimeSince = (fromDate?: string, toDate?: string): string => {
  if (!fromDate || !toDate) return '';
  
  const from = new Date(fromDate);
  const to = new Date(toDate);
  
  if (isNaN(from.getTime()) || isNaN(to.getTime())) return '';
  
  const diffMs = to.getTime() - from.getTime();
  
  if (diffMs < 0) return '';
  
  const diffMinutes = Math.floor(diffMs / (1000 * 60));
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
  
  if (diffDays > 0) {
    return `${diffDays} day${diffDays > 1 ? 's' : ''}`;
  } else if (diffHours > 0) {
    return `${diffHours} hour${diffHours > 1 ? 's' : ''}`;
  } else if (diffMinutes > 0) {
    return `${diffMinutes} minute${diffMinutes > 1 ? 's' : ''}`;
  } else {
    return 'less than a minute';
  }
};

/**
 * Formats duration between two dates with enhanced display showing "X later" when appropriate
 * @param startDate - The start date
 * @param endDate - The end date
 * @param referenceDate - Optional reference date for "later" calculation
 * @returns Enhanced duration string
 */
export const formatEnhancedDuration = (
  startDate?: string, 
  endDate?: string,
  referenceDate?: string
): string => {
  if (!startDate) return '-';
  
  if (endDate) {
    // Show actual duration
    const duration = formatTimeSince(startDate, endDate);
    return duration || '-';
  }
  
  if (referenceDate && startDate) {
    // Show "X later" format
    const laterTime = formatTimeSince(referenceDate, startDate);
    return laterTime ? `${laterTime} later` : '-';
  }
  
  return '-';
};