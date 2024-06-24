using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using SoftUni.Data;
using SoftUni.Models;
using System.Globalization;
using System.Net.Mime;
using System.Text;

namespace SoftUni;

public class StartUp
{
    public static void Main(string[] args)
    {
        SoftUniContext dbContext = new SoftUniContext();
        string employees = RemoveTown(dbContext);
        Console.WriteLine(employees);
    }

    // 03. Employees Full Information
    public static string GetEmployeesFullInformation(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();

        Employee[] employees = context.Employees.OrderBy(e => e.EmployeeId).ToArray();

        foreach (var employee in employees)
        {
            sb.AppendLine($"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:f2}");
        }

        return sb.ToString().TrimEnd();
    }

    // 04. Employees with Salary Over 50k
    public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();

        var employees = context.Employees
            .Where(e => e.Salary > 50_000)
            .OrderBy(e => e.FirstName)
            .ToArray();

        foreach (var employee in employees)
        {
            sb.AppendLine($"{employee.FirstName} - {employee.Salary:f2}");
        }

        return sb.ToString().TrimEnd();
    }

    // 05. Employees from Research and Development
    public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
    {
        StringBuilder sb = new StringBuilder();

        var employeesInRnD = context.Employees
            .Where(e => e.Department.Name == "Research and Development")
            .Select(e => new
            {
                e.FirstName,
                e.LastName,
                DepartmentName = e.Department.Name,
                e.Salary
            })
            .OrderBy(e => e.Salary)
            .ThenByDescending(e => e.FirstName)
            .ToArray();

        foreach (var employee in employeesInRnD)
        {
            sb.AppendLine($"{employee.FirstName} {employee.LastName} from Research and Development - ${employee.Salary:f2}");
        }

        return sb.ToString().TrimEnd();
    }

    // 06. Adding a New Address and Updating Employee
    public static string AddNewAddressToEmployee(SoftUniContext context)
    {
        Address address = new Address();
        address.AddressText = "Vitoshka 15";
        address.TownId = 4;

        context.Addresses.Add(address);
        context.SaveChanges();

        var searchedEmployee = context.Employees
            .Where(e => e.LastName == "Nakov")
            .FirstOrDefault();

        searchedEmployee.Address = address;
        context.SaveChanges();

        var employees = context.Employees
            .Select(e => new { e.AddressId, e.Address })
            .OrderByDescending(e => e.AddressId)
            .Take(10);

        StringBuilder sb = new StringBuilder();

        foreach (var employee in employees)
        {
            sb.AppendLine($"{employee.Address.AddressText}");
        }

        return sb.ToString().TrimEnd();
    }

    // 07. Employees and Projects
    public static string GetEmployeesInPeriod(SoftUniContext context)
    {
        var EmployeeInfo = context.Employees
        .Take(10)
        .Select(e => new
        {
            e.FirstName,
            e.LastName,
            ManagerFirstName = e.Manager.FirstName,
            ManagerLastName = e.Manager.LastName,
            Projects = e.EmployeesProjects.Where(ep => ep.Project.StartDate.Year >= 2001 & ep.Project.StartDate.Year <= 2003)
                .Select(ep => new
                {
                    ProjectName = ep.Project.Name,
                    StartDate = ep.Project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                    EndDate = ep.Project.EndDate != null
                        ? ep.Project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)
                        : "not finished"
                })
        })
        .ToList();

        StringBuilder sb = new StringBuilder();

        foreach (var e in EmployeeInfo)
        {
            sb.AppendLine($"{e.FirstName} {e.LastName} - Manager: {e.ManagerFirstName} {e.ManagerLastName}");

            if (e.Projects.Any())
            {
                sb.AppendLine(String.Join(Environment.NewLine, e.Projects
                    .Select(p => $"--{p.ProjectName} - {p.StartDate} - {p.EndDate}")));
            }
        }

        return sb.ToString().TrimEnd();
    }

    // 08. Addresses by Town
    public static string GetAddressesByTown(SoftUniContext context)
    {
        string[] addressesInfo = context.Addresses
           .OrderByDescending(a => a.Employees.Count)
           .ThenBy(a => a.Town!.Name)
           .ThenBy(a => a.AddressText)
           .Take(10)
           .Select(a => $"{a.AddressText}, {a.Town!.Name} - {a.Employees.Count} employees")
           .ToArray();

        return String.Join(Environment.NewLine, addressesInfo);
    }

    // 09. Employee 147
    public static string GetEmployee147(SoftUniContext context)
    {
        var employee = context.Employees.Where(e => e.EmployeeId == 147).Select(e => new
        {
            e.FirstName,
            e.LastName,
            e.JobTitle,
            Projects = e.EmployeesProjects.Select(p => new { p.Project.Name }).OrderBy(p => p.Name).ToArray()
        })
            .FirstOrDefault();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{employee!.FirstName} {employee!.LastName} - {employee!.JobTitle}");
        sb.Append(String.Join(Environment.NewLine, employee!.Projects.Select(p => p.Name)));

        return sb.ToString().TrimEnd();
    }

    // 10. Departments with More Than 5 Employees
    public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
    {
        var departmentsInfo = context.Departments
            .Where(d => d.Employees.Count > 5)
            .OrderBy(d => d.Employees.Count)
            .ThenBy(d => d.Name)
            .Select(d => new
            {
                DepartmentName = d.Name,
                ManagerName = d.Manager.FirstName + " " + d.Manager.LastName,
                Employees = d.Employees
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .Select(e => new
                    {
                        EmployeeData = $"{e.FirstName} {e.LastName} - {e.JobTitle}"
                    })
                    .ToArray()
            })
            .ToArray();

        StringBuilder sb = new StringBuilder();
        foreach (var d in departmentsInfo)
        {
            sb.AppendLine($"{d.DepartmentName} - {d.ManagerName}");
            sb.Append(String.Join(Environment.NewLine, d.Employees.Select(e => e.EmployeeData)));
        }

        return sb.ToString().TrimEnd();
    }

    // 11. Find Latest 10 Projects
    public static string GetLatestProjects(SoftUniContext context)
    {
        var projectsInfo = context.Projects
           .OrderByDescending(p => p.StartDate)
           .Take(10)
           .OrderBy(p => p.Name)
           .Select(p => new
           {
               p.Name,
               p.Description,
               StartDate = p.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
           })
           .ToArray();

        StringBuilder sb = new StringBuilder();
        foreach (var p in projectsInfo)
        {
            sb.AppendLine(p.Name);
            sb.AppendLine(p.Description);
            sb.AppendLine(p.StartDate);
        }

        return sb.ToString().TrimEnd();
    }

    // 12. Increase Salaries
    public static string IncreaseSalaries(SoftUniContext context)
    {
        decimal salaryModifier = 1.12m;
        string[] departmentNames = new string[] { "Engineering", "Tool Design", "Marketing", "Information Services" };

        var employeesForSalaryIncrease = context.Employees
            .Where(e => departmentNames.Contains(e.Department.Name))
            .ToArray();

        foreach (var e in employeesForSalaryIncrease)
        {
            e.Salary *= salaryModifier;
        }

        context.SaveChanges();

        string[] emplyeesInfoText = context.Employees
            .Where(e => departmentNames.Contains(e.Department.Name))
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .Select(e => $"{e.FirstName} {e.LastName} (${e.Salary:f2})")
            .ToArray();

        return String.Join(Environment.NewLine, emplyeesInfoText);
    }

    // 13. Find Employees by First Name Starting With Sa
    public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
    {
        string[] employeesInfoText = context.Employees
            .Where(e => e.FirstName.Substring(0, 2).ToLower() == "sa")
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .Select(e => $"{e.FirstName} {e.LastName} - {e.JobTitle} - (${e.Salary:f2})")
            .ToArray();

        return String.Join(Environment.NewLine, employeesInfoText);
    }

    // 14. Delete Project by Id
    public static string DeleteProjectById(SoftUniContext context)
    {
        var employeesProjectsToDelete = context.EmployeesProjects.Where(ep => ep.ProjectId == 2);
        context.EmployeesProjects.RemoveRange(employeesProjectsToDelete);

        var projectToDelete = context.Projects.Where(p => p.ProjectId == 2);
        context.Projects.RemoveRange(projectToDelete);

        context.SaveChanges();

        string[] projectsNames = context.Projects
            .Take(10)
            .Select(p => p.Name)
            .ToArray();

        return String.Join(Environment.NewLine, projectsNames);
    }

    // 15. Remove Town
    public static string RemoveTown(SoftUniContext context)
    {
        Town townToDelete = context.Towns
                .Where(t => t.Name == "Seattle")
                .FirstOrDefault();

        Address[] addressesToDelete = context.Addresses
            .Where(a => a.TownId == townToDelete.TownId)
            .ToArray();

        Employee[] employeesToRemoveAddressFrom = context.Employees
            .Where(e => addressesToDelete
            .Contains(e.Address))
            .ToArray();

        foreach (Employee e in employeesToRemoveAddressFrom)
        {
            e.AddressId = null;
        }

        context.Addresses.RemoveRange(addressesToDelete);
        context.Towns.Remove(townToDelete);
        context.SaveChanges();

        return $"{addressesToDelete.Count()} addresses in Seattle were deleted";
    }
}

