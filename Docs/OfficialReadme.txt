PER PLAYER FARM (PPF) - v0.1.0
Crie uma fazenda paralela completa para cada convidado, mantendo a Fazenda principal como hub social. Consulte os screenshots na página do mod para visualizar o teleporter, a fachada e o menu de viagem.

:: REQUISITOS ::
• Stardew Valley 1.6.15
• SMAPI 4.3.2 ou superior
• .NET 6 Runtime (para builds a partir do código-fonte)

:: CARACTERÍSTICAS PRINCIPAIS ::
• Cria locations `PPF_<UID>` verdadeiramente do tipo Farm, com limpeza inicial e suporte a construções vanilla.
• Adiciona Log Cabins de fachada em cada PPF; a porta te leva direto à sua cabana vanilla e o correio replica as animações originais.
• Teleporter exclusivo `(BC)DerexSV.PPF_Teleporter` aparece na Fazenda principal e em todas as PPFs, exibindo menu com disponibilidade e status do dono.
• Warps de entrada (Bus Stop, Forest, Backwoods, etc.) redirecionados para cada PPF, mantendo acesso livre à Fazenda principal.
• Console commands (`ppf.ensure-teleports`, `ppf.clean`, `ppf.strip`) para hosts garantirem teleporter, limpeza e remoção de estruturas vanilla.
• Configuração em `config.json` permite ajustar o tile preferido do teleporter; fallback automático e clamp ao mapa evitam travamentos.
• Eventos idempotentes: repetir criação/limpeza/retag não duplica objetos nem corrompe saves.

:: DOCUMENTAÇÃO E SUPORTE ::
• README completo (Português-BR) e traduções em README.en.md/.de.md/.es.md/.fr.md/.hu.md/.it.md/.ja.md/.ko.md/.ru.md/.tr.md/.zh.md.
• Descrição BBCode pronta em `Docs/BBCodeDescription.txt` para publicação em páginas que aceitam formatação.
• Logs e troubleshooting detalhados disponíveis no README principal.

:: COMO USAR ::
• Faça backup da sua pasta `Stardew Valley/Mods`.
• Instale o mod na pasta `Mods/PerPlayerFarm`.
• Inicie o jogo como host; o mod criará ou atualizará todas as PPFs existentes.
• Use o teleporter ou a porta da fachada para alternar entre a Fazenda principal e sua PPF.
• Execute os comandos SMAPI quando precisar reforçar teleporters, limpar ou remover Farmhouse/Greenhouse quebrada.

:: OUTROS DETALHES GERAIS ::
• Todos os assets personalizados são carregados via `AssetRequested` com prioridade, minimizando conflitos com mods de mapa.
• Compatível com partidas split-screen; o host continua sendo a autoridade sobre mudanças permanentes.
• Persistência salva em `ppf.locations`; remover o mod não destrói cabanas vanilla.

:: VANTAGEM EQUILIBRADA ::
• A proposta é ampliar espaço e organização, não fornecer itens ou progressão gratuita.
• Cada jogador continua responsável por evoluir sua PPF de acordo com o progresso no mundo do host.

RECOMENDAÇÕES ::
- Automate ou mods de maquinário (opcionais) funcionam bem graças ao status de `Farm` real das PPFs.
- Mods de textura (por exemplo, Seasonal Outfits, Eemie’s Map Recolor) deixam as PPFs visualmente mais ricas, mas são opcionais.
- Consulte o README para saber como ajustar load order caso use outros mods que modifiquem `Maps/Farm` e `Data/Buildings`.

:: PROBLEMAS CONHECIDOS ::
• Mods que substituem totalmente `Maps/Farm` podem precisar ter prioridade carregada depois do PPF para manter warps personalizados.
• Remover manualmente o teleporter sem usar os comandos pode exigir `ppf.ensure-teleports` para reconfigurar a localização.
• As PPFs não replicam automaticamente animais ou itens já existentes na Fazenda principal; isso é intencional.

:: CRÉDITOS ::
♦ DerexSV – Autor do mod, código e assets customizados.
♦ Pathoschild – Ferramentas do SMAPI e ModBuildConfig.
♦ Comunidade Stardew Valley – Testes e feedback contínuo.
♦ Jogadores que apoiam o desenvolvimento – [https://jadiael.dev/donate](https://jadiael.dev/donate)
