using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Collections.Generic;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{    
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectPlanningController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IStaffJobService _staffJobService;

        public ProjectPlanningController(IMapper mapper, IStaffJobService staffJobService)
        {
            _mapper = mapper;
            _staffJobService = staffJobService;
        }

        public async Task<IActionResult> Index()
        {

            var model = new Models.ProjectPlanningViewModel
            {
                ProjectCode = "FZ2000",
                ProjectDescription = "Salmonell Surveillance and control programme",
                SelectedProgramme = "Bact",
                SelectedYear = "2025-2026",
                UserName = "Ken Rod",
                BudgetCVL = 5000,
                StaffBookedGrid =  await GetStaffBookedDataGrid("FZ2000"),

                // AnimalsBookedList - Sample animal resources
                AnimalsBookedList = new List<AnimalBookedItem>
                {
                    new AnimalBookedItem {
                        Id = 1,
                        AnimalType = "Animal_001, Type_A",
                        Day = 10,
                        NoReq = 5,
                        DailyRt = 25.00m,
                        Cost = 1250.00m
                    },
                    new AnimalBookedItem {
                        Id = 2,
                        AnimalType = "Animal_002, Type_B",
                        Day = 15,
                        NoReq = 3,
                        DailyRt = 30.50m,
                        Cost = 1372.50m
                    },
                    new AnimalBookedItem {
                        Id = 3,
                        AnimalType = "Animal_003, Type_C",
                        Day = 7,
                        NoReq = 8,
                        DailyRt = 22.00m,
                        Cost = 1232.00m
                    }
                },

                // TestsBookedList - Sample laboratory tests
                TestsBookedList = new List<TestBookedItem>
                {
                    new TestBookedItem {
                        Id = 1,
                        Test = "TestCode_001",
                        Description = "Salmonella Detection Test",
                        ReCUP = 85.00m,
                        Num = 50,
                        AgrUP = 80.00m,
                        TestCost = 4000.00m
                    },
                    new TestBookedItem {
                        Id = 2,
                        Test = "TestCode_002",
                        Description = "Bacterial Culture Analysis",
                        ReCUP = 120.00m,
                        Num = 30,
                        AgrUP = 115.00m,
                        TestCost = 3450.00m
                    },
                    new TestBookedItem {
                        Id = 3,
                        Test = "TestCode_003",
                        Description = "PCR Diagnostic Test",
                        ReCUP = 150.00m,
                        Num = 25,
                        AgrUP = 145.00m,
                        TestCost = 3625.00m
                    }
                },
                
                 // ExceptionalCostsList - Sample exceptional expenses
                 ExceptionalCostsList = new List<ExceptionalCostItem>
                {
                    new ExceptionalCostItem {
                        Id = 1,
                        Description = "Laboratory Equipment Rental",
                        Account = "Account_001",
                        TotalCost = 2500.00m,
                        FreqOrMnth = "Monthly",
                        Supplier = "Scientific Equipment Ltd"
                    },
                    new ExceptionalCostItem {
                        Id = 2,
                        Description = "Specialized Reagents",
                        Account = "Account_002",
                        TotalCost = 1800.00m,
                        FreqOrMnth = "One-time",
                        Supplier = "BioSupply Co"
                    },
                    new ExceptionalCostItem {
                        Id = 3,
                        Description = "Transport and Logistics",
                        Account = "Account_003",
                        TotalCost = 950.00m,
                        FreqOrMnth = "Quarterly",
                        Supplier = "FastLog Services"
                    }
                }
            };
           
            return View(model);
        }

        private async Task<DataGridConfig<StaffJobItem>> GetStaffBookedDataGrid(string jobcode)
        {
            var staffJobPagedData = await _staffJobService.GetAllStaffJobsAsync(new QueryParameters<string>(), jobcode);
            List<StaffJobItem> staffJobItems = new List<StaffJobItem>();
            if (staffJobPagedData.Data != null)
            {
                staffJobItems = _mapper.Map<List<StaffJobItem>>(staffJobPagedData.Data.ToList());
            }
            PaginationModel paginationModel = _mapper.Map<PaginationModel>(staffJobPagedData.Pagination);            

            var staffJobGridConfig = new DataGridConfig<StaffJobItem>
            {
                GridId = "staffBookedGrid",
                Title = "Staff Booked",
                ShowCheckboxColumn = true,
                ShowPagination = true,
                KeyProperty = "StaffID",
                AddFunction = "addStaffJob",
                EditFunction = "editStaffJob",
                DeleteFunction = "deleteStaffJob",
                ExtraFilterMethod = "getStaffJobExtraFilters",
                BindGridUrl = "/FPS/StaffJob/LoadStaffJobGrid",                
                Data = staffJobItems,
                Columns = GridDataProvider.GetColumnsDefination<StaffJobItem>(null),
                Pagination = paginationModel
            };

            return staffJobGridConfig;
        }
    }
}
