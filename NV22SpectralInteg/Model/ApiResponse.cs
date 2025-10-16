using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV22SpectralInteg.Model;

public class ApiResponse<T>
{
    public bool isSucceed { get; set; }
    public string message { get; set; }

    public T data { get; set; }
}

// 2. A helper record for consistent result handling in your service layer
public record ApiResult<T>(bool Success, string Message, T Data);
