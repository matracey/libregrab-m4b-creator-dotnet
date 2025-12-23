namespace LibreGrabM4BCreator.Core.Abstractions;

/// <summary>
/// Abstraction for logging and displaying messages to the user.
/// </summary>
public interface IUserInterface
{
    /// <summary>
    /// Displays an informational message.
    /// </summary>
    void DisplayInfo(string message);

    /// <summary>
    /// Displays a processing status message.
    /// </summary>
    void DisplayProcessingStatus(string message);
    
    /// <summary>
    /// Displays a success message.
    /// </summary>
    void DisplaySuccess(string message);
    
    /// <summary>
    /// Displays a warning message.
    /// </summary>
    void DisplayWarning(string message);
    
    /// <summary>
    /// Displays an error message.
    /// </summary>
    void DisplayError(string message);
}