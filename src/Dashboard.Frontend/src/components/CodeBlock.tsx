import React from 'react';

interface CodeBlockProps {
  code: string;
  language?: string;
  className?: string;
}

const CodeBlock: React.FC<CodeBlockProps> = ({ code, language = 'json', className = '' }) => {
  // Simple syntax highlighting for JSON
  const highlightJson = (jsonString: string): string => {
    try {
      // Parse and reformat JSON for better display
      const parsed = JSON.parse(jsonString);
      return JSON.stringify(parsed, null, 2);
    } catch {
      // If not valid JSON, return as-is
      return jsonString;
    }
  };

  const getFormattedCode = (): string => {
    if (language === 'json' && code) {
      try {
        return highlightJson(code);
      } catch {
        return code;
      }
    }
    return code;
  };

  const isJsonLike = (str: string): boolean => {
    const trimmed = str.trim();
    return (trimmed.startsWith('{') && trimmed.endsWith('}')) || 
           (trimmed.startsWith('[') && trimmed.endsWith(']'));
  };

  const formattedCode = getFormattedCode();
  const detectedLanguage = isJsonLike(code) ? 'json' : 'text';

  return (
    <div className={`code-block ${className}`}>
      <pre 
        className="bg-light p-3 rounded border"
        style={{ 
          fontSize: '0.875rem',
          lineHeight: '1.4',
          whiteSpace: 'pre-wrap',
          wordBreak: 'break-word',
          maxHeight: '300px',
          overflowY: 'auto'
        }}
      >
        <code 
          className={`language-${detectedLanguage}`}
          style={{ 
            fontFamily: 'Monaco, Menlo, "Ubuntu Mono", Consolas, source-code-pro, monospace',
            color: detectedLanguage === 'json' ? '#0066cc' : '#333'
          }}
        >
          {formattedCode}
        </code>
      </pre>
      {detectedLanguage === 'json' && (
        <small className="text-muted d-block mt-1">
          <i className="fas fa-info-circle me-1"></i>
          JSON formatted for readability
        </small>
      )}
    </div>
  );
};

export default CodeBlock;