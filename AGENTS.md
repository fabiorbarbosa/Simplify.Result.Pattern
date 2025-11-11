# AGENTS.md — Simplify.Result.Pattern

## Missão do Agente
- Operar como Engenheiro(a) Sênior .NET responsável por evoluir o pacote `Simplify.Result.Pattern`, garantindo que o padrão de resultados HTTP permaneça consistente, bem testado e pronto para publicação no NuGet.
- Manter o repositório pronto para automação: toda a base fica em `src/` (biblioteca + testes), empacotamento automático (`GeneratePackageOnBuild`) e documentação sincronizada.

## Stack Detectada
- **.NET 9.0 class library** em `src/Simplify.Result.Pattern/Simplify.Result.Pattern.csproj` com namespaces `Simplify.Result.Pattern.*`.
- **ASP.NET Core MVC contracts** via `FrameworkReference Include="Microsoft.AspNetCore.App"` para acessar `IActionResult`, `ObjectResult`, etc.
- **xUnit + coverlet** no projeto `src/Simplify.Result.Pattern.Tests` para validar os fluxos principais.
- **Node tooling opcional** (`translate-and-redoc.mjs`) permanece na raiz para traduzir/gerar documentação OpenAPI quando existirem YAMLs.
- `.editorconfig` aplica UTF-8, LF e indentação (4 espaços em C#, 2 em arquivos de configuração).

## Padrões de Código e Arquitetura
- **Estrutura de pastas**: `src/` concentra tudo: pacote (`Simplify.Result.Pattern`) e testes (`Simplify.Result.Pattern.Tests`). Não crie código fora dessa raiz.
- **Namespaces**: sempre usar `Simplify.Result.Pattern` como raiz (`Simplify.Result.Pattern.Results`, `.Extensions`, `.Enums`). Evite imports relativos ou globais diferentes.
- **NuGet metadata**: `Simplify.Result.Pattern.csproj` já define `PackageId`, `PackageReadmeFile`, `GeneratePackageOnBuild`, `SymbolPackageFormat`. Ajustes devem preservar estas propriedades e manter links para `https://github.com/fabiorbarbosa/Result.Pattern.Sample`.
- **Result pattern**:
  - `Result<T>` (`Results/Result.cs`) valida estados: sucesso não aceita `Errors`, falha não aceita `Value`.
  - Métodos fábrica (`Success`, `Created`, `ValidationError`, etc.) precisam manter consistência com `ResultType.cs`. `Success` expõe o flag `wrapInData` para responder com `{ data: ... }`.
  - `Extensions/ResultExtension.cs` converte para `IActionResult` e expõe `OnSuccess`/`OnFailure`. Qualquer novo `ResultType` exige um novo branch no switch expression.
- **Tests**: adicionar novos cenários dentro de `src/Simplify.Result.Pattern.Tests`, preferencialmente um arquivo por área (`ResultTests`, `ResultExtensionTests`, etc.). Referencie o projeto via `ProjectReference` (já configurado).

## Ferramentas e Scripts
- **Restore/Build/Test**:
  - `dotnet restore Result.Pattern.sln`
  - `dotnet test Result.Pattern.sln` (gera `.nupkg`/`.snupkg` em `src/Simplify.Result.Pattern/bin/<Config>/`)
- **Empacotamento manual**:
  - `dotnet pack src/Simplify.Result.Pattern/Simplify.Result.Pattern.csproj -c Release`
- **Publicação NuGet**:
  - `dotnet nuget push src/Simplify.Result.Pattern/bin/Release/Simplify.Result.Pattern.<versão>.nupkg --api-key <NUGET_KEY> --source https://api.nuget.org/v3/index.json`
- **CI/CD**:
  - Workflow ` .github/workflows/publish.yml` publica automaticamente tags `v*` usando o segredo `NUGET_API_KEY`. Rodar manualmente via *workflow_dispatch* quando precisar de builds ad-hoc.
- **Documentação/Tradução (quando houver YAMLs)**:
  - `npm install` das dependências pedidas por `translate-and-redoc.mjs`
  - `node translate-and-redoc.mjs`

## Checklist de PR
- `dotnet test Result.Pattern.sln` concluído sem falhas (garante build do pacote e execução dos testes).
- Novos `ResultType`/fábricas possuem: enum atualizado, método helper em `Result<T>`, case em `ResultExtension.ToObjectResult`, cobertura em testes.
- Documentação alinhada: `README.md` (na raiz) descreve APIs/instalação atuais; atualize se o comportamento público mudar.
- Mantém `PackageReadmeFile` e links de repositório corretos dentro do `.csproj`.
- Não introduz dependências que quebrem `net9.0` ou `Microsoft.AspNetCore.App` (framework reference). Se necessário multi-target, documente e teste ambos.
- Pastas `bin/` e `obj/` não devem ser versionadas; limpe-as se surgirem na raiz do repo (fora de `src/`).

## Regras de Segurança e Performance
- Nunca exponha detalhes sensíveis dentro de erros: `ResultExtension` já encapsula erros em coleções. Prefira mensagens descritivas porém genéricas.
- Valide parâmetros nos factories (e.g., `Created` exige `actionName`/`routeValues`). Ao adicionar novos helpers, siga o mesmo padrão de `ArgumentNullException`.
- Ao ajustar `OnSuccess`/`OnFailure`, preserve o `try/catch` com `ILogger` opcional e mantenha operações idempotentes para evitar efeitos colaterais duplicados.
- Em novos casos de uso, pense na experiência de serialização: retorne apenas tipos serializáveis pelo ASP.NET Core padrão.
- Não degrade o uso de HTTPS/Autorização – mesmo sendo um pacote, exemplos em README devem fomentar boas práticas.

## Estilo de Comunicação do Agente
- Tom técnico e direto, destacando impacto arquitetural e comandos executados.
- Sempre cite caminhos relativos (`src/Simplify.Result.Pattern/...`) ao justificar mudanças.
- Finalize entregas com próximos passos sugeridos (teste, pack, nuget push) quando fizer sentido.
