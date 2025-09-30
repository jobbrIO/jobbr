import React, { useState, useEffect } from 'react';
import { Card, Row, Col, Form, Button, Spinner, Alert, Badge, ButtonGroup } from 'react-bootstrap';
import { useParams, useNavigate } from 'react-router-dom';
import { useApi } from '../context/ApiContext';
import { JobDto, JobTriggerDto, InstantTriggerDto, ScheduledTriggerDto, RecurringTriggerDto } from '../types';
import { TriggerType, isInstantTrigger, isScheduledTrigger, isRecurringTrigger, isValidTriggerType } from '../utils/typeGuards';

const TriggerEdit: React.FC = () => {
  const { apiClient } = useApi();
  const navigate = useNavigate();
  const { jobId, triggerId } = useParams<{ jobId: string; triggerId: string }>();
  
  const [job, setJob] = useState<JobDto | null>(null);
  const [trigger, setTrigger] = useState<JobTriggerDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<{[key: string]: string}>({});
  
  // Form state
  const [triggerType, setTriggerType] = useState<TriggerType>('Instant');
  const [isActive, setIsActive] = useState(true);
  const [comment, setComment] = useState('');
  const [parameters, setParameters] = useState('');
  const [userId, setUserId] = useState('');
  const [userDisplayName, setUserDisplayName] = useState('');
  
  // Type-specific form state
  const [delayedMinutes, setDelayedMinutes] = useState(0);
  const [startDateTime, setStartDateTime] = useState('');
  const [cronExpression, setCronExpression] = useState('');

  useEffect(() => {
    if (jobId && triggerId) {
      loadTriggerData();
    }
  }, [jobId, triggerId]);

  const loadTriggerData = async () => {
    if (!apiClient || !jobId || !triggerId) return;
    
    try {
      setLoading(true);
      setError(null);
      
      const [jobData, triggerData] = await Promise.all([
        apiClient.getJob(parseInt(jobId)),
        apiClient.getTrigger(parseInt(jobId), parseInt(triggerId))
      ]);
      
      setJob(jobData);
      setTrigger(triggerData);
      
      // Populate form with existing data
      if (isValidTriggerType(triggerData.triggerType)) {
        setTriggerType(triggerData.triggerType);
      } else {
        throw new Error(`Invalid trigger type: ${triggerData.triggerType}`);
      }
      setIsActive(triggerData.isActive);
      setComment(triggerData.comment || '');
      setParameters(
        typeof triggerData.parameters === 'string' 
          ? triggerData.parameters 
          : triggerData.parameters 
            ? JSON.stringify(triggerData.parameters, null, 2)
            : ''
      );
      setUserId(triggerData.userId || '');
      setUserDisplayName(triggerData.userDisplayName || '');
      
      // Type-specific data using type guards
      if (isInstantTrigger(triggerData)) {
        setDelayedMinutes(triggerData.delayedMinutes);
      }
      
      if (isScheduledTrigger(triggerData) || isRecurringTrigger(triggerData)) {
        if (triggerData.startDateTimeUtc) {
          // Convert UTC to local datetime-local format
          const date = new Date(triggerData.startDateTimeUtc);
          const localDateTime = new Date(date.getTime() - date.getTimezoneOffset() * 60000)
            .toISOString()
            .slice(0, 16);
          setStartDateTime(localDateTime);
        }
      }
      
      if (isRecurringTrigger(triggerData)) {
        setCronExpression(triggerData.definition);
      }
      
    } catch (err) {
      setError(`Failed to load trigger data: ${err instanceof Error ? err.message : String(err)}`);
    } finally {
      setLoading(false);
    }
  };

  const validateForm = (): boolean => {
    const errors: {[key: string]: string} = {};
    
    if (triggerType === 'Scheduled' && !startDateTime) {
      errors.startDateTime = 'Start date and time is required for scheduled triggers';
    }
    
    if (triggerType === 'Recurring' && !cronExpression.trim()) {
      errors.cronExpression = 'Cron expression is required for recurring triggers';
    }
    
    if (triggerType === 'Instant' && delayedMinutes < 0) {
      errors.delayedMinutes = 'Delayed minutes cannot be negative';
    }
    
    // Validate JSON parameters if provided
    if (parameters.trim()) {
      try {
        JSON.parse(parameters);
      } catch {
        errors.parameters = 'Parameters must be valid JSON';
      }
    }
    
    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSave = async () => {
    if (!validateForm() || !apiClient || !jobId || !trigger) return;
    
    try {
      setSaving(true);
      setError(null);
      
      let updatedTrigger: JobTriggerDto;
      
      // Use switch statement with exhaustive checking for better type safety
      switch (triggerType) {
        case 'Instant': {
          updatedTrigger = {
            ...trigger,
            triggerType: 'Instant',
            isActive,
            comment,
            parameters,
            userId,
            userDisplayName,
            delayedMinutes
          } as InstantTriggerDto;
          break;
        }
        case 'Scheduled': {
          const startDateTimeUtc = new Date(startDateTime).toISOString();
          updatedTrigger = {
            ...trigger,
            triggerType: 'Scheduled',
            isActive,
            comment,
            parameters,
            userId,
            userDisplayName,
            startDateTimeUtc
          } as ScheduledTriggerDto;
          break;
        }
        case 'Recurring': {
          const existingRecurring = isRecurringTrigger(trigger) ? trigger : {} as Partial<RecurringTriggerDto>;
          updatedTrigger = {
            ...trigger,
            triggerType: 'Recurring',
            isActive,
            comment,
            parameters,
            userId,
            userDisplayName,
            definition: cronExpression,
            startDateTimeUtc: existingRecurring.startDateTimeUtc,
            endDateTimeUtc: existingRecurring.endDateTimeUtc
          } as RecurringTriggerDto;
          break;
        }
        default: {
          // Exhaustive check - TypeScript will error if we miss a case
          const _exhaustiveCheck: never = triggerType;
          throw new Error(`Unhandled trigger type: ${_exhaustiveCheck}`);
        }
      }
      
      await apiClient.updateTrigger(updatedTrigger, parseInt(jobId));
      
      // Navigate back to job detail
      navigate(`/jobs/${jobId}`);
      
    } catch (err) {
      setError(`Failed to update trigger: ${err instanceof Error ? err.message : String(err)}`);
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    navigate(`/jobs/${jobId}`);
  };

  const validateCron = async () => {
    if (!cronExpression.trim() || !apiClient) return;
    
    try {
      const isValid = await apiClient.validateCron(cronExpression);
      if (!isValid) {
        setValidationErrors(prev => ({ ...prev, cronExpression: 'Invalid cron expression' }));
      } else {
        setValidationErrors(prev => {
          const newErrors = { ...prev };
          delete newErrors.cronExpression;
          return newErrors;
        });
      }
    } catch {
      setValidationErrors(prev => ({ ...prev, cronExpression: 'Failed to validate cron expression' }));
    }
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

  if (error || !job || !trigger) {
    return (
      <Alert variant="danger">
        <Alert.Heading>Error</Alert.Heading>
        <p>{error || 'Trigger not found'}</p>
        <Button variant="outline-danger" onClick={handleCancel}>
          Back to Job
        </Button>
      </Alert>
    );
  }

  return (
    <div className="container-fluid">
      <div className="row">
        <div className="col-12">
          <h3 className="mb-3">
            <i className="fas fa-edit me-2"></i>
            Edit Trigger for {job.title}
          </h3>
        </div>
      </div>

      <Row>
        <Col lg={8}>
          <Card>
            <Card.Header>
              <h5 className="mb-0">Trigger Configuration</h5>
            </Card.Header>
            <Card.Body>
              <Form>
                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Trigger Type</Form.Label>
                      <Form.Select 
                        value={triggerType} 
                        onChange={(e) => {
                          const value = e.target.value;
                          if (isValidTriggerType(value)) {
                            setTriggerType(value);
                          }
                        }}
                      >
                        <option value="Instant">Instant</option>
                        <option value="Scheduled">Scheduled</option>
                        <option value="Recurring">Recurring</option>
                      </Form.Select>
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Status</Form.Label>
                      <ButtonGroup className="d-block">
                        <Button 
                          variant={isActive ? 'success' : 'outline-success'}
                          onClick={() => setIsActive(true)}
                        >
                          Enabled
                        </Button>
                        <Button 
                          variant={!isActive ? 'danger' : 'outline-danger'}
                          onClick={() => setIsActive(false)}
                        >
                          Disabled
                        </Button>
                      </ButtonGroup>
                    </Form.Group>
                  </Col>
                </Row>

                {/* Type-specific fields */}
                {triggerType === 'Instant' && (
                  <Form.Group className="mb-3">
                    <Form.Label>Delay (Minutes)</Form.Label>
                    <Form.Control
                      type="number"
                      value={delayedMinutes}
                      onChange={(e) => setDelayedMinutes(parseInt(e.target.value) || 0)}
                      min="0"
                      isInvalid={!!validationErrors.delayedMinutes}
                    />
                    <Form.Control.Feedback type="invalid">
                      {validationErrors.delayedMinutes}
                    </Form.Control.Feedback>
                    <Form.Text className="text-muted">
                      Set to 0 for immediate execution
                    </Form.Text>
                  </Form.Group>
                )}

                {triggerType === 'Scheduled' && (
                  <Form.Group className="mb-3">
                    <Form.Label>Start Date & Time</Form.Label>
                    <Form.Control
                      type="datetime-local"
                      value={startDateTime}
                      onChange={(e) => setStartDateTime(e.target.value)}
                      isInvalid={!!validationErrors.startDateTime}
                    />
                    <Form.Control.Feedback type="invalid">
                      {validationErrors.startDateTime}
                    </Form.Control.Feedback>
                  </Form.Group>
                )}

                {triggerType === 'Recurring' && (
                  <Form.Group className="mb-3">
                    <Form.Label>Cron Expression</Form.Label>
                    <div className="d-flex gap-2">
                      <Form.Control
                        type="text"
                        value={cronExpression}
                        onChange={(e) => setCronExpression(e.target.value)}
                        onBlur={validateCron}
                        placeholder="* * * * *"
                        isInvalid={!!validationErrors.cronExpression}
                      />
                      <Button 
                        variant="outline-info" 
                        size="sm"
                        onClick={() => window.open(`https://crontab.guru/#${encodeURIComponent(cronExpression.replace(/\s+/g, '_'))}`, '_blank')}
                        disabled={!cronExpression.trim()}
                      >
                        <i className="fas fa-external-link-alt"></i>
                      </Button>
                    </div>
                    <Form.Control.Feedback type="invalid">
                      {validationErrors.cronExpression}
                    </Form.Control.Feedback>
                    <Form.Text className="text-muted">
                      Use cron format (minute hour day month day-of-week)
                    </Form.Text>
                  </Form.Group>
                )}

                <Form.Group className="mb-3">
                  <Form.Label>Comment</Form.Label>
                  <Form.Control
                    type="text"
                    value={comment}
                    onChange={(e) => setComment(e.target.value)}
                    placeholder="Enter a comment describing this trigger"
                  />
                </Form.Group>

                <Row>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>User ID</Form.Label>
                      <Form.Control
                        type="text"
                        value={userId}
                        onChange={(e) => setUserId(e.target.value)}
                        placeholder="Optional user ID"
                      />
                    </Form.Group>
                  </Col>
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>User Display Name</Form.Label>
                      <Form.Control
                        type="text"
                        value={userDisplayName}
                        onChange={(e) => setUserDisplayName(e.target.value)}
                        placeholder="Optional display name"
                      />
                    </Form.Group>
                  </Col>
                </Row>

                <Form.Group className="mb-3">
                  <Form.Label>Parameters (JSON)</Form.Label>
                  <Form.Control
                    as="textarea"
                    rows={4}
                    value={parameters}
                    onChange={(e) => setParameters(e.target.value)}
                    placeholder='{"key": "value"}'
                    isInvalid={!!validationErrors.parameters}
                  />
                  <Form.Control.Feedback type="invalid">
                    {validationErrors.parameters}
                  </Form.Control.Feedback>
                  <Form.Text className="text-muted">
                    Optional JSON parameters to pass to the job
                  </Form.Text>
                </Form.Group>
              </Form>
            </Card.Body>
          </Card>
        </Col>

        <Col lg={4}>
          <Card>
            <Card.Header>
              <h5 className="mb-0">Current Trigger</h5>
            </Card.Header>
            <Card.Body>
              <Row className="mb-2">
                <Col sm={4}><strong>ID:</strong></Col>
                <Col sm={8}>{trigger.id}</Col>
              </Row>
              <Row className="mb-2">
                <Col sm={4}><strong>Type:</strong></Col>
                <Col sm={8}>
                  <Badge bg="info">{trigger.triggerType}</Badge>
                </Col>
              </Row>
              <Row className="mb-2">
                <Col sm={4}><strong>State:</strong></Col>
                <Col sm={8}>
                  <Badge bg={trigger.isActive ? 'success' : 'danger'}>
                    {trigger.isActive ? 'Active' : 'Inactive'}
                  </Badge>
                </Col>
              </Row>
            </Card.Body>
          </Card>

          <Card className="mt-3">
            <Card.Header>
              <h5 className="mb-0">Actions</h5>
            </Card.Header>
            <Card.Body className="d-grid gap-2">
              <Button 
                variant="primary" 
                onClick={handleSave}
                disabled={saving}
              >
                {saving ? (
                  <>
                    <Spinner animation="border" size="sm" className="me-2" />
                    Saving...
                  </>
                ) : (
                  <>
                    <i className="fas fa-save me-2"></i>
                    Save Changes
                  </>
                )}
              </Button>
              <Button 
                variant="secondary" 
                onClick={handleCancel}
                disabled={saving}
              >
                <i className="fas fa-times me-2"></i>
                Cancel
              </Button>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default TriggerEdit;