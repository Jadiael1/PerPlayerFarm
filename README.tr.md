# Per Player Farm (PPF)

> 🌍 Diğer dillerde README: [Português (BR)](README.md) · [English](README.en.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md) · [Magyar](README.hu.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Русский](README.ru.md) · [中文](README.zh.md)

> **Her oyuncu için paralel bir çiftlik dünyası.**
>
> **Stardew Valley 1.6.15** ve **SMAPI 4.3.2** ile uyumlu.

---

## ✨ Genel Bakış

**Per Player Farm (PPF)** aynı kayıt dosyasında *oyuncu başına ayrı bir çiftlik* oluşturur. Her çiftçi (farmhand) `Farm` tipinde, `PPF_<UniqueMultiplayerID>` adlı bir konum kazanır; başlangıç temizliği, kabin cephesi ve modun **özel teleporterı** sayesinde **ana çiftlik ↔ PPF** arasında hızlı seyahat sunar.

Tüm yapı multiplayer (host yetkili) mantığıyla tasarlanmıştır; bölünmüş ekran (split-screen) ve tekrar bağlanma senaryolarında da çalışır.

---

## 📦 Gereksinimler

* **Stardew Valley** 1.6.15
* **SMAPI** 4.3.2
* **.NET 6** (kaynak koddan derleme için)
* (Dev) **Pathoschild.Stardew.ModBuildConfig** (NuGet) otomatik deploy/zip işlemleri için

---

## 🧭 Nasıl Çalışır (üst seviye)

* **PPF Oluşturma**

  * Host: dünya yüklendiğinde/oluşturulduğunda mod, bilinen tüm `PPF_*` konumlarını üretir/garanti eder ve yeni oyuncular katıldığında yenilerini kaydeder. `PPF_*` konumları **gerçek `Farm`** ( `GameLocation` değil ) olduğu için yalnızca çiftliğe özel vanilla içerikler de yerleştirilebilir.
  * Client: host olmadan da warp/UI/seyahat işlemleri sürsün diye yerel `Farm` *stub*’ları oluşturur.

* **Harita/Assetler**

  * `Maps/PPF_*`, `Maps/Farm`’ın *klonu* olarak başlar; ayarlamalar: `CanBuildHere = T`, `MailboxLocation` kaldırılmış, istenmeyen action/warp yok.
  * `Data/Buildings` **PPF_CabinFacade** cephesini ekler; kapı tile’ı (`Action: PPF_Door`) ve vanilla davranışlı posta kutusu içerir.
  * `Data/BigCraftables` **özel teleporter** `(BC)DerexSV.PPF_Teleporter` öğesini enjekte eder; Mini Obelisk sprite’ını kullanır (istediğiniz zaman özelleştirilebilir).

* **Seyahat**

  * **PPF teleporterı** ile etkileşim, “Host Çiftliği” + her `PPF_*` girişini ve sahibin çevrimiçi durumunu gösteren bir **seyahat menüsü** açar.
  * Cephe kapısı (`PPF_Door`) sahibini doğrudan gerçek evinin (FarmHouse) içine taşır; vanilla davranış korunur.

* **Teleporter: Akıllı Konumlandırma**

  * Önce `config.json`’daki `Teleporter.PreferredTileX/PreferredTileY` ile tanımlanan **tercih edilen koordinatı** dener. Engellenmişse, `GameLocation.CanItemBePlacedHere` kuralını kullanarak sprial + tarama ile geçerli tile arar.
  * Teleporter **kaldırılıp yeniden yerleştirildiğinde** de işlev kaybetmez: event’ler objeyi izler ve **yeniden etiketler**.

* **Temizlik ve Vanilla Yapıları Kaldırma**

  * `PPF_*` için *hafif başlangıç temizliği*: otlar/taşlar/dallar, ağaçlar/çimler, resource clumps temizlenir.
  * `PPF_*` üzerinde *sürekli strip*: **Farmhouse her zaman silinir**; **Greenhouse yalnızca kırık (kilidi açılmamış) ise** kaldırılır; **Shipping Bin** ve **Pet Bowl** korunur. Bozuk sera kaldırıldığında `Greenhouse` warp’ları da silinir.

* **Warp Senkronizasyonu**

  * Event tabanlı yardımcılar (eski `PpfWarpHelper` mantığı) **Cabin → PPF** bağlantısını senkronize eder; kabin kapısı sahibini PPF’ine, çıkış ise ilgili cepheye götürür.

* **Kalıcı Veri**

  * Sahip/PPF listesi `ppf.locations` (mod save-data) dosyasında tutulur. Yeniden oluşturma daima idempotenttir.

---

## 🧠 Motivasyon ve Tasarım Kararları

Stardew Valley başta solo oyun olarak tasarlanmıştır; co-op yapı (paylaşılan çiftlik, az sayıda kabin, opsiyonel para bölüşümü) bağımsız ilerlemek isteyenler için kısıtlıdır. Bu mod, vanilla akışını (kabinler, Robin, kayıt dosyası) bozmadan her davetliye tam kapasitede bir çiftlik sunmayı hedefler.

### İlk Denemeler ve Karşılaşılan Sorunlar

İlk prototipte host’un yönetimindeki *şablonlar* (`PPF_Template`) düşünülmüştü:

1. Kayıt `PPF_Template` konumu ile başlar.
2. Host, Robin aracılığıyla bu şablona kabin/ev inşa eder.
3. Davetli oyuncu katıldığında `PPF_Template`, `PPF_<INVITER_UID>` olur.
4. Host, gelecekteki misafirler için yeni bir `PPF_Template` alır ve döngü sürer.

