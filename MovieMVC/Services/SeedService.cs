namespace MovieMVC.Services
{
    public class SeedService
    {
        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedService(IOptions<AppSettings> appSettings, ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task ManageDataAsync()
        {
            await UpdateDatabaseAsync();
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedCollections();
        }

        private async Task UpdateDatabaseAsync()
        {
            await _context.Database.MigrateAsync();     
        }

        private async Task SeedRolesAsync()
        {
            if (_context.Roles.Any()) return;

            var adminRole = _appSettings.MovieMVCSettings.DefaultCredentials.Role;
            
            await _roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        private async Task SeedUsersAsync()
        {
            if (_context.Users.Any()) return;

            var credentials = _appSettings.MovieMVCSettings.DefaultCredentials;

            var user = new IdentityUser
            {
                Email = credentials.Email,
                UserName = credentials.Email,
                EmailConfirmed = true
            };
            
            await _userManager.CreateAsync(user, credentials.Password);
            await _userManager.AddToRoleAsync(user, credentials.Role);
        }

        private async Task SeedCollections()
        {
            if (_context.Collection.Any()) return;

            _context.Add(new Collection()
            {
                Name = _appSettings.MovieMVCSettings.DefaultCollection.Name,
                Description = _appSettings.MovieMVCSettings.DefaultCollection.Description
            });

            await _context.SaveChangesAsync();
        }
    }
}
