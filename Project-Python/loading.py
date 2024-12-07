import pandas as pd
from haversine import haversine
import time  # Para adicionar o delay de "loading"

# Carregar os dados dos clientes e clínicas a partir de arquivos CSV
clientes_df = pd.read_csv('clientes_com_latitude_longitude.csv')
clinicas_df = pd.read_csv('clinicas_com_latitude_longitude.csv')

# Função para salvar os agendamentos no arquivo CSV
def salvar_agendamento(cliente, clinica, tipo, data, horario):
    agendamento = {
        "Cliente": cliente['Cliente'],
        "Endereço Cliente": cliente['Endereço'],
        "Bairro Cliente": cliente['Bairro'],
        "CEP Cliente": cliente['CEP'],
        "Clínica": clinica['Clinica'],
        "Endereço Clínica": clinica['Endereço'],
        "Bairro Clínica": clinica['Bairro'],
        "CEP Clínica": clinica['CEP'],
        "Tipo": tipo,
        "Data": data,
        "Horário": horario
    }
    
    try:
        agendamentos_df = pd.read_csv('agendamentos.csv')
        agendamentos_df = pd.concat([agendamentos_df, pd.DataFrame([agendamento])], ignore_index=True)
    except FileNotFoundError:
        agendamentos_df = pd.DataFrame([agendamento])
    
    agendamentos_df.to_csv('agendamentos.csv', index=False)




# Função para o cliente escolher data e horário para atendimento
def escolher_data_horario(cliente, clinica, tipo):
    if tipo == "Imediato":
        data_escolhida = "Hoje"
        horarios_disponiveis = ["10:00", "11:30", "14:00", "16:00"]
    else:
        datas_disponiveis = ["01/12", "02/12", "03/12"]
        horarios_disponiveis = ["09:00", "11:00", "13:30", "15:30"]
        
        print("\nDatas disponíveis:")
        for idx, data in enumerate(datas_disponiveis, 1):
            print(f"{idx} - {data}")
        opcao_data = input("Escolha uma data pelo número: ").strip()
        
        while not opcao_data.isdigit() or int(opcao_data) not in range(1, len(datas_disponiveis) + 1):
            print("Opção inválida! Escolha uma data pelo número.")
            opcao_data = input("Escolha uma data pelo número: ").strip()
        data_escolhida = datas_disponiveis[int(opcao_data) - 1]
    
    print("\nHorários disponíveis:")
    for idx, horario in enumerate(horarios_disponiveis, 1):
        print(f"{idx} - {horario}")
    opcao_horario = input("Escolha um horário pelo número: ").strip()
    
    while not opcao_horario.isdigit() or int(opcao_horario) not in range(1, len(horarios_disponiveis) + 1):
        print("Opção inválida! Escolha um horário pelo número.")
        opcao_horario = input("Escolha um horário pelo número: ").strip()
    horario_escolhido = horarios_disponiveis[int(opcao_horario) - 1]
    
    print(f"\nSua consulta foi agendada para {data_escolhida} às {horario_escolhido}.")
    salvar_agendamento(cliente, clinica, tipo, data_escolhida, horario_escolhido)
    
    

# Função principal para gerenciar a solicitação de emergência
def gerenciar_solicitacao(cliente):
    print("=== Nova Solicitação de Emergência ===")
    print("Informações do Cliente:")
    print(f"Nome: {cliente['Cliente'].values[0]}")
    print(f"Endereço: {cliente['Endereço'].values[0]}")
    print(f"Bairro: {cliente['Bairro'].values[0]}")
    print(f"CEP: {cliente['CEP'].values[0]}")
    print(f"Cidade/Estado: {cliente['Cidade/Estado'].values[0]}")
    print()

    lat_cliente, lng_cliente = cliente['Latitude'].values[0], cliente['Longitude'].values[0]
    clinicas_proximas = clinicas_df.copy()
    clinicas_proximas['Distancia'] = clinicas_proximas.apply(
        lambda clinica: haversine((lat_cliente, lng_cliente), (clinica['Latitude'], clinica['Longitude'])),
        axis=1
    )
    clinicas_proximas = clinicas_proximas.sort_values(by='Distancia')

    for _, clinica in clinicas_proximas.iterrows():
        print("Buscando clínica mais próxima... carregando", end="")
        for _ in range(3):  # Loop para criar o efeito de "loading" com 3 pontos
            time.sleep(1)
            print(".", end="", flush=True)
        print()  # Quebra a linha após a carga
        
        print("Enviando solicitação para a clínica mais próxima...")
        print("Informações da Clínica:")
        print(f"Clínica: {clinica['Clinica']}")
        print(f"Distância do cliente: {clinica['Distancia']:.2f} km")
        print()

        while True:
            resposta = input("A clínica aceita a solicitação? (sim/não): ").strip().lower()
            if resposta == "sim":
                print(f"A clínica {clinica['Clinica']} aceitou a solicitação!")
                print("Informando o cliente...")

                print("\nOpções para o cliente:")
                print("1 - Atendimento imediato")
                print("2 - Agendamento futuro")
                opcao = input("Escolha uma opção (1 ou 2): ").strip()
                
                while opcao not in ["1", "2"]:
                    print("Opção inválida! Escolha 1 ou 2.")
                    opcao = input("Escolha uma opção (1 ou 2): ").strip()

                tipo = "Imediato" if opcao == "1" else "Agendamento futuro"
                escolher_data_horario(cliente.iloc[0], clinica, tipo)
                return
            elif resposta == "não":
                # Adicionando opções de motivos para recusa
                print("\nMotivos para a recusa da clínica:")
                motivos = [
                    "Fechado no momento", 
                    "Sem disponibilidade de horário", 
                    "Alto volume de atendimentos no momento",
                    "Horário solicitado fora da operação"
                ]
                for idx, motivo in enumerate(motivos, 1):
                    print(f"{idx} - {motivo}")
                opcao_motivo = input("Escolha um motivo pelo número: ").strip()

                while not opcao_motivo.isdigit() or int(opcao_motivo) not in range(1, len(motivos) + 1):
                    print("Opção inválida! Escolha um motivo pelo número.")
                    opcao_motivo = input("Escolha um motivo pelo número: ").strip()
                
                motivo_escolhido = motivos[int(opcao_motivo) - 1]
                print(f"Motivo da recusa: {motivo_escolhido}")
                print("Solicitação recusada. Buscando próxima clínica...\n")
                break
            else:
                print("Resposta inválida. Por favor, responda 'sim' ou 'não'.\n")

    print("Nenhuma clínica disponível aceitou a solicitação. Tente novamente mais tarde.")
    
    
    
    
    

# Selecionar um cliente aleatório do banco de dados
cliente_aleatorio = clientes_df.sample(1)
gerenciar_solicitacao(cliente_aleatorio)