Bu akış, çeşitli engeller nedeniyle rafa kaldırıldı:

* **Robin entegrasyonu:** vanilla menüye özel haritaları eklemek ve ana çiftlik dışında inşa sağlamaya çalışmak diğer modlarla çatışır; UI hook, tile doğrulaması, maliyet hesabı gibi ağır müdahaleler gerektirir.
* **Kırılgan kalıcılık:** davetlinin evini tamamen özel lokasyona taşımak, mod devre dışı kaldığında referansların kaybına ve geçersiz spawn/çiftlik noktasına yol açabilir.
* **Kayıt uyumluluğu:** her oyuncu için `GameLocation` ↔ `Farm` dinamik dönüşümü; veri kaybı, evsiz hayvanlar, bozuk görev riski taşır.
* **Şablon yönetimi:** her zaman boş bir `PPF_Template` sağlamak, benzersiz adlar vermek ve artık verileri temizlemek çok oyunculu, sık giriş/çıkış durumlarında hataya açık.

### Son Mimari

Riskleri azaltmak için davetlilerin kabinleri ana çiftlikte kaldı; PPF ise **paralel çiftlik** oldu:

* Her oyuncunun `PPF_<UID>` lokasyonu (gerçek `Farm`) var; vanilla kabin ana çiftlikte durur. Mod devre dışı kalsa bile ev ve spawn noktası kaybolmaz.
* PPF’ye **cephe kabin (Log Cabin)** eklenir; kapıya custom action konur ve gerçek eve warp’lanır. Posta kutusu vanilla davranışını ve yeni posta animasyonunu korur.
* Tüm çiftliklere **özel obelisk** (`(BC)DerexSV.PPF_Teleporter`) yerleştirilir; menü tabanlı seyahat sağlar, müsaitlik/çevrimiçi durum gösterir.
* Çiftlik giriş warp’ları (Bus Stop, Forest, Backwoods vb.) oyuncunun `PPF_*`’ine yönlendirilir; ana çiftlik erişimi de devam eder.

Bu yapı, taban oyunla uyumlu kalır; mod kaldırıldığında sorun çıkmaz ve oyunculara vaadedilen özel alanı sağlar.

---

## 🕹️ Oyun İçi Kullanım

1. **Host** kaydı başlatır. Bilinen tüm PPF’ler oluşturulur ve güvence altına alınır.
2. **Ana çiftlikte** veya bir **PPF**’de teleporterla etkileşime geçin; seyahat menüsü açılır.
3. **Cephe kapısı** sahibi gerçek evinin (FarmHouse) içine taşır.
4. PPF üzerindeki **mektup ikonu** yeni posta olduğunda görünür (sadece sahibi görür).

> İpucu: Tercih edilen teleporter konumu engellenmişse mod otomatik olarak yakındaki uygun tile’a taşır.

---

## ⌨️ SMAPI Konsol Komutları

> Dünyayı **yalnızca host** değiştirebilir. Oyun açıkken SMAPI konsolunda çalıştırın.

* `ppf.ensure-teleports all|here|farm|ppf|<LocationName>`

  * **all**: tüm `PPF_*` ve **ana çiftliği** garanti eder.
  * **here**: sadece mevcut konumu garanti eder.
  * **farm**: yalnızca ana çiftlikte garanti eder.
  * **ppf**: sadece PPF_* (tümü) için.
  * **<LocationName>**: belirli isimli konumu hedef alır.

* `ppf.clean here|all|ppf`

  * Hafif temizlik (artıklar, yabani otlar, ağaçlar, resource clump) yapar.

* `ppf.strip here|all`

  * PPF_* üzerinde **Farmhouse’u daima**, **Greenhouse’u kırık ise** kaldırır; Shipping Bin/Pet Bowl olduğu yerde kalır. Kırık sera kaldırıldığında `Greenhouse` warp’larını da temizler.

---

## ⚙️ Konfigürasyon & Özelleştirme

* **Teleporter Ankrajı**: `config.json` içindeki `Teleporter.PreferredTileX` / `PreferredTileY` değerlerini ayarlayın. Hatalı veya eksik değerler 74,15’e döner ve harita sınırına clamp olur.
* **Teleporter Görünümü**: `AssetRequested` içerisindeki `Data/BigCraftables` bloğunda `Texture`/`SpriteIndex` değiştirilebilir.
* **Temizlik**: `ppf.clean` (konsol komutu) ile PPF’leri tekrar temizleyebilirsiniz.

---

## 🧩 Uyumluluk

* Tasarlanan sürümler: **Stardew Valley 1.6.15** / **SMAPI 4.3.2**
* Host **otorite** olarak kalır; client/stub kalıcı değişiklik yapmaz.
* `Maps/Farm`, `Maps/BusStop`, `Data/Buildings`, `Data/BigCraftables` gibi **aynı assetleri** editleyen modlarla yükleme sırası ayarlanabilir. PPF, öncelikli `AssetRequested` kullanır ve idempotent davranış hedeflenir.

---

## 🛠️ Geliştirme

### Hızlı Build

1. **.NET 6 SDK** kur.
2. Projeye **Pathoschild.Stardew.ModBuildConfig** paketini dahil et.
3. `PerPlayerFarm.csproj` içinde:

   ```xml
   <EnableModDeploy>true</EnableModDeploy>
   <EnableModZip>true</EnableModZip>
   <ModFolderName>PerPlayerFarm</ModFolderName>
   ```.
