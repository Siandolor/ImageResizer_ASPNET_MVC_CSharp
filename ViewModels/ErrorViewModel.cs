namespace ImageResizer.Models
{
    // ==========================================================
    //  ERROR VIEW MODEL
    //  Provides information for displaying error details in the UI.
    //  Commonly used by the global exception handler to show
    //  request identifiers for debugging or user-facing error pages.
    // ==========================================================
    public class ErrorViewModel
    {
        // Unique identifier of the HTTP request that caused the error.
        public string? RequestId { get; set; }

        // Indicates whether a valid RequestId should be displayed.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
