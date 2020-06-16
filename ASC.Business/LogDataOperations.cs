using System;
using System.Collections.Generic;
using System.Text;
using ASC.Models.Models;
using ASC.Business.Interfaces;
using System.Threading.Tasks;
using ASC.DataAccess.Interfaces;

namespace ASC.Business
{
    public class LogDataOperations : ILogDataOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public LogDataOperations(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public async Task CreateExceptionLogAsync(string id, string message, string stackTrace)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<ExceptionLog>().AddAsync(new ExceptionLog()
                {
                    RowKey = id,
                    PartitionKey = "Exception",
                    Message = message,
                    StackTrace = stackTrace
                });
                _unitOfWork.CommitTransaction();
            }
        }

        public async Task CreateLogAsync(string category, string message)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<Log>().AddAsync(new Log()
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = category,
                    Message = message
                });
                _unitOfWork.CommitTransaction();
            }
        }

        public async Task CreateUserActivityAsync(string email, string action)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<UserActivity>().AddAsync(new UserActivity()
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = email,
                    Action = action
                }
                );
                _unitOfWork.CommitTransaction();
            }
        }
    }
}
