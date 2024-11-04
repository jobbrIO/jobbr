using Jobbr.ComponentModel.Execution.Model;

namespace Jobbr.ComponentModel.Execution
{
    public interface IJobRunInformationService
    {
        JobRunInfo GetByJobRunId(long jobRunId);
    }
}
