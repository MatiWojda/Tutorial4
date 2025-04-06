using Tutorial3.Models;

public class EmpDeptSalgradeTests
{
    // 1. Simple WHERE filter
    // SQL: SELECT * FROM Emp WHERE Job = 'SALESMAN';
    [Fact]
    public void ShouldReturnAllSalesmen()
    {
        var emps = Database.GetEmps();

        List<Emp> result = emps.Where(e => e.Job == "SALESMAN").ToList();
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("SALESMAN", e.Job));
    }

    // 2. WHERE + OrderBy
    // SQL: SELECT * FROM Emp WHERE DeptNo = 30 ORDER BY Sal DESC;
    [Fact]
    public void ShouldReturnDept30EmpsOrderedBySalaryDesc()
    {
        var emps = Database.GetEmps();

        List<Emp> result = emps.Where(e => e.DeptNo == 30)
            .OrderByDescending(e => e.Sal)
            .ToList();
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("ALLEN", result[0].EName);
        Assert.Equal("WARD", result[1].EName);
        Assert.True(result[0].Sal >= result[1].Sal);
    }

    // 3. Subquery using LINQ (IN clause)
    // SQL: SELECT * FROM Emp WHERE DeptNo IN (SELECT DeptNo FROM Dept WHERE Loc = 'CHICAGO');
    [Fact]
    public void ShouldReturnEmployeesFromChicago()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();
        
        var chicagoDeptNos = depts.Where(d => d.Loc == "CHICAGO")
            .Select(d => d.DeptNo)
            .ToList();

        List<Emp> result = emps.Where(e => chicagoDeptNos.Contains(e.DeptNo))
            .ToList();
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal(30, e.DeptNo));
    }

    // 4. SELECT projection
    // SQL: SELECT EName, Sal FROM Emp;
    [Fact]
    public void ShouldSelectNamesAndSalaries()
    {
        var emps = Database.GetEmps();

        var result = emps.Select(e => new { e.EName, e.Sal }).ToList();
        
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        Assert.All(result, r => 
        {
            Assert.False(string.IsNullOrWhiteSpace(r.EName));
            Assert.True(r.Sal > 0); 
        });
        Assert.Contains(result, r => r.EName == "KING" && r.Sal == 5000);
    }

    // 5. JOIN Emp to Dept
    // SQL: SELECT E.EName, D.DName FROM Emp E JOIN Dept D ON E.DeptNo = D.DeptNo;
    [Fact]
    public void ShouldJoinEmployeesWithDepartments()
    {
        var emps = Database.GetEmps();
        var depts = Database.GetDepts();

        var result = emps.Join(depts,
                emp => emp.DeptNo,
                dept => dept.DeptNo,
                (emp, dept) => new
                {
                    emp.EName,
                    dept.DName
                })
            .ToList();
        
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        Assert.Contains(result, r => r.DName == "SALES" && r.EName == "ALLEN");
        Assert.Contains(result, r => r.DName == "RESEARCH" && r.EName == "SMITH");
        Assert.Contains(result, r => r.DName == "ACCOUNTING" && r.EName == "KING");
    }

    // 6. Group by DeptNo
    // SQL: SELECT DeptNo, COUNT(*) FROM Emp GROUP BY DeptNo;
    [Fact]
    public void ShouldCountEmployeesPerDepartment()
    {
        var emps = Database.GetEmps();

        var result = emps.GroupBy(e => e.DeptNo)
            .Select(g => new
            {
                DeptNo = g.Key,
                Count = g.Count()
            })
            .ToList();
        
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, g => g.DeptNo == 10 && g.Count == 2);
        Assert.Contains(result, g => g.DeptNo == 20 && g.Count == 1);
        Assert.Contains(result, g => g.DeptNo == 30 && g.Count == 2);
    }

    // 7. SelectMany (simulate flattening)
    // SQL: SELECT EName, Comm FROM Emp WHERE Comm IS NOT NULL;
    [Fact]
    public void ShouldReturnEmployeesWithCommission()
    {
        var emps = Database.GetEmps();

        var result = emps.Where(e => e.Comm.HasValue)
            .Select(e => new { e.EName, e.Comm })
            .ToList(); 
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.NotNull(r.Comm));
        Assert.Contains(result, r => r.EName == "ALLEN" && r.Comm == 300);
        Assert.Contains(result, r => r.EName == "WARD" && r.Comm == 500);
    }

    // 8. Join with Salgrade
    // SQL: SELECT E.EName, S.Grade FROM Emp E JOIN Salgrade S ON E.Sal BETWEEN S.Losal AND S.Hisal;
    [Fact]
    public void ShouldMatchEmployeeToSalaryGrade()
    {
        var emps = Database.GetEmps();
        var grades = Database.GetSalgrades();

        var result = emps
            .SelectMany(emp => grades
                .Where(sg => emp.Sal >= sg.Losal && emp.Sal <= sg.Hisal)
                .Select(sg => new
                {
                    EName = emp.EName,
                    Grade = sg.Grade
                }))
            .ToList();
        
        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        Assert.Contains(result, r => r.EName == "SMITH" && r.Grade == 1);
        Assert.Contains(result, r => r.EName == "ALLEN" && r.Grade == 3);
        Assert.Contains(result, r => r.EName == "WARD" && r.Grade == 2);
        Assert.Contains(result, r => r.EName == "KING" && r.Grade == 5);
        Assert.Contains(result, r => r.EName == "FORD" && r.Grade == 5);
    }

    // 9. Aggregation (AVG)
    // SQL: SELECT DeptNo, AVG(Sal) FROM Emp GROUP BY DeptNo;
    [Fact]
    public void ShouldCalculateAverageSalaryPerDept()
    {
        var emps = Database.GetEmps();

        var result = emps.GroupBy(e => e.DeptNo)
            .Select(g => new
            {
                DeptNo = g.Key,
                AvgSal = g.Average(e => e.Sal)
            })
            .ToList();
        
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, r => r.DeptNo == 10 && r.AvgSal == 5000);
        Assert.Contains(result, r => r.DeptNo == 20 && r.AvgSal == 800);
        Assert.Contains(result, r => r.DeptNo == 30 && r.AvgSal == 1425);
        Assert.Contains(result, r => r.DeptNo == 30 && r.AvgSal > 1000);
    }

    // 10. Complex filter with subquery and join
    // SQL: SELECT E.EName FROM Emp E WHERE E.Sal > (SELECT AVG(Sal) FROM Emp WHERE DeptNo = E.DeptNo);
    [Fact]
    public void ShouldReturnEmployeesEarningMoreThanDeptAverage()
    {
        var emps = Database.GetEmps();
        
        var avgSalariesByDept = emps.GroupBy(e => e.DeptNo)
            .Select(g => new
            {
                DeptNo = g.Key,
                AvgSal = g.Average(e => e.Sal)
            })
            .ToDictionary(x => x.DeptNo, x => x.AvgSal);
        
        var result = emps
            .Where(e => e.Sal > avgSalariesByDept[e.DeptNo])
            .Select(e => e.EName)
            .ToList();
        
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("ALLEN", result);
        Assert.DoesNotContain("SMITH", result);
        Assert.DoesNotContain("WARD", result);
        Assert.DoesNotContain("KING", result);
        Assert.DoesNotContain("FORD", result);
    }
}
