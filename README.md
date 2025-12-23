# **Guia de Configuração de Ambiente - POC Extração Híbrida de Texto em PDF**

## 1\. Objetivo

Este documento descreve o passo a passo para configurar um ambiente de desenvolvimento .NET capaz de executar a solução de extração de texto de PDFs. A solução utiliza uma abordagem híbrida: primeiramente tenta a extração de texto nativo e, caso não encontre, utiliza OCR (Reconhecimento Óptico de Caracteres).

**Bibliotecas Utilizadas:**

  * **UglyToad.PdfPig:** Para extração de texto nativo de arquivos PDF.
  * **PDFiumSharp:** Para renderizar páginas de PDF como imagens (necessário para o OCR).
  * **Tesseract:** O motor de OCR para extrair texto de imagens.

## 2\. Pré-requisitos

  * **.NET SDK:** Versão 8.0 ou superior.
  * **IDE de Desenvolvimento:** Visual Studio 2022, VS Code ou JetBrains Rider.

## 3\. Passo a Passo da Configuração

### Passo 3.1: Criação do Projeto

1.  Abra seu terminal ou Prompt de Comando.
2.  Crie um novo projeto de Console App com o seguinte comando:
    ```sh
    dotnet new console -n PocPdfExtractor
    cd PocPdfExtractor
    ```
3.  Abra o projeto na sua IDE de preferência.

### Passo 3.2: Instalação das Bibliotecas (Pacotes NuGet)

Execute os seguintes comandos no terminal, dentro da pasta do projeto, para instalar as dependências necessárias.

```sh
# Biblioteca para extração de texto nativo
dotnet add package UglyToad.PdfPig

# Biblioteca para renderização de PDF (wrapper do PDFium)
dotnet add package PDFiumSharp

# Biblioteca do motor de OCR Tesseract
dotnet add package Tesseract
```

### Passo 3.3: Adição das Dependências Nativas

Esta solução depende de componentes que não são código .NET (DLLs nativas). Eles precisam ser adicionados manualmente ao projeto.

#### A. Configurando o PDFium

1.  **Baixar a DLL:** Acesse o repositório de binários do PDFium: [https://github.com/bblanchon/pdfium-binaries/releases](https://github.com/bblanchon/pdfium-binaries/releases).
2.  Faça o download da versão mais recente para **Windows x64** (arquivo `pdfium-windows-x64.tgz`).
3.  **Extrair:** Descompacte o arquivo. Dentro da pasta `bin`, você encontrará o arquivo `pdfium.dll`.
4.  **Adicionar ao Projeto:** Copie o arquivo `pdfium.dll` para a pasta raiz do seu projeto (a mesma pasta onde está o arquivo `.csproj`).
5.  **Configurar Cópia:** Na sua IDE (Visual Studio, por exemplo), clique com o botão direito sobre o arquivo `pdfium.dll`, vá em **Propriedades**, e configure a opção **"Copiar para Diretório de Saída"** como **"Copiar se for mais novo"**. Isso garante que a DLL estará junto do seu executável ao compilar.

#### B. Configurando o Tesseract (tessdata)

1.  **Criar a Pasta:** Na raiz do seu projeto, crie uma nova pasta chamada `tessdata`.
2.  **Baixar os Dados de Idioma:** O Tesseract precisa de arquivos de dados para cada idioma que ele irá reconhecer. Acesse o repositório oficial: [https://github.com/tesseract-ocr/tessdata\_best](https://github.com/tesseract-ocr/tessdata_best).
3.  Baixe os arquivos para os idiomas desejados. Para esta POC, baixe:
      * `por.traineddata` (Português)
      * `eng.traineddata` (Inglês)
4.  **Adicionar ao Projeto:** Mova os arquivos `.traineddata` que você baixou para dentro da pasta `tessdata` que você criou.
5.  **Configurar Cópia:** Na sua IDE, selecione todos os arquivos dentro da pasta `tessdata`, clique com o botão direito, vá em **Propriedades**, e configure **"Copiar para Diretório de Saída"** como **"Copiar se for mais novo"**.

### Passo 3.4: Estrutura Final do Projeto

Após seguir os passos acima, a estrutura de arquivos do seu projeto deve se parecer com isto:

```
PocPdfExtractor/
├── PocPdfExtractor.csproj
├── Program.cs
├── pdfium.dll                <-- DLL nativa na raiz
└── tessdata/
    ├── eng.traineddata       <-- Arquivo de idioma
    └── por.traineddata       <-- Arquivo de idioma
```

### Passo 3.5: Adicionando o Código Fonte

Substitua todo o conteúdo do arquivo `Program.cs` pelo código da **Solução 2** fornecido anteriormente. O código está otimizado para encontrar as dependências (`tessdata`) no diretório de execução do programa.

*(O código completo da Solução 2 deve ser inserido aqui).*

### Passo 3.6: Executando a POC

1.  Coloque um arquivo PDF de teste na pasta do projeto para facilitar o acesso.
2.  Altere a variável `pdfPath` no código para apontar para o seu arquivo PDF.
    ```csharp
    // Exemplo de como alterar no código
    var pdfPath = Path.GetFullPath("meu-arquivo-de-teste.pdf");
    ```
3.  Compile e execute o projeto através da sua IDE ou pelo terminal com o comando:
    ```sh
    dotnet run
    ```
4.  Ao final da execução, uma mensagem de "OK\!" será exibida no console, e um arquivo `.txt` com o mesmo nome do PDF original será gerado na mesma pasta, contendo o texto extraído.

## 4\. Solução de Problemas Comuns (Troubleshooting)

  * **`System.DllNotFoundException: 'pdfium.dll'`**:

      * **Causa:** O programa não conseguiu encontrar a `pdfium.dll`.
      * **Solução:** Verifique se o arquivo `pdfium.dll` está na pasta raiz do projeto e se a propriedade "Copiar para Diretório de Saída" está configurada como "Copiar se for mais novo".

  * **Erro do Tesseract sobre não encontrar o arquivo de idioma**:

      * **Causa:** O programa não conseguiu encontrar a pasta `tessdata` ou os arquivos `.traineddata`.
      * **Solução:** Confirme que a pasta `tessdata` existe na raiz do projeto e que os arquivos de idioma (`.traineddata`) estão dentro dela. Verifique também se a propriedade "Copiar para Diretório de Saída" está corretamente configurada para eles.

-----
