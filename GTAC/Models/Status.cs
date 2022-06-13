using System.ComponentModel.DataAnnotations;

namespace GTAC.Models
{
    public enum Status
    {
        Pending,
        Approved,
        Reject,
        Done,
        [Display(Name = "Pending Request")]
        PendingRequest
    }

    public enum Area
    {
        Student,
        Quiz,
        Schedule,
        [Display(Name = "Request Reschedule")]
        RequestReschedule,
        Module,
        Certificate,
        [Display(Name = "Company User")]
        CompanyUser,
        [Display(Name = "Request Profile Change")]
        RequestProfileChange
    }

    public enum Action
    {
        Create,
        Edit,
        View,
        Delete,
        Download,
    }
}
