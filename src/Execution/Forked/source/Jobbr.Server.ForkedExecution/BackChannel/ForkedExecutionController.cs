using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.Server.ForkedExecution.BackChannel.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Jobbr.Server.ForkedExecution.BackChannel
{
    /// <summary>
    /// Controller for controlling the execution of jobs.
    /// </summary>
    [ApiController]
    [Route("fex")]
    public class ForkedExecutionController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IJobRunInformationService _jobRunInformationService;
        private readonly IJobRunProgressChannel _progressChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForkedExecutionController"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="jobRunInformationService">Service for getting information about the job runs.</param>
        /// <param name="progressChannel">Channel for job run progress information.</param>
        public ForkedExecutionController(ILoggerFactory loggerFactory, IJobRunInformationService jobRunInformationService, IJobRunProgressChannel progressChannel)
        {
            _logger = loggerFactory.CreateLogger<ForkedExecutionController>();
            _jobRunInformationService = jobRunInformationService;
            _progressChannel = progressChannel;
        }

        /// <summary>
        /// Get information on job run based on ID.
        /// </summary>
        /// <param name="jobRunId">The target job run ID.</param>
        /// <returns>A result containing a <see cref="JobRunInfoDto"/> or 404 if not found.</returns>
        [HttpGet("jobrun/{jobRunId}")]
        public async Task<IActionResult> GetJobRunInfosAsync(long jobRunId)
        {
            _logger.LogDebug("ConsoleExecutor is requesting information about a job run with id '{jobRunId}'", jobRunId);

            var jobRun = _jobRunInformationService.GetByJobRunId(jobRunId);

            if (jobRun == null)
            {
                _logger.LogWarning("Job run information for job run with '{jobRunId}' cannot be found.", jobRunId);
                return NotFound();
            }

            var infoDto = new JobRunInfoDto
            {
                JobId = jobRun.JobId,
                TriggerId = jobRun.TriggerId,
                UserId = jobRun.UserId,
                UserDisplayName = jobRun.UserDisplayName,
                JobRunId = jobRunId,
                JobName = jobRun.UniqueName,
                JobType = jobRun.Type,
                JobParameter = jobRun.JobParameters != null ? JsonSerializer.Deserialize<object>(jobRun.JobParameters, DefaultJsonOptions.Options) : null,
                InstanceParameter = jobRun.InstanceParameters != null ? JsonSerializer.Deserialize<object>(jobRun.InstanceParameters) : null,
            };

            _logger.LogDebug("Returning job run information for job run '{jobRunId}'", jobRunId);

            return Ok(infoDto);
        }

        /// <summary>
        /// Updates job run state.
        /// </summary>
        /// <param name="jobRunId">The ID of the target job run.</param>
        /// <param name="dto">The payload for updating a job run with.</param>
        /// <returns>A result containing either 202(Accepted), 404(NotFound) or 400(BadRequest).</returns>
        [HttpPut("jobrun/{jobRunId}")]
        public async Task<IActionResult> PutJobRunUpdateAsync(long jobRunId, [FromBody] JobRunUpdateDto dto)
        {
            _logger.LogDebug("ConsoleExecutor is trying to update job with ID '{jobRunId}'", jobRunId);
            var jobRun = _jobRunInformationService.GetByJobRunId(jobRunId);

            if (jobRun == null)
            {
                _logger.LogWarning("Job run with ID '{jobRunId}' cannot be found.", jobRunId);
                return NotFound();
            }

            if (dto.State == JobRunStates.Null)
            {
                _logger.LogWarning("Job run with ID '{jobRunId}' sent an invalid state which defaulted to 'None' but still cannot be accepted.", jobRunId);
                return BadRequest("Invalid state");
            }

            _logger.LogInformation("Publishing state update '{state}' for job run ID '{jobRunId}'", dto.State, jobRunId);
            _progressChannel.PublishStatusUpdate(jobRun.Id, dto.State);

            return Accepted();
        }

        /// <summary>
        /// Adds an artifact to the job run.
        /// </summary>
        /// <param name="jobRunId">The target job run ID.</param>
        /// <returns>A result that contains NotFound(404) or Accepted(202).</returns>
        [HttpPost("jobrun/{jobRunId}/artefacts")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddArtefactsAsync(long jobRunId)
        {
            _logger.LogDebug("ConsoleExecutor is upload job artifacts for job with ID '{jobRunId}'", jobRunId);
            var jobRun = _jobRunInformationService.GetByJobRunId(jobRunId);

            if (jobRun == null)
            {
                _logger.LogWarning("Job run with ID '{jobRunId}' cannot be found.", jobRunId);
                return NotFound();
            }

            var request = HttpContext.Request;

            if (!request.HasFormContentType || !MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) || string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }

            var files = request.Form.Files;

            foreach (var file in files)
            {
                var result = file.OpenReadStream();

                var untrustedFileName = Path.GetFileName(file.FileName);

                _logger.LogInformation("Publishing job run artifact '{filename}' for JobRun (ID '{jobRunId}') with '{resultLength}' bytes", untrustedFileName, jobRun.Id, result.Length);
                _progressChannel.PublishArtefact(jobRun.Id, untrustedFileName, result);
            }

            return Accepted();
        }
    }
}
