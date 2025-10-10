# Per Player Farm (PPF)

> **Um mundo de fazendas paralelas, uma para cada jogador.**
>
> Compatível com **Stardew Valley 1.6.15** e **SMAPI 4.3.2**.

---

## ✨ Visão geral

**Per Player Farm (PPF)** cria uma *fazenda separada por jogador* no mesmo save. Cada jogador (farmhand) ganha uma location própria do tipo `Farm`, chamada `PPF_<UniqueMultiplayerID>`, com limpeza inicial, fachada de cabana, e um fluxo de viagem rápido entre **Farm (principal)** ↔ **PPF** por meio de um **teleporter exclusivo do mod**.

Tudo foi pensado para multiplayer (host autoritativo) e também funciona em split‑screen e reconexões.

---

## 📦 Requisitos

* **Stardew Valley** 1.6.15
* **SMAPI** 4.3.2
* **.NET 6** (para compilar a partir do código‑fonte)
* (Dev) **Pathoschild.Stardew.ModBuildConfig** (NuGet) para deploy/zip automáticos

---

## 🧭 Como funciona (em alto nível)

* **Criação de PPF**

  * Host: ao carregar/criar o mundo, o mod cria/garante todas as `PPF_*` conhecidas (e registra novas quando alguém entra). As `PPF_*` são **`Farm` reais** (não `GameLocation`), então itens vanilla que exigem "só em fazenda" podem ser posicionados.
  * Cliente: cria *stubs* locais (também `Farm`) para permitir /warp, UI e viagem mesmo sem ser host.

* **Mapa/Assets**

  * `Maps/PPF_*` nasce como *clone* de `Maps/Farm` com ajustes: `CanBuildHere = T`, sem `MailboxLocation` e sem actions/warps indesejados.
  * `Data/Buildings` recebe a **fachada** `PPF_CabinFacade` com tiles de **porta** (`Action: PPF_Door`) e **caixa de correio** (Action padrão).
  * `Data/BigCraftables` injeta o **teleporter exclusivo** `(BC)DerexSV.PPF_Teleporter`, visual reaproveitando o sprite do Mini‑Obelisk (pode ser customizado depois).

* **Viagem**

  * Ao interagir com o **teleporter PPF**, abre um **menu de viagem** listando: "Fazenda do Host" + uma entrada por `PPF_*` (mostrando se o dono está online).
  * Porta da fachada (`PPF_Door`) leva o dono da PPF direto ao *interior* da sua casa (FarmHouse) — preservando o uso vanilla.

* **Teleporter: colocação inteligente**

  * Tenta primeiro na **posição preferencial** (definida no código por `PreferredAnchor`); se não for possível, busca um tile válido (espiral + varredura do mapa) usando a regra oficial `GameLocation.CanItemBePlacedHere`.
  * O teleporter **não perde a utilidade** quando removido e recolocado: eventos observam a adição e **re‑etiquetam** o objeto quando colocado novamente.

* **Limpezas e strip vanilla**

  * *Limpeza inicial/leve* em `PPF_*`: remove detritos (weeds/pedras/galhos), árvores/grass e resource clumps.
  * *Strip contínuo* em `PPF_*`: **remove sempre** a **Farmhouse**; **remove a Greenhouse somente se estiver quebrada** (não desbloqueada); **não toca** em **Shipping Bin** nem **Pet Bowl**. Ao remover a estufa quebrada, remove os warps para `Greenhouse` na PPF.

* **Sincronização de warps**

  * `PpfWarpHelper` sincroniza a ligação **Cabin → PPF** (porta da cabin do jogador passa a entrar na PPF do dono e sair na fachada correspondente).

* **Persistência**

  * Lista de donos/PPFs é salva em `ppf.locations` (save‑data do mod). A recriação é idempotente.

---

## 🕹️ Como usar (jogador)

1. **Host** carrega o save normalmente. As PPFs dos jogadores conhecidos são criadas/garantidas.
2. Em **Farm** ou numa **PPF**, **interaja com o teleporter do PPF** (big craftable exclusivo do mod) para abrir o menu de viagem.
3. Alternativamente, interaja na **porta** da fachada `PPF_CabinFacade` para entrar/voltar do interior da casa do dono.
4. O ícone de **carta flutuante** aparece sobre a caixa de correio da sua PPF quando há correspondência (apenas para o dono).

> Dica: se o teleporter estiver indisponível na posição preferida, o mod o realoca automaticamente para um tile válido próximo.

---

## ⌨️ Comandos de console (SMAPI)

> Somente **host** altera mundo. Execute no console do SMAPI com o jogo carregado.

* `ppf.ensure-teleports all|here|farm|ppf|<LocationName>`

  * **all**: garante teleporters em **todas** as `PPF_*` e na **Farm**.
  * **here**: garante **apenas** na location atual.
  * **farm**: garante **apenas** na Farm principal.
  * **ppf**: garante **apenas** nas PPF_* (todas).
  * **<LocationName>**: garante especificamente naquela location.

* `ppf.clean here|all|ppf`

  * Limpeza leve de detritos/ervas/árvores/resource clumps.

* `ppf.strip here|all`

  * Remove **Farmhouse** sempre e **Greenhouse se quebrada** nas PPF_* (não altera Shipping Bin/Pet Bowl). Também limpa warps para `Greenhouse` quando a estufa for removida.

---

## ⚙️ Configuração & personalização

