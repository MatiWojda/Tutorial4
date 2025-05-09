namespace Tutorial3Tests;

public class AdvancedEmpDeptTests
{
    // 11. MAX salary
    // SQL: SELECT MAX(Sal) FROM Emp;
    [Fact]
    public void ShouldReturnMaxSalary()
    {
        var emps = Database.GetEmps();

        decimal? maxSalary = emps.Max(e => e.Sal); 

        Assert.Equal(5000, maxSalary);
    }

    // 12. MIN salary in department 30
    // SQL: SELECT MIN(Sal) FROM Emp WHERE DeptNo = 30;
    [Fact]
    public void ShouldReturnMinSalaryInDept30()
    {
        var emps = Database.GetEmps();

        decimal? minSalary = emps.Where(e => e.DeptNo == 30)
            .Min(e => e.Sal);

        Assert.Equal(1250, minSalary);
    }

    // 13. Take first 2 employees ordered by hire date
    // SQL: SELECT * FROM Emp ORDER BY HireDate ASC FETCH FIRST 2 ROWS ONLY;
    [Fact]
    public void ShouldReturnFirstTwoHiredEmployees()
    {
        var emps = Database.GetEmps();

        var firstTwo = emps.OrderBy(e => e.HireDate)
        .Take(2)
            .ToList();
        
        Assert.Equal(2, firstTwo.Count);
        Assert.Equal("SMITH", firstTwo[0].EName);
        Assert.Equal("ALLEN", firstTwo[1].EName);
        Assert.True(firstTwo[0].HireDate <= firstTwo[1].HireDate);
    }

    // 14. DISTINCT job titles
    // SQL: SELECT DISTINCT Job FROM Emp;
    [Fact]
    public void ShouldReturnDistinctJobTitles()
    {
        var emps = Database.GetEmps();

        var jobs = emps.Select(e => e.Job)
        .Distinct()
            .ToList();
        
        Assert.Equal(3, jobs.Count);
        Assert.Contains("PRESIDENT", jobs);
        Assert.Contains("SALESMAN", jobs);
        Assert.Contains("CLERK", jobs);
    }

    // 15. Employees with managers (NOT NULL Mgr)
    // SQL: SELECT * FROM Emp WHERE Mgr IS NOT NULL;
    [Fact]
    public void ShouldReturnEmployeesWithManagers()
    {
        var emps = Database.GetEmps();

        var withMgr = emps.Where(e => e.Mgr.HasValue)
            .ToList();
        
        Assert.Equal(4, withMgr.Count);
        Assert.All(withMgr, e => Assert.NotNull(e.Mgr));
        Assert.DoesNotContain(withMgr, e => e.EName == "KING");
    }

    // 16. All employees earn more than 500
    // SQL: SELECT * FROM Emp WHERE Sal > 500; (simulate all check)
    [Fact]
    public void AllEmployeesShouldEarnMoreThan500()
    {
        var emps = Database.GetEmps();

        var result = emps.All(e => e.Sal > 500);
        
        Assert.True(result);
    }

    // 17. Any employee with commission over 400
    // SQL: SELECT * FROM Emp WHERE Comm > 400;
    [Fact]
    public void ShouldFindAnyWithCommissionOver400()
    {
        var emps = Database.GetEmps();

        var result = emps.Any(e => e.Comm.HasValue && e.Comm.Value > 400);
        
        Assert.True(result);
    }

    // 18. Self-join to get employee-manager pairs
    // SQL: SELECT E1.EName AS Emp, E2.EName AS Manager FROM Emp E1 JOIN Emp E2 ON E1.Mgr = E2.EmpNo;
    [Fact]
    public void ShouldReturnEmployeeManagerPairs()
    {
        var emps = Database.GetEmps();

        var result = emps
            .Where(e1 => e1.Mgr.HasValue)
            .Join(emps,
                e1 => e1.Mgr.Value,
                e2 => e2.EmpNo,
                (e1, e2) => new
                {
                    Employee = e1.EName,
                    Manager = e2.EName
                })
            .ToList();
        
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Employee == "SMITH" && r.Manager == "FORD");
        Assert.Contains(result, r => r.Employee == "FORD" && r.Manager == "KING");
    }

    // 19. Let clause usage (sal + comm)
    // SQL: SELECT EName, (Sal + COALESCE(Comm, 0)) AS TotalIncome FROM Emp;
    [Fact]
    public void ShouldReturnTotalIncomeIncludingCommission()
    {
        var emps = Database.GetEmps();

        var result = emps
            .Select(e => new 
            {
                e.EName,
                Total = e.Sal + (e.Comm ?? 0M)
            })
            .ToList();

        Assert.Equal(5, result.Count);
        
        Assert.Contains(result, r => r.EName == "ALLEN" && r.Total == 1900);
        Assert.Contains(result, r => r.EName == "WARD" && r.Total == 1750);
        Assert.Contains(result, r => r.EName == "SMITH" && r.Total == 800);
        Assert.Contains(result, r => r.EName == "KING" && r.Total == 5000);
    }

    // 20. Join all three: Emp → Dept → Salgrade
    // SQL: SELECT E.EName, D.DName, S.Grade FROM Emp E JOIN Dept D ON E.DeptNo = D.DeptNo JOIN Salgrade S ON E.Sal BETWEEN S.Losal AND S.Hisal;
    [Fact]
    public void ShouldJoinEmpDeptSalgrade()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();
        var grades = Database.GetSalgrades();

        var result = emps
            .Join(depts,
                emp => emp.DeptNo,
                dept => dept.DeptNo,
                (emp, dept) => new { Emp = emp, Dept = dept })
            .SelectMany(empDept => grades
                .Where(sg => empDept.Emp.Sal >= sg.Losal && empDept.Emp.Sal <= sg.Hisal)
                .Select(sg => new
                {
                    EName = empDept.Emp.EName,
                    DName = empDept.Dept.DName,
                    Grade = sg.Grade
                }))
            .ToList();
        
        Assert.Equal(5, result.Count);
        
        Assert.Contains(result, r => r.EName == "ALLEN" && r.DName == "SALES" && r.Grade == 3);
        Assert.Contains(result, r => r.EName == "SMITH" && r.DName == "RESEARCH" && r.Grade == 1);
        Assert.Contains(result, r => r.EName == "WARD" && r.DName == "SALES" && r.Grade == 2);
        Assert.Contains(result, r => r.EName == "KING" && r.DName == "ACCOUNTING" && r.Grade == 5);
        Assert.Contains(result, r => r.EName == "FORD" && r.DName == "ACCOUNTING" && r.Grade == 5);
    }
}
