﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Business.Interfaces
{
    public interface ILogDataOperations
    {
        Task CreateLogAsync(string category, string message);
        Task CreateExceptionLogAsync(string id, string message, string stackTrace);
        Task CreateUserActivityAsync(string email, string action);
    }
}
