# Deploy da API em VPS com Docker e GitHub Actions

## Arquivos adicionados
- `ApiFestaJulina/Dockerfile`
- `ApiFestaJulina/.dockerignore`
- `docker-compose.vps.yml`
- `.env.vps.example`
- `.github/workflows/main_apifestajunina.yml`

## 1) Preparar VPS
Na VPS, crie a pasta da stack:

```bash
sudo mkdir -p /opt/festajunina
sudo chown -R deploy:deploy /opt/festajunina
```

Copie para `/opt/festajunina`:
- `docker-compose.vps.yml`
- um arquivo `.env` baseado em `.env.vps.example`

## 2) Variaveis do .env na VPS
Crie o arquivo:

```bash
cd /opt/festajunina
cp .env.vps.example .env
nano .env
```

Preencha as variaveis (principalmente senhas, JWT e GHCR_OWNER).

## 3) Secrets no GitHub (Repository Secrets)
Crie os secrets no repositorio:
- `VPS_HOST`
- `VPS_PORT`
- `VPS_USER`
- `VPS_SSH_KEY`
- `GHCR_USER`
- `GHCR_TOKEN`

## 4) Como funciona o workflow
1. Builda a imagem da API com Dockerfile.
2. Publica no GHCR como `ghcr.io/<owner>/api-festajunina`.
3. Conecta por SSH na VPS.
4. Executa `docker compose pull api` e `docker compose up -d api`.

## 5) Primeiro deploy manual (opcional)
Antes do CI/CD, na VPS:

```bash
cd /opt/festajunina
echo "SEU_GHCR_TOKEN" | docker login ghcr.io -u "SEU_GHCR_USER" --password-stdin
docker compose -f docker-compose.vps.yml pull api
docker compose -f docker-compose.vps.yml up -d
```

## 6) Observacoes de producao
- `Program.cs` foi ajustado para ler:
  - `Jwt:Key` (obrigatorio)
  - `Cors:AllowedOrigins` (lista)
- A API esta pronta para rodar atras de proxy reverso com `X-Forwarded-*`.
