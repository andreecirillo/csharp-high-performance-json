# C# High-Performance JSON Processing

🚀 **Processamento de JSON de alta performance em .NET 8.** Este projeto explora a evolução de um pipeline de transformação de dados, saindo do LINQ declarativo padrão para uma abordagem de streaming de alto desempenho e baixa alocação de memória usando `ReadOnlySpan<char>` e `yield return`.

## 📌 O Desafio

O objetivo é processar um array JSON "sujo" vindo de um sistema legado. O processo envolve:
* **Limpeza de Dados:** Remover espaços em branco de nomes (*Trim*).
* **Validação de Dados:** Garantir que as pontuações (*scores*) sejam inteiros válidos entre *0* e *100*.
* **Resiliência:** Ignorar registros inválidos (nulos, pontuações alfanuméricas, etc.) sem interromper a execução.
* **Performance:** Minimizar alocações de memória e ciclos de CPU ao processar **mais de 10 milhões de registros**.

## 🛠 Funcionalidades e Implementação Moderna em C#

Este laboratório demonstra diversos recursos modernos da linguagem:
* **C# 12:** Expressões de coleção (`[]`).
* **C# 11:** Literais de string bruta (*Raw String Literals* - `"""`).
* **C# 9:** Tipos Record (*Positional Records*) e `new()` com tipo de destino simplificado.
* **C# 7:** Separadores de dígitos (`10_000_000`).
* **Gerenciamento de Memória:** `ReadOnlySpan<char>` para manipulação de strings com alocação zero.
* **Streaming:** `yield return` para execução adiada (*State Machine*).

## 📊 Resultados do Benchmark (10 Milhões de Registros)

Os resultados abaixo foram obtidos processando 10.000.000 de registros gerados aleatoriamente. O objetivo foi medir o impacto do "String Churn" e a pressão sobre o Garbage Collector (GC).

| Estratégia | Tempo de Execução | Memória Alocada | Insight Chave |
| :--- | :--- | :--- | :--- |
| **Padrão (LINQ)** | *~6.942,88 ms* | **~1.049.033 KB** | Alta pressão no GC devido a alocações no `.Trim()`. |
| **Otimizado (Local Function)** | *~6.165,17 ms* | **~1.436.654 KB** | Processamento em lote equilibrado usando `Span`. |
| **Alta Performance (Yield + Span)** | **~5.808,51 ms** | **~1.438.492 KB** | **Vencedor:** Parsing atômico e streaming. |

> **Nota:** O método "Padrão" parece usar menos memória apenas porque o Garbage Collector é forçado a rodar agressivamente para limpar milhões de strings temporárias, o que atrasa significativamente o tempo de execução total.

## 💻 Evolução do Código

### 1. A Abordagem Padrão (LINQ)
Altamente legível, mas ineficiente para grandes conjuntos de dados devido a repetidas alocações de string.
```csharp
return users
    .Where(u => !string.IsNullOrEmpty(u.name?.Trim()))
    .Select(u => new { Name = u.name.Trim(), Score = int.Parse(u.score) }) // Alocações pesadas aqui
```

### 2. A Abordagem Otimizada (Span)
Utiliza `ReadOnlySpan<char>` para validar e limpar dados na **Stack** em vez da **Heap**.
```csharp
static bool IsUserValid(User u) {
    var nameSpan = u.name.AsSpan().Trim();
    return int.TryParse(u.score.AsSpan().Trim(), out int val); // Nenhuma string nova criada aqui!
}
```

### 3. A Solução de Elite (Streaming)
Combina `Span` com `yield return` para fornecer uma transformação atômica em passagem única, evitando o "double parsing" e a materialização desnecessária de listas intermediárias.

## 🚀 Como Executar

1. Clone o repositório.
2. Certifique-se de ter o **SDK do .NET 8** instalado.
3. Execute o projeto em modo `Release` para obter resultados de benchmark precisos:
   ```bash
   dotnet run -c Release
   ```

## 📝 Artigo no Blog
Escrevi um artigo detalhado explicando o "porquê" e o "como" destas otimizações no meu blog:  
🔗 **[Challenges - JSON Processing - C#](https://andreecirillo.hashnode.dev/challeges-json-processing-csharp)**

---
**André Cirillo** *Architect of Chaos | Software Engineer | Tech Lead*