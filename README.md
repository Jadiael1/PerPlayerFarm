# Per Player Farm (PPF)

> 🌍 README em outros idiomas: [Português (BR)](README.md) · [English](README.en.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md) · [Magyar](README.hu.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Русский](README.ru.md) · [Türkçe](README.tr.md) · [中文](README.zh.md)

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

## 🧠 Motivação & decisões de design

Stardew Valley nasceu como experiência solo; mesmo com o multiplayer, a estrutura padrão (uma fazenda compartilhada, poucas cabines e dinheiro conjunto opcional) é apertada para quem quer progresso independente. O objetivo deste mod é dar a cada convidado uma fazenda plena, isolada do host, sem abandonar os fluxos vanilla (cabines, Robin, save original).

### Primeiros experimentos e contras levantados

No protótipo inicial imaginei um ciclo com *templates* (`PPF_Template`) controlados pelo host:

1. O save começa com uma `PPF_Template`.
2. O host visitaria a Robin e construiria uma cabine/casa nessa template.
3. Quando um convidado entrasse, a `PPF_Template` viraria `PPF_<INVITER_UID>`.
4. O host ganharia outra `PPF_Template` vazia para futuros convidados, repetindo o processo.

Esse fluxo esbarrou em diversos problemas:

* **Integração com a Robin:** alterar o menu vanilla para listar mapas personalizados e permitir construção fora da fazenda principal conflita com outros mods e exige hooks profundos em UI, validação de tiles e custos.
* **Persistência delicada:** mover a casa do convidado para uma location totalmente customizada significa que, se o mod for desativado, o jogo perde a referência da residência; o convidado pode ficar sem ponto de spawn/cama válidos.
* **Compatibilidade com saves existentes:** converter ou reconstruir `GameLocation` ↔ `Farm` dinamicamente para cada convidado gera riscos de corrupção (itens perdidos, animais sem casa, missões quebradas).
* **Manutenção de múltiplas templates:** garantir que sempre exista uma `PPF_Template` livre, promover nomes únicos e limpar resíduos deixaria o fluxo propenso a inconsistências, especialmente em sessões com muitos convidados entrando/saindo.

### Abordagem final

Para mitigar esses riscos, optei por manter as cabines vanilla no mapa principal e criar **PPFs dedicadas apenas como “fazendas paralelas”**:

* Cada convidado ganha uma location `PPF_<UID>` de fato (`Farm` completa), mas a cabine original permanece na fazenda do host. Se o mod for desativado, o jogador ainda tem casa e spawn válidos.
* Uma **fachada (Log Cabin customizada)** é adicionada à PPF do dono; a porta executa ação personalizada que teleporta o jogador para a sua cabine real. A caixa de correio usa os tiles vanilla e exibe animação de cartas novas.
* Um **obelisco customizado** (`(BC)DerexSV.PPF_Teleporter`) aparece tanto na fazenda principal quanto em todas as PPFs. Ele gerencia a viagem entre fazendas por um menu que respeita disponibilidade e status online.
* Warps de entrada da fazenda (Bus Stop, Forest, Backwoods, etc.) são sincronizados para levar cada jogador diretamente à sua `PPF_*`, mantendo a experiência “cada um na sua fazenda”.

Essa arquitetura mantém a compatibilidade com o jogo base, reduz impacto se o mod for removido e ainda entrega o espaço dedicado que motivou o projeto.

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

* **Âncora do teleporter**: ajuste `Teleporter.PreferredTileX` / `Teleporter.PreferredTileY` em `config.json`. Se ausentes ou inválidos, o mod usa o fallback padrão (74,15) e ainda aplica clamp ao mapa.
* **Aparência do teleporter**: mude o sprite apontando `Texture`/`SpriteIndex` no bloco `Data/BigCraftables` de `AssetRequested`.
* **Limpeza**: utilize `ppf.clean` (console) para reaplicar a limpeza leve nas PPFs.

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

  * `AssetRequested/*`: injeta/edita `Maps/*`, `Data/Buildings`, `Data/BigCraftables` e marca warps customizados.
  * `ButtonPressed/*`: trata interação com o teleporter exclusivo, porta da fachada e menu de viagem.
  * `DayStarted/*`: limpeza inicial, garantia de teleporters e ajustes de warps no começo do dia.
  * `LoadStageChanged/*`: garante PPFs conhecidas ao carregar/criar o save, com base em dados persistidos e farmers.
  * `ModMessageReceived`: sincroniza o “registro” de PPFs via mensagens SMAPI.
  * `ObjectListChanged`: reetiqueta Mini‑Obelisks colocados manualmente para manter o teleporte funcional.
  * `PeerConnected/*`: host cria/garante recursos da PPF para novos jogadores e atualiza warps da casa.
  * `RenderedWorld` / `RenderingWorld`: gerenciam e desenham o indicador de correio sobre a fachada do dono.
  * `ReturnedToTitle`: limpa caches de cliente ao voltar para o menu principal.
  * `SaveLoaded/*`: carrega a PPF do convidado (cliente) e faz strip de construções vanilla nas PPF_* do host.
  * `Saving`: persiste `ppf.locations` com os UIDs conhecidos.
  * `TouchAction`: processa os touch actions personalizados para levar cada jogador à sua `PPF_*`.
