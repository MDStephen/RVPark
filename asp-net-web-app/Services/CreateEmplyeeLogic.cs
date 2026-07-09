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
			_db.Employees.Add(employee);
			_db.SaveChanges();
		}
	}
}
