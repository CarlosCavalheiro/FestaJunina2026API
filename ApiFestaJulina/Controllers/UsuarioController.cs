using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApiFestaJulina.Models;
using ApiFestaJulina.Repository;
using Microsoft.EntityFrameworkCore;
using ApiFestaJulina.DTO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using ApiFestaJulina.Services;


namespace ApiFestaJulina.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private HashAlgorithm _algoritmo;
        private readonly IWebHostEnvironment _env;
        private readonly EmailService _emailService;

        private readonly string[] _allowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        public UsuarioController(
            AppDbContext context,
            HashAlgorithm algoritmo,
            IWebHostEnvironment env,
            EmailService emailService)
        {
            _context = context;
            _algoritmo = algoritmo;
            _env = env;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(ValidacaoUsuario login)
        {
            var UsuarioExist = await _context.Usuarios
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(u => u.Email == login.Email);

            if (UsuarioExist == null)
            {
                return NotFound("Usuario não encontrado!!");
            }

            // return Ok(new 
            // { 
            //     mensagem = "Login realizado com sucesso",
            //     usuario = usuario
            // });

    var senhaCriptografada =
        _algoritmo.ComputeHash(Encoding.UTF8.GetBytes(login.Senha));

    var sb = new StringBuilder();

    foreach (var caractere in senhaCriptografada)
    {
        sb.Append(caractere.ToString("X2"));
    }

    if ((UsuarioExist.Senha ?? string.Empty) != sb.ToString())
    {
        return BadRequest("Senha incorreta");
    }

    if (UsuarioExist.Status == false)
    {
        return BadRequest("Usuario inativo, entre em contato com o administrador");
    }

            // //verificar se é um admin
            // if (UsuarioExist.Perfil?.Descricao == "Administrador")
            // {
            //     var adminLogado = await _context.Sessoes.Include(s => s.Usuario).ThenInclude(u => u.Perfil)
            //                                 .AnyAsync(s => s.Ativo == true && s.Usuario != null && s.Usuario.Perfil != null && s.Usuario.Perfil.Descricao == "Administrador");

            //     if (adminLogado)
            //     {
            //         return BadRequest("Já existe um administrador logado!");
            //     }
            // }

            //gerar um token
            var claims = new[]
            {
                new Claim("id", UsuarioExist.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, UsuarioExist.Nome ?? string.Empty),
                new Claim(ClaimTypes.Role, UsuarioExist.Perfil?.Descricao ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("f8A9xK2#pL0zQw7@Rm5TnY3uVb6C!dE1")
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "apiFestaJulina",
                audience: "apiFestaJulina",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // //salvar a sessao se for adm
            // if (UsuarioExist.Perfil?.Descricao == "Administrador")
            // {
            //     var sessao = new Sessao
            //     {
            //         IdUsuario = UsuarioExist.IdUsuario,
            //         Token = tokenString,
            //         DataInicio = DateTime.Now,
            //         Ativo = true
            //     };

            //     await _context.Sessoes.AddAsync(sessao);
            //     await _context.SaveChangesAsync();
            // }

            //retorno
            return Ok(new
            {
                idUsuario = UsuarioExist.IdUsuario,
                token = tokenString,

                usuario = UsuarioExist.Nome ?? string.Empty,
                perfil = UsuarioExist.Perfil?.Descricao ?? string.Empty,
                imagemPerfil = UsuarioExist.ImagemPerfil ?? string.Empty
            });
        }
//     var claims = new[]
//     {
//         new Claim("id", UsuarioExist.IdUsuario.ToString()),
//         new Claim(ClaimTypes.Name, UsuarioExist.Nome ?? string.Empty),
//         new Claim(ClaimTypes.Role, UsuarioExist.Perfil?.Descricao ?? string.Empty)
//     };

//     var key = new SymmetricSecurityKey(
//         Encoding.UTF8.GetBytes("f8A9xK2#pL0zQw7@Rm5TnY3uVb6C!dE1"));

//     var creds = new SigningCredentials(
//         key,
//         SecurityAlgorithms.HmacSha256);

//     var token = new JwtSecurityToken(
//         issuer: "apiFestaJulina",
//         audience: "apiFestaJulina",
//         claims: claims,
//         expires: DateTime.Now.AddHours(1),
//         signingCredentials: creds
//     );

//     var tokenString =
//         new JwtSecurityTokenHandler().WriteToken(token);

//     return Ok(new
//     {
//         idUsuario = UsuarioExist.IdUsuario,
//         token = tokenString,  
//         usuario = UsuarioExist.Nome ?? string.Empty,
//         perfil = UsuarioExist.Perfil?.Descricao ?? string.Empty
//     });
// }
        [HttpPost("logout")]
        public async Task<ActionResult> Logout(string email)
        {
            var sessaoAtiva = await _context.Sessoes
                .FirstOrDefaultAsync(s => s.Usuario.Email == email && s.Ativo == true);

            if (sessaoAtiva == null)
            {
                return NotFound("Sessão ativa não encontrada para este usuário.");
            }

            sessaoAtiva.Ativo = false;
            _context.Sessoes.Update(sessaoAtiva);
            await _context.SaveChangesAsync();

            return Ok("Logout realizado com sucesso.");
        }

        [HttpPost("MudarSenha")]
        public async Task<ActionResult<Usuarios>> MudarSenha(int id, NewSenha newSenha)
        {
            var UsuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (UsuarioExistente == null)
            {
                return NotFound("Usuário não encontrado");
            }

            var new_password = CriptografarSenha(newSenha.Senha);
            UsuarioExistente.Senha = new_password;

            _context.Usuarios.Update(UsuarioExistente);
            await _context.SaveChangesAsync();

            return Ok("Senha alterada com sucesso");
        }

        [HttpPost("solicitarMudancaSenha")]
public async Task<ActionResult> SolicitarMudancaSenha(RecuperarSenhaDTO dados)
{
    var usuario = await _context.Usuarios
        .FirstOrDefaultAsync(u => u.Email == dados.Email);

    if (usuario == null)
    {
        return NotFound("Email não encontrado");
    }

    var token = Guid.NewGuid().ToString();

    usuario.TokenRecuperacaoSenha = token;
    usuario.NovaSenhaTemporaria = CriptografarSenha(dados.Senha);
    usuario.TokenExpiracao = DateTime.Now.AddMinutes(15);

    await _context.SaveChangesAsync();

    var linkConfirmacao =
        $"https://festajulina.senailp.com.br/confirmarSenha?token={token}";

    _ = Task.Run(async () =>
    {
        try
        {
            await _emailService.EnviarEmailAsync(
                usuario.Email,
                "Confirmação de alteração de senha - Festa Julina",
                $@"
                <h2>Confirmação de alteração de senha</h2>

                <p>Recebemos uma solicitação para alterar sua senha.</p>

                <p>Para confirmar, clique no botão abaixo:</p>

                <a href='{linkConfirmacao}'
                   style='
                   display:inline-block;
                   padding:14px 24px;
                   background:#d34800;
                   color:white;
                   text-decoration:none;
                   border-radius:8px;
                   font-weight:bold;
                   '>
                   Confirmar alteração de senha
                </a>

                <p>Esse link expira em 15 minutos.</p>

                <p>Se não foi você, ignore este email.</p>
                "
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERRO AO ENVIAR EMAIL:");
            Console.WriteLine(ex.ToString());
        }
            });

            return Ok(new
            {
                mensagem = "Email de confirmação enviado!"
            });
        }

        [HttpPut("confirmarMudancaSenha")]
        public async Task<ActionResult> ConfirmarMudancaSenha(string token)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.TokenRecuperacaoSenha == token
                );

            if (usuario == null)
            {
                return NotFound("Token inválido");
            }

            if (usuario.TokenExpiracao < DateTime.Now)
            {
                return BadRequest("Token expirado");
            }

            if (string.IsNullOrEmpty(usuario.NovaSenhaTemporaria))
            {
                return BadRequest("Nova senha inválida");
            }

            usuario.Senha = usuario.NovaSenhaTemporaria;

            usuario.TokenRecuperacaoSenha = null;
            usuario.NovaSenhaTemporaria = null;
            usuario.TokenExpiracao = null;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Senha alterada com sucesso!"
            });
        }

        
        [HttpGet("ListarUsuarios")]
        public async Task<ActionResult<List<Usuarios>>> GetUsuarios()
        {
            var ListaUsuarios = await _context.Usuarios
                .Include(p => p.Perfil)
                .ToListAsync();

            return Ok(ListaUsuarios);
        }

        
        [HttpGet("ListarUsuarioID")]
        public async Task<ActionResult<Usuarios>> GetUsuariosId(int id)
        {
            var usuarios = await _context.Usuarios
                .Include(p => p.Perfil)
                .FirstOrDefaultAsync(i => i.IdUsuario == id);

            if (usuarios == null)
            {
                return NotFound("Usuario não encontrado");
            }

            return Ok(usuarios);
        }

        
        [HttpPost("Registro")]
        public async Task<ActionResult<Usuarios>> RegistroUsuario(NewUsuario newUsuario)
        {
            Usuarios usuario = new Usuarios
            {
                Nome = newUsuario.Nome,
                Email = newUsuario.Email,
                Senha = CriptografarSenha(newUsuario.Senha),
                Telefone = newUsuario.Telefone,
                IdPerfil = newUsuario.IdPerfil,
                PossuiDeficiencia = newUsuario.PossuiDeficiencia,
                TipoDeficiencia = newUsuario.TipoDeficiencia,
                Status = true
              
            };

            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
            return Ok(usuario);
        }

        
        public string CriptografarSenha(string senha)
        {
            var encodedValue = Encoding.UTF8.GetBytes(senha);
            var encryptedPassword = _algoritmo.ComputeHash(encodedValue);

            var sb = new StringBuilder();

            foreach (var caracter in encryptedPassword)
            {
                sb.Append(caracter.ToString("X2"));
            }

            return sb.ToString();
        }

        
        [HttpPut("EditarUsuario")]
        public async Task<ActionResult<Usuarios>> EditarUsuario(int id, UsuarioUpdate usuarioUpdate)
        {
            var usuarioExist = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuarioExist == null)
            {
                return NotFound("Usuario não encontrado!");
            }

            usuarioExist.Nome = usuarioUpdate.Nome;
            usuarioExist.Email = usuarioUpdate.Email;

            if (!string.IsNullOrWhiteSpace(usuarioUpdate.Senha))
            {
                usuarioExist.Senha = CriptografarSenha(usuarioUpdate.Senha);
            }

            usuarioExist.Telefone = usuarioUpdate.Telefone;
            usuarioExist.IdPerfil = usuarioUpdate.IdPerfil;

            _context.Usuarios.Update(usuarioExist);
            await _context.SaveChangesAsync();

            return Ok("Usuario Editado com Sucesso");
        }

        
        [HttpPut("AlterarStatus")]
        public async Task<ActionResult<Usuarios>> AlterarStatus(int id, statusUsuarioUpdate statusUpdate)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return BadRequest("Esse usuario não existe");
            }

            usuario.Status = statusUpdate.Status;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return Ok(usuario.Status);
        }

        [RequestSizeLimit(5 * 1024 * 1024)]
        [HttpPost("ImagemPerfil")]
        public async Task<ActionResult<Usuarios>> ImagemPerfil(IFormFile file, int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound("Usuário não encontrado!");
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("Nenhuma imagem foi enviada!");
            }

            const long limite = 5 * 1024 * 1024;

            if (file.Length > limite)
            {
                return BadRequest("A imagem deve ter no máximo 5MB.");
            }

            var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extensao = Path.GetExtension(file.FileName).ToLower();
            var pathSaveImage = Path.Combine("\\uploads\\FotoPerfil");

            if (!Directory.Exists(pathSaveImage))
            {
                Directory.CreateDirectory(pathSaveImage);
            }

            if (!string.IsNullOrEmpty(usuario.ImagemPerfil))
            {
                var caminhoAntigo = Path.Combine(pathSaveImage, usuario.ImagemPerfil);

                if (System.IO.File.Exists(caminhoAntigo))
                {
                    System.IO.File.Delete(caminhoAntigo);
                }
            }

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var fullPath = Path.Combine(pathSaveImage, fileName);

            if (!_allowedExtensions.Contains(extensao))
            {
                return BadRequest("Formato inválido! Apenas JPG, PNG, JPEG e WEBP");
            }

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            usuario.ImagemPerfil = fileName;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem = "Foto atualizada!",
                arquivo = fileName
            });
        }
    }
}