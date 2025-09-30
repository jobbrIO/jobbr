import React, { useState } from 'react';
import { Card, Row, Col, Spinner, Alert, Table, Badge, Button } from 'react-bootstrap';
import { useParams, useNavigate } from 'react-router-dom';
import { useJob, useJobRunsByJobId, useTriggersByJobId, useUpdateTrigger } from '../hooks';
import { JobTriggerDto } from '../types';
import { safeFormatDate, renderParameter, formatEnhancedDuration } from '../utils/formatters';
import { isInstantTrigger, isScheduledTrigger, isRecurringTrigger } from '../utils/typeGuards';
import CodeBlock from '../components/CodeBlock';
import TriggerCreateModal from '../components/TriggerCreateModal';
import TriggerEditModal from '../components/TriggerEditModal';

const JobDetail: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  
  // Parse the job ID
  const jobId = id ? parseInt(id) : undefined;
  
  // State for pagination and modals
  const [triggersPage, setTriggersPage] = useState(1);
  const [actionLoading, setActionLoading] = useState<{[key: number]: string}>({});
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editTriggerId, setEditTriggerId] = useState<number | null>(null);

  // Use TanStack Query hooks
  const { 
    data: job, 
    isLoading: jobLoading, 
    error: jobError 
  } = useJob(jobId);
  
  const { 
    data: triggers, 
    isLoading: triggersLoading 
  } = useTriggersByJobId(jobId, triggersPage);
  
  const { 
    data: jobRuns, 
    isLoading: runsLoading 
  } = useJobRunsByJobId(jobId, { page: 1, sort: '-ActualEndDateTimeUtc', pageSize: 10 });
  
  const updateTriggerMutation = useUpdateTrigger();

  // Combined loading and error states
  const isLoading = jobLoading || triggersLoading || runsLoading;
  const error = jobError;

  const handleTriggersPageChange = (page: number) => {
    setTriggersPage(page);
  };

  const handleToggleTrigger = async (trigger: JobTriggerDto) => {
    if (!jobId) return;
    
    try {
      setActionLoading(prev => ({ ...prev, [trigger.id]: 'toggle' }));
      
      // Create updated trigger with toggled state
      const updatedTrigger = {
        ...trigger,
        isActive: !trigger.isActive
      };
      
      await updateTriggerMutation.mutateAsync({ trigger: updatedTrigger, jobId });
    } catch (err) {
      console.error('Failed to toggle trigger:', err);
    } finally {
      setActionLoading(prev => {
        const newState = { ...prev };
        delete newState[trigger.id];
        return newState;
      });
    }
  };

  const handleEditTrigger = (trigger: JobTriggerDto) => {
    setEditTriggerId(trigger.id);
    setShowEditModal(true);
  };

  const formatDate = (dateString?: string): string => {
    return safeFormatDate(dateString);
  };

  const getStatusBadgeVariant = (state: string): string => {
    switch (state?.toLowerCase()) {
      case 'completed':
        return 'success';
      case 'failed':
        return 'danger';
      case 'running':
        return 'primary';
      case 'scheduled':
        return 'info';
      default:
        return 'secondary';
    }
  };

  const handleViewRuns = () => {
    navigate(`/jobs/${id}/runs`);
  };

  const handleCreateTrigger = () => {
    setShowCreateModal(true);
  };

  const handleViewAllRuns = () => {
    navigate('/runs');
  };

  const handleTriggerCreated = () => {
    // TanStack Query will automatically refetch due to cache invalidation
  };

  const handleTriggerUpdated = () => {
    // TanStack Query will automatically refetch due to cache invalidation
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

  if (error || !job) {
    return (
      <Alert variant="danger">
        <Alert.Heading>Error</Alert.Heading>
        <p>{error ? (error instanceof Error ? error.message : String(error)) : 'Job not found'}</p>
        <Button variant="outline-danger" onClick={() => window.location.reload()}>
          Try Again
        </Button>
      </Alert>
    );
  }

  return (
    <>
    <div className="container-fluid">
      <div className="row">
        <div className="col-12">
          <h3 className="mb-3">
            <i className="fas fa-cog me-2"></i>
            {job.title || job.uniqueName}
          </h3>
        </div>
      </div>

      <Row>
        <Col lg={8}>
          <Card className="mb-4">
            <Card.Header>
              <h5 className="mb-0">Job Information</h5>
            </Card.Header>
            <Card.Body>
              <Row>
                <Col sm={3}><strong>ID:</strong></Col>
                <Col sm={9}>{job.id}</Col>
              </Row>
              <Row>
                <Col sm={3}><strong>Title:</strong></Col>
                <Col sm={9}>{job.title || '-'}</Col>
              </Row>
              <Row>
                <Col sm={3}><strong>Unique Name:</strong></Col>
                <Col sm={9}><strong>{job.uniqueName}</strong></Col>
              </Row>
              <Row>
                <Col sm={3}><strong>Type:</strong></Col>
                <Col sm={9}>
                  <span className="font-monospace">{job.type}</span>
                </Col>
              </Row>
              <Row>
                <Col sm={3}><strong>Created:</strong></Col>
                <Col sm={9}>{formatDate(job.createdDateTimeUtc)}</Col>
              </Row>
              <Row>
                <Col sm={3}><strong>Last Updated:</strong></Col>
                <Col sm={9}>{formatDate(job.updatedDateTimeUtc)}</Col>
              </Row>
            </Card.Body>
          </Card>

          {job.parameters && (
            <Card className="mb-4">
              <Card.Header>
                <h5 className="mb-0">Parameters</h5>
              </Card.Header>
              <Card.Body>
                <CodeBlock code={renderParameter(job.parameters)} />
              </Card.Body>
            </Card>
          )}

          <Card className="mb-4">
            <Card.Header className="d-flex justify-content-between align-items-center">
              <h5 className="mb-0">Triggers</h5>
              <div className="d-flex gap-2">
                <Button variant="primary" size="sm" onClick={handleCreateTrigger}>
                  <i className="fas fa-plus me-1"></i>
                  Create Trigger
                </Button>
              </div>
            </Card.Header>
            <Card.Body>
              {!triggers || triggers.totalItems === 0 ? (
                <p className="text-muted mb-0">No triggers found.</p>
              ) : (
                <>
                  <Table hover responsive>
                    <thead>
                      <tr>
                        <th>ID</th>
                        <th>State</th>
                        <th>Type</th>
                        <th>Definition</th>
                        <th>Comment</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {triggers.items.map(trigger => (
                        <tr key={trigger.id}>
                          <td>{trigger.id}</td>
                          <td>
                            <Badge bg={trigger.isActive ? 'success' : 'danger'}>
                              {trigger.isActive ? 'Active' : 'Inactive'}
                            </Badge>
                          </td>
                          <td>
                            <Badge bg="info">{trigger.triggerType}</Badge>
                          </td>
                          <td>
                            {(() => {
                              // Recurring trigger - show cron expression with link
                              if (isRecurringTrigger(trigger)) {
                                return (
                                  <div>
                                    <div className="d-flex align-items-center">
                                      <i className="fas fa-sync-alt text-primary me-2" title="Recurring"></i>
                                      <a 
                                        href={`https://crontab.guru/#${encodeURIComponent(trigger.definition.replace(/\s+/g, '_'))}`}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="text-decoration-none"
                                        title="View on Crontab Guru"
                                      >
                                        <code className="text-primary">{trigger.definition}</code>
                                      </a>
                                    </div>
                                    {/* Show start/end times if set */}
                                    {(trigger.startDateTimeUtc || trigger.endDateTimeUtc) && (
                                      <div className="mt-1">
                                        {trigger.startDateTimeUtc && (
                                          <div className="small text-muted">
                                            <i className="fas fa-play me-1"></i>
                                            Start: {safeFormatDate(trigger.startDateTimeUtc)}
                                          </div>
                                        )}
                                        {trigger.endDateTimeUtc && (
                                          <div className="small text-muted">
                                            <i className="fas fa-stop me-1"></i>
                                            End: {safeFormatDate(trigger.endDateTimeUtc)}
                                          </div>
                                        )}
                                      </div>
                                    )}
                                  </div>
                                );
                              }
                              
                              // Scheduled trigger - show formatted date/time
                              if (isScheduledTrigger(trigger)) {
                                const date = new Date(trigger.startDateTimeUtc);
                                return (
                                  <div className="d-flex align-items-center">
                                    <i className="fas fa-calendar-alt text-info me-2" title="Scheduled"></i>
                                    <span className="text-muted" title={date.toISOString()}>
                                      {date.toLocaleDateString()} {date.toLocaleTimeString()}
                                    </span>
                                  </div>
                                );
                              }
                              
                              // Instant trigger - show delay information
                              if (isInstantTrigger(trigger)) {
                                return (
                                  <div className="d-flex align-items-center">
                                    <i className="fas fa-bolt text-warning me-2" title="Instant"></i>
                                    {trigger.delayedMinutes > 0 ? (
                                      <span className="text-warning">
                                        Delayed {trigger.delayedMinutes}min
                                      </span>
                                    ) : (
                                      <span className="text-success">Immediate</span>
                                    )}
                                  </div>
                                );
                              }
                              
                              return '-';
                            })()}
                          </td>
                          <td>{trigger.comment || '-'}</td>
                          <td>
                            <div className="d-flex gap-1">
                              <Button 
                                variant={trigger.isActive ? "outline-warning" : "outline-success"} 
                                size="sm" 
                                title={trigger.isActive ? "Disable Trigger" : "Enable Trigger"}
                                onClick={() => handleToggleTrigger(trigger)}
                                disabled={actionLoading[trigger.id] === 'toggle'}
                                style={{ width: '32px' }}
                              >
                                {actionLoading[trigger.id] === 'toggle' ? (
                                  <Spinner animation="border" size="sm" />
                                ) : (
                                  <i className={trigger.isActive ? "fas fa-pause" : "fas fa-play"}></i>
                                )}
                              </Button>
                              <Button 
                                variant="outline-secondary" 
                                size="sm" 
                                title="Edit Trigger"
                                onClick={() => handleEditTrigger(trigger)}
                                style={{ width: '32px' }}
                              >
                                <i className="fas fa-edit"></i>
                              </Button>
                              <Button 
                                variant="outline-danger" 
                                size="sm" 
                                title="Delete Trigger (Not Available)"
                                disabled
                                style={{ width: '32px' }}
                              >
                                <i className="fas fa-trash"></i>
                              </Button>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </Table>
                  {Math.ceil(triggers.totalItems / triggers.pageSize) > 1 && (
                    <div className="d-flex justify-content-center mt-3">
                      <nav>
                        <ul className="pagination pagination-sm">
                          <li className={`page-item ${triggersPage === 1 ? 'disabled' : ''}`}>
                            <button 
                              className="page-link" 
                              onClick={() => handleTriggersPageChange(triggersPage - 1)}
                              disabled={triggersPage === 1}
                            >
                              Previous
                            </button>
                          </li>
                          {Array.from({ length: Math.ceil(triggers.totalItems / triggers.pageSize) }, (_, i) => i + 1).map(page => (
                            <li key={page} className={`page-item ${page === triggersPage ? 'active' : ''}`}>
                              <button 
                                className="page-link" 
                                onClick={() => handleTriggersPageChange(page)}
                              >
                                {page}
                              </button>
                            </li>
                          ))}
                          <li className={`page-item ${triggersPage === Math.ceil(triggers.totalItems / triggers.pageSize) ? 'disabled' : ''}`}>
                            <button 
                              className="page-link" 
                              onClick={() => handleTriggersPageChange(triggersPage + 1)}
                              disabled={triggersPage === Math.ceil(triggers.totalItems / triggers.pageSize)}
                            >
                              Next
                            </button>
                          </li>
                        </ul>
                      </nav>
                    </div>
                  )}
                </>
              )}
            </Card.Body>
          </Card>

          <Card>
            <Card.Header className="d-flex justify-content-between align-items-center">
              <h5 className="mb-0">Last 10 Runs</h5>
              <Button variant="outline-primary" size="sm" onClick={handleViewRuns}>
                View All Job Runs
              </Button>
            </Card.Header>
            <Card.Body>
              {!jobRuns || jobRuns.totalItems === 0 ? (
                <p className="text-muted mb-0">No runs found.</p>
              ) : (
                <Table hover responsive>
                  <thead>
                    <tr>
                      <th>ID</th>
                      <th>State</th>
                      <th>Started</th>
                      <th>Duration</th>
                    </tr>
                  </thead>
                  <tbody>
                    {jobRuns.items.map(run => (
                      <tr 
                        key={run.jobRunId}
                        className="table-row-clickable"
                        onClick={() => navigate(`/runs/${run.jobRunId}`)}
                        style={{ cursor: 'pointer' }}
                      >
                        <td>
                          <span className="text-primary fw-bold">{run.jobRunId}</span>
                        </td>
                        <td>
                          <Badge bg={getStatusBadgeVariant(run.state)}>
                            {run.state}
                          </Badge>
                        </td>
                        <td>{formatDate(run.actualStartUtc)}</td>
                        <td>
                          {run.actualStartUtc && run.actualEndUtc ? (
                            <span className="text-success">
                              {formatEnhancedDuration(run.actualStartUtc, run.actualEndUtc)}
                            </span>
                          ) : (
                            '-'
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              )}
            </Card.Body>
          </Card>
        </Col>

        <Col lg={4}>
          <Card>
            <Card.Header>
              <h5 className="mb-0">Actions</h5>
            </Card.Header>
            <Card.Body className="d-grid gap-2">
              <Button variant="primary" onClick={handleCreateTrigger}>
                <i className="fas fa-plus me-2"></i>
                Create Trigger
              </Button>
              <Button variant="outline-primary" onClick={handleViewRuns}>
                <i className="fas fa-flag-checkered me-2"></i>
                View All Job Runs
              </Button>
              <Button variant="outline-secondary" onClick={handleViewAllRuns}>
                <i className="fas fa-list me-2"></i>
                View All Runs
              </Button>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </div>

    {/* Modals */}
    <TriggerCreateModal
      show={showCreateModal}
      onHide={() => setShowCreateModal(false)}
      jobId={job?.id || 0}
      onTriggerCreated={handleTriggerCreated}
    />
    
    <TriggerEditModal
      show={showEditModal}
      onHide={() => setShowEditModal(false)}
      jobId={job?.id || 0}
      triggerId={editTriggerId}
      onTriggerUpdated={handleTriggerUpdated}
    />
    </>
  );
};

export default JobDetail;