## Register Services
    public class Module {
        public void ConfigureServices(IServiceCollection services)
		{
			// Inline service Type
			services.RegisterApiService(typeof(DataApiService<>).MakeGenericType(typeof(Department)), "Departments");
			
			// Generic notation
			services.RegisterApiService<DataApiService<Department>>("Departments");
			
			// Without prefix
			services.RegisterApiService<AuthService>();
			
			// Delegate without DI Container
			CommandManager.Register(nameof(FeatureService.FeatureList), FeatureService.FeatureList);
			// OR
			CommandManager.Register("HelloWorld", (string? name) => "Hello " + name);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.Use(CommandMiddleware.InvokeAsync);
		}
    }

## Examples
### API Server
    public class DataApiService<T> {
		private List<T> data;
		
		public IEnumerable<T> List() => this.data;
		
		public T? Get(in id) => id < this.data.Count ? this.data[id] : null;
		
		public int Add(T entity, int? id) {
			if (id > 0){
				this.data[id] = entity;
			} else {
				this.data.Add(entity);
			}
			
			return this.data.Count;
		}
    }
	
    public class AuthService {
		public bool Login(string login, string pass) => true;
		
		public bool Logout() => true;
    }
	
    public class FeatureService {
		public string FeatureList() => new string[] { "RegisterApiService", "Register Delegate", "LightWeight Endpoint" }
    }
### JS Client
1. GET

		fetch('DepartmentsList')
	Result:

		{
			success: true,
			result: [{
				//Department
			},...]
		}
	
2. GET + query

		fetch('DepartmentsGet?id=1')
	Result:

		{
			success: true,
			result: {
				//Department
			}
		}
	
3. POST + query

		fetch('DepartmentsSave?id=1', {
				headers: {
					'Content-Type': 'application/json'
				},
				body: JSON.stringify({
					//Department
				}),
				method: 'POST',
				credentials: 'include'
			})
	Result:

		{
			success: true,
			result: 1
		}
4. GET without prefix

		fetch('FeatureList')
	Result:

		{
			success: true,
			result: [ "RegisterApiService", "Register Delegate", "LightWeight Endpoint" ]
		}
5. ERROR

		fetch('DepartmentsSave?id=999')
	Result:

		{
			success: false,
			// Exception details
		}