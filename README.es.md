# Per Player Farm (PPF)

> 🌍 README en otros idiomas: [Português (BR)](README.md) · [English](README.en.md) · [Deutsch](README.de.md) · [Français](README.fr.md) · [Magyar](README.hu.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Русский](README.ru.md) · [Türkçe](README.tr.md) · [中文](README.zh.md)

> **Un mundo de granjas paralelas, una para cada jugador.**
>
> Compatible con **Stardew Valley 1.6.15** y **SMAPI 4.3.2**.

---

## ✨ Resumen

**Per Player Farm (PPF)** crea una *granja separada por jugador* dentro del mismo guardado. Cada granjero invitado recibe una location propia de tipo `Farm`, llamada `PPF_<UniqueMultiplayerID>`, con limpieza inicial, fachada de cabaña y un flujo de viaje rápido entre la **granja principal** ↔ **PPF** mediante un **teletransportador exclusivo** del mod.

Todo está pensado para multijugador (anfitrión autoritativo) y también funciona en pantalla dividida y reconexiones.

---

## 📦 Requisitos

* **Stardew Valley** 1.6.15
* **SMAPI** 4.3.2
* **.NET 6** (para compilar desde el código fuente)
* (Dev) **Pathoschild.Stardew.ModBuildConfig** (NuGet) para despliegues/zips automáticos

---

## 🧭 Cómo funciona (alto nivel)

* **Creación de PPF**

  * Anfitrión: al cargar/crear el mundo, el mod crea/asegura todos los `PPF_*` conocidos (y registra nuevos cuando alguien entra). Las `PPF_*` son **granjas reales (`Farm`)** (no `GameLocation`), así que los ítems vanilla restringidos a granjas pueden colocarse sin problemas.
  * Cliente: crea *stubs* locales (también `Farm`) para permitir warps, UI y viaje incluso sin el anfitrión presente.

* **Mapas/Assets**

  * `Maps/PPF_*` nace como *clon* de `Maps/Farm` con ajustes: `CanBuildHere = T`, sin `MailboxLocation` ni acciones/warps indeseados.
  * `Data/Buildings` recibe la **fachada** `PPF_CabinFacade` con tiles de puerta (`Action: PPF_Door`) y buzón con acción vanilla.
  * `Data/BigCraftables` inyecta el **teletransportador exclusivo** `(BC)DerexSV.PPF_Teleporter`, reutilizando el sprite del Mini-Obelisco (personalizable a futuro).

* **Viajes**

  * Al interactuar con el **teletransportador PPF**, se abre un **menú de viaje** con “Granja del anfitrión” y una entrada por cada `PPF_*` (indicando si el dueño está en línea).
  * La puerta de la fachada (`PPF_Door`) lleva al dueño directamente al interior de su casa (FarmHouse), manteniendo el comportamiento original.

* **Teletransportador: colocación inteligente**

  * Primero intenta la **posición preferida** (configurable via `Teleporter.PreferredTileX/PreferredTileY` en `config.json`). Si está bloqueada, busca un tile válido (espiral + escaneo del mapa) usando la regla `GameLocation.CanItemBePlacedHere`.
  * El teletransportador **sigue funcionando** al moverlo: los eventos vigilan la colocación y **retaggean** el objeto automáticamente.

* **Limpieza y retiro de estructuras vanilla**

  * *Limpieza inicial ligera* en `PPF_*`: quita malezas/piedras/ramas, árboles/pasto y resource clumps.
  * *Retiro continuo* en `PPF_*`: **elimina siempre** la **Farmhouse**; **elimina el invernadero solo si está roto** (no desbloqueado); **no toca** la **caja de envíos** ni el **platón de mascota**. Al quitar el invernadero roto, también se eliminan los warps hacia `Greenhouse`.

* **Sincronización de warps**

  * Los eventos sincronizan la conexión **Cabin → PPF** (la puerta de la cabin lleva a la PPF del dueño y la salida conduce a la fachada correspondiente).

* **Persistencia**

  * La lista de dueños/PPFs se guarda en `ppf.locations` (datos del mod). La recreación es idempotente.

---

## 🧠 Motivación y decisiones

