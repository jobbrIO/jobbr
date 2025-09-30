import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Spinner, Alert, Badge, Button, Table, Dropdown, ButtonGroup } from 'react-bootstrap';
import { useParams, useNavigate } from 'react-router-dom';
import { useApi } from '../context/ApiContext';
import { JobRunDto } from '../types';
import { safeFormatDate, formatFileSize, renderParameter } from '../utils/formatters';
import CodeBlock from '../components/CodeBlock';

const RunDetail: React.FC = () => {
  const { apiClient } = useApi();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  
  const [jobRun, setJobRun] = useState<JobRunDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      loadRunDetails();
    }
  }, [id]);

  const loadRunDetails = async () => {
    if (!id) return;
    
    try {
      setLoading(true);
      setError(null);
      
      const runId = parseInt(id);
      const result = await apiClient.getJobRun(runId);
      setJobRun(result);
    } catch (err) {
      setError(`Failed to load job run details: ${err instanceof Error ? err.message : String(err)}`);
    } finally {
      setLoading(false);
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

  const calculateDuration = (): string => {
    if (!jobRun?.actualStartUtc || !jobRun?.actualEndUtc) {
      return '-';
    }
    
    const start = new Date(jobRun.actualStartUtc);
    const end = new Date(jobRun.actualEndUtc);
    const durationMs = end.getTime() - start.getTime();
    
    const hours = Math.floor(durationMs / (1000 * 60 * 60));
    const minutes = Math.floor((durationMs % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((durationMs % (1000 * 60)) / 1000);
    
    if (hours > 0) {
      return `${hours}h ${minutes}m ${seconds}s`;
    } else if (minutes > 0) {
      return `${minutes}m ${seconds}s`;
    } else {
      return `${seconds}s`;
    }
  };

  const handleViewJob = () => {
    if (jobRun) {
      navigate(`/jobs/${jobRun.jobId}`);
    }
  };

  const handleBack = () => {
    // Go back in browser history, or fallback to runs list if no history
    if (window.history.length > 1) {
      navigate(-1);
    } else {
      navigate('/runs');
    }
  };

  const handleViewAllRuns = () => {
    navigate('/runs');
  };

  const handleRetryRun = async () => {
    if (!jobRun) return;
    
    try {
      await apiClient.retryJobRun(jobRun);
      loadRunDetails();
    } catch (err) {
      console.error('Error retrying job run:', err);
    }
  };

  const handleDeleteRun = async () => {
    if (!jobRun) return;
    
    if (window.confirm('Are you sure you want to delete this job run?')) {
      try {
        await apiClient.deleteJobRun(jobRun.jobRunId);
        loadRunDetails();
      } catch (err) {
        console.error('Error deleting job run:', err);
      }
    }
  };

  const handleRefresh = async () => {
    loadRunDetails();
  };

  const handleDownloadArtifact = async (filename: string) => {
    if (!jobRun) return;
    
    // Get the API URL and construct download URL
    const apiUrl = await apiClient.getApiUrl();
    const downloadUrl = `${apiUrl}/jobruns/${jobRun.jobRunId}/artefacts/${filename}`;
    
    // Open the download URL in a new window/tab
    window.open(downloadUrl, '_blank');
  };

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '200px' }}>
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </div>
    );
  }

  if (error || !jobRun) {
    return (
      <Alert variant="danger">
        <Alert.Heading>Error</Alert.Heading>
        <p>{error || 'Job run not found'}</p>
        <Button variant="outline-danger" onClick={loadRunDetails}>
          Try Again
        </Button>
      </Alert>
    );
  }

  return (
    <div className="container-fluid">
      <div className="row">
        <div className="col-12">
          <h3>
            <Button variant="link" className="p-0 me-2" onClick={handleBack}>
              <i className="fas fa-angle-left"></i>
            </Button>
            <i className="fas fa-flag-checkered me-2"></i>
            Run #{jobRun.jobRunId}
            {jobRun.deleted && (
              <Badge bg="warning" className="ms-2">
                <i className="fas fa-trash me-1"></i>
                Deleted
              </Badge>
            )}
          </h3>
          <p className="text-muted">{jobRun.jobTitle || jobRun.jobName}</p>
        </div>
      </div>

      <Row>
        <Col lg={8}>
          <Card className="mb-4">
            <Card.Header>
              <h5 className="mb-0">Run Information</h5>
            </Card.Header>
            <Card.Body>
              <Row className="mb-2">
                <Col sm={3}><strong>Run ID:</strong></Col>
                <Col sm={9}>{jobRun.jobRunId}</Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>Job ID:</strong></Col>
                <Col sm={9}>
                  <Button variant="link" className="p-0" onClick={handleViewJob}>
                    {jobRun.jobId}
                  </Button>
                </Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>Job Name:</strong></Col>
                <Col sm={9}><strong>{jobRun.jobName}</strong></Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>State:</strong></Col>
                <Col sm={9}>
                  <Badge bg={getStatusBadgeVariant(jobRun.state)} className="fs-6">
                    {jobRun.state}
                  </Badge>
                </Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>Progress:</strong></Col>
                <Col sm={9}>
                  {jobRun.progress !== undefined && jobRun.progress !== null ? (
                    <div className="d-flex align-items-center">
                      <div className="progress me-2" style={{ width: '150px' }}>
                        <div 
                          className="progress-bar" 
                          role="progressbar" 
                          style={{ width: `${jobRun.progress}%` }}
                          aria-valuenow={jobRun.progress}
                          aria-valuemin={0}
                          aria-valuemax={100}
                        >
                          {jobRun.progress}%
                        </div>
                      </div>
                      <span>{jobRun.progress}%</span>
                    </div>
                  ) : '-'}
                </Col>
              </Row>
              {jobRun.pid && (
                <Row className="mb-2">
                  <Col sm={3}><strong>Process ID:</strong></Col>
                  <Col sm={9}>{jobRun.pid}</Col>
                </Row>
              )}
              {(jobRun.userId || jobRun.userDisplayName) && (
                <Row className="mb-2">
                  <Col sm={3}><strong>User:</strong></Col>
                  <Col sm={9}>
                    {jobRun.userId && jobRun.userDisplayName ? (
                      <span>
                        {jobRun.userDisplayName} <span className="text-muted">({jobRun.userId})</span>
                      </span>
                    ) : (
                      <span>{jobRun.userId || jobRun.userDisplayName}</span>
                    )}
                  </Col>
                </Row>
              )}
            </Card.Body>
          </Card>

          <Card className="mb-4">
            <Card.Header>
              <h5 className="mb-0">
                <i className="fas fa-bolt me-2"></i>
                Trigger Information
              </h5>
            </Card.Header>
            <Card.Body>
              <Row className="mb-2">
                <Col sm={3}><strong>Trigger ID:</strong></Col>
                <Col sm={9}>{jobRun.triggerId}</Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>Trigger Type:</strong></Col>
                <Col sm={9}>
                  <Badge bg="info" className="fs-6">
                    {jobRun.triggerType}
                  </Badge>
                </Col>
              </Row>
              {jobRun.definition && jobRun.triggerType === 'Recurring' && (
                <Row className="mb-2">
                  <Col sm={3}><strong>Schedule:</strong></Col>
                  <Col sm={9}>
                    <div className="d-flex align-items-center">
                      <code className="me-2">{jobRun.definition}</code>
                      <a 
                        href={`https://crontab.guru/#${jobRun.definition.replace(/\s+/g, '_')}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="btn btn-outline-success btn-sm"
                      >
                        <i className="fas fa-external-link-alt me-1"></i>
                        Explain
                      </a>
                    </div>
                  </Col>
                </Row>
              )}
              {jobRun.comment && (
                <Row className="mb-2">
                  <Col sm={3}><strong>Comment:</strong></Col>
                  <Col sm={9}>{jobRun.comment}</Col>
                </Row>
              )}
            </Card.Body>
          </Card>

          <Card className="mb-4">
            <Card.Header>
              <h5 className="mb-0">Timing Information</h5>
            </Card.Header>
            <Card.Body>
              <Row className="mb-2">
                <Col sm={3}><strong>Planned Start:</strong></Col>
                <Col sm={9}>{formatDate(jobRun.plannedStartUtc)}</Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>Actual Start:</strong></Col>
                <Col sm={9}>{formatDate(jobRun.actualStartUtc)}</Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>Actual End:</strong></Col>
                <Col sm={9}>{formatDate(jobRun.actualEndUtc)}</Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>Estimated End:</strong></Col>
                <Col sm={9}>{formatDate(jobRun.estimatedEndtUtc)}</Col>
              </Row>
              <Row className="mb-2">
                <Col sm={3}><strong>Duration:</strong></Col>
                <Col sm={9}>{calculateDuration()}</Col>
              </Row>
            </Card.Body>
          </Card>

          {(jobRun.instanceParameter || jobRun.jobParameter || jobRun.resultParameter) && (
            <Card className="mb-4">
              <Card.Header>
                <h5 className="mb-0">Parameters</h5>
              </Card.Header>
              <Card.Body>
                {jobRun.instanceParameter && (
                  <Row className="mb-3">
                    <Col sm={3}><strong>Instance Parameter:</strong></Col>
                    <Col sm={9}>
                      <CodeBlock code={renderParameter(jobRun.instanceParameter)} />
                    </Col>
                  </Row>
                )}
                {jobRun.jobParameter && (
                  <Row className="mb-3">
                    <Col sm={3}><strong>Job Parameter:</strong></Col>
                    <Col sm={9}>
                      <CodeBlock code={renderParameter(jobRun.jobParameter)} />
                    </Col>
                  </Row>
                )}
                {jobRun.resultParameter && (
                  <Row className="mb-2">
                    <Col sm={3}><strong>Result Parameter:</strong></Col>
                    <Col sm={9}>
                      <CodeBlock code={renderParameter(jobRun.resultParameter)} />
                    </Col>
                  </Row>
                )}
              </Card.Body>
            </Card>
          )}

          {jobRun.artefacts && jobRun.artefacts.length > 0 && (
            <Card className="mb-4">
              <Card.Header>
                <h5 className="mb-0">
                  <i className="fas fa-hdd me-2"></i>
                  Artifacts ({jobRun.artefacts.length})
                </h5>
              </Card.Header>
              <Card.Body>
                <Table hover responsive>
                  <thead>
                    <tr>
                      <th>Filename</th>
                      <th>Size</th>
                      <th>Content Type</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {jobRun.artefacts.map((artifact, index) => (
                      <tr key={index}>
                        <td>
                          <Button
                            variant="link"
                            className="p-0 text-start"
                            onClick={() => handleDownloadArtifact(artifact.filename)}
                          >
                            <i className="fas fa-download me-1"></i>
                            {artifact.filename}
                          </Button>
                        </td>
                        <td>{formatFileSize(artifact.size)}</td>
                        <td>
                          <code className="small">{artifact.contentType}</code>
                        </td>
                        <td>
                          <Button
                            variant="outline-primary"
                            size="sm"
                            onClick={() => handleDownloadArtifact(artifact.filename)}
                          >
                            <i className="fas fa-download me-1"></i>
                            Download
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              </Card.Body>
            </Card>
          )}
        </Col>

        <Col lg={4}>
          <Card>
            <Card.Header>
              <h5 className="mb-0">Actions</h5>
            </Card.Header>
            <Card.Body className="d-grid gap-2">
              <Button variant="primary" onClick={handleViewJob}>
                <i className="fas fa-calendar-alt me-2"></i>
                View Job
              </Button>
              
              {jobRun.state === 'Failed' && !jobRun.deleted && (
                <ButtonGroup>
                  <Button variant="warning" onClick={handleRetryRun}>
                    <i className="fas fa-redo me-2"></i>
                    Retry
                  </Button>
                  <Dropdown as={ButtonGroup}>
                    <Dropdown.Toggle split variant="warning" />
                    <Dropdown.Menu>
                      <Dropdown.Item onClick={handleDeleteRun} className="text-danger">
                        <i className="fas fa-trash me-2"></i>
                        Delete
                      </Dropdown.Item>
                    </Dropdown.Menu>
                  </Dropdown>
                </ButtonGroup>
              )}
              
              {jobRun.state !== 'Failed' && !jobRun.deleted && (
                <Button variant="primary" onClick={handleRefresh}>
                  <i className="fas fa-sync-alt me-2"></i>
                  Refresh
                </Button>
              )}
              
              <Button variant="outline-secondary" onClick={handleViewAllRuns}>
                <i className="fas fa-list me-2"></i>
                View All Runs
              </Button>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default RunDetail;