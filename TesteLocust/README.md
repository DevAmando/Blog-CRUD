# Testes de Carga com Locust - Blog API

Este diretório contém os testes de carga para a API do Blog usando Locust.

## Instalação

1. Instale as dependências:
```bash
pip install -r requirements.txt
```

## Configuração

Antes de executar os testes, certifique-se de que:

1. A API está rodando (por padrão em `http://localhost:5000`)
2. Se necessário, ajuste o `host` nas classes de usuário no `locustfile.py`
3. Para testes com autenticação, configure um usuário válido em `AuthenticatedUser.login()`

## Executando os Testes

### Interface Web (Recomendado)

```bash
locust -f locustfile.py
```

Depois acesse `http://localhost:8089` no navegador para configurar e iniciar os testes.

### Linha de Comando

```bash
# Executar com 10 usuários, taxa de spawn de 2 usuários/segundo
locust -f locustfile.py --headless -u 10 -r 2 -t 60s

# Executar apenas usuários não autenticados
locust -f locustfile.py --headless -u 10 -r 2 -t 60s BlogUser

# Executar apenas usuários autenticados
locust -f locustfile.py --headless -u 5 -r 1 -t 60s AuthenticatedUser
```

## Classes de Usuário Disponíveis

### BlogUser
Usuário básico que apenas visualiza conteúdo (não autenticado):
- Visualiza home
- Lista posts
- Visualiza detalhes de posts
- Filtra posts por categoria
- Lista categorias
- Visualiza detalhes de categorias

### AuthenticatedUser
Usuário autenticado que pode criar e gerenciar categorias:
- Todas as funcionalidades do BlogUser
- Criar categorias
- Atualizar categorias
- Requer login válido

### RegistrationUser
Usuário que testa o fluxo de registro:
- Registra novos usuários

### SequentialBlogUser
Usuário que segue um fluxo sequencial de navegação:
- Navegação passo a passo pela aplicação

## Parâmetros de Teste

- `wait_time`: Tempo de espera entre requisições (entre 1-3 segundos para BlogUser)
- `host`: URL base da API (padrão: `http://localhost:5000`)
- Pesos das tarefas (`@task(n)`): Números maiores = execução mais frequente

## Relatórios

Os relatórios podem ser exportados através da interface web ou salvos automaticamente com:

```bash
locust -f locustfile.py --headless -u 10 -r 2 -t 60s --html report.html
```






