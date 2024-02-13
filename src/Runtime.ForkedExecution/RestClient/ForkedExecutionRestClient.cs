using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Jobbr.Runtime.ForkedExecution.RestClient
{
    /// <summary>
    /// The Jobbr runtime client.
    /// </summary>
    public class ForkedExecutionRestClient : IDisposable
    {
        private readonly long _jobRunId;

        private HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForkedExecutionRestClient"/> class.
        /// </summary>
        /// <param name="jobServer">Job server address.</param>
        /// <param name="jobRunId">ID for the job run.</param>
        public ForkedExecutionRestClient(string jobServer, long jobRunId)
        {
            _jobRunId = jobRunId;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(jobServer + (jobServer.EndsWith("/") ? string.Empty : "/") + "fex/")
            };
        }

        /// <summary>
        /// Publishes job run state.
        /// </summary>
        /// <param name="state">State to publish.</param>
        /// <returns>If state publish returned 202 (Accepted).</returns>
        public bool PublishState(JobRunStates state)
        {
            var url = $"jobRun/{_jobRunId}";
            var content = new JobRunUpdateDto
            {
                State = state
            };

            var serializedObject = JsonSerializer.Serialize(content, DefaultJsonOptions.Options);

            var request = _httpClient.PutAsync(url, new StringContent(serializedObject, Encoding.UTF8, "application/json"));
            var result = request.Result;

            return result.StatusCode == HttpStatusCode.Accepted;
        }

        /// <summary>
        /// Sends files to job run artifact endpoint.
        /// </summary>
        /// <param name="files">The files that are being sent.</param>
        /// <returns>If file send returned 202 (Accepted).</returns>
        public bool SendFiles(string[] files)
        {
            var multipartContent = new MultipartFormDataContent();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                multipartContent.Add(new StreamContent(File.OpenRead(file)), "result", fileName);
            }

            var url = $"jobRun/{_jobRunId}/artefacts";
            var response = _httpClient.PostAsync(url, multipartContent).Result;

            return response.StatusCode == HttpStatusCode.Accepted;
        }

        /// <summary>
        /// Gets job run info.
        /// </summary>
        /// <returns><see cref="JobRunInfoDto"/> containing info about the run.</returns>
        public JobRunInfoDto GetJobRunInfo()
        {
            var url = $"jobRun/{_jobRunId}";

            var request = _httpClient.GetAsync(url);
            var result = request.Result;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                var content = result.Content.ReadAsStringAsync().Result;

                var dto = JsonSerializer.Deserialize<JobRunInfoDto>(content, DefaultJsonOptions.Options);

                return dto;
            }

            return null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Conditional dispose.
        /// </summary>
        /// <param name="isDisposing">Condition for disposing.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (_httpClient != null)
                {
                    _httpClient.Dispose();
                    _httpClient = null;
                }
            }
        }
    }
}
