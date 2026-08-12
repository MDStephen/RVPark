using asp_net_web_app.Data;

namespace asp_net_web_app.Services
{
	public class CreateEmployeeLogic
	{
		private readonly DatabaseWrapper _db;

		public CreateEmployeeLogic(DatabaseWrapper db)
		{
			_db = db;
		}

		public void CreateEmployee(Employee employee)
		{
			// Set authoritatively here rather than trusting whatever came in on the
			// bound model - CreatedAt in particular must never be caller-supplied.
			var now = DateTime.Now;
			employee.CreatedAt = now;
			employee.LastModifiedAt = now;

			_db.Employees.Add(employee);
			_db.SaveChanges();
		}
	}
}
