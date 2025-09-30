import React, { useState } from 'react';
import { Table, Spinner, Alert, Form, Button, Badge, Pagination, Dropdown, InputGroup } from 'react-bootstrap';
import { useNavigate, useParams } from 'react-router-dom';
import { useJobRuns, useJobRunsByJobId, useJob } from '../hooks';
import { JobRunDto, PagedResult } from '../types';
import { safeFormatDate, formatEnhancedDuration } from '../utils/formatters';

const Runs: React.FC = () => {
  const navigate = useNavigate();
  const { jobId } = useParams<{ jobId: string }>();
  
  // Parse jobId to number
  const parsedJobId = jobId ? parseInt(jobId) : undefined;
  
  // Filter states
  const [currentPage, setCurrentPage] = useState(1);
  const [query, setQuery] = useState('');
  const [sort, setSort] = useState('-PlannedStartDateTimeUtc');
  const [selectedStates, setSelectedStates] = useState<string[]>([]);
  const [showDeleted, setShowDeleted] = useState(false);

  const jobRunStates = [
    'Scheduled', 'Preparing', 'Starting', 'Started', 'Connected', 
    'Initializing', 'Processing', 'Finishing', 'Collecting',
    'Completed', 'Failed', 'Omitted'
  ];

  // Use TanStack Query hooks
  const { 
    data: jobRuns, 
    isLoading: runsLoading, 
    error: runsError,
    refetch: refetchRuns
  } = parsedJobId 
    ? useJobRunsByJobId(parsedJobId, {
        page: currentPage,
        sort,
        pageSize: 20,
        showDeleted
      })
    : useJobRuns({
        page: currentPage,
        sort,
        query,
        states: selectedStates.length > 0 ? selectedStates : undefined,
        showDeleted,
        pageSize: 20
      });

  const { 
    data: job, 
    isLoading: jobLoading 
  } = useJob(parsedJobId);

  const isLoading = runsLoading || jobLoading;
  const error = runsError;

  const handleStateToggle = (state: string) => {
    setSelectedStates(prev => 
      prev.includes(state) 
        ? prev.filter(s => s !== state)
        : [...prev, state]
    );
    setCurrentPage(1); // Reset to first page when filtering
  };

  const handleSort = (field: string) => {
    const currentSort = sort;
    let newSort = field;
    
    // If we're already sorting by this field, toggle the direction
    if (currentSort === field) {
      newSort = `-${field}`;
    } else if (currentSort === `-${field}`) {
      newSort = field;
    }
    
    setSort(newSort);
    setCurrentPage(1); // Reset to first page when sorting
  };

  const getSortIcon = (field: string) => {
    if (sort === field) {
      return <i className="fas fa-sort-up ms-1"></i>;
    } else if (sort === `-${field}`) {
      return <i className="fas fa-sort-down ms-1"></i>;
    } else {
      return <i className="fas fa-sort ms-1 text-muted"></i>;
    }
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleBack = () => {
    // Go back in browser history, or fallback to jobs list if no history
    if (window.history.length > 1) {
      navigate(-1);
    } else {
      navigate('/jobs');
    }
  };

  const formatDate = (dateString?: string): string => {
    return safeFormatDate(dateString);
  };

  const getStatusBadgeVariant = (state: string): string => {
    switch (state) {
      case 'Completed': return 'success';
      case 'Failed': return 'danger';
      case 'Started':
      case 'Processing':
      case 'Running': return 'info';
      case 'Scheduled': return 'warning';
      case 'Omitted': return 'secondary';
      default: return 'primary';
    }
  };

  const handleRunClick = (runId: number) => {
    navigate(`/runs/${runId}`);
  };

  // Helper function to calculate total pages
  const getTotalPages = (jobRuns: PagedResult<JobRunDto> | null): number => {
    if (!jobRuns || jobRuns.pageSize === 0) return 0;
    return Math.ceil(jobRuns.totalItems / jobRuns.pageSize);
  };

  const renderPagination = () => {
    if (!jobRuns || jobRuns.totalItems === 0) return null;
    
    const totalPages = getTotalPages(jobRuns);
    
    // Skip showing anything for a single page
    if (totalPages <= 1) {
      return null;
    }

    const items = [];
    const maxPagesToShow = 5;
    const startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
    const endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

    if (startPage > 1) {
      items.push(<Pagination.First key="first" onClick={() => handlePageChange(1)} />);
      if (startPage > 2) {
        // Jump to middle of first section when ellipsis is clicked
        const jumpToPage = Math.max(1, Math.floor(startPage / 2));
        items.push(
          <Pagination.Ellipsis 
            key="ellipsis1" 
            onClick={() => handlePageChange(jumpToPage)}
            style={{ cursor: 'pointer' }}
          />
        );
      }
    }

    for (let page = startPage; page <= endPage; page++) {
      items.push(
        <Pagination.Item
          key={page}
          active={page === currentPage}
          onClick={() => handlePageChange(page)}
        >
          {page}
        </Pagination.Item>
      );
    }

    if (endPage < totalPages) {
      if (endPage < totalPages - 1) {
        // Jump to middle of last section when ellipsis is clicked
        const jumpToPage = Math.min(totalPages, Math.ceil((endPage + totalPages) / 2));
        items.push(
          <Pagination.Ellipsis 
            key="ellipsis2" 
            onClick={() => handlePageChange(jumpToPage)}
            style={{ cursor: 'pointer' }}
          />
        );
      }
      items.push(<Pagination.Last key="last" onClick={() => handlePageChange(totalPages)} />);
    }

    return <Pagination className="justify-content-center">{items}</Pagination>;
  };

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '200px' }}>
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </div>
    );
  }

  if (error) {
    return (
      <Alert variant="danger">
        <Alert.Heading>Error</Alert.Heading>
        <p>{error instanceof Error ? error.message : String(error)}</p>
        <Button variant="outline-danger" onClick={() => refetchRuns()}>
          Try Again
        </Button>
      </Alert>
    );
  }

  return (
    <div className="container-fluid">
      {/* Header Row - Full Width */}
      <div className="row">
        <div className="col-12">
          {jobId && job ? (
            <div>
              <h3>
                <Button variant="link" className="p-0 me-2" onClick={handleBack}>
                  <i className="fas fa-angle-left"></i>
                </Button>
                <i className="fas fa-calendar-alt me-2"></i>
                {job.uniqueName} - Job Runs
              </h3>
              <small className="text-muted">{job.type}</small>
            </div>
          ) : (
            <h3>
              <i className="fas fa-flag-checkered me-2"></i>
              All Runs
            </h3>
          )}
        </div>
      </div>

      {/* Filters and Actions */}
      <div className="row mt-3">
        <div className="col-12">
          <div className="mb-3">
            <div className="d-flex flex-wrap align-items-center gap-2">
              {/* Action buttons - always visible */}
              <Button variant="primary" onClick={() => refetchRuns()}>
                <i className="fas fa-sync-alt me-1"></i>
                Refresh
              </Button>
              <Button
                variant={showDeleted ? 'warning' : 'outline-warning'}
                onClick={() => setShowDeleted(!showDeleted)}
              >
                <i className="fas fa-trash me-1"></i>
                {showDeleted ? 'Hide Deleted' : 'Show Deleted'}
              </Button>

              {/* State filters and search - only show for all runs */}
              {!jobId && (
                <>
                  {/* Divider */}
                  <div className="vr mx-2"></div>
                  
                  {/* State filters dropdown */}
                  <Dropdown>
                    <Dropdown.Toggle variant="outline-primary" id="state-filter-dropdown">
                      <i className="fas fa-filter me-1"></i>
                      States {selectedStates.length > 0 && `(${selectedStates.length})`}
                    </Dropdown.Toggle>
                    <Dropdown.Menu style={{ maxHeight: '300px', overflowY: 'auto' }}>
                      <Dropdown.Item onClick={() => setSelectedStates([])}>
                        <i className="fas fa-times me-2"></i>
                        Clear All
                      </Dropdown.Item>
                      <Dropdown.Divider />
                      {jobRunStates.map(state => (
                        <Dropdown.Item
                          key={state}
                          active={selectedStates.includes(state)}
                          onClick={() => handleStateToggle(state)}
                        >
                          {selectedStates.includes(state) && <i className="fas fa-check me-2"></i>}
                          {!selectedStates.includes(state) && <span className="me-4"></span>}
                          {state}
                        </Dropdown.Item>
                      ))}
                    </Dropdown.Menu>
                  </Dropdown>

                  {/* Search with icon */}
                  <InputGroup style={{ maxWidth: '250px' }}>
                    <InputGroup.Text>
                      <i className="fas fa-search"></i>
                    </InputGroup.Text>
                    <Form.Control
                      type="text"
                      placeholder="Search..."
                      value={query}
                      onChange={(e) => setQuery(e.target.value)}
                    />
                    {query && (
                      <Button
                        variant="outline-secondary"
                        onClick={() => setQuery('')}
                        title="Clear search"
                      >
                        <i className="fas fa-times"></i>
                      </Button>
                    )}
                  </InputGroup>
                </>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Results */}
      <div className="row">
        <div className="col-12">
          {(!jobRuns || jobRuns.totalItems === 0) ? (
            <p className="text-muted">No job runs found.</p>
          ) : (
            <>
              <p className="text-muted mb-3">
                Showing {jobRuns.items.length} of {jobRuns.totalItems} runs
                {Math.ceil(jobRuns.totalItems / jobRuns.pageSize) > 1 && ` (Page ${currentPage} of ${Math.ceil(jobRuns.totalItems / jobRuns.pageSize)})`}
                {query && (
                  <span className="ms-2">
                    <Badge bg="info" className="ms-1">
                      <i className="fas fa-filter me-1"></i>
                      Filtered by: "{query}"
                    </Badge>
                  </span>
                )}
              </p>

              <Table hover responsive>
                <thead>
                  <tr>
                    <th 
                      className="sortable-header" 
                      style={{ cursor: 'pointer' }}
                      onClick={() => handleSort('Id')}
                    >
                      ID {getSortIcon('Id')}
                    </th>
                    {!jobId && (
                      <th>Job</th>
                    )}
                    <th 
                      className="sortable-header" 
                      style={{ cursor: 'pointer' }}
                      onClick={() => handleSort('State')}
                    >
                      State {getSortIcon('State')}
                    </th>
                    <th 
                      className="sortable-header" 
                      style={{ cursor: 'pointer' }}
                      onClick={() => handleSort('Progress')}
                    >
                      Progress {getSortIcon('Progress')}
                    </th>
                    <th 
                      className="sortable-header" 
                      style={{ cursor: 'pointer' }}
                      onClick={() => handleSort('PlannedStartDateTimeUtc')}
                    >
                      Planned Start {getSortIcon('PlannedStartDateTimeUtc')}
                    </th>
                    <th 
                      className="sortable-header" 
                      style={{ cursor: 'pointer' }}
                      onClick={() => handleSort('ActualStartDateTimeUtc')}
                    >
                      Actual Start {getSortIcon('ActualStartDateTimeUtc')}
                    </th>
                    <th 
                      className="sortable-header" 
                      style={{ cursor: 'pointer' }}
                      onClick={() => handleSort('ActualEndDateTimeUtc')}
                    >
                      Actual End {getSortIcon('ActualEndDateTimeUtc')}
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {jobRuns.items.map(run => (
                    <tr 
                      key={run.jobRunId}
                      className="table-row-clickable"
                      onClick={() => handleRunClick(run.jobRunId)}
                      style={{ 
                        cursor: 'pointer',
                        opacity: run.deleted ? 0.6 : 1 
                      }}
                    >
                      <td>
                        <span className="text-primary fw-bold">{run.jobRunId}</span>
                        {run.deleted && <i className="fas fa-trash text-muted ms-1" title="Deleted"></i>}
                      </td>
                      {!jobId && (
                        <td>
                          <div className="d-flex align-items-center">
                            <div className="flex-grow-1">
                              <div className="fw-bold">{run.jobTitle || run.jobName}</div>
                              {run.jobTitle && run.jobName && run.jobTitle !== run.jobName && (
                                <small className="text-muted">{run.jobName}</small>
                              )}
                            </div>
                            <Button
                              variant="link"
                              size="sm"
                              className="p-0 ms-2 text-muted"
                              onClick={(e) => {
                                e.stopPropagation();
                                setQuery(run.jobName);
                              }}
                              title={`Filter by job: ${run.jobName}`}
                            >
                              <i className="fas fa-filter"></i>
                            </Button>
                          </div>
                        </td>
                      )}
                      <td>
                        <Badge bg={getStatusBadgeVariant(run.state)}>
                          {run.state}
                        </Badge>
                      </td>
                      <td>
                        {run.progress !== undefined && run.progress !== null ? (
                          <div className="progress" style={{ width: '100px', height: '20px' }}>
                            <div 
                              className="progress-bar d-flex align-items-center justify-content-center" 
                              role="progressbar" 
                              style={{ width: `${run.progress}%` }}
                              aria-valuenow={run.progress}
                              aria-valuemin={0}
                              aria-valuemax={100}
                            >
                              {run.progress}%
                            </div>
                          </div>
                        ) : '-'}
                      </td>
                      <td>{formatDate(run.plannedStartUtc)}</td>
                      <td>
                        <div>
                          {formatDate(run.actualStartUtc)}
                          {run.actualStartUtc && run.plannedStartUtc && (
                            <div>
                              <small className="text-muted">
                                {formatEnhancedDuration(run.plannedStartUtc, run.actualStartUtc)} later
                              </small>
                            </div>
                          )}
                        </div>
                      </td>
                      <td>
                        <div>
                          {formatDate(run.actualEndUtc)}
                          {run.actualEndUtc && run.actualStartUtc && (
                            <div>
                              <small className="text-success">
                                {formatEnhancedDuration(run.actualStartUtc, run.actualEndUtc)}
                              </small>
                            </div>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>

              {renderPagination()}
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default Runs;