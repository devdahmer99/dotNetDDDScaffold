using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using LibGit2Sharp;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        // Solicitar informacoes ao usuario
        Console.Write("Digite o nome do projeto: ");
        string? projectName = Console.ReadLine();

        Console.Write("Digite o nome de usuario do banco de dados: ");
        string? dbUser = Console.ReadLine();

        Console.Write("Digite a senha do banco de dados: ");
        string? dbPassword = Console.ReadLine();

        Console.Write("Digite o caminho para o projeto: ");
        string? projectDir = Console.ReadLine()?.Trim();

        // Verificar se o caminho e valido
        if (string.IsNullOrWhiteSpace(projectDir) || !Directory.Exists(projectDir))
        {
            Console.WriteLine("O caminho fornecido e invalido. Por favor, forneca um caminho valido.");
            return;
        }

        if (string.IsNullOrWhiteSpace(projectName) || string.IsNullOrWhiteSpace(dbUser) || string.IsNullOrWhiteSpace(dbPassword))
        {
            Console.WriteLine("O nome do projeto, usuario do banco de dados e senha do banco de dados sao obrigatorios.");
            return;
        }

        string srcDir = Path.Combine(projectDir, projectName, "src");

        try
        {
            // Criar pasta do projeto e mudar para o diretorio
            Directory.CreateDirectory(Path.Combine(projectDir, projectName));
            Directory.SetCurrentDirectory(Path.Combine(projectDir, projectName));

            // Criar solucao do projeto
            RunCommand("dotnet", $"new sln -n {projectName}");

            // Criar pasta src para os projetos
            Directory.CreateDirectory(srcDir);

            // Criar projetos dentro do diretorio src
            RunCommand("dotnet", $"new webapi -n {projectName}.API --use-controllers --no-restore -o {Path.Combine(srcDir, projectName + ".API")}");
            RunCommand("dotnet", $"new classlib -n {projectName}.Aplicacao --no-restore -o {Path.Combine(srcDir, projectName + ".Aplicacao")}");
            RunCommand("dotnet", $"new classlib -n {projectName}.Dominio --no-restore -o {Path.Combine(srcDir, projectName + ".Dominio")}");
            RunCommand("dotnet", $"new classlib -n {projectName}.Infra --no-restore -o {Path.Combine(srcDir, projectName + ".Infra")}");
            RunCommand("dotnet", $"new classlib -n {projectName}.Comunicacao --no-restore -o {Path.Combine(srcDir, projectName + ".Comunicacao")}");
            RunCommand("dotnet", $"new classlib -n {projectName}.Exception --no-restore -o {Path.Combine(srcDir, projectName + ".Exception")}");

            // Remover arquivos Class1.cs dos projetos de class library
            RemoveDefaultClassFile(srcDir, projectName + ".Aplicacao");
            RemoveDefaultClassFile(srcDir, projectName + ".Dominio");
            RemoveDefaultClassFile(srcDir, projectName + ".Infra");
            RemoveDefaultClassFile(srcDir, projectName + ".Comunicacao");
            RemoveDefaultClassFile(srcDir, projectName + ".Exception");

            // Criar estrutura de pastas para cada projeto
            CreateDirectoryStructure(srcDir, projectName);

            // Adicionar projetos na solucao
            RunCommand("dotnet", $"sln {projectName}.sln add {Path.Combine(srcDir, projectName + ".API", projectName + ".API.csproj")}");
            RunCommand("dotnet", $"sln {projectName}.sln add {Path.Combine(srcDir, projectName + ".Aplicacao", projectName + ".Aplicacao.csproj")}");
            RunCommand("dotnet", $"sln {projectName}.sln add {Path.Combine(srcDir, projectName + ".Dominio", projectName + ".Dominio.csproj")}");
            RunCommand("dotnet", $"sln {projectName}.sln add {Path.Combine(srcDir, projectName + ".Infra", projectName + ".Infra.csproj")}");
            RunCommand("dotnet", $"sln {projectName}.sln add {Path.Combine(srcDir, projectName + ".Comunicacao", projectName + ".Comunicacao.csproj")}");
            RunCommand("dotnet", $"sln {projectName}.sln add {Path.Combine(srcDir, projectName + ".Exception", projectName + ".Exception.csproj")}");

            // Adicionar referencias entre projetos
            AddProjectReferences(srcDir, projectName);

            // Instalar pacotes necessarios
            InstallPackages(srcDir, projectName);

            // Criar arquivos e conteudo
            CreateFilesAndContent(srcDir, projectName, dbUser, dbPassword);

            // Inicializar repositorio Git
            InitializeGitRepo(projectDir, projectName);

            Console.WriteLine($"Estrutura do projeto criada com sucesso em: {Path.Combine(projectDir, projectName)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }

    static void RemoveDefaultClassFile(string srcDir, string projectName)
    {
        string classFilePath = Path.Combine(srcDir, projectName, "Class1.cs");
        if (File.Exists(classFilePath))
        {
            File.Delete(classFilePath);
            Console.WriteLine($"Arquivo removido: {classFilePath}");
        }
    }

    static void RunCommand(string command, string args)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }

static void CreateDirectoryStructure(string srcDir, string projectName)
{
    // Verificar se o diretório srcDir existe
    if (!Directory.Exists(srcDir))
    {
        Console.WriteLine($"O diretório {srcDir} não existe.");
        return;
    }

    Console.WriteLine("Criando estrutura de pastas...");

    // Lista de diretórios a serem criados
    var directoriesToCreate = new List<string>
    {
        Path.Combine(srcDir, projectName + ".Aplicacao", "AutoMapper"),
        Path.Combine(srcDir, projectName + ".Aplicacao", "Enums"),
        Path.Combine(srcDir, projectName + ".Aplicacao", "Reports"),
        Path.Combine(srcDir, projectName + ".Aplicacao", "UseCase"),
        Path.Combine(srcDir, projectName + ".Dominio", "Entidades"),
        Path.Combine(srcDir, projectName + ".Dominio", "Enums"),
        Path.Combine(srcDir, projectName + ".Dominio", "Extensoes"),
        Path.Combine(srcDir, projectName + ".Dominio", "Reports"),
        Path.Combine(srcDir, projectName + ".Dominio", "Repositories"),
        Path.Combine(srcDir, projectName + ".Dominio", "Seguranca"),
        Path.Combine(srcDir, projectName + ".Dominio", "Services"),
        Path.Combine(srcDir, projectName + ".Infra", "DataAccess"),
        Path.Combine(srcDir, projectName + ".Infra", "Extensoes"),
        Path.Combine(srcDir, projectName + ".Infra", "Migrations"),
        Path.Combine(srcDir, projectName + ".Infra", "Seguranca"),
        Path.Combine(srcDir, projectName + ".Infra", "Services"),
        Path.Combine(srcDir, projectName + ".Infra", "Repositories"),
        Path.Combine(srcDir, projectName + ".Comunicacao", "Enums"),
        Path.Combine(srcDir, projectName + ".Comunicacao", "Requests"),
        Path.Combine(srcDir, projectName + ".Comunicacao", "Responses"),
        Path.Combine(srcDir, projectName + ".Exception", "ExceptionBase")
    };

    // Criação dos diretórios e arquivos .gitkeep
    foreach (var dir in directoriesToCreate)
    {
        try
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Console.WriteLine($"Diretório criado: {dir}");
            }
            else
            {
                Console.WriteLine($"Diretório já existe: {dir}");
            }

            // Adicionar arquivo .gitkeep
            string gitkeepFilePath = Path.Combine(dir, ".gitkeep");
            if (!File.Exists(gitkeepFilePath))
            {
                File.WriteAllText(gitkeepFilePath, string.Empty);
                Console.WriteLine($"Arquivo .gitkeep criado em: {gitkeepFilePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar o diretório {dir}: {ex.Message}");
        }
    }

    Console.WriteLine("Estrutura de pastas criada com sucesso.");
}

    static void AddProjectReferences(string srcDir, string projectName)
    {
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".API", projectName + ".API.csproj")} reference {Path.Combine(srcDir, projectName + ".Aplicacao", projectName + ".Aplicacao.csproj")}");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".API", projectName + ".API.csproj")} reference {Path.Combine(srcDir, projectName + ".Comunicacao", projectName + ".Comunicacao.csproj")}");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".API", projectName + ".API.csproj")} reference {Path.Combine(srcDir, projectName + ".Exception", projectName + ".Exception.csproj")}");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".Aplicacao", projectName + ".Aplicacao.csproj")} reference {Path.Combine(srcDir, projectName + ".Dominio", projectName + ".Dominio.csproj")}");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".Aplicacao", projectName + ".Aplicacao.csproj")} reference {Path.Combine(srcDir, projectName + ".Infra", projectName + ".Infra.csproj")}");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".Infra", projectName + ".Infra.csproj")} reference {Path.Combine(srcDir, projectName + ".Dominio", projectName + ".Dominio.csproj")}");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".Dominio", projectName + ".Dominio.csproj")} reference {Path.Combine(srcDir, projectName + ".Comunicacao", projectName + ".Comunicacao.csproj")}");
    }

    static void InstallPackages(string srcDir, string projectName)
    {
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".API", projectName + ".API.csproj")} package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.0");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".API", projectName + ".API.csproj")} package Microsoft.EntityFrameworkCore.Design --version 8.0.0");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".API", projectName + ".API.csproj")} package Microsoft.EntityFrameworkCore.Tools --version 8.0.0");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".API", projectName + ".API.csproj")} package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".API", projectName + ".API.csproj")} package Pomelo.EntityFrameworkCore.MySql --version 8.0.0");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".API", projectName + ".API.csproj")} package FluentValidation --version 11.7.1");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".API", projectName + ".API.csproj")} package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.0");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".API", projectName + ".API.csproj")} package Microsoft.Extensions.Options --version 8.0.0");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".Infra", projectName + ".Infra.csproj")} package Microsoft.EntityFrameworkCore.Design --version 8.0.0");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".Infra", projectName + ".Infra.csproj")} package Microsoft.EntityFrameworkCore.Tools --version 8.0.0");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".Infra", projectName + ".Infra.csproj")} package Pomelo.EntityFrameworkCore.MySql --version 8.0.0");
        RunCommand("dotnet", $"add {Path.Combine(srcDir, projectName + ".Infra", projectName + ".Infra.csproj")} package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0");
    }

    static void CreateFilesAndContent(string srcDir, string projectName, string dbUser, string dbPassword)
    {
        // Criacao de arquivos e conteudo (exemplos)
        File.WriteAllText(Path.Combine(srcDir, projectName + ".Dominio", "Entidades", "Usuario.cs"), @"
using System.ComponentModel.DataAnnotations;
namespace " + projectName + @".Dominio.Entidades {
    public class Usuario {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Senha { get; set; }
    }
}");

        File.WriteAllText(Path.Combine(srcDir, projectName + ".Infra", "DataAccess", "AppDbContext.cs"), @"
using Microsoft.EntityFrameworkCore;
using " + projectName + @".Dominio.Entidades;
namespace " + projectName + @".Infra.DataAccess {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}");

        File.WriteAllText(Path.Combine(srcDir, projectName + ".Infra", "Repositories", "IUsuarioRepository.cs"), @"
using " + projectName + @".Dominio.Entidades;
using System.Threading.Tasks;

namespace " + projectName + @".Infra.Repositories
{
    public interface IUsuarioRepository
    {
        Task AdicionarUsuarioAsync(Usuario usuario);
        // Adicione outros métodos conforme necessário
    }
}");

        File.WriteAllText(Path.Combine(srcDir, projectName + ".Infra", "Repositories", "UsuarioRepository.cs"), @"
using " + projectName + @".Dominio.Entidades;
using " + projectName + @".Infra.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace " + projectName + @".Infra.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarUsuarioAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        // Implemente outros métodos conforme necessário
    }
}");

        File.WriteAllText(Path.Combine(srcDir, projectName + ".Aplicacao", "UseCase", "AdicionarUsuarioUseCase.cs"), @"
using System.Threading.Tasks;
using " + projectName + @".Dominio.Entidades;
using " + projectName + @".Infra.Repositories;

namespace " + projectName + @".Aplicacao.UseCase {
    public class AdicionarUsuarioUseCase {
        private readonly IUsuarioRepository _usuarioRepository;
        public AdicionarUsuarioUseCase(IUsuarioRepository usuarioRepository) {
            _usuarioRepository = usuarioRepository;
        }
        public async Task Execute(Usuario usuario) {
            await _usuarioRepository.AdicionarUsuarioAsync(usuario);
        }
    }
}");

        File.WriteAllText(Path.Combine(srcDir, projectName + ".API", "Controllers", "UsuarioController.cs"), @"
using Microsoft.AspNetCore.Mvc;
using " + projectName + @".Aplicacao.UseCase;
using " + projectName + @".Comunicacao.Requests;
using " + projectName + @".Comunicacao.Responses;
using " + projectName + @".Dominio.Entidades;
using AutoMapper;

namespace " + projectName + @".API.Controllers {
    [ApiController]
    [Route(""api/[controller]"")]
    public class UsuarioController : ControllerBase {
        private readonly AdicionarUsuarioUseCase _useCase;
        private readonly IMapper _mapper;
        public UsuarioController(AdicionarUsuarioUseCase useCase, IMapper mapper) {
            _useCase = useCase;
            _mapper = mapper;
        }
        [HttpPost]
        public async Task<ActionResult<UsuarioResponse>> Post(UsuarioRequest request) {
            var usuario = _mapper.Map<Usuario>(request);
            await _useCase.Execute(usuario);
            var response = _mapper.Map<UsuarioResponse>(usuario);
            return CreatedAtAction(nameof(Post), new { id = usuario.Id }, response);
        }
    }
}");

        File.WriteAllText(Path.Combine(srcDir, projectName + ".Comunicacao", "Requests", "UsuarioRequest.cs"), @"
namespace " + projectName + @".Comunicacao.Requests {
    public class UsuarioRequest {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
    }
}");

        File.WriteAllText(Path.Combine(srcDir, projectName + ".Comunicacao", "Responses", "UsuarioResponse.cs"), @"
namespace " + projectName + @".Comunicacao.Responses {
    public class UsuarioResponse {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
    }
}");

        File.WriteAllText(Path.Combine(srcDir, projectName + ".API", "Program.cs"), @"
using Microsoft.EntityFrameworkCore;
using " + projectName + @".Infra.DataAccess;
using " + projectName + @".Aplicacao.UseCase;
using " + projectName + @".Infra.Repositories;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString(""DefaultConnection"");

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<AdicionarUsuarioUseCase>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

ApplyMigrations(app);
app.Run();

void ApplyMigrations(IHost app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
");

        File.WriteAllText(Path.Combine(srcDir, projectName + ".Infra", "Seguranca", "JwtService.cs"), @"
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace " + projectName + @".Infra.Seguranca {
    public class JwtService {
        private readonly string _secret;
        private readonly string _expDate;
        public JwtService(string secret, string expDate) {
            _secret = secret;
            _expDate = expDate;
        }
        public string GenerateSecurityToken(string email) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_expDate)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
};
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}");

        File.WriteAllText(Path.Combine(srcDir, projectName + ".API", "appsettings.json"), @"
{
  ""ConnectionStrings"": {
    ""DefaultConnection"": ""Server=localhost;Database=" + projectName + @";User=" + dbUser + @";Password=" + dbPassword + @"""
  },
  ""Jwt"": {
    ""SecretKey"": ""YourSecretKey"",
    ""ExpiryMinutes"": ""60""
  },
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }
  },
  ""AllowedHosts"": ""*""
}
");
    }

    static void InitializeGitRepo(string projectDir, string projectName)
    {
        try
        {
            string repoPath = Path.Combine(projectDir, projectName);
            Repository.Init(repoPath);
            using (var repo = new Repository(repoPath))
            {
                Commands.Stage(repo, "*");
                Signature author = new Signature("Author", "author@example.com", DateTime.Now);
                Signature committer = author;
                repo.Commit("Initial commit", author, committer);
            }
            Console.WriteLine("Repositorio Git inicializado com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao inicializar o repositorio Git: {ex.Message}");
        }
    }
}