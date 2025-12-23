namespace LibreGrabM4BCreator.Core.Abstractions;

/// <summary>
/// Abstraction for displaying progress during long-running operations.
/// </summary>
public interface IProgressReporter
{
    /// <summary>
    /// Executes a task with progress reporting.
    /// </summary>
    /// <param name="taskDescription">Description of the task being performed</param>
    /// <param name="operation">The operation to perform, receiving a progress callback</param>
    /// <returns>The result of the operation</returns>
    Task<bool> ExecuteWithProgressAsync(string taskDescription, Func<Action<double>, Task<bool>> operation);
}