from locust import HttpUser, task, between, SequentialTaskSet
import random
import json

class BlogUser(HttpUser):
   
    wait_time = between(1, 3)
    host = "http://localhost:5010" 

    def on_start(self):
       
        pass

    @task(3)
    def get_home(self):
        
        with self.client.get("/Home", catch_response=True) as response:
            if response.status_code == 200:
                response.success()
            else:
                response.failure(f"Status code: {response.status_code}")
@task(5)
def get_posts(self):
    
    skip = random.randint(0, 20) * 25 
    with self.client.get(f"/Post/v1/posts?skip={skip}&take=25", catch_response=True) as response:
        if response.status_code == 200:
            response.success()
        else:
            response.failure(f"Status code: {response.status_code}")
    @task(3)
    def get_post_detail(self):
        post_id = 1
        with self.client.get(f"/Post/v1/posts/{post_id}", catch_response=True) as response:
            if response.status_code == 200:
                response.success()
            elif response.status_code == 404:
                response.success()  
            else:
                response.failure(f"Status code: {response.status_code}")

    @task(2)
    def get_posts_by_category(self):
        
        categories = ["backend", "frontend"]
        category = random.choice(categories)
        with self.client.get(f"/Post/v1/posts/category/{category}", catch_response=True) as response:
            if response.status_code == 200:
                response.success()
            else:
                response.failure(f"Status code: {response.status_code}")

    @task(2)
    def get_categories(self):
        with self.client.get("/v1/categories", catch_response=True) as response:
            if response.status_code == 200:
                response.success()
            else:
                response.failure(f"Status code: {response.status_code}")

    @task(1)
    def get_category_detail(self):
        category_id = 1
        with self.client.get(f"/v1/categories/{category_id}", catch_response=True) as response:
            if response.status_code == 200:
                response.success()
            elif response.status_code == 404:
                response.success()  
            else:
                response.failure(f"Status code: {response.status_code}")


class AuthenticatedUser(HttpUser):
    wait_time = between(2, 5)
    host = "http://localhost:5010"
    token = None
    # Opcional: configure estas propriedades em tempo de execução para usar credenciais fixas
    auth_email = None
    auth_password = None

    def on_start(self):
        # Tenta autenticar ao iniciar o usuário autenticado
        self.login()

    def login(self):
        """Autenticação desativada para testes: não realiza chamadas a /accounts ou /accounts/login."""
        print("[locust] Autenticação desativada para testes; prosseguindo sem token")
        self.token = None

    @task(3)
    def get_posts(self):

        headers = {"Authorization": f"Bearer {self.token}"} if self.token else {}
        with self.client.get("/Post/v1/posts", 
                           headers=headers,
                           catch_response=True) as response:
            if response.status_code == 200:
                response.success()
            else:
                response.failure(f"Status code: {response.status_code}")

    @task(2)
    def get_categories(self):
     
        headers = {"Authorization": f"Bearer {self.token}"} if self.token else {}
        with self.client.get("/v1/categories",
                           headers=headers,
                           catch_response=True) as response:
            if response.status_code == 200:
                response.success()
            else:
                response.failure(f"Status code: {response.status_code}")

    @task(1)
    def create_category(self):
        if not self.token:
            return
            
        category_data = {
            "name": f"Categoria Teste {random.randint(1000, 9999)}",
            "slug": f"categoria-teste-{random.randint(1000, 9999)}"
        }
        
        headers = {"Authorization": f"Bearer {self.token}"}
        with self.client.post("/v1/categories",
                            json=category_data,
                            headers=headers,
                            catch_response=True) as response:
            if response.status_code == 201:
                response.success()
            elif response.status_code == 400:
                response.success() 
            else:
                response.failure(f"Status code: {response.status_code}")

    @task(1)
    def update_category(self):
       
        if not self.token:
            return
            
        category_id = 1  
        category_data = {
            "name": f"Categoria Atualizada {random.randint(1000, 9999)}",
            "slug": f"categoria-atualizada-{random.randint(1000, 9999)}"
        }
        
        headers = {"Authorization": f"Bearer {self.token}"}
        with self.client.put(f"/v1/categories/{category_id}",
                           json=category_data,
                           headers=headers,
                           catch_response=True) as response:
            if response.status_code == 200:
                response.success()
            elif response.status_code == 404:
                response.success()  # Categoria não encontrada é esperado
            else:
                response.failure(f"Status code: {response.status_code}")


# Classe RegistrationUser desativada para evitar testes contra /v1/accounts
# class RegistrationUser(HttpUser):
#     wait_time = between(3, 6)
#     host = "http://localhost:5010"
#
#     @task(1)
#     def register_user(self):
#         random_id = random.randint(10000, 99999)
#         register_data = {
#             "name": f"Usuário Teste {random_id}",
#             "email": f"teste{random_id}@exemplo.com"
#         }
#
#         with self.client.post("/v1/accounts",
#                             json=register_data,
#                             catch_response=True) as response:
#             if response.status_code == 200:
#                 try:
#                     result = response.json()
#                     if result.get("data"):
#                         response.success()
#                     else:
#                         response.failure("Dados não encontrados na resposta")
#                 except:
#                     response.failure("Resposta inválida")
#             elif response.status_code == 400:
#                 response.success()  # Email já cadastrado é esperado
#             else:
#                 response.failure(f"Status code: {response.status_code}")


class SequentialBlogUser(HttpUser):
    
    wait_time = between(1, 2)
    host = "http://localhost:5010"

    @task
    class UserFlow(SequentialTaskSet):
       
        def on_start(self):
            
            pass

        @task
        def step1_get_home(self):
            self.client.get("/Home")

        @task
        def step2_get_categories(self):
            self.client.get("/v1/categories")

        @task
        def step3_get_posts(self):

            self.client.get("/Post/v1/posts")

        @task
        def step4_get_post_detail(self):
            post_id = 1
            self.client.get(f"/Post/v1/posts/{post_id}")

        @task
        def step5_get_posts_by_category(self):
            categories = ["backend", "frontend"]
            category = random.choice(categories)
            self.client.get(f"/Post/v1/posts/category/{category}")
