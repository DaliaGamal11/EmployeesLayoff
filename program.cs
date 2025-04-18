namespace part2 //Events 
{
    public enum LayOffCause
    {
        VacationStockNegative,
        AgeGreaterThan60,
        FailedToMeetTarget,
        Resigned
    }
    public class EmployeeLayOffEventArgs : EventArgs
    {
        public LayOffCause Cause { get; set; }

        public EmployeeLayOffEventArgs(LayOffCause cause)
        {
            Cause = cause;
        }
    }
    class Employee
    {
        public event EventHandler<EmployeeLayOffEventArgs> EmployeeLayOff;

        protected virtual void OnEmployeeLayOff(EmployeeLayOffEventArgs e)
        {
            EmployeeLayOff?.Invoke(this, e);
        }

        public int EmployeeID { get; set; }
        public DateTime BirthDate { get; set; }
        public int VacationStock { get; set; }

        public int GetAge()
        {
            return DateTime.Now.Year - BirthDate.Year;
        }

        public bool RequestVacation(DateTime From, DateTime To)
        {
            int days = (To - From).Days;
            if (VacationStock - days < 0)
            {
                VacationStock -= days;
                OnEmployeeLayOff(new EmployeeLayOffEventArgs(LayOffCause.VacationStockNegative));
                return false;
            }

            VacationStock -= days;
            return true;
        }

        public virtual void EndOfYearOperation()
        {
            if (VacationStock < 0)
            {
                OnEmployeeLayOff(new EmployeeLayOffEventArgs(LayOffCause.VacationStockNegative));
            }
            else if (GetAge() > 60)
            {
                OnEmployeeLayOff(new EmployeeLayOffEventArgs(LayOffCause.AgeGreaterThan60));
            }
        }
    }
    class SalesPerson : Employee
    {
        public int AchievedTarget { get; set; }

        public bool CheckTarget(int Quota)
        {
            if (AchievedTarget < Quota)
            {
                OnEmployeeLayOff(new EmployeeLayOffEventArgs(LayOffCause.FailedToMeetTarget));
                return false;
            }
            return true;
        }

        // SalesPerson has no vacation stock checks
        public override void EndOfYearOperation()
        {
            if (GetAge() > 60)
            {
                OnEmployeeLayOff(new EmployeeLayOffEventArgs(LayOffCause.AgeGreaterThan60));
            }
        }
    }
    class BoardMember : Employee
    {
        public void Resign()
        {
            OnEmployeeLayOff(new EmployeeLayOffEventArgs(LayOffCause.Resigned));
        }

        public override void EndOfYearOperation()
        {
            // Not fired due to age or vacation.
        }
    }
    class Department
    {
        public int DeptID { get; set; }
        public string DeptName { get; set; }
        List<Employee> Staff = new List<Employee>();

        public void AddStaff(Employee E)
        {
            Staff.Add(E);
            E.EmployeeLayOff += RemoveStaff;
        }

        public void RemoveStaff(object sender, EmployeeLayOffEventArgs e)
        {
            Employee emp = sender as Employee;
            if (emp != null && Staff.Contains(emp))
            {
                Staff.Remove(emp);
                Console.WriteLine($"[Department] Removed Employee {emp.EmployeeID} due to {e.Cause}");
            }
        }
    }
    class Club
    {
        public int ClubID { get; set; }
        public string ClubName { get; set; }
        List<Employee> Members = new List<Employee>();

        public void AddMember(Employee E)
        {
            Members.Add(E);
            E.EmployeeLayOff += RemoveMember;
        }

        public void RemoveMember(object sender, EmployeeLayOffEventArgs e)
        {
            Employee emp = sender as Employee;

            // Board members should never be removed from the Club
            if (emp is BoardMember)
            {
                return;
            }

            if (e.Cause == LayOffCause.VacationStockNegative && Members.Contains(emp))
            {
                Members.Remove(emp);
                Console.WriteLine($"[Club] Removed Employee {emp.EmployeeID} due to Vacation Stock < 0");
            }
        }

    }



    internal class Program
    {
        static void Main(string[] args)
        {
            // Creating Employees
            Employee e1 = new Employee() { EmployeeID = 1, BirthDate = new DateTime(1955, 5, 15), VacationStock = 10 };
            Employee e2 = new Employee() { EmployeeID = 2, BirthDate = new DateTime(1980, 3, 20), VacationStock = -5 };
            SalesPerson sp1 = new SalesPerson() { EmployeeID = 3, BirthDate = new DateTime(1995, 7, 25), VacationStock = 20, AchievedTarget = 90 };
            SalesPerson sp2 = new SalesPerson() { EmployeeID = 4, BirthDate = new DateTime(1985, 10, 5), VacationStock = 10, AchievedTarget = 50 };
            BoardMember bm1 = new BoardMember() { EmployeeID = 5, BirthDate = new DateTime(1970, 2, 14), VacationStock = 0 };

            // Creating Department and Club
            Department dept = new Department() { DeptID = 101, DeptName = "Sales" };
            Club club = new Club() { ClubID = 1, ClubName = "Employee Club" };

            // Adding Employees to Department and Club
            dept.AddStaff(e1);
            dept.AddStaff(e2);
            dept.AddStaff(sp1);
            dept.AddStaff(sp2);
            dept.AddStaff(bm1);

            club.AddMember(e1);
            club.AddMember(e2);
            club.AddMember(sp1);
            club.AddMember(sp2);
            club.AddMember(bm1);

            // Triggering End-of-Year Operations
            Console.WriteLine("End of Year Operations :");
            e1.EndOfYearOperation(); 
            e2.EndOfYearOperation(); 
            sp1.EndOfYearOperation(); 
            sp2.EndOfYearOperation(); 
            bm1.EndOfYearOperation(); 

            //  Resignation
            bm1.Resign(); 
        
        }
    }
}
