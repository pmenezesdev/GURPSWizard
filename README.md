# GURPS Wizard (MVP)

Um guia interativo para criação de personagens GURPS 4ª Edição, utilizando dados da biblioteca GCS-PTBR.

## Funcionalidades
- **Fluxo Guiado**: 8 passos desde o conceito até a revisão final.
- **Cálculo em Tempo Real**: Pontos gastos e restantes atualizados instantaneamente.
- **Biblioteca GCS-PTBR**: Carregamento automático de vantagens, perícias e equipamentos traduzidos.
- **Persistência**: Salve seus personagens em um banco SQLite local e continue editando depois.
- **Cálculos Oficiais**: Custos de atributos, secundários (HP, PF, Vontade, Percepção) e perícias (por dificuldade) seguindo o Módulo Básico.

## Requisitos
- .NET 9 SDK
- (Linux) Bibliotecas X11/Mesa para o Avalonia UI

## Como Executar

### 1. Build da Solução
O projeto utiliza o novo formato de solução do SDK 9 (`.slnx`).
```bash
dotnet build GurpsWizard.slnx
```

### 2. Executar o Aplicativo
```bash
dotnet run --project src/GurpsWizard.App
```
*Na primeira execução, o app irá processar os arquivos em `data/gcs-ptbr/` para popular o banco de dados. Isso pode levar alguns segundos.*

### 3. Rodar Testes
```bash
dotnet test
```

## Estrutura do Projeto
- `src/GurpsWizard.Core`: Lógica de domínio, modelos imutáveis e PointCalculator.
- `src/GurpsWizard.Data`: EF Core, SQLite e GcsLoader para processamento de arquivos GCS.
- `src/GurpsWizard.App`: Interface Avalonia UI seguindo o padrão MVVM com ReactiveUI.

## Dados
Os dados de vantagens, perícias e equipamentos são fornecidos pelo projeto [GCS-PTBR](https://github.com/tiagoaquinofl/GCS-PTBR), incluído como submódulo ou cópia em `data/gcs-ptbr/`.

---
*GURPS is a trademark of Steve Jackson Games. This software is a fan-made tool and is not affiliated with SJGames.*