Stardew Valley se diseñó como experiencia en solitario; incluso con multijugador, la estructura estándar (una granja compartida, pocas cabañas, dinero opcional por separado) queda limitada para quienes desean progresar de forma independiente. El objetivo del mod es dar a cada invitado una granja propia sin romper los flujos vanilla (cabañas, Robin, guardado).

### Primeros experimentos y contras

El primer prototipo planteaba un ciclo con *plantillas* (`PPF_Template`) administradas por el anfitrión:

1. El guardado empezaba con una `PPF_Template`.
2. El anfitrión pediría en casa de Robin que construyera una cabaña en esa plantilla.
3. Al entrar un invitado, la `PPF_Template` pasaba a ser `PPF_<INVITER_UID>`.
4. El anfitrión recibía otra `PPF_Template` para futuros invitados, repitiendo el proceso.

Problemas de ese flujo:

* **Integración con Robin:** ampliar el menú vanilla para mostrar mapas personalizados y permitir construir fuera de la granja principal choca con otros mods y requiere hooks profundos, validaciones de tiles y costos.
* **Persistencia frágil:** mover la casa a una location totalmente personalizada implica que, si el mod se desactiva, el juego pierde la referencia del hogar y el invitado podría quedar sin punto de spawn/cama válidos.
* **Compatibilidad con saves existentes:** convertir `GameLocation` ↔ `Farm` según cada invitado arriesga corrupción (ítems perdidos, animales sin casa, misiones rotas).
* **Gestión de múltiples plantillas:** garantizar que siempre exista una `PPF_Template` libre, promover nombres únicos y limpiar restos es propenso a errores, especialmente con muchos invitados entrando/saliendo.

### Arquitectura final

Para evitarlo, la cabaña invitada permanece en la granja principal y la PPF actúa como **granja paralela**:

* Cada jugador tiene una `PPF_<UID>` (real `Farm`), pero su cabaña vanilla sigue en la granja principal; si el mod se desactiva, la casa y el spawn siguen válidos.
* Se añade una **fachada (Log Cabin)** a la PPF; su puerta tiene una acción personalizada que lleva a la cabaña real. El buzón replica los tiles vanilla (incluyendo la animación de nuevo correo).
* Un **obelisco personalizado** (`(BC)DerexSV.PPF_Teleporter`) aparece en todas las granjas, gestionando viajes vía menú y mostrando disponibilidad/estado en línea.
* Las entradas de la granja (Bus Stop, Forest, Backwoods, etc.) se sincronizan para que cada jugador entre directamente a su `PPF_*`, manteniendo el acceso a la granja principal.

Esta arquitectura evita romper el juego base, minimiza problemas si el mod se quita y entrega el espacio independiente planeado.

---

## 🕹️ Uso (jugadores)

1. El **anfitrión** carga el guardado. Se crean/aseguran las PPF de jugadores conocidos.
2. En la **granja principal** o en una **PPF**, interactúa con el teletransportador para abrir el menú de viaje.
3. La **puerta de la fachada** teletransporta al dueño al interior de su casa real (FarmHouse).
4. El ícono flotante de **carta** aparece sobre el buzón cuando hay correo (solo para el dueño).

> Consejo: si la posición preferida del teletransportador está bloqueada, el mod lo reubica automáticamente en un tile válido cercano.

---

## ⌨️ Comandos de consola (SMAPI)

> Solo el **anfitrión** cambia el mundo. Ejecútalos en la consola de SMAPI con el juego cargado.

* `ppf.ensure-teleports all|here|farm|ppf|<LocationName>`

  * **all**: garantiza teletransportadores en **todas** las `PPF_*` y la **granja principal**.
  * **here**: solo en la location actual.
  * **farm**: solo en la granja principal.
  * **ppf**: solo en todas las `PPF_*`.
  * **<LocationName>**: garantiza en la location nombrada.

* `ppf.clean here|all|ppf`

  * Limpieza ligera de restos/árboles/piedras/recursos.

* `ppf.strip here|all`

  * Quita **Farmhouse** siempre y **Greenhouse si está rota** en `PPF_*` (no toca caja de envíos/platón). Elimina warps a `Greenhouse` al quitar el invernadero roto.

