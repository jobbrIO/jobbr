import React from 'react';

interface ParameterDisplayProps {
  label: string;
  value: any;
  className?: string;
}

/**
 * Component for safely displaying parameter values that might be objects
 * Handles JSON objects, strings, and other data types gracefully
 */
const ParameterDisplay: React.FC<ParameterDisplayProps> = ({ 
  label, 
  value, 
  className = "bg-light p-2 rounded small" 
}) => {
  const renderValue = (param: any): string => {
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

  if (!value && value !== 0) {
    return null;
  }

  return (
    <div className="mb-3">
      <div className="row">
        <div className="col-sm-3">
          <strong>{label}:</strong>
        </div>
        <div className="col-sm-9">
          <pre className={className}>
            {renderValue(value)}
          </pre>
        </div>
      </div>
    </div>
  );
};

export default ParameterDisplay;