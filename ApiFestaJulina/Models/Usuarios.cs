using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ApiFestaJulina.Models
{
    public class Usuarios
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("nome")]
        public string Nome { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("telefone")]
        public string? Telefone { get; set; }

        [Column("senha")]
        public string Senha { get; set; }

        [Column("id_perfil")]
        public int IdPerfil { get; set; }

        [Column("imagem_perfil")]
        public string? ImagemPerfil { get; set; }

        [Column("status")]
        public bool Status { get; set; }

        [Column("possui_deficiencia")]
        public bool PossuiDeficiencia { get; set; }

        [Column("tipo_deficiencia")]
        public string? TipoDeficiencia { get; set; }

        // recuperação de senha
        [Column("token_recuperacao_senha")]
        public string? TokenRecuperacaoSenha { get; set; }

        [Column("nova_senha_temporaria")]
        public string? NovaSenhaTemporaria { get; set; }

        [Column("token_expiracao")]
        public DateTime? TokenExpiracao { get; set; }

        public virtual Perfil Perfil { get; set; }

        public virtual ICollection<Ingressos> Ingressos { get; set; }
    }
}