---

## ⚙️ Configuración y personalización

* **Ancla del teletransportador:** ajusta `Teleporter.PreferredTileX` / `Teleporter.PreferredTileY` en `config.json`. Valores ausentes/invalidos vuelven a 74,15 y se ajustan al mapa.
* **Apariencia del teletransportador:** cambia `Texture`/`SpriteIndex` en el bloque `Data/BigCraftables` de `AssetRequested`.
* **Limpieza:** usa `ppf.clean` (consola) para re-limpiar una PPF.

---

## 🧩 Compatibilidad

* Diseñado para **1.6.15** / **SMAPI 4.3.2**.
* El anfitrión es **autoridad**: los clientes no aplican cambios permanentes.
* Mods que modifiquen los mismos assets (`Maps/Farm`, `Maps/BusStop`, `Data/Buildings`, `Data/BigCraftables`) pueden requerir ajustar el orden de carga. El mod usa `AssetRequested` con prioridades y busca ser idempotente.

---

## 🛠️ Desarrollo

### Compilación rápida

1. Instala el **.NET 6 SDK**.
2. Asegura el paquete **Pathoschild.Stardew.ModBuildConfig**.
3. En `PerPlayerFarm.csproj`, habilita:

   ```xml
   <EnableModDeploy>true</EnableModDeploy>
   <EnableModZip>true</EnableModZip>
   <ModFolderName>PerPlayerFarm</ModFolderName>
   ```
4. (Si es necesario) crea `stardewvalley.targets` con `GamePath`/`GameModsPath`.
5. Compila:

   ```bash
   dotnet build -c Release
   ```

   El ModBuildConfig copia la carpeta a `…/Stardew Valley/Mods/PerPlayerFarm` y puede generar `.zip`.

### Estructura del código

