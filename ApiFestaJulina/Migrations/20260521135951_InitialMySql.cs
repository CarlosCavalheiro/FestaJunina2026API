using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiFestaJulina.Migrations
{
    /// <inheritdoc />
    public partial class InitialMySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "evento",
                columns: table => new
                {
                    id_evento = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nome_evento = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ativo = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    local = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descricao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    qtde_ingressos = table.Column<int>(type: "int", nullable: false),
                    qtde_lotes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evento", x => x.id_evento);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    id_lote = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_evento = table.Column<int>(type: "int", nullable: false),
                    qtde_ingressos_lotes = table.Column<int>(type: "int", nullable: false),
                    valor_ing = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    dta_criacao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    dta_fechamento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    descricao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    saldo = table.Column<int>(type: "int", nullable: false),
                    ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    tipo_lote = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.id_lote);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    id_pedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    quantidade = table.Column<int>(type: "int", nullable: false),
                    id_status = table.Column<int>(type: "int", nullable: false),
                    valor = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    dta_reserva = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    id_tipo_pagamento = table.Column<int>(type: "int", nullable: false),
                    dta_fechamento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ft_comprovante = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ultima_acao_por = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.id_pedido);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Perfil",
                columns: table => new
                {
                    id_perfil = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    descricao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfil", x => x.id_perfil);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Perguntas",
                columns: table => new
                {
                    id_pergunta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_tp_user = table.Column<int>(type: "int", nullable: false),
                    descricao_pergunta = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tp_pergunta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perguntas", x => x.id_pergunta);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Respostas",
                columns: table => new
                {
                    id_resposta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_pergunta = table.Column<int>(type: "int", nullable: false),
                    resposta = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_resposta = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Respostas", x => x.id_resposta);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tipo",
                columns: table => new
                {
                    id_tipo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    descricao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tipo", x => x.id_tipo);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TipoUsuario",
                columns: table => new
                {
                    id_tp_user = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    descricao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoUsuario", x => x.id_tp_user);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    senha = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_perfil = table.Column<int>(type: "int", nullable: false),
                    imagem_perfil = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    possui_deficiencia = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    tipo_deficiencia = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    token_recuperacao_senha = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nova_senha_temporaria = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    token_expiracao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Perfil_id_perfil",
                        column: x => x.id_perfil,
                        principalTable: "Perfil",
                        principalColumn: "id_perfil",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Ingressos",
                columns: table => new
                {
                    id_ingresso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_pedido = table.Column<int>(type: "int", nullable: false),
                    id_lote = table.Column<int>(type: "int", nullable: false),
                    id_tipo = table.Column<int>(type: "int", nullable: false),
                    valor = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    qr_code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    id_status_validacao = table.Column<int>(type: "int", nullable: false),
                    dt_entrada = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    usuario_que_leu = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingressos", x => x.id_ingresso);
                    table.ForeignKey(
                        name: "FK_Ingressos_Lotes_id_lote",
                        column: x => x.id_lote,
                        principalTable: "Lotes",
                        principalColumn: "id_lote",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ingressos_Pedidos_id_pedido",
                        column: x => x.id_pedido,
                        principalTable: "Pedidos",
                        principalColumn: "id_pedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ingressos_Usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sessao",
                columns: table => new
                {
                    id_sessao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    token = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_inicio = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ativo = table.Column<bool>(type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessao", x => x.id_sessao);
                    table.ForeignKey(
                        name: "FK_sessao_Usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Ingressos_id_lote",
                table: "Ingressos",
                column: "id_lote");

            migrationBuilder.CreateIndex(
                name: "IX_Ingressos_id_pedido",
                table: "Ingressos",
                column: "id_pedido");

            migrationBuilder.CreateIndex(
                name: "IX_Ingressos_id_usuario",
                table: "Ingressos",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_sessao_id_usuario",
                table: "sessao",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_id_perfil",
                table: "Usuarios",
                column: "id_perfil");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evento");

            migrationBuilder.DropTable(
                name: "Ingressos");

            migrationBuilder.DropTable(
                name: "Perguntas");

            migrationBuilder.DropTable(
                name: "Respostas");

            migrationBuilder.DropTable(
                name: "sessao");

            migrationBuilder.DropTable(
                name: "Tipo");

            migrationBuilder.DropTable(
                name: "TipoUsuario");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Perfil");
        }
    }
}
