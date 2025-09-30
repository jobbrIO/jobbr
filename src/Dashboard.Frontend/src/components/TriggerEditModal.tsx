import { useState, useEffect } from 'react';
import { Modal, Row, Col, Spinner, Alert, Button, Form, ButtonGroup } from 'react-bootstrap';
import { useTrigger, useUpdateTrigger } from '../hooks';
import { renderParameter } from '../utils/formatters';
import { TriggerType, isValidTriggerType, isInstantTrigger, isScheduledTrigger, isRecurringTrigger } from '../utils/typeGuards';

interface TriggerEditModalProps {
  show: boolean;
  onHide: () => void;
  jobId: number;
  triggerId: number | null;
  onTriggerUpdated: () => void;
}

const TriggerEditModal = ({ 
  show, 
  onHide, 
  jobId, 
  triggerId,
  onTriggerUpdated 
}: TriggerEditModalProps) => {
  // Use TanStack Query hooks
  const { data: trigger, isLoading: triggerLoading, error: triggerError } = useTrigger(jobId, triggerId);
  const updateTriggerMutation = useUpdateTrigger();
  
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const loading = triggerLoading;
  
  // Form state
  const [triggerType, setTriggerType] = useState<TriggerType>('Instant');
  const [isActive, setIsActive] = useState<boolean>(true);
  const [comment, setComment] = useState<string>('');
  const [userId, setUserId] = useState<string>('');
  const [userDisplayName, setUserDisplayName] = useState<string>('');
  const [definition, setDefinition] = useState<string>('');
  const [startDateTimeUtc, setStartDateTimeUtc] = useState<string>('');
  const [endDateTimeUtc, setEndDateTimeUtc] = useState<string>('');
  const [delayedMinutes, setDelayedMinutes] = useState<number>(0);
  const [parameters, setParameters] = useState<string>('{}');

  // Populate form when trigger data loads
  useEffect(() => {
    if (trigger) {
      // Populate form fields
      if (isValidTriggerType(trigger.triggerType)) {
        setTriggerType(trigger.triggerType);
      }
      setIsActive(trigger.isActive);
      setComment(trigger.comment || '');
      setUserId(trigger.userId || '');
      setUserDisplayName(trigger.userDisplayName || '');
      setParameters(trigger.parameters ? renderParameter(trigger.parameters) : '{}');
      
      // Type-specific fields using type guards
      if (isRecurringTrigger(trigger)) {
        setDefinition(trigger.definition || '');
        setStartDateTimeUtc(trigger.startDateTimeUtc ? trigger.startDateTimeUtc.slice(0, 16) : '');
        setEndDateTimeUtc(trigger.endDateTimeUtc ? trigger.endDateTimeUtc.slice(0, 16) : '');
      } else if (isScheduledTrigger(trigger)) {
        setStartDateTimeUtc(trigger.startDateTimeUtc ? trigger.startDateTimeUtc.slice(0, 16) : '');
      } else if (isInstantTrigger(trigger)) {
        setDelayedMinutes(trigger.delayedMinutes || 0);
      }
    }
  }, [trigger]);

  const resetForm = () => {
    setTriggerType('Instant');
    setIsActive(true);
    setComment('');
    setUserId('');
    setUserDisplayName('');
    setDefinition('');
    setStartDateTimeUtc('');
    setEndDateTimeUtc('');
    setDelayedMinutes(0);
    setParameters('{}');
    setError(null);
  };

  const handleClose = () => {
    resetForm();
    onHide();
  };

  const handleSave = async () => {
    if (!trigger || !triggerId) return;

    try {
      setSaving(true);
      setError(null);

      // Basic validation
      if (triggerType === 'Recurring' && !definition.trim()) {
        setError('Cron expression is required for recurring triggers');
        return;
      }

      if (triggerType === 'Scheduled' && !startDateTimeUtc) {
        setError('Start date/time is required');
        return;
      }

      // Parse parameters JSON
      let parsedParameters = null;
      if (parameters.trim()) {
        try {
          parsedParameters = JSON.parse(parameters);
        } catch {
          setError('Invalid JSON in parameters');
          return;
        }
      }

      let updatedTrigger: any; // Will be a specific trigger type

      const baseProps = {
        id: trigger.id,
        isActive,
        comment: comment.trim(),
        userId: userId.trim() || undefined,
        userDisplayName: userDisplayName.trim() || undefined,
        parameters: parsedParameters,
      };

      if (triggerType === 'Instant') {
        updatedTrigger = {
          triggerType: 'Instant' as const,
          ...baseProps,
          delayedMinutes,
        };
      } else if (triggerType === 'Scheduled') {
        updatedTrigger = {
          triggerType: 'Scheduled' as const,
          ...baseProps,
          startDateTimeUtc: startDateTimeUtc + ':00',
        };
      } else if (triggerType === 'Recurring') {
        updatedTrigger = {
          triggerType: 'Recurring' as const,
          ...baseProps,
          definition: definition.trim(),
          startDateTimeUtc: startDateTimeUtc ? startDateTimeUtc + ':00' : undefined,
          endDateTimeUtc: endDateTimeUtc ? endDateTimeUtc + ':00' : undefined,
        };
      } else {
        setError('Invalid trigger type');
        return;
      }

      await updateTriggerMutation.mutateAsync({ trigger: updatedTrigger, jobId });
      
      // Notify parent and close modal
      onTriggerUpdated();
      handleClose();
    } catch (err) {
      setError(`Failed to update trigger${err instanceof Error ? `: ${err.message}` : ''}`);
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal show={show} onHide={handleClose} size="lg">
      <Modal.Header closeButton>
        <Modal.Title>
          <i className="fas fa-edit me-2"></i>
          Edit Trigger {trigger && `(ID: ${trigger.id})`}
        </Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {loading ? (
          <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '200px' }}>
            <Spinner animation="border" role="status">
              <span className="visually-hidden">Loading...</span>
            </Spinner>
          </div>
        ) : error && !trigger ? (
          <Alert variant="danger">
            <Alert.Heading>Error</Alert.Heading>
            <p>{error}</p>
            <Button variant="outline-danger" onClick={() => window.location.reload()}>
              Try Again
            </Button>
          </Alert>
        ) : !trigger ? (
          <Alert variant="warning">
            <Alert.Heading>Not Found</Alert.Heading>
            <p>Trigger not found</p>
          </Alert>
        ) : (
          <>
            {error && (
              <Alert variant="danger" className="mb-3">
                {error}
              </Alert>
            )}

            <Form>
              <Row>
                <Col md={6}>
                  <Form.Group className="mb-3">
                    <Form.Label>Type</Form.Label>
                    <ButtonGroup className="d-block">
                      <Button 
                        variant={triggerType === 'Instant' ? 'primary' : 'outline-primary'}
                        onClick={() => setTriggerType('Instant')}
                      >
                        Instant
                      </Button>
                      <Button 
                        variant={triggerType === 'Scheduled' ? 'primary' : 'outline-primary'}
                        onClick={() => setTriggerType('Scheduled')}
                      >
                        Scheduled
                      </Button>
                      <Button 
                        variant={triggerType === 'Recurring' ? 'primary' : 'outline-primary'}
                        onClick={() => setTriggerType('Recurring')}
                      >
                        Recurring
                      </Button>
                    </ButtonGroup>
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

              <Form.Group className="mb-3">
                <Form.Label>Comment</Form.Label>
                <Form.Control 
                  type="text" 
                  value={comment}
                  onChange={(e) => setComment(e.target.value)}
                  placeholder="Enter a comment for this trigger"
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
                      placeholder="Enter user ID (optional)"
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
                      placeholder="Enter user display name (optional)"
                    />
                  </Form.Group>
                </Col>
              </Row>

              {triggerType === 'Recurring' && (
                <Form.Group className="mb-3">
                  <Form.Label>Cron Expression *</Form.Label>
                  <div className="d-flex gap-2">
                    <Form.Control 
                      type="text" 
                      value={definition}
                      onChange={(e) => setDefinition(e.target.value)}
                      placeholder="0 0 12 * * ?"
                      required
                    />
                    <Button 
                      variant="outline-info" 
                      size="sm"
                      onClick={() => window.open(`https://crontab.guru/#${encodeURIComponent(definition.replace(/\s+/g, '_'))}`, '_blank')}
                      disabled={!definition.trim()}
                    >
                      <i className="fas fa-external-link-alt"></i>
                    </Button>
                  </div>
                  <Form.Text className="text-muted">
                    Enter a valid cron expression (e.g., "0 0 12 * * ?" for daily at noon)
                  </Form.Text>
                </Form.Group>
              )}

              <Row>
                {triggerType !== 'Instant' && (
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Start Date/Time {triggerType === 'Scheduled' ? '*' : ''}</Form.Label>
                      <div className="d-flex">
                        <Form.Control 
                          type="datetime-local" 
                          value={startDateTimeUtc}
                          onChange={(e) => setStartDateTimeUtc(e.target.value)}
                          required={triggerType === 'Scheduled'}
                        />
                        {triggerType === 'Recurring' && startDateTimeUtc && (
                          <Button 
                            variant="outline-secondary" 
                            size="sm"
                            className="ms-2"
                            onClick={() => setStartDateTimeUtc('')}
                            title="Clear start date"
                          >
                            <i className="fas fa-times"></i>
                          </Button>
                        )}
                      </div>
                      {triggerType === 'Recurring' && (
                        <Form.Text className="text-muted">
                          Optional start date for recurring triggers
                        </Form.Text>
                      )}
                    </Form.Group>
                  </Col>
                )}
                {triggerType === 'Recurring' && (
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>End Date/Time</Form.Label>
                      <div className="d-flex">
                        <Form.Control 
                          type="datetime-local" 
                          value={endDateTimeUtc}
                          onChange={(e) => setEndDateTimeUtc(e.target.value)}
                        />
                        {endDateTimeUtc && (
                          <Button 
                            variant="outline-secondary" 
                            size="sm"
                            className="ms-2"
                            onClick={() => setEndDateTimeUtc('')}
                            title="Clear end date"
                          >
                            <i className="fas fa-times"></i>
                          </Button>
                        )}
                      </div>
                      <Form.Text className="text-muted">
                        Optional end date for recurring triggers
                      </Form.Text>
                    </Form.Group>
                  </Col>
                )}
                {triggerType === 'Instant' && (
                  <Col md={6}>
                    <Form.Group className="mb-3">
                      <Form.Label>Delay (minutes)</Form.Label>
                      <Form.Control 
                        type="number" 
                        min="0"
                        step="1"
                        value={delayedMinutes}
                        onChange={(e) => setDelayedMinutes(parseInt(e.target.value) || 0)}
                      />
                      <Form.Text className="text-muted">
                        Number of minutes to delay execution (0 = immediate)
                      </Form.Text>
                    </Form.Group>
                  </Col>
                )}
              </Row>

              <Form.Group className="mb-3">
                <Form.Label>
                  <i className="fas fa-wrench me-2"></i>
                  Trigger Parameters
                </Form.Label>
                <Form.Control 
                  as="textarea"
                  rows={4}
                  value={parameters}
                  onChange={(e) => setParameters(e.target.value)}
                  placeholder='{"key": "value"}'
                  style={{ fontFamily: 'monospace' }}
                />
                <Form.Text className="text-muted">
                  Optional JSON parameters for this trigger
                </Form.Text>
              </Form.Group>
            </Form>
          </>
        )}
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={handleClose} disabled={saving}>
          Cancel
        </Button>
        <Button 
          variant="primary" 
          onClick={handleSave}
          disabled={saving || loading || !trigger}
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
      </Modal.Footer>
    </Modal>
  );
};

export default TriggerEditModal;