* **ModEntry**: registra manejadores e inicializa gestores.
* **Events/**

  * `AssetRequested/*`: inyecta/edita `Maps/*`, `Data/Buildings`, `Data/BigCraftables`, marca warps personalizados.
  * `ButtonPressed/*`: maneja interacción con teletransportador, puerta de fachada y menú de viaje.
  * `DayStarted/*`: limpieza, teletransportadores y ajustes de warps al comenzar el día.
  * `LoadStageChanged/*`: asegura PPF conocidas al cargar el guardado (datos persistidos/farmers).
  * `ModMessageReceived`: sincroniza el registro de PPF vía mensajes SMAPI.
  * `ObjectListChanged`: retaggea Mini-Obeliscos manuales para mantener el teletransporte.
  * `PeerConnected/*`: el host garantiza recursos PPF para nuevos jugadores y actualiza warps de casa.
  * `RenderedWorld` / `RenderingWorld`: gestionan/dibujan el indicador de correo en la fachada del dueño.
  * `ReturnedToTitle`: limpia caches de cliente al regresar al menú.
  * `SaveLoaded/*`: carga PPFs para invitados (cliente) y retira estructuras vanilla en PPFs del anfitrión.
  * `Saving`: guarda `ppf.locations` con UIDs conocidos.
  * `UpdateTicked`: reemplaza warps para que cada jugador llegue a su `PPF_*`.
* **Utils/**

  * `Constants`: claves compartidas/modData.
  * `ListHelper`: parsing/serialización de cadenas de warps.
  * `MailboxState`: estado temporal para Rendering/RenderedWorld.
  * `PpfConsoleCommands`: comandos `ppf.ensure-teleports`, `ppf.clean`, `ppf.strip`.
* **Types/**

  * `PpfFarmEntry`, `PpfRegistryMessage`, `PpfSaveData`, `WarpLocations`: modelos de datos para multijugador/persistencia.
* **Contents/**

  * `Buildings/LogCabin.cs`: fachada `PPF_CabinFacade`.
  * `Itens/PPFTeleporter.cs`: teletransportador exclusivo `(BC)DerexSV.PPF_Teleporter`.
* **i18n/**

  * Mensajes y registros localizados en `*.json`.
* **Configuration/**

  * `ModConfig`: opciones desde `config.json`.

### Reutilizaciones clave

Algunos helpers se usan en varios flujos; cualquier cambio debe considerar todos los llamadores:

* `TeleportItem.Initializer` (`Events/DayStarted`, `Utils/PpfConsoleCommands`): asegura teletransportadores en el ciclo diario y vía comandos.
* `HouseWarpUtils.OverrideDefaultHouseWarpToPPF` (`Events/DayStarted`, `Events/LoadStageChanged`, `Events/PeerConnected`): redirige la salida de las cabinas a la PPF del dueño.
* `Peerconnected.Locations.LoadInvitedPpfFarmsForHost` / `LoadFacadeCabinInPpfOfInvitedForHost` (`Events/LoadStageChanged`, `Events/PeerConnected`): garantiza que el anfitrión tenga infraestructuras PPF para invitados.
* `Peerconnected.Locations.TrackOwner` (`Events/LoadStageChanged`, `Events/PeerConnected`): mantiene el vínculo UID ↔ PPF guardado.
* `SaveLoaded.Locations.LoadPpfFarmsForInvited` (`Events/SaveLoaded`, `Events/DayStarted`): carga la “PPF sombra” del cliente.
* `StripAllBuildingsDefault.Strip` (`Events/SaveLoaded`, `Events/DayStarted`): elimina estructuras vanilla en PPFs continuamente.
* `PlayerDataInitializer.CleanLocation` (`Events/DayStarted`, `Utils/PpfConsoleCommands`): limpieza inicial reutilizada por `ppf.clean`.
* `ListHelper.ConvertStringForList` (`Events/AssetRequested/FarmEntries`, `Events/UpdateTicked`): deserializa warps inyectados.
* `PpfBuildingHelper.TryGetOwnerUid` / `GetMailboxTile` (`Events/ButtonPressed`, `Events/RenderedWorld`): determina dueño y tiles clave para interacción/overlay.

---

## 🧯 Solución de problemas

* **“El teletransportador no apareció.”**

  * Ejecuta `ppf.ensure-teleports here` en la location actual; revisa la consola SMAPI por logs `[PPF]`.
* **“Al hacer clic se abrió el teletransportador vanilla.”**

  * Asegúrate de usar el **teletransportador PPF** (ítem exclusivo), no el Mini-Obelisk vanilla.
* **“No puedo colocar el Mini-Obelisk vanilla en la PPF.”**

  * Las PPF deben ser de tipo `Farm`. Este mod crea `PPF_*` como `Farm`. Si migraste de una versión antigua (`GameLocation`), quita/rehabilita el mod para recrear locations o conviértelas manualmente.

---

## 🤝 Contribuciones

¡Las contribuciones **son bienvenidas** (issues, sugerencias, parches, PRs)! Para alinearnos:

* Antes de una gran funcionalidad, abre una **issue** describiendo la idea y espera feedback.
* Define el alcance en la issue, luego envía el PR referenciándola, evitando retrabajo.
* Para correcciones pequeñas (docs/bugs), PR directo está bien, pero recomienda avisar en la issue.
* Estilo: C# (.NET 6, nullable), logs `[PPF]` con Trace/Info/Warn, lógica idempotente, anfitrión autoritativo.

> **Importante (licencia):** Este mod **no es software libre**. **No se permiten forks ni distribuciones modificadas** sin autorización del autor. Los PRs y patches se aceptan aquí bajo revisión.

---

## 📜 Licencia

**Todos los derechos reservados.**

* Puedes **instalar y usar** el mod en tu juego.
* Puedes **proponer mejoras** vía issues/PRs en este repositorio oficial.
* **No está permitido** publicar forks, crear mods derivados o redistribuir versiones modificadas sin consentimiento explícito del autor.

---

## 🙌 Créditos

* **Autor:** DerexSV
* **Frameworks:** SMAPI, Pathoschild.Stardew.ModBuildConfig
* **Comunidad:** gracias a testers y a la comunidad modder de Stardew Valley.

---

## 📫 Soporte

Abre una *issue* con los logs de SMAPI y una descripción del escenario (anfitrión/cliente, mapa, pasos para reproducir). Mejor si puedes reproducir con otros mods desactivados y casos simples.
