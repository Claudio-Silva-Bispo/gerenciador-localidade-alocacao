import random
import csv
import requests
import os
from dotenv import load_dotenv




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



# Gerando as clínicas
nome_clinicas = ['Esmeralda', 'Pérola', 'Sol de Luar', 'Claríssima', 'Céu', 'Mendes', 'Levitar']
enderecos = [
    ("Rua Víctor Simeão", "Jardim Capão Redondo", "São Paulo/SP", "05882-210"),
    ("Rua Waldemiro Caldeira", "Jardim Capão Redondo", "São Paulo/SP", "05882-220"),
    ("Avenida Antônio Carlos Benjamin dos Santos", "Parque Grajaú", "São Paulo/SP", "04843-555"),
    ("Rua Sabiá Natal", "Parque Santa Cecília (Grajaú)", "São Paulo/SP", "04862-025"),
    ("Rua Seresta dos Veríssimos", "Chácara Santo Amaro", "São Paulo/SP", "04875-154"),
    ("Rua Yoshio Matsumura", "Chácara Santo Amaro", "São Paulo/SP", "04875-175"),
    ("Praça Imbirite", "Parque Santo Amaro", "São Paulo/SP", "04932-140")
]

clinicas = []

while len(clinicas) < 10:
    clinica_nome = random.choice(nome_clinicas)
    endereco = random.choice(enderecos)
    
    # Monta o endereço completo da clínica
    endereco_completo = f"{endereco[0]}, {endereco[1]}, {endereco[2]}, {endereco[3]}"
    
    # Obtém a latitude e longitude usando a API Here
    latitude, longitude = geocode_address(endereco_completo)
    
     # Se as coordenadas forem encontradas, adiciona ao dicionário
    if latitude and longitude:
        clinica = {
            "Clinica": clinica_nome,
            "Endereço": endereco[0],
            "Bairro": endereco[1],
            "CEP": endereco[3],
            "Cidade/Estado": endereco[2],
            "Latitude": latitude,
            "Longitude": longitude
        }
        if clinica not in clinicas:
            clinicas.append(clinica)
   


# Escrever as clínicas em um arquivo CSV
with open('clinicas_com_latitude_longitude.csv', mode='w', newline='', encoding='utf-8') as file:
    writer = csv.DictWriter(file, fieldnames=["Clinica", "Endereço", "Bairro", "CEP", "Cidade/Estado", "Latitude", "Longitude"])
    writer.writeheader()
    for clinica in clinicas:
        writer.writerow(clinica)

print("Arquivo clinicas_com_latitude_longitude.csv gerado com sucesso!")

