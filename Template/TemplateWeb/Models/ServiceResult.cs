namespace TemplateWeb.Models;

public class ServiceResult
{
    public bool Success { get; protected set; }
    public string Message { get; protected set; } = string.Empty;

    public static ServiceResult Ok() => new ServiceResult { Success = true };
    public static ServiceResult Failure(string message) => new ServiceResult { Success = false, Message = message };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private set; }

    public static ServiceResult<T> Ok(T data) => new ServiceResult<T> { Success = true, Data = data };
    public new static ServiceResult<T> Failure(string message) => new ServiceResult<T> { Success = false, Message = message };
}
