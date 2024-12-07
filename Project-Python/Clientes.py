
import random
import csv
import requests
import os
from dotenv import load_dotenv


# Carregar variáveis de ambiente (API Key)
load_dotenv()
API_KEY = os.getenv('HERE_API_KEY')

# Função para obter latitude e longitude do endereço usando a API Here
def geocode_address(address):
    url = f'https://geocode.search.hereapi.com/v1/geocode'
    params = {
        'q': address,
        'apiKey': API_KEY
    }
    response = requests.get(url, params=params)
    
    # Verificar o status da resposta
    if response.status_code == 200:
        data = response.json()
        if data['items']:
            latitude = data['items'][0]['position']['lat']
            longitude = data['items'][0]['position']['lng']
            return latitude, longitude
    return None, None  # Retorna None se não encontrar as coordenadas


# Gerando os clientes
clientes = []

nomes = ['João', 'Maria', 'Ricardo', 'Ana', 'Luiz', 'Claudete', 'Paula']
sobrenomes = ['Silva', 'Ramalho', 'Souza', 'Melo', 'Alves']
enderecos = [
    ("Área Rural", "Área Rural de Taboão da Serra", "Taboão da Serra/SP", "06799-899"),
    ("Rua Avaré", "Parque Industrial Taboão da Serra", "Taboão da Serra/SP", "06785-320"),
    ("Rua Francisco D'Amico", "Parque Industrial Taboão da Serra", "Taboão da Serra/SP", "06785-290"),
    ("Área Rural", "Área Rural de Itapecerica da Serra", "Itapecerica da Serra/SP", "06889-899"),
    ("Área Rural", "Área Rural de Embu das Artes", "Embu das Artes/SP", "06849-899"),
    ("Praça Salvador Correia", "Capão Redondo", "São Paulo/SP", "05866-140"),
    ("Rua Alexandre Ivanov", "Capão Redondo", "São Paulo/SP", "05859-200"),
    ("Rua Ana Elisa", "Capão Redondo", "São Paulo/SP", "05859-010"),
    ("Rua Antônio de Luca", "Capão Redondo", "São Paulo/SP", "05859-020"),
    ("Rua Assad Bechara", "Capão Redondo", "São Paulo/SP", "05867-360")
]



# Gerar 10 clientes aleatórios
while len(clientes) < 10:
    nome_completo = f"{random.choice(nomes)} {random.choice(sobrenomes)}"
    endereco = random.choice(enderecos)
    
    # Monta o endereço completo do cliente
    endereco_completo = f"{endereco[0]}, {endereco[1]}, {endereco[2]}, {endereco[3]}"
    
    # Obtém a latitude e longitude usando a API Here
    latitude, longitude = geocode_address(endereco_completo)
    
    # Se as coordenadas forem encontradas, adiciona ao dicionário
    if latitude and longitude:
        cliente = {
            "Cliente": nome_completo,
            "Endereço": endereco[0],
            "Bairro": endereco[1],
            "CEP": endereco[3],
            "Cidade/Estado": endereco[2],
            "Latitude": latitude,
            "Longitude": longitude
        }
        if cliente not in clientes:
            clientes.append(cliente)



# Escrever os clientes em um arquivo CSV
with open('clientes_com_latitude_longitude.csv', mode='w', newline='', encoding='utf-8') as file:
    writer = csv.DictWriter(file, fieldnames=["Cliente", "Endereço", "Bairro", "CEP", "Cidade/Estado", "Latitude", "Longitude"])
    writer.writeheader()
    for cliente in clientes:
        writer.writerow(cliente)

print("Arquivo clientes_com_latitude_longitude.csv gerado com sucesso!")