* **Âncora do teleporter**: ajuste `PreferredAnchor` em `PpfTeleportItemPlacer` para definir a posição preferencial (x,y). O mod fará fallback automático se o tile estiver bloqueado.
* **Aparência do teleporter**: mude o sprite apontando `Texture`/`SpriteIndex` no bloco `Data/BigCraftables` de `AssetRequested`.
* **Limpeza**: `PpfCleanHelper.CleanLocation(Farm)` pode ser chamada manualmente (ou via console) se quiser re‑limpar uma PPF.

---

## 🧩 Compatibilidade

* Projetado para **1.6.15** / **SMAPI 4.3.2**.
* Host é **autoridade**: clientes/stubs não fazem mudanças permanentes.
* Mods que alteram **os mesmos assets** (ex.: `Maps/Farm`, `Maps/BusStop`, `Data/Buildings`, `Data/BigCraftables`) podem precisar de ajuste de **ordem de carregamento**. O mod usa `AssetRequested` com prioridades e tenta ser idempotente.

---

## 🛠️ Desenvolvimento

### Build rápido

1. Instale o **.NET 6 SDK**.
2. Garanta o pacote **Pathoschild.Stardew.ModBuildConfig** no projeto.
3. No `PerPlayerFarm.csproj`, ative:

   ```xml
   <EnableModDeploy>true</EnableModDeploy>
   <EnableModZip>true</EnableModZip>
   <ModFolderName>PerPlayerFarm</ModFolderName>
   ```
4. (Se necessário) crie `stardewvalley.targets` no seu diretório de usuário apontando `GamePath`/`GameModsPath`.
5. Compile:

   ```bash
   dotnet build -c Release
   ```

   O ModBuildConfig copia a pasta para `…/Stardew Valley/Mods/PerPlayerFarm` e pode gerar o `.zip`.

### Estrutura principal do código

* **ModEntry**: registra handlers e inicializa gerenciadores.
* **Events/**

  * `AssetRequested`: injeta/edita `Maps/*`, `Data/Buildings`, `Data/BigCraftables`.
  * `LoadStageChanged`, `PeerConnected`: garantem PPFs conforme players conhecidos/entrantes.
  * `DayStartedHostSync` / `DayStartedClientShadow` / `SaveLoadedClientShadow`: sincronizam PPFs e stubs.
  * `ButtonPressed`: porta/caixa‑de‑correio da fachada e abertura do menu via interação.
  * `MailboxDisplay`: overlay do ícone de carta na PPF do dono.
* **Utils/**

  * `EnsurePerPlayerFarm`: cria/garante PPF_* e persiste `ppf.locations`.
  * `PpfWarpHelper`: cabin ↔ PPF (warps) e entrada/saída.
  * `PpfTravelMenuManager`: menu de viagem + mensagens multiplayer com registro de PPFs.
  * `PpfTeleportItemPlacer`: teleporter `(BC)DerexSV.PPF_Teleporter` (âncora + fallback + re‑tag).
  * `PpfFarmVanillaStripper`: remove **Farmhouse** sempre e **Greenhouse se quebrada** nas PPF_* (contínuo).
  * `PpfCleanHelper`: limpeza leve de objetos/terreno/resource clumps.
  * `PpfBuildingHelper`: metadados do dono e helpers da fachada.
  * `ListHelper` / `WarpLocations`: utilitários para parsing de warps.

---

## 🧯 Solução de problemas

* **“O teleporter não apareceu.”**

  * Use `ppf.ensure-teleports here` na location atual; verifique o console por logs `[PPF]` de tentativa/falha de colocação.
* **“Clicar no teleporter abriu o vanilla.”**

  * Certifique‑se de que é o **teleporter do PPF** (item exclusivo) e não o Mini‑Obelisk vanilla. O PPF só intercepta o item exclusivo do mod.
* **“Não consigo colocar Mini‑Obelisk vanilla na PPF.”**

  * PPFs precisam ser do tipo `Farm`. Este mod já cria `PPF_*` como `Farm`. Se você migrou de versão antiga (que usava `GameLocation`), remova/reative o mod para recriar as locations, ou converta manualmente.

---

## 🤝 Contribuições

Contribuições **são bem‑vindas** (issues, sugestões, PRs)! Siga as boas práticas:

* Estilo C# consistente (.NET 6, nullable‑enabled).
* Eventos SMAPI: preferir handlers enxutos por classe, log `[PPF]` com `LogLevel.Trace/Info/Warn` conforme o caso.
* Idempotência primeiro: toda ação deve poder rodar múltiplas vezes sem duplicar objetos/dados.
* Multiplayer: lembre que apenas o **host** altera mundo; clientes devem apenas refletir estado.

> **Importante (licenciamento):** Este mod **não é software livre**. **Cópias modificadas ou forks para redistribuição não são permitidos** sem autorização do autor. Pull Requests e patches são aceitos neste repositório, sob revisão.

---

## 📜 Licença

**Todos os direitos reservados.**

* Você pode **instalar e usar** o mod no seu jogo.
* Você pode **propor mudanças** via PR/patch neste repositório.
* **Não é permitido** publicar forks, criar mods derivados ou redistribuir versões modificadas sem consentimento explícito do autor.

---

## 🙌 Créditos

* **Autor:** DerexSV
* **Frameworks:** SMAPI, Pathoschild.Stardew.ModBuildConfig
* **Comunidade:** agradecimentos aos testadores e à comunidade de modding de Stardew Valley.

---

## 📫 Suporte

Abra uma *issue* com logs do SMAPI e uma descrição do cenário (host/cliente, mapa, passos para reproduzir). Dê preferência a reproduções curtas e com outros mods desativados, quando possível.
