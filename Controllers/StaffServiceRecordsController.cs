using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiStock.Application.Dtos.Common;
using VehiStock.Application.Dtos.Staff;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Domain.Constants;
using VehiStock.Entities;
using System.Security.Claims;

namespace VehiStock.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Staff)]
[Route("api/staff/service-records")]
public class StaffServiceRecordsController : ControllerBase
{
    private readonly IServiceRecordService _serviceRecordService;
    public StaffServiceRecordsController(
        IServiceRecordService serviceRecordService)
    {
        _serviceRecordService = serviceRecordService;
    }


}