* **Utils/**

  * `Constants`: chaves/modData compartilhadas pelo mod.
  * `ListHelper`: parsing/serialização das strings de warp.
  * `MailboxState`: estado temporário usado durante Rendering/RenderedWorld.
  * `PpfConsoleCommands`: comandos `ppf.ensure-teleports`, `ppf.clean` e `ppf.strip`.
* **Types/**

  * `PpfFarmEntry`, `PpfRegistryMessage`, `PpfSaveData`, `WarpLocations`: modelos de dados para multiplayer e persistência.
* **Contents/**

  * `Buildings/LogCabin.cs`: fachada PPF (`PPF_CabinFacade`).
  * `Itens/PPFTeleporter.cs`: teleporter exclusivo `(BC)DerexSV.PPF_Teleporter`.
* **i18n/**

  * Arquivos `*.json` com todas as mensagens de log e feedback localizados.
* **Configuration/**

  * `ModConfig`: opções de configuração carregadas do `config.json` do SMAPI.

### Pontos de reutilização chave

Alguns utilitários aparecem em diversos fluxos. Ao alterá-los, verifique todos os chamadores:

* `TeleportItem.Initializer` (`Events/DayStarted`, `Utils/PpfConsoleCommands`): garante/reativa teleporters tanto no ciclo diário quanto via comandos.
* `HouseWarpUtils.OverrideDefaultHouseWarpToPPF` (`Events/DayStarted`, `Events/LoadStageChanged`, `Events/PeerConnected`): substitui o warp padrão das cabanas pela saída na PPF do dono.
* `Peerconnected.Locations.LoadInvitedPpfFarmsForHost` / `LoadFacadeCabinInPpfOfInvitedForHost` (`Events/LoadStageChanged`, `Events/PeerConnected`): asseguram que host tenha a infraestrutura da PPF dos convidados.
* `Peerconnected.Locations.TrackOwner` (`Events/LoadStageChanged`, `Events/PeerConnected`): mantém o registro UID ↔ PPF consolidado no save.
* `SaveLoaded.Locations.LoadPpfFarmsForInvited` (`Events/SaveLoaded`, `Events/DayStarted`): carrega a PPF “shadow” para clientes reconectarem-se.
* `StripAllBuildingsDefault.Strip` (`Events/SaveLoaded`, `Events/DayStarted`): remove continuamente construções vanilla em PPF_*.
* `PlayerDataInitializer.CleanLocation` (`Events/DayStarted`, `Utils/PpfConsoleCommands`): limpeza inicial reutilizada pelo comando `ppf.clean`.
* `ListHelper.ConvertStringForList` (`Events/AssetRequested/FarmEntries`, `Events/TouchAction`): desserializa warps injetados no mapa.
* `PpfBuildingHelper.TryGetOwnerUid` / `GetMailboxTile` (`Events/ButtonPressed`, `Events/RenderedWorld`): definem dono e tiles para interações e overlay.

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

Contribuições **são bem‑vindas** (issues, sugestões, patches, PRs)! Para alinhar expectativas:

- Antes de começar uma funcionalidade nova, **abra uma issue** descrevendo a proposta e aguarde validação.
- Combine escopo/esforço na issue; depois envie o PR referenciando-a. Isso evita trabalho não aprovado.
- Para correções pontuais (docs/bugs), PR direto é aceito, mas ainda é recomendado sinalizar na issue correspondente.
- No código C#, mantenha o estilo (.NET 6, nullable enabled), logs `[PPF]` consistentes e ações idempotentes.

> **Importante (licenciamento):** Este mod **não é software livre**. **Cópias modificadas ou forks para redistribuição não são permitidos** sem autorização do autor. Pull Requests e patches são aceitos neste repositório, sob revisão.

---

## 📜 Licença

**Todos os direitos reservados.**

* Você pode **instalar e usar** o mod no seu jogo.
* Você pode **propor melhorias** via issues/PRs neste repositório oficial.
* **Não é permitido** publicar forks, criar mods derivados ou redistribuir versões modificadas sem consentimento explícito do autor.

---

## 🙌 Créditos

* **Autor:** DerexSV
* **Frameworks:** SMAPI, Pathoschild.Stardew.ModBuildConfig
* **Comunidade:** agradecimentos aos testadores e à comunidade de modding de Stardew Valley.

---

## 📫 Suporte

Abra uma *issue* com logs do SMAPI e uma descrição do cenário (host/cliente, mapa, passos para reproduzir). Dê preferência a reproduções curtas e com outros mods desativados, quando possível.
