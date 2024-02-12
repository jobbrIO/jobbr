namespace Jobbr.ComponentModel.Management.Model
{
    public interface IJobTrigger
    {
        long Id { get; set; }

        string Comment { get; set; }

        bool IsActive { get; set; }

        string Parameters { get; set; }

        string UserDisplayName { get; set; }

        string UserId { get; set; }

        long JobId { get; set; }

        bool Deleted { get; set; }
    }